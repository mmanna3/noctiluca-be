using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCarpetaPadre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarpetaPadreId",
                table: "Carpetas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carpetas_CarpetaPadreId",
                table: "Carpetas",
                column: "CarpetaPadreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carpetas_Carpetas_CarpetaPadreId",
                table: "Carpetas",
                column: "CarpetaPadreId",
                principalTable: "Carpetas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carpetas_Carpetas_CarpetaPadreId",
                table: "Carpetas");

            migrationBuilder.DropIndex(
                name: "IX_Carpetas_CarpetaPadreId",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "CarpetaPadreId",
                table: "Carpetas");
        }
    }
}
