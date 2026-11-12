using Pagamentos.DTO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos.Servicos;

public class ServPagamento(DataContext banco, ILogger<ServPagamento> logger)
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
}
