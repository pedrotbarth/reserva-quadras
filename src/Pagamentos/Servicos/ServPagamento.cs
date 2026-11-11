using Pagamentos.DTO;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos.Servicos;

public class ServPagamento(DataContext banco)
{
    public async Task<Pagamento> RegistrarAsync(CriarPagamentoDto dados)
    {
        var pagamento = new Pagamento
        {
            ReservaId = dados.ReservaId.GetValueOrDefault(),
            Valor = dados.Valor,
            FormaPagamento = dados.FormaPagamento.GetValueOrDefault(),
            Status = dados.SimularRecusa ? StatusPagamento.Recusado : StatusPagamento.Aprovado,
            DataPagamento = DateTime.UtcNow,
            ReservaConfirmada = false
        };

        banco.Pagamentos.Add(pagamento);
        await banco.SaveChangesAsync();
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
