using Quadras.DTO;
using Microsoft.EntityFrameworkCore;

namespace Quadras.Servicos;

public class ServQuadra(DataContext banco)
{
    public async Task<Quadra> CadastrarAsync(CriarQuadraDto dados)
    {
        var quadra = new Quadra
        {
            Nome = dados.Nome.Trim(),
            Modalidade = dados.Modalidade.Trim(),
            ValorPorHora = dados.ValorPorHora,
            HorarioAbertura = dados.HorarioAbertura.GetValueOrDefault(),
            HorarioFechamento = dados.HorarioFechamento.GetValueOrDefault(),
            Ativa = true
        };

        banco.Quadras.Add(quadra);
        await banco.SaveChangesAsync();
        return quadra;
    }

    public async Task<Quadra?> BuscarPorIdAsync(Guid id)
    {
        return await banco.Quadras.FindAsync(id);
    }
    public async Task<List<Quadra>> ListarAsync()
    {
        return await banco.Quadras
            .OrderBy(quadra => quadra.Nome)
            .ToListAsync();
    }

    public async Task<Quadra?> EditarAsync(Guid id, EditarQuadraDto dados)
    {
        var quadra = await BuscarPorIdAsync(id);

        if (quadra is null)
            return null;

        quadra.Nome = dados.Nome.Trim();
        quadra.Modalidade = dados.Modalidade.Trim();
        quadra.ValorPorHora = dados.ValorPorHora;
        quadra.HorarioAbertura = dados.HorarioAbertura.GetValueOrDefault();
        quadra.HorarioFechamento = dados.HorarioFechamento.GetValueOrDefault();
        quadra.Ativa = dados.Ativa == true;

        await banco.SaveChangesAsync();
        return quadra;
    }
}