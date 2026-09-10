using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PacesController : ControllerBase
{
    private readonly CheckStudentPaceProgressUseCase _checkStudentPaceProgressUseCase;
    private readonly ObtenerCatalogoPacesUseCase _obtenerCatalogoPacesUseCase;
    private readonly CrearPaceUseCase _crearPaceUseCase;
    private readonly AsignarPaceUseCase _asignarPaceUseCase;
    private readonly ObtenerPacesAlumnoUseCase _obtenerPacesAlumnoUseCase;

    public PacesController(
        CheckStudentPaceProgressUseCase checkStudentPaceProgressUseCase,
        ObtenerCatalogoPacesUseCase obtenerCatalogoPacesUseCase,
        CrearPaceUseCase crearPaceUseCase,
        AsignarPaceUseCase asignarPaceUseCase,
        ObtenerPacesAlumnoUseCase obtenerPacesAlumnoUseCase)
    {
        _checkStudentPaceProgressUseCase = checkStudentPaceProgressUseCase;
        _obtenerCatalogoPacesUseCase = obtenerCatalogoPacesUseCase;
        _crearPaceUseCase = crearPaceUseCase;
        _asignarPaceUseCase = asignarPaceUseCase;
        _obtenerPacesAlumnoUseCase = obtenerPacesAlumnoUseCase;
    }

    [HttpGet]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> GetCatalogo([FromQuery] string? subject, CancellationToken cancellationToken)
    {
        var paces = await _obtenerCatalogoPacesUseCase.ExecuteAsync(subject, cancellationToken);
        return Ok(paces);
    }

    [HttpPost]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> Create([FromBody] CreatePaceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _crearPaceUseCase.ExecuteAsync(
                request.Materia,
                request.NumeroPace,
                request.PuntajeMaximo,
                request.PuntajeMinimoAprobacion,
                request.TotalPaginas,
                cancellationToken);

            return CreatedAtAction(nameof(GetCatalogo), null, new { id, message = "PACE agregado al catálogo exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("asignar")]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> Asignar([FromBody] AsignarPaceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _asignarPaceUseCase.ExecuteAsync(request.AlumnoId, request.PaceId, cancellationToken);
            return Ok(new { id, message = "PACE asignado al alumno exitosamente." });
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
        var paces = await _obtenerPacesAlumnoUseCase.ExecuteAsync(alumnoId, cancellationToken);
        return Ok(paces);
    }

    [HttpPost("{alumnoPaceId:guid}/check")]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> CheckPaceProgress(Guid alumnoPaceId, [FromBody] CheckPaceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _checkStudentPaceProgressUseCase.ExecuteAsync(alumnoPaceId, request.Exitoso, cancellationToken);
            return Ok(new { message = "AutoTest completado y estado actualizado exitosamente." });
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

public class CreatePaceRequest
{
    public string Materia { get; set; } = string.Empty;
    /// <summary>
    /// Alfanumérico: 1045 o RR01. Se normaliza a mayúsculas y no admite
    /// espacios ni signos.
    /// </summary>
    public string NumeroPace { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; } = 100;
    public int PuntajeMinimoAprobacion { get; set; } = 80;

    /// <summary>
    /// Total de páginas del cuadernillo. Opcional: sin él no se valida el
    /// rango de páginas de una meta contra el PACE (issue #6).
    /// </summary>
    public int? TotalPaginas { get; set; }
}

public class AsignarPaceRequest
{
    public Guid AlumnoId { get; set; }
    public Guid PaceId { get; set; }
}

public class CheckPaceRequest
{
    public bool Exitoso { get; set; }
}
