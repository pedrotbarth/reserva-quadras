using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservas.DTO;
using Reservas.Servicos;
using System.Text.Json;

namespace Reservas.Controllers;

[ApiController]
[Route("api/reservas")]
public class ReservasController(DataContext banco, ServReserva servico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Reserva>>> Listar(CancellationToken ct)
    {
        var reservas = await banco.Reservas.AsNoTracking()
            .OrderBy(reserva => reserva.Data).ThenBy(reserva => reserva.HorarioInicio).ToListAsync(ct);
        return Ok(reservas);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Reserva>> BuscarPorId(Guid id, CancellationToken ct)
    {
        var reserva = await banco.Reservas.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return reserva is null ? NotFound() : Ok(reserva);
    }

    [HttpPost]
    public async Task<ActionResult<Reserva>> Criar(CriarReservaDto dados, CancellationToken ct)
    {
        try
        {
            var reserva = await servico.CriarAsync(dados, ct);
            return CreatedAtAction(nameof(BuscarPorId), new { id = reserva.Id }, reserva);
        }
        catch (HorarioOcupadoException erro)
        {
            return Conflict(new { mensagem = erro.Message, sugestoes = erro.Sugestoes });
        }
        catch (ReservaInvalidaException erro)
        {
            return BadRequest(new { mensagem = erro.Message });
        }
        catch (Exception erro) when (erro is HttpRequestException or JsonException ||
            (erro is OperationCanceledException && !ct.IsCancellationRequested))
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Não foi possível consultar Clientes ou Quadras.");
        }
    }

    [HttpPatch("{id:guid}/confirmar-pagamento")]
    public async Task<IActionResult> ConfirmarPagamento(Guid id, ConfirmarPagamentoDto dados, CancellationToken ct)
    {
        var pagamentoId = dados.PagamentoId.GetValueOrDefault();
        if (pagamentoId == Guid.Empty)
            return BadRequest(new { mensagem = "Informe o ID do pagamento." });

        var alteradas = await banco.Reservas
            .Where(reserva => reserva.Id == id && reserva.Status == StatusReserva.AguardandoPagamento)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(reserva => reserva.Status, StatusReserva.Confirmada)
                .SetProperty(reserva => reserva.PagamentoId, (Guid?)pagamentoId), ct);
        if (alteradas > 0)
            return NoContent();

        var atual = await banco.Reservas.AsNoTracking().FirstOrDefaultAsync(reserva => reserva.Id == id, ct);
        if (atual is null)
            return NotFound();

        if (atual.Status == StatusReserva.Confirmada && atual.PagamentoId == pagamentoId)
            return NoContent();

        return Conflict(new { mensagem = "A reserva não está aguardando pagamento." });
    }

    [HttpDelete("{id:guid}")]
    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        var alteradas = await banco.Reservas
            .Where(reserva => reserva.Id == id && reserva.Status != StatusReserva.Cancelada)
            .ExecuteUpdateAsync(setters => setters.SetProperty(reserva => reserva.Status, StatusReserva.Cancelada), ct);

        if (alteradas > 0 || await banco.Reservas.AnyAsync(reserva => reserva.Id == id, ct))
            return NoContent();

        return NotFound();
    }
}
