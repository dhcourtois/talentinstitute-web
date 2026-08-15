using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.API.Controllers.v1;

[Authorize(Roles = "Principal")]
[ApiController]
[Route("api/v1/[controller]")]
public class StaffController : ControllerBase
{
    private readonly ObtenerStaffUseCase _obtenerStaffUseCase;
    private readonly CrearStaffUseCase _crearStaffUseCase;
    private readonly EditarStaffUseCase _editarStaffUseCase;
    private readonly DesactivarStaffUseCase _desacivarStaffUseCase;

    public StaffController(
        ObtenerStaffUseCase obtenerStaffUseCase,
        CrearStaffUseCase crearStaffUseCase,
        EditarStaffUseCase editarStaffUseCase,
        DesactivarStaffUseCase desacivarStaffUseCase)
    {
        _obtenerStaffUseCase = obtenerStaffUseCase;
        _crearStaffUseCase = crearStaffUseCase;
        _editarStaffUseCase = editarStaffUseCase;
        _desacivarStaffUseCase = desacivarStaffUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var staff = await _obtenerStaffUseCase.ExecuteAsync(cancellationToken);
        return Ok(staff);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _crearStaffUseCase.ExecuteAsync(
                request.Email,
                request.Password,
                request.Rol,
                cancellationToken);

            return CreatedAtAction(nameof(GetAll), null, new { id, message = "Miembro de Staff registrado exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{staffId:guid}")]
    public async Task<IActionResult> Edit(Guid staffId, [FromBody] EditStaffRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _editarStaffUseCase.ExecuteAsync(staffId, request.Rol, cancellationToken);
            return Ok(new { message = "Rol de Staff actualizado exitosamente." });
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

    [HttpPatch("{staffId:guid}/desactivar")]
    public async Task<IActionResult> Deactivate(Guid staffId, CancellationToken cancellationToken)
    {
        try
        {
            await _desacivarStaffUseCase.ExecuteAsync(staffId, cancellationToken);
            return Ok(new { message = "Miembro de Staff desactivado exitosamente." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class CreateStaffRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class EditStaffRequest
{
    public string Rol { get; set; } = string.Empty;
}
