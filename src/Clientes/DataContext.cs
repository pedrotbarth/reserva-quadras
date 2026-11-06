using Clientes.Servicos;
using Microsoft.EntityFrameworkCore;

namespace Clientes;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
}