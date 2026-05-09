using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TalentInstitute.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class EntrevistasPadresController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEntrevista([FromBody] CreateEntrevistaRequest request)
    {
        // El caso de uso de registro se implementará más adelante.
        // Aquí retornaríamos 201 Created si es válido, o un DomainException si rompe las reglas.
        await Task.CompletedTask;
        return Created("", new { message = "Entrevista registrada. (Stub)" });
    }
}

public class CreateEntrevistaRequest
{
    public string NombrePadre { get; set; } = string.Empty;
    public int NumeroHijos { get; set; }
    public bool RiesgoViolencia { get; set; }
    public bool RiesgoDivorcio { get; set; }
    public bool ConoceADios { get; set; }
    public string Comentarios { get; set; } = string.Empty;
}
