using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Reservas.Migrations;

[DbContext(typeof(DataContext))]
[Migration("20261118090000_CriarReservas")]
public class CriarReservas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Reservas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                QuadraId = table.Column<Guid>(type: "TEXT", nullable: false),
                Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                HorarioInicio = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                HorarioFim = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                PagamentoId = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Reservas", item => item.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Reservas_QuadraId_Data",
            table: "Reservas",
            columns: new[] { "QuadraId", "Data" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Reservas");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        ReservaMigrationModel.Configurar(modelBuilder);
    }
}
