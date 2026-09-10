using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases.Portal;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

/// <summary>
/// Administración de las cuentas de padres de familia (issue #8).
///
/// Restringido al Principal: dar de alta una cuenta y vincularle un alumno
/// decide quién puede ver el expediente de ese alumno, así que es una
/// atribución de dirección, no de operación diaria.
/// </summary>
[Authorize(Roles = "Principal")]
[ApiController]
[Route("api/v1/[controller]")]
public class PadresFamiliaController : ControllerBase
{
    private readonly CrearPadreFamiliaUseCase _crearPadreUseCase;
    private readonly VincularAlumnoAPadreUseCase _vincularUseCase;
    private readonly ObtenerPadresFamiliaUseCase _obtenerPadresUseCase;
    private readonly CambiarEstadoPadreFamiliaUseCase _cambiarEstadoUseCase;

    public PadresFamiliaController(
        CrearPadreFamiliaUseCase crearPadreUseCase,
        VincularAlumnoAPadreUseCase vincularUseCase,
        ObtenerPadresFamiliaUseCase obtenerPadresUseCase,
        CambiarEstadoPadreFamiliaUseCase cambiarEstadoUseCase)
    {
        _crearPadreUseCase = crearPadreUseCase;
        _vincularUseCase = vincularUseCase;
        _obtenerPadresUseCase = obtenerPadresUseCase;
        _cambiarEstadoUseCase = cambiarEstadoUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _obtenerPadresUseCase.ExecuteAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPadreFamiliaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _crearPadreUseCase.ExecuteAsync(
                request.Email,
                request.Password,
                request.Nombre,
                cancellationToken);

            return Ok(new { id, message = "Cuenta de padre de familia creada exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/hijos")]
    public async Task<IActionResult> Vincular(Guid id, [FromBody] VincularHijoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _vincularUseCase.VincularAsync(id, request.AlumnoId, cancellationToken);
            return Ok(new { message = "Alumno vinculado a la cuenta exitosamente." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/hijos/{alumnoId:guid}")]
    public async Task<IActionResult> Desvincular(Guid id, Guid alumnoId, CancellationToken cancellationToken)
    {
        try
        {
            await _vincularUseCase.DesvincularAsync(id, alumnoId, cancellationToken);
            return Ok(new { message = "Alumno desvinculado de la cuenta." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoPadreRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _cambiarEstadoUseCase.ExecuteAsync(id, request.Activo, cancellationToken);
            return Ok(new { message = request.Activo ? "Cuenta reactivada." : "Cuenta desactivada." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class CrearPadreFamiliaRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class VincularHijoRequest
{
    public Guid AlumnoId { get; set; }
}

public class CambiarEstadoPadreRequest
{
    public bool Activo { get; set; }
}
