using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quadras.Migrations
{
    /// <inheritdoc />
    public partial class CriarQuadras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quadras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Modalidade = table.Column<string>(type: "TEXT", nullable: false),
                    ValorPorHora = table.Column<decimal>(type: "TEXT", nullable: false),
                    HorarioAbertura = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    HorarioFechamento = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quadras", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quadras");
        }
    }
}
