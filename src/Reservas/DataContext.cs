using Microsoft.EntityFrameworkCore;
using Reservas.Servicos;

namespace Reservas;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reserva>().Property(reserva => reserva.Status).HasConversion<string>();
        modelBuilder.Entity<Reserva>().HasIndex(reserva => new { reserva.QuadraId, reserva.Data });
    }
}

