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

        // O SQLite não guarda o fuso; a data é sempre gravada em UTC e lida de volta como UTC.
        pagamento.Property(p => p.DataPagamento)
            .HasConversion(data => data, data => DateTime.SpecifyKind(data, DateTimeKind.Utc));

        // Uma reserva só pode ter um pagamento aprovado. Recusados não bloqueiam nova tentativa.
        pagamento.HasIndex(p => p.ReservaId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Aprovado'");
    }
}
