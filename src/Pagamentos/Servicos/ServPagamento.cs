using Pagamentos.DTO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos.Servicos;

public class ServPagamento(DataContext banco, ClienteReservas reservas, ILogger<ServPagamento> logger)
{
    // Código de erro do SQLite para violação de restrição (aqui, o índice único de ReservaId).
    private const int ErroRestricaoSqlite = 19;

    public async Task<Pagamento> RegistrarAsync(CriarPagamentoDto dados)
    {
        var reservaId = dados.ReservaId.GetValueOrDefault();

        var jaPaga = await banco.Pagamentos.AnyAsync(pagamento =>
            pagamento.ReservaId == reservaId &&
            pagamento.Status == StatusPagamento.Aprovado);

        if (jaPaga)
        {
            logger.LogWarning("Pagamento recusado por duplicidade: a reserva {ReservaId} já está paga", reservaId);
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
            // Dois pedidos ao mesmo tempo passaram pela checagem acima; o índice do banco barrou o segundo.
            logger.LogWarning("Pagamento recusado por duplicidade (pedidos simultâneos): a reserva {ReservaId} já está paga", reservaId);
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
                "Pagamento não aprovado.",
                "Só um pagamento aprovado pode confirmar a reserva.");
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
            logger.LogWarning("Pagamento recusado: a reserva {ReservaId} não existe", reservaId);
            throw new ErroPagamentoException(
                StatusCodes.Status404NotFound,
                "Reserva não encontrada.",
                $"A reserva {reservaId} não existe.");
        }

        if (reserva.Status != ClienteReservas.StatusAguardandoPagamento)
        {
            logger.LogWarning("Pagamento recusado: a reserva {ReservaId} está com status {Status}", reservaId, reserva.Status);
            throw new ErroPagamentoException(
                StatusCodes.Status409Conflict,
                "Reserva não está aguardando pagamento.",
                $"A reserva {reservaId} está com status {reserva.Status}.");
        }

        if (reserva.Valor != valor)
        {
            logger.LogWarning("Pagamento recusado: valor {Valor} diferente do valor {ValorReserva} da reserva {ReservaId}", valor, reserva.Valor, reservaId);
            throw new ErroPagamentoException(
                StatusCodes.Status400BadRequest,
                "Valor diferente do da reserva.",
                $"O valor da reserva é {reserva.Valor:F2}, mas foi informado {valor:F2}.");
        }
    }

    private async Task ConfirmarNaReservaAsync(Pagamento pagamento)
    {
        if (await reservas.ConfirmarAsync(pagamento.ReservaId))
        {
            pagamento.ReservaConfirmada = true;
            await banco.SaveChangesAsync();
            logger.LogInformation("Reserva {ReservaId} confirmada pelo pagamento {PagamentoId}", pagamento.ReservaId, pagamento.Id);
        }
        else
        {
            logger.LogWarning(
                "Reserva {ReservaId} não foi confirmada; o pagamento {PagamentoId} ficou pendente de confirmação",
                pagamento.ReservaId, pagamento.Id);
        }
    }
}
