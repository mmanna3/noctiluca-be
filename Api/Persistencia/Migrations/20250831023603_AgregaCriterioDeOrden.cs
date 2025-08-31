using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCriterioDeOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CriterioDeOrdenId",
                table: "Carpetas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "CriterioDeOrden",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Criterio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterioDeOrden", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CriterioDeOrden",
                columns: new[] { "Id", "Criterio" },
                values: new object[,]
                {
                    { 1, "Creación Desc" },
                    { 2, "Edición Desc" },
                    { 3, "A-Z" },
                    { 4, "Creación Asc" },
                    { 5, "Edición Asc" },
                    { 6, "Z-A" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carpetas_CriterioDeOrdenId",
                table: "Carpetas",
                column: "CriterioDeOrdenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carpetas_CriterioDeOrden_CriterioDeOrdenId",
                table: "Carpetas",
                column: "CriterioDeOrdenId",
                principalTable: "CriterioDeOrden",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carpetas_CriterioDeOrden_CriterioDeOrdenId",
                table: "Carpetas");

            migrationBuilder.DropTable(
                name: "CriterioDeOrden");

            migrationBuilder.DropIndex(
                name: "IX_Carpetas_CriterioDeOrdenId",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "CriterioDeOrdenId",
                table: "Carpetas");
        }
    }
}
