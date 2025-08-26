using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenombraNotaAEscrito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notas_Carpetas_CarpetaId",
                table: "Notas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notas",
                table: "Notas");

            migrationBuilder.RenameTable(
                name: "Notas",
                newName: "Escritos");

            migrationBuilder.RenameIndex(
                name: "IX_Notas_CarpetaId",
                table: "Escritos",
                newName: "IX_Escritos_CarpetaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Escritos",
                table: "Escritos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Escritos_Carpetas_CarpetaId",
                table: "Escritos",
                column: "CarpetaId",
                principalTable: "Carpetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escritos_Carpetas_CarpetaId",
                table: "Escritos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Escritos",
                table: "Escritos");

            migrationBuilder.RenameTable(
                name: "Escritos",
                newName: "Notas");

            migrationBuilder.RenameIndex(
                name: "IX_Escritos_CarpetaId",
                table: "Notas",
                newName: "IX_Notas_CarpetaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notas",
                table: "Notas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notas_Carpetas_CarpetaId",
                table: "Notas",
                column: "CarpetaId",
                principalTable: "Carpetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
