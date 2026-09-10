using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

/// <summary>
/// Observaciones semanales por alumno, para el reporte semanal (issue #20).
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AnotacionesController : ControllerBase
{
    private readonly RegistrarAnotacionUseCase _registrarAnotacionUseCase;
    private readonly ObtenerAnotacionesAlumnoUseCase _obtenerAnotacionesAlumnoUseCase;

    public AnotacionesController(
        RegistrarAnotacionUseCase registrarAnotacionUseCase,
        ObtenerAnotacionesAlumnoUseCase obtenerAnotacionesAlumnoUseCase)
    {
        _registrarAnotacionUseCase = registrarAnotacionUseCase;
        _obtenerAnotacionesAlumnoUseCase = obtenerAnotacionesAlumnoUseCase;
    }

    /// <summary>
    /// Los tres roles escriben observaciones: la Monitora es quien acompaña el
    /// trabajo diario y por eso es la que más tiene que anotar.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAnotacionRequest request, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var staffId))
        {
            return Unauthorized(new { message = "Identidad del usuario no válida." });
        }

        try
        {
            var id = await _registrarAnotacionUseCase.ExecuteAsync(
                request.AlumnoId,
                staffId,
                request.Texto,
                request.Fecha,
                cancellationToken);

            return Ok(new { id, message = "Anotación registrada exitosamente." });
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
        var anotaciones = await _obtenerAnotacionesAlumnoUseCase.ExecuteAsync(alumnoId, cancellationToken);
        return Ok(anotaciones);
    }
}

public class RegistrarAnotacionRequest
{
    public Guid AlumnoId { get; set; }
    public string Texto { get; set; } = string.Empty;

    /// <summary>
    /// Día al que corresponde la observación; se ancla al lunes de esa semana.
    /// Nula la registra en la semana en curso.
    /// </summary>
    public DateOnly? Fecha { get; set; }
}
