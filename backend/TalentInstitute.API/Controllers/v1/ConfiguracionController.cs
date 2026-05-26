using System;
using System.Security.Claims;
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
public class ConfiguracionController : ControllerBase
{
    private readonly ObtenerConfiguracionPrivilegiosUseCase _obtenerConfiguracionUseCase;
    private readonly ActualizarConfiguracionPrivilegiosUseCase _actualizarConfiguracionUseCase;

    public ConfiguracionController(
        ObtenerConfiguracionPrivilegiosUseCase obtenerConfiguracionUseCase,
        ActualizarConfiguracionPrivilegiosUseCase actualizarConfiguracionUseCase)
    {
        _obtenerConfiguracionUseCase = obtenerConfiguracionUseCase;
        _actualizarConfiguracionUseCase = actualizarConfiguracionUseCase;
    }

    [HttpGet("privilegios")]
    public async Task<IActionResult> GetUmbrales(CancellationToken cancellationToken)
    {
        var config = await _obtenerConfiguracionUseCase.ExecuteAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut("privilegios")]
    public async Task<IActionResult> UpdateUmbrales([FromBody] UpdateConfiguracionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var staffId))
            {
                return Unauthorized(new { message = "Identidad del usuario no válida." });
            }

            await _actualizarConfiguracionUseCase.ExecuteAsync(
                request.UmbralOficina, request.UmbralOficinaRevocado,
                request.UmbralComedor, request.UmbralComedorRevocado,
                request.UmbralPatio, request.UmbralPatioRevocado,
                request.UmbralBiblioteca, request.UmbralBibliotecaRevocado,
                request.UmbralActividades, request.UmbralActividadesRevocado,
                staffId,
                cancellationToken);

            return Ok(new { message = "Umbrales de privilegios actualizados exitosamente." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateConfiguracionRequest
{
    public int UmbralOficina { get; set; }
    public int UmbralOficinaRevocado { get; set; }
    public int UmbralComedor { get; set; }
    public int UmbralComedorRevocado { get; set; }
    public int UmbralPatio { get; set; }
    public int UmbralPatioRevocado { get; set; }
    public int UmbralBiblioteca { get; set; }
    public int UmbralBibliotecaRevocado { get; set; }
    public int UmbralActividades { get; set; }
    public int UmbralActividadesRevocado { get; set; }
}
