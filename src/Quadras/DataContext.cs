using Microsoft.EntityFrameworkCore;
using Quadras.Servicos;

namespace Quadras;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Quadra> Quadras => Set<Quadra>();
}