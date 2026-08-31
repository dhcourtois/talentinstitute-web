using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

[Authorize(Roles = "Principal,Supervisora")]
[ApiController]
[Route("api/v1/Entrevistas")]
public class EntrevistasPadresController : ControllerBase
{
    private readonly RegistrarEntrevistaUseCase    _registrar;
    private readonly ObtenerEntrevistasUseCase     _obtenerTodas;
    private readonly ObtenerDetalleEntrevistaUseCase _obtenerDetalle;

    public EntrevistasPadresController(
        RegistrarEntrevistaUseCase    registrar,
        ObtenerEntrevistasUseCase     obtenerTodas,
        ObtenerDetalleEntrevistaUseCase obtenerDetalle)
    {
        _registrar      = registrar;
        _obtenerTodas   = obtenerTodas;
        _obtenerDetalle = obtenerDetalle;
    }

    /// <summary>Lista todas las entrevistas registradas, ordenadas por fecha descendente.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var entrevistas = await _obtenerTodas.ExecuteAsync(cancellationToken);
        return Ok(entrevistas.Select(MapToResponse));
    }

    /// <summary>Detalle de una entrevista específica.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var entrevista = await _obtenerDetalle.ExecuteAsync(id, cancellationToken);
            return Ok(MapToResponse(entrevista));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Registra una nueva entrevista a padre / familia interesada.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEntrevistaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _registrar.ExecuteAsync(
                request.NombrePadre,
                request.NumeroHijos,
                request.RiesgoViolencia,
                request.RiesgoDivorcio,
                request.ConoceADios,
                request.Comentarios ?? string.Empty,
                request.Aceptado,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Entrevista registrada exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static EntrevistaResponse MapToResponse(TalentInstitute.Domain.Entities.EntrevistaPadre e) =>
        new(e.Id, e.NombrePadre, e.NumeroHijos, e.RiesgoViolencia, e.RiesgoDivorcio,
            e.ConoceADios, e.Comentarios, e.Aceptado, e.FechaEntrevista);
}

public record CreateEntrevistaRequest(
    string NombrePadre,
    int    NumeroHijos,
    bool   RiesgoViolencia,
    bool   RiesgoDivorcio,
    bool   ConoceADios,
    string? Comentarios,
    bool   Aceptado);

public record EntrevistaResponse(
    Guid     Id,
    string   NombrePadre,
    int      NumeroHijos,
    bool     RiesgoViolencia,
    bool     RiesgoDivorcio,
    bool     ConoceADios,
    string   Comentarios,
    bool     Aceptado,
    DateTime FechaEntrevista);

