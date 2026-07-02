using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSincronizacionObjetivosHabitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "RegistrosHabito",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "RegistrosHabito",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "RegistrosHabito",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "ListasObjetivo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "ListasObjetivo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "ListasObjetivo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "ItemsObjetivo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "ItemsObjetivo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "ItemsObjetivo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                table: "Habitos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Habitos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Habitos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill de filas existentes: ClientId único (evita violar el índice
            // único) y Version monotónica continuando el contador global (evita que
            // el pull inicial "desde 0" las omita). Solo corre en la base real; los
            // tests usan InMemory + EnsureCreated y no ejecutan migraciones.
            migrationBuilder.Sql(@"
                UPDATE Habitos SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                UPDATE RegistrosHabito SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                UPDATE ListasObjetivo SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                UPDATE ItemsObjetivo SET ClientId = NEWID(), ActualizadoEn = SYSUTCDATETIME()
                WHERE ClientId = '00000000-0000-0000-0000-000000000000';

                DECLARE @offset bigint = (SELECT ISNULL(UltimoValor, 0) FROM ContadoresSync WHERE Id = 1);

                WITH ordenHabitos AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM Habitos
                )
                UPDATE ordenHabitos SET Version = @offset + rn;
                SET @offset = @offset + (SELECT COUNT(*) FROM Habitos);

                WITH ordenRegistros AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM RegistrosHabito
                )
                UPDATE ordenRegistros SET Version = @offset + rn;
                SET @offset = @offset + (SELECT COUNT(*) FROM RegistrosHabito);

                WITH ordenListas AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM ListasObjetivo
                )
                UPDATE ordenListas SET Version = @offset + rn;
                SET @offset = @offset + (SELECT COUNT(*) FROM ListasObjetivo);

                WITH ordenItems AS (
                    SELECT Version, ROW_NUMBER() OVER (ORDER BY Id) AS rn FROM ItemsObjetivo
                )
                UPDATE ordenItems SET Version = @offset + rn;
                SET @offset = @offset + (SELECT COUNT(*) FROM ItemsObjetivo);

                UPDATE ContadoresSync SET UltimoValor = @offset WHERE Id = 1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosHabito_ClientId",
                table: "RegistrosHabito",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListasObjetivo_ClientId",
                table: "ListasObjetivo",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsObjetivo_ClientId",
                table: "ItemsObjetivo",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Habitos_ClientId",
                table: "Habitos",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistrosHabito_ClientId",
                table: "RegistrosHabito");

            migrationBuilder.DropIndex(
                name: "IX_ListasObjetivo_ClientId",
                table: "ListasObjetivo");

            migrationBuilder.DropIndex(
                name: "IX_ItemsObjetivo_ClientId",
                table: "ItemsObjetivo");

            migrationBuilder.DropIndex(
                name: "IX_Habitos_ClientId",
                table: "Habitos");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "RegistrosHabito");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "RegistrosHabito");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RegistrosHabito");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "ListasObjetivo");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ListasObjetivo");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ListasObjetivo");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "ItemsObjetivo");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ItemsObjetivo");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ItemsObjetivo");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                table: "Habitos");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Habitos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Habitos");
        }
    }
}
