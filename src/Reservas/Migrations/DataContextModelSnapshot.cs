using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Reservas.Migrations;

[DbContext(typeof(DataContext))]
public class DataContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        ReservaMigrationModel.Configurar(modelBuilder);
    }
}

internal static class ReservaMigrationModel
{
    internal static void Configurar(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.13");

        modelBuilder.Entity("Reservas.Servicos.Reserva", reserva =>
        {
            reserva.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
            reserva.Property<Guid>("ClienteId").HasColumnType("TEXT");
            reserva.Property<DateOnly>("Data").HasColumnType("TEXT");
            reserva.Property<TimeOnly>("HorarioFim").HasColumnType("TEXT");
            reserva.Property<TimeOnly>("HorarioInicio").HasColumnType("TEXT");
            reserva.Property<Guid?>("PagamentoId").HasColumnType("TEXT");
            reserva.Property<Guid>("QuadraId").HasColumnType("TEXT");
            reserva.Property<int>("Status").HasColumnType("INTEGER");
            reserva.Property<decimal>("ValorTotal").HasColumnType("TEXT");
            reserva.HasKey("Id");
            reserva.HasIndex("QuadraId", "Data");
            reserva.ToTable("Reservas");
        });
    }
}
