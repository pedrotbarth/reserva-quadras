using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Migrations
{
    /// <inheritdoc />
    public partial class CriarReservas : Migration
    {
        /// <inheritdoc />
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
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_QuadraId_Data",
                table: "Reservas",
                columns: new[] { "QuadraId", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");
        }
    }
}
