namespace Api.Core.DTOs.Sync;

public class SyncPushResultDTO
{
    public long Cursor { get; set; }

    public List<SyncOpResultDTO> Resultados { get; set; } = new();
}

public class SyncOpResultDTO
{
    public Guid ClientOpId { get; set; }

    public Guid ClientEntityId { get; set; }

    /// <summary>"aplicado" | "rechazado" | "error" | "duplicado".</summary>
    public string Estado { get; set; } = "";

    /// <summary>Id asignado por el servidor (para mapear el GUID del cliente).</summary>
    public int? ServerId { get; set; }

    public long? Version { get; set; }

    public string? Mensaje { get; set; }
}

public static class EstadoSyncOp
{
    public const string Aplicado = "aplicado";
    public const string Rechazado = "rechazado";
    public const string Error = "error";
    public const string Duplicado = "duplicado";
}
