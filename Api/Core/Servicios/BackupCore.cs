using System.IO.Compression;
using System.Text.Json;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.Data.SqlClient;

namespace Api.Core.Servicios;

public class BackupCore : IBackupCore
{
    private const int MaxArchivosLocales = 2;

    private readonly IConfiguration _configuration;
    private readonly AppPaths _appPaths;

    public BackupCore(IConfiguration configuration, AppPaths appPaths)
    {
        _configuration = configuration;
        _appPaths = appPaths;
    }

    public async Task<string> GuardarBackupBaseDeDatosEnDisco()
    {
        var backupDir = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        Directory.CreateDirectory(backupDir);

        var jsonFileName = $"BaseDeDatos-{FechaUtils.AhoraEnArgentinaFormatoBackupDisco}.json";
        var jsonPath = Path.Combine(backupDir, jsonFileName);

        var zipFileName = $"backup-bd-{FechaUtils.AhoraEnArgentinaFormatoBackupDisco}.zip";
        var zipPath = Path.Combine(backupDir, zipFileName);

        try
        {
            await ExportarBaseDeDatosComoJson(jsonPath);

            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(jsonPath, jsonFileName);
            });
        }
        finally
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
        }

        return zipPath;
    }

    public void ValidarCantidadArchivosEnCarpetaBackup()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        if (!Directory.Exists(carpetaBackup))
            return;

        var cantidadArchivos = Directory.GetFiles(carpetaBackup).Length;
        if (cantidadArchivos > MaxArchivosLocales)
            throw new ExcepcionControlada(
                $"La carpeta App_Data/backup tiene {cantidadArchivos} archivos. No puede haber más de {MaxArchivosLocales}. Por favor, eliminá los backups antiguos antes de generar uno nuevo.");
    }

    public string ObtenerRutaBackupBdEnDisco()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        var archivo = Directory.GetFiles(carpetaBackup, "backup-bd-*.zip")
            .OrderByDescending(f => f)
            .FirstOrDefault()
            ?? throw new ExcepcionControlada("No se encontró ningún backup de BD en App_Data/backup.");
        return archivo;
    }

    public void LimpiarBackupsLocales()
    {
        var carpetaBackup = Path.Combine(_appPaths.BackupAbsolute(), "backup");
        if (!Directory.Exists(carpetaBackup)) return;
        foreach (var archivo in Directory.GetFiles(carpetaBackup, "*.zip"))
            File.Delete(archivo);
    }

    private async Task ExportarBaseDeDatosComoJson(string rutaArchivo)
    {
        var connectionString = _configuration.GetConnectionString("Default")!;

        await Task.Run(() =>
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var tablas = ObtenerNombresDeTablas(conn);
            var datos = new Dictionary<string, List<Dictionary<string, object?>>>();

            foreach (var tabla in tablas)
                datos[tabla] = ObtenerDatosDeTabla(conn, tabla);

            var opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(rutaArchivo, JsonSerializer.Serialize(datos, opciones));
        });
    }

    private static List<string> ObtenerNombresDeTablas(SqlConnection conn)
    {
        var tablas = new List<string>();
        using var cmd = new SqlCommand(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME",
            conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            tablas.Add(reader.GetString(0));
        return tablas;
    }

    private static List<Dictionary<string, object?>> ObtenerDatosDeTabla(SqlConnection conn, string nombreTabla)
    {
        var filas = new List<Dictionary<string, object?>>();
        using var cmd = new SqlCommand($"SELECT * FROM [{nombreTabla.Replace("]", "")}] WITH (NOLOCK)", conn);
        cmd.CommandTimeout = 300;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var fila = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                fila[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            filas.Add(fila);
        }
        return filas;
    }
}
