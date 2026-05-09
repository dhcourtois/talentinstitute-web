using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PacesController : ControllerBase
{
    private readonly CheckStudentPaceProgressUseCase _checkStudentPaceProgressUseCase;

    public PacesController(CheckStudentPaceProgressUseCase checkStudentPaceProgressUseCase)
    {
        _checkStudentPaceProgressUseCase = checkStudentPaceProgressUseCase;
    }

    [HttpPost("{alumnoPaceId:guid}/check")]
    public async Task<IActionResult> CheckPaceProgress(Guid alumnoPaceId, [FromBody] CheckPaceRequest request)
    {
        await _checkStudentPaceProgressUseCase.ExecuteAsync(alumnoPaceId, request.Exitoso);
        return Ok(new { message = "AutoTest completado y estado actualizado exitosamente." });
    }
}

public class CheckPaceRequest
{
    public bool Exitoso { get; set; }
}
