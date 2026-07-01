using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSincronizacionOffline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "Escritos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Escritos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Escritos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "Carpetas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Carpetas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Carpetas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ContadoresSync",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UltimoValor = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContadoresSync", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncOpLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientOpId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcesadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultadoJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncOpLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tombstones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEntidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tombstones", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ContadoresSync",
                columns: new[] { "Id", "UltimoValor" },
                values: new object[] { 1, 0L });

            // Backfill de las filas existentes: asignar un ClientId único (evita
            // violar el índice único) y una Version monotónica > 0 (evita que el
            // pull inicial "desde 0" las omita). Solo corre en la base real; los
            // tests usan InMemory + EnsureCreated y no ejecutan migraciones.
            migrationBuilder.Sql(@"
                UPDATE Carpetas SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                UPDATE Escritos SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                WITH ordenCarpetas AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM Carpetas
                )
                UPDATE ordenCarpetas SET Version = rn;

                DECLARE @offset bigint = (SELECT ISNULL(MAX(Version), 0) FROM Carpetas);

                WITH ordenEscritos AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM Escritos
                )
                UPDATE ordenEscritos SET Version = @offset + rn;

                UPDATE ContadoresSync
                SET UltimoValor = (
                    SELECT ISNULL(MAX(v), 0) FROM (
                        SELECT Version v FROM Carpetas
                        UNION ALL
                        SELECT Version FROM Escritos
                    ) x
                )
                WHERE Id = 1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Escritos_ClientId",
                table: "Escritos",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carpetas_ClientId",
                table: "Carpetas",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncOpLogs_ClientOpId",
                table: "SyncOpLogs",
                column: "ClientOpId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tombstones_Version",
                table: "Tombstones",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContadoresSync");

            migrationBuilder.DropTable(
                name: "SyncOpLogs");

            migrationBuilder.DropTable(
                name: "Tombstones");

            migrationBuilder.DropIndex(
                name: "IX_Escritos_ClientId",
                table: "Escritos");

            migrationBuilder.DropIndex(
                name: "IX_Carpetas_ClientId",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "Escritos");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Escritos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Escritos");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Carpetas");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Carpetas");
        }
    }
}
