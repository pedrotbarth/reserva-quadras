using Pagamentos.DTO;
using Pagamentos.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Pagamentos.Controllers;

[ApiController]
[Route("api/pagamentos")]
public class PagamentosController(ServPagamento servico) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Pagamento>> Registrar(CriarPagamentoDto dados)
    {
        try
        {
            var pagamento = await servico.RegistrarAsync(dados);
            return CreatedAtAction(nameof(BuscarPorId), new { id = pagamento.Id }, pagamento);
        }
        catch (PagamentoDuplicadoException erro)
        {
            return Problem(
                title: "Pagamento duplicado.",
                detail: erro.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Pagamento>> BuscarPorId(Guid id)
    {
        var pagamento = await servico.BuscarPorIdAsync(id);

        if (pagamento is null)
            return NotFound();

        return Ok(pagamento);
    }

    [HttpGet]
    public async Task<ActionResult<List<Pagamento>>> Listar()
    {
        return Ok(await servico.ListarAsync());
    }
}
