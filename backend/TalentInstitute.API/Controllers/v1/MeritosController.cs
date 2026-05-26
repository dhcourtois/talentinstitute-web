using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class MeritosController : ControllerBase
{
    private readonly RegistrarMeritoUseCase _registrarMeritoUseCase;
    private readonly ObtenerMeritosAlumnoUseCase _obtenerMeritosAlumnoUseCase;
    private readonly RevocarMeritoUseCase _revocarMeritoUseCase;

    public MeritosController(
        RegistrarMeritoUseCase registrarMeritoUseCase,
        ObtenerMeritosAlumnoUseCase obtenerMeritosAlumnoUseCase,
        RevocarMeritoUseCase revocarMeritoUseCase)
    {
        _registrarMeritoUseCase = registrarMeritoUseCase;
        _obtenerMeritosAlumnoUseCase = obtenerMeritosAlumnoUseCase;
        _revocarMeritoUseCase = revocarMeritoUseCase;
    }

    [HttpPost]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarMeritoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var staffId))
            {
                return Unauthorized(new { message = "Identidad del usuario no válida." });
            }

            if (!Enum.TryParse<TipoMerito>(request.Tipo, true, out var tipoEnum))
            {
                return BadRequest(new { message = $"El tipo '{request.Tipo}' no es válido. Debe ser Merito o Demerito." });
            }

            var id = await _registrarMeritoUseCase.ExecuteAsync(
                request.AlumnoId,
                staffId,
                tipoEnum,
                request.Puntos,
                request.Motivo,
                cancellationToken);

            return Ok(new { id, message = $"{request.Tipo} registrado exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("alumno/{alumnoId:guid}")]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> GetByAlumno(Guid alumnoId, CancellationToken cancellationToken)
    {
        var meritos = await _obtenerMeritosAlumnoUseCase.ExecuteAsync(alumnoId, cancellationToken);
        return Ok(meritos);
    }

    [PATCH("{meritoId:guid}/revocar")]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> Revocar(Guid meritoId, CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var staffId))
            {
                return Unauthorized(new { message = "Identidad del usuario no válida." });
            }

            await _revocarMeritoUseCase.ExecuteAsync(meritoId, staffId, cancellationToken);
            return Ok(new { message = "Mérito revocado y privilegios del alumno recalculados exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class RegistrarMeritoRequest
{
    public Guid AlumnoId { get; set; }
    public string Tipo { get; set; } = string.Empty; // Merito / Demerito
    public int Puntos { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
