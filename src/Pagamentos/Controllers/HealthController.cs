using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos.Controllers;

[ApiController]
[Route("health")]
public class HealthController(DataContext banco) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Verificar()
    {
        try
        {
            // Consulta real na tabela: CanConnectAsync daria "ok" mesmo sem a tabela,
            // porque o SQLite cria um arquivo vazio quando o banco não existe.
            await banco.Pagamentos.AnyAsync();
            return Ok(new { status = "Healthy", banco = "SQLite acessível" });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = "Unhealthy", banco = "SQLite inacessível" });
        }
    }
}
