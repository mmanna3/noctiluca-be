using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombraFechaHoraAFechaHoraCreacionEnNotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Notas",
                newName: "FechaHoraCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaHoraCreacion",
                table: "Notas",
                newName: "FechaHora");
        }
    }
}
