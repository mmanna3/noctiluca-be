namespace Api.Core.Servicios;

/// <summary>
/// Lógica pura para rotar backups en Drive: agrupa backup-bd-*.zip por día
/// y calcula qué archivos borrar cuando hay más días distintos que el máximo permitido.
/// </summary>
public static class GoogleDriveBackupRotacion
{
    public const int MaxDiasPorDefecto = 3;
    private const string PrefijoBackupBd = "backup-bd-";

    /// <summary>
    /// Devuelve el sufijo yyyy-MM-dd-HH-mm de un nombre backup-bd-*.zip, o null.
    /// </summary>
    public static string? ExtraerTimestamp(string nombre)
    {
        if (nombre.StartsWith(PrefijoBackupBd) && nombre.EndsWith(".zip"))
            return nombre[PrefijoBackupBd.Length..^4];
        return null;
    }

    /// <summary>
    /// Fecha calendario yyyy-MM-dd para agrupar backups del mismo día.
    /// </summary>
    public static string? ExtraerFechaDesdeNombreBackup(string nombre)
    {
        var ts = ExtraerTimestamp(nombre);
        if (string.IsNullOrEmpty(ts) || ts.Length < 10)
            return null;
        return ts[..10];
    }

    public static RotacionBackupsDriveResult Calcular(
        IReadOnlyList<(string Id, string Name)> archivosEnCarpeta,
        int maxDias = MaxDiasPorDefecto)
    {
        var todosLosNombres = archivosEnCarpeta
            .Select(a => a.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var conFecha = archivosEnCarpeta
            .Select(a => (a.Id, a.Name, Fecha: ExtraerFechaDesdeNombreBackup(a.Name)))
            .Where(x => x.Fecha != null)
            .Select(x => (x.Id, x.Name, Fecha: x.Fecha!))
            .ToList();

        var gruposPorFecha = conFecha
            .GroupBy(x => x.Fecha)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var diasDetectados = gruposPorFecha.Count;

        var idsABorrar = new List<string>();
        var nombresBorrados = new List<string>();

        if (gruposPorFecha.Count > maxDias)
        {
            var cantidadGruposABorrar = gruposPorFecha.Count - maxDias;
            foreach (var g in gruposPorFecha.Take(cantidadGruposABorrar))
            {
                foreach (var item in g.OrderBy(x => x.Name, StringComparer.Ordinal))
                {
                    idsABorrar.Add(item.Id);
                    nombresBorrados.Add(item.Name);
                }
            }
        }

        return new RotacionBackupsDriveResult
        {
            BackupsEnDrive = todosLosNombres,
            DiasDetectados = diasDetectados,
            IdsABorrar = idsABorrar,
            ArchivosBorrados = nombresBorrados
        };
    }
}

public sealed class RotacionBackupsDriveResult
{
    public required IReadOnlyList<string> BackupsEnDrive { get; init; }
    public int DiasDetectados { get; init; }
    /// <summary>Ids a pasar a la API de Drive para borrar.</summary>
    public required IReadOnlyList<string> IdsABorrar { get; init; }
    public required IReadOnlyList<string> ArchivosBorrados { get; init; }
}
