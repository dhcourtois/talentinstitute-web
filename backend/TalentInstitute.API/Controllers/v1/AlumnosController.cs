using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly ObtenerAlumnosUseCase _obtenerAlumnosUseCase;
    private readonly ObtenerDetalleAlumnoUseCase _obtenerDetalleAlumnoUseCase;
    private readonly CrearAlumnoUseCase _crearAlumnoUseCase;
    private readonly EditarAlumnoUseCase _editarAlumnoUseCase;

    public AlumnosController(
        ObtenerAlumnosUseCase obtenerAlumnosUseCase,
        ObtenerDetalleAlumnoUseCase obtenerDetalleAlumnoUseCase,
        CrearAlumnoUseCase crearAlumnoUseCase,
        EditarAlumnoUseCase editarAlumnoUseCase)
    {
        _obtenerAlumnosUseCase = obtenerAlumnosUseCase;
        _obtenerDetalleAlumnoUseCase = obtenerDetalleAlumnoUseCase;
        _crearAlumnoUseCase = crearAlumnoUseCase;
        _editarAlumnoUseCase = editarAlumnoUseCase;
    }

    [HttpGet]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var alumnos = await _obtenerAlumnosUseCase.ExecuteAsync(cancellationToken);
        return Ok(alumnos);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Principal,Supervisora,Monitora")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var alumno = await _obtenerDetalleAlumnoUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(alumno);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> Create([FromBody] CreateAlumnoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _crearAlumnoUseCase.ExecuteAsync(
                request.NumeroMatricula,
                request.Nombre,
                request.Apellido,
                request.Nivel,
                request.FechaIngreso,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Alumno creado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (TalentInstitute.Domain.Exceptions.DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAlumnoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _editarAlumnoUseCase.ExecuteAsync(
                id,
                request.Nombre,
                request.Apellido,
                request.Nivel,
                request.FechaIngreso,
                cancellationToken);

            return Ok(new { message = "Alumno actualizado exitosamente." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (TalentInstitute.Domain.Exceptions.DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateAlumnoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;

    /// <summary>
    /// Nula deja la fecha de ingreso como está (issue #22): así un cliente que
    /// no manda el campo no reescribe un dato que el usuario no editó.
    /// </summary>
    public DateTime? FechaIngreso { get; set; }
}

public class CreateAlumnoRequest
{
    public string NumeroMatricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;

    /// <summary>Nula toma la fecha de alta, el comportamiento previo.</summary>
    public DateTime? FechaIngreso { get; set; }
}
