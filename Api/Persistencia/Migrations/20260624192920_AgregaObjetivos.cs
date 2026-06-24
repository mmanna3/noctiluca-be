using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaObjetivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsSistema",
                table: "Carpetas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PropositoCarpeta",
                table: "Carpetas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ListasObjetivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ClavePeriodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasObjetivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsObjetivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListaObjetivoId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Completado = table.Column<bool>(type: "bit", nullable: false),
                    Posicion = table.Column<int>(type: "int", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsObjetivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsObjetivo_ListasObjetivo_ListaObjetivoId",
                        column: x => x.ListaObjetivoId,
                        principalTable: "ListasObjetivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsObjetivo_ListaObjetivoId",
                table: "ItemsObjetivo",
                column: "ListaObjetivoId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasObjetivo_Tipo_ClavePeriodo",
                table: "ListasObjetivo",
                columns: new[] { "Tipo", "ClavePeriodo" },
                unique: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Carpetas WHERE PropositoCarpeta = 1)
BEGIN
    SET IDENTITY_INSERT Carpetas ON;
    INSERT INTO Carpetas (Id, Titulo, RequiereAutenticacion, Posicion, CriterioDeOrdenId, CarpetaPadreId, EsSistema, PropositoCarpeta)
    VALUES (9001, N'objetivos', 0, 0, 1, NULL, 1, 1);
    INSERT INTO Carpetas (Id, Titulo, RequiereAutenticacion, Posicion, CriterioDeOrdenId, CarpetaPadreId, EsSistema, PropositoCarpeta)
    VALUES (9002, N'día', 0, 0, 1, 9001, 1, 2);
    INSERT INTO Carpetas (Id, Titulo, RequiereAutenticacion, Posicion, CriterioDeOrdenId, CarpetaPadreId, EsSistema, PropositoCarpeta)
    VALUES (9003, N'semana', 0, 1, 1, 9001, 1, 3);
    INSERT INTO Carpetas (Id, Titulo, RequiereAutenticacion, Posicion, CriterioDeOrdenId, CarpetaPadreId, EsSistema, PropositoCarpeta)
    VALUES (9004, N'mes', 0, 2, 1, 9001, 1, 4);
    SET IDENTITY_INSERT Carpetas OFF;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Carpetas WHERE Id IN (9002, 9003, 9004, 9001);
");

            migrationBuilder.DropTable(
                name: "ItemsObjetivo");

            migrationBuilder.DropTable(
                name: "ListasObjetivo");

            migrationBuilder.DropColumn(
                name: "EsSistema",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "PropositoCarpeta",
                table: "Carpetas");
        }
    }
}
