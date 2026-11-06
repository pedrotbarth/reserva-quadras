using Clientes.DTO;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Servicos;

public class ServCliente(DataContext banco)
{
    public async Task<Cliente> CadastrarAsync(CriarClienteDto dados)
    {
        var cliente = new Cliente
        {
            Nome = dados.Nome.Trim(),
            Telefone = dados.Telefone.Trim(),
            Email = dados.Email.Trim(),
            Ativo = true
        };

        banco.Clientes.Add(cliente);
        await banco.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente?> BuscarPorIdAsync(Guid id)
    {
        return await banco.Clientes.FindAsync(id);
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        return await banco.Clientes
            .OrderBy(cliente => cliente.Nome)
            .ToListAsync();
    }

    public async Task<Cliente?> EditarAsync(Guid id, EditarClienteDto dados)
    {
        var cliente = await BuscarPorIdAsync(id);

        if (cliente is null)
            return null;

        cliente.Nome = dados.Nome.Trim();
        cliente.Telefone = dados.Telefone.Trim();
        cliente.Email = dados.Email.Trim();
        cliente.Ativo = dados.Ativo == true;

        await banco.SaveChangesAsync();
        return cliente;
    }
}