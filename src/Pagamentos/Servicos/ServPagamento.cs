using Pagamentos.DTO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos.Servicos;

public class ServPagamento(DataContext banco, ClienteReservas reservas, ILogger<ServPagamento> logger)
{
    // CÃ³digo de erro do SQLite para violaÃ§Ã£o de restriÃ§Ã£o (aqui, o Ã­ndice Ãºnico de ReservaId).
    private const int ErroRestricaoSqlite = 19;

    public async Task<Pagamento> RegistrarAsync(CriarPagamentoDto dados)
    {
        var reservaId = dados.ReservaId.GetValueOrDefault();

        var jaPaga = await banco.Pagamentos.AnyAsync(pagamento =>
            pagamento.ReservaId == reservaId &&
            pagamento.Status == StatusPagamento.Aprovado);

        if (jaPaga)
        {
            logger.LogWarning("Pagamento recusado por duplicidade: a reserva {ReservaId} jÃ¡ estÃ¡ paga", reservaId);
            throw new PagamentoDuplicadoException(reservaId);
        }

        // Confere com Reservas antes de gravar qualquer coisa.
        await ValidarReservaAsync(reservaId, dados.Valor);

        var pagamento = new Pagamento
        {
            ReservaId = reservaId,
            Valor = dados.Valor,
            FormaPagamento = dados.FormaPagamento.GetValueOrDefault(),
            Status = dados.SimularRecusa ? StatusPagamento.Recusado : StatusPagamento.Aprovado,
            DataPagamento = DateTime.UtcNow,
            ReservaConfirmada = false
        };

        banco.Pagamentos.Add(pagamento);

        try
        {
            await banco.SaveChangesAsync();
        }
        catch (DbUpdateException erro) when (erro.InnerException is SqliteException { SqliteErrorCode: ErroRestricaoSqlite })
        {
            // Dois pedidos ao mesmo tempo passaram pela checagem acima; o Ã­ndice do banco barrou o segundo.
            logger.LogWarning("Pagamento recusado por duplicidade (pedidos simultÃ¢neos): a reserva {ReservaId} jÃ¡ estÃ¡ paga", reservaId);
            throw new PagamentoDuplicadoException(reservaId);
        }

        logger.LogInformation(
            "Pagamento {PagamentoId} registrado: reserva {ReservaId}, {Valor} via {FormaPagamento}, status {Status}",
            pagamento.Id, pagamento.ReservaId, pagamento.Valor, pagamento.FormaPagamento, pagamento.Status);

        if (pagamento.Status == StatusPagamento.Aprovado)
            await ConfirmarNaReservaAsync(pagamento);

        return pagamento;
    }

    // Nova tentativa de confirmar a reserva de um pagamento aprovado que ficou pendente.
    public async Task<Pagamento?> ConfirmarReservaAsync(Guid id)
    {
        var pagamento = await BuscarPorIdAsync(id);

        if (pagamento is null)
            return null;

        if (pagamento.Status != StatusPagamento.Aprovado)
        {
            throw new ErroPagamentoException(
                StatusCodes.Status409Conflict,
                "Pagamento nÃ£o aprovado.",
                "SÃ³ um pagamento aprovado pode confirmar a reserva.");
        }

        if (!pagamento.ReservaConfirmada)
        {
            await ConfirmarNaReservaAsync(pagamento);

            if (!pagamento.ReservaConfirmada)
                throw new ReservasIndisponivelException();
        }

        return pagamento;
    }

    public async Task<Pagamento?> BuscarPorIdAsync(Guid id)
    {
        return await banco.Pagamentos.FindAsync(id);
    }

    public async Task<List<Pagamento>> ListarAsync()
    {
        return await banco.Pagamentos
            .OrderByDescending(pagamento => pagamento.DataPagamento)
            .ToListAsync();
    }

    private async Task ValidarReservaAsync(Guid reservaId, decimal valor)
    {
        var reserva = await reservas.ConsultarAsync(reservaId);

        if (reserva is null)
        {
            logger.LogWarning("Pagamento recusado: a reserva {ReservaId} nÃ£o existe", reservaId);
            throw new ErroPagamentoException(
                StatusCodes.Status404NotFound,
                "Reserva nÃ£o encontrada.",
                $"A reserva {reservaId} nÃ£o existe.");
        }

        if (reserva.Status != ClienteReservas.StatusAguardandoPagamento)
        {
            logger.LogWarning("Pagamento recusado: a reserva {ReservaId} estÃ¡ com status {Status}", reservaId, reserva.Status);
            throw new ErroPagamentoException(
                StatusCodes.Status409Conflict,
                "Reserva nÃ£o estÃ¡ aguardando pagamento.",
                $"A reserva {reservaId} estÃ¡ com status {reserva.Status}.");
        }

        if (reserva.ValorTotal != valor)
        {
            logger.LogWarning("Pagamento recusado: valor {Valor} diferente do valor {ValorReserva} da reserva {ReservaId}", valor, reserva.ValorTotal, reservaId);
            throw new ErroPagamentoException(
                StatusCodes.Status400BadRequest,
                "Valor diferente do da reserva.",
                $"O valor da reserva Ã© {reserva.ValorTotal:F2}, mas foi informado {valor:F2}.");
        }
    }

    private async Task ConfirmarNaReservaAsync(Pagamento pagamento)
    {
        if (await reservas.ConfirmarAsync(pagamento.ReservaId, pagamento.Id))
        {
            pagamento.ReservaConfirmada = true;
            await banco.SaveChangesAsync();
            logger.LogInformation("Reserva {ReservaId} confirmada pelo pagamento {PagamentoId}", pagamento.ReservaId, pagamento.Id);
        }
        else
        {
            logger.LogWarning(
                "Reserva {ReservaId} nÃ£o foi confirmada; o pagamento {PagamentoId} ficou pendente de confirmaÃ§Ã£o",
                pagamento.ReservaId, pagamento.Id);
        }
    }
}

