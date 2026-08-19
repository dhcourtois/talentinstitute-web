using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TalentInstitute.Application.UseCases;

namespace TalentInstitute.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;

    public AuthController(LoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _loginUseCase.ExecuteAsync(request.Email, request.Password, request.VistaInicial);
            return Ok(new { token });
        }
        catch (System.Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VistaInicial { get; set; } = string.Empty;
}
