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
        catch (ErroPagamentoException erro)
        {
            return Erro(erro);
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

    [HttpPost("{id:guid}/confirmar-reserva")]
    public async Task<ActionResult<Pagamento>> ConfirmarReserva(Guid id)
    {
        try
        {
            var pagamento = await servico.ConfirmarReservaAsync(id);

            if (pagamento is null)
                return NotFound();

            return Ok(pagamento);
        }
        catch (ErroPagamentoException erro)
        {
            return Erro(erro);
        }
    }

    private ObjectResult Erro(ErroPagamentoException erro)
    {
        return Problem(title: erro.Titulo, detail: erro.Message, statusCode: erro.StatusCode);
    }
}
