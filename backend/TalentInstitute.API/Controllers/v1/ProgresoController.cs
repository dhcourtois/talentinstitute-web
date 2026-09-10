using System;
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
public class ProgresoController : ControllerBase
{
    private readonly ObtenerMetasSemanaUseCase _obtenerMetasSemanaUseCase;
    private readonly RegistrarMetaUseCase _registrarMetaUseCase;
    private readonly ActualizarEstatusMetaUseCase _actualizarEstatusMetaUseCase;

    public ProgresoController(
        ObtenerMetasSemanaUseCase obtenerMetasSemanaUseCase,
        RegistrarMetaUseCase registrarMetaUseCase,
        ActualizarEstatusMetaUseCase actualizarEstatusMetaUseCase)
    {
        _obtenerMetasSemanaUseCase = obtenerMetasSemanaUseCase;
        _registrarMetaUseCase = registrarMetaUseCase;
        _actualizarEstatusMetaUseCase = actualizarEstatusMetaUseCase;
    }

    [HttpGet("{alumnoId:guid}/semana/{fechaInicio:datetime}")]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> GetMetasSemana(Guid alumnoId, DateTime fechaInicio, CancellationToken cancellationToken)
    {
        try
        {
            var metas = await _obtenerMetasSemanaUseCase.ExecuteAsync(alumnoId, fechaInicio, cancellationToken);
            return Ok(metas);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> RegistrarMeta([FromBody] RegistrarMetaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<Turno>(request.Turno, true, out var turnoEnum))
            {
                return BadRequest(new { message = $"El turno '{request.Turno}' no es válido. Debe ser Mañana o Tarde." });
            }

            var id = await _registrarMetaUseCase.ExecuteAsync(
                request.AlumnoPaceId,
                turnoEnum,
                request.PaginaInicial,
                request.PaginaFinal,
                request.FechaObjetivo,
                cancellationToken);

            return Ok(new { id, message = "Meta registrada exitosamente." });
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

    [HttpPut("metas/{metaId:guid}/estatus")]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> ActualizarEstatusMeta(Guid metaId, [FromBody] ActualizarEstatusMetaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Seguridad: Solo Principal o Supervisora pueden registrar el Score de un Score Station
            if (request.Estado.Equals("Scored", StringComparison.OrdinalIgnoreCase) &&
                !User.IsInRole("Principal") && !User.IsInRole("Supervisora"))
            {
                return StatusCode(403, new { message = "Acceso denegado. Solo la Supervisora o el Principal pueden calificar en la Score Station." });
            }

            await _actualizarEstatusMetaUseCase.ExecuteAsync(
                metaId,
                request.Estado,
                request.PuntajeObtenido,
                cancellationToken);

            return Ok(new { message = "Estado de meta actualizado exitosamente." });
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

public class RegistrarMetaRequest
{
    public Guid AlumnoPaceId { get; set; }
    public string Turno { get; set; } = string.Empty;

    /// <summary>Primera página del rango, inclusive.</summary>
    public int PaginaInicial { get; set; }

    /// <summary>
    /// Última página del rango, inclusive. Igual a la inicial registra una sola
    /// página, como antes del issue #6.
    /// </summary>
    public int PaginaFinal { get; set; }

    public DateOnly FechaObjetivo { get; set; }
}

public class ActualizarEstatusMetaRequest
{
    public string Estado { get; set; } = string.Empty;
    public decimal? PuntajeObtenido { get; set; }
}
