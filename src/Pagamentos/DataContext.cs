using Pagamentos.Servicos;
using Microsoft.EntityFrameworkCore;

namespace Pagamentos;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var pagamento = modelBuilder.Entity<Pagamento>();

        pagamento.Property(p => p.FormaPagamento).HasConversion<string>();
        pagamento.Property(p => p.Status).HasConversion<string>();

        // Uma reserva só pode ter um pagamento aprovado. Recusados não bloqueiam nova tentativa.
        pagamento.HasIndex(p => p.ReservaId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Aprovado'");
    }
}
