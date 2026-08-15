using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ObtenerResumenDashboardUseCase _obtenerResumenUseCase;
    private readonly ObtenerAlertasDashboardUseCase _obtenerAlertasUseCase;

    public DashboardController(
        ObtenerResumenDashboardUseCase obtenerResumenUseCase,
        ObtenerAlertasDashboardUseCase obtenerAlertasUseCase)
    {
        _obtenerResumenUseCase = obtenerResumenUseCase;
        _obtenerAlertasUseCase = obtenerAlertasUseCase;
    }

    [HttpGet("resumen")]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        var resumen = await _obtenerResumenUseCase.ExecuteAsync(cancellationToken);
        return Ok(resumen);
    }

    [HttpGet("alertas")]
    [Authorize(Roles = "Principal,Supervisora")]
    public async Task<IActionResult> GetAlertas(CancellationToken cancellationToken)
    {
        var alertas = await _obtenerAlertasUseCase.ExecuteAsync(cancellationToken);
        return Ok(alertas);
    }
}
