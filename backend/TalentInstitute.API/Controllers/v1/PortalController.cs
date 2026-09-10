using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Application.UseCases.Portal;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

/// <summary>
/// Portal de consulta para padres de familia (issue #8).
///
/// Vive aparte de los controladores del personal a propósito. Toda su
/// superficie es de lectura y queda restringida al rol Padre en un solo lugar;
/// si en cambio se hubiera agregado ese rol a los controladores internos,
/// bastaría con olvidar un filtro en cualquiera de ellos para exponer datos de
/// alumnos ajenos.
/// </summary>
[Authorize(Roles = PadreFamilia.RolToken)]
[ApiController]
[Route("api/v1/[controller]")]
public class PortalController : ControllerBase
{
    private readonly ObtenerHijosDelPadreUseCase _obtenerHijosUseCase;
    private readonly VerificarAccesoDelPadreUseCase _verificarAcceso;
    private readonly ObtenerMetasSemanaUseCase _obtenerMetasSemanaUseCase;
    private readonly ObtenerMeritosAlumnoUseCase _obtenerMeritosAlumnoUseCase;

    public PortalController(
        ObtenerHijosDelPadreUseCase obtenerHijosUseCase,
        VerificarAccesoDelPadreUseCase verificarAcceso,
        ObtenerMetasSemanaUseCase obtenerMetasSemanaUseCase,
        ObtenerMeritosAlumnoUseCase obtenerMeritosAlumnoUseCase)
    {
        _obtenerHijosUseCase = obtenerHijosUseCase;
        _verificarAcceso = verificarAcceso;
        _obtenerMetasSemanaUseCase = obtenerMetasSemanaUseCase;
        _obtenerMeritosAlumnoUseCase = obtenerMeritosAlumnoUseCase;
    }

    [HttpGet("hijos")]
    public async Task<IActionResult> GetHijos(CancellationToken cancellationToken)
    {
        if (!TryGetPadreId(out var padreId)) return Unauthorized(new { message = "Identidad del usuario no válida." });

        var hijos = await _obtenerHijosUseCase.ExecuteAsync(padreId, cancellationToken);
        return Ok(hijos);
    }

    [HttpGet("hijos/{alumnoId:guid}/metas")]
    public async Task<IActionResult> GetMetas(Guid alumnoId, [FromQuery] DateTime? semana, CancellationToken cancellationToken)
    {
        if (!TryGetPadreId(out var padreId)) return Unauthorized(new { message = "Identidad del usuario no válida." });

        try
        {
            await _verificarAcceso.EnsureAsync(padreId, alumnoId, cancellationToken);

            // Sin parámetro se asume la semana en curso; el caso de uso exige lunes.
            var inicio = semana ?? LunesDeLaSemana(DateTime.UtcNow);
            var metas = await _obtenerMetasSemanaUseCase.ExecuteAsync(alumnoId, inicio.Date, cancellationToken);

            return Ok(metas);
        }
        catch (AccesoDenegadoException ex)
        {
            return Forbid_(ex.Message);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("hijos/{alumnoId:guid}/meritos")]
    public async Task<IActionResult> GetMeritos(Guid alumnoId, CancellationToken cancellationToken)
    {
        if (!TryGetPadreId(out var padreId)) return Unauthorized(new { message = "Identidad del usuario no válida." });

        try
        {
            await _verificarAcceso.EnsureAsync(padreId, alumnoId, cancellationToken);

            var meritos = await _obtenerMeritosAlumnoUseCase.ExecuteAsync(alumnoId, cancellationToken);
            return Ok(meritos);
        }
        catch (AccesoDenegadoException ex)
        {
            return Forbid_(ex.Message);
        }
    }

    private bool TryGetPadreId(out Guid padreId)
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(idStr, out padreId);
    }

    /// <summary>
    /// 403 con cuerpo. `Forbid()` delega en el esquema de autenticación y
    /// responde sin mensaje, y aquí el frontend necesita algo que mostrar.
    /// </summary>
    private ObjectResult Forbid_(string message)
        => StatusCode(StatusCodes.Status403Forbidden, new { message });

    private static DateTime LunesDeLaSemana(DateTime fecha)
    {
        int desplazamiento = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.Date.AddDays(-desplazamiento);
    }
}
