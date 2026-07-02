using System.Text.Json;

namespace Api.Core.DTOs.Sync;

/// <summary>
/// Lote de operaciones que el cliente acumuló en su outbox mientras estaba offline
/// (o simplemente para aplicar de forma unificada).
/// </summary>
public class SyncPushDTO
{
    public Guid DeviceId { get; set; }

    public List<SyncOpDTO> Operaciones { get; set; } = new();
}

public class SyncOpDTO
{
    /// <summary>Identificador único de la operación, para idempotencia.</summary>
    public Guid ClientOpId { get; set; }

    /// <summary>"carpeta" | "escrito".</summary>
    public string EntityType { get; set; } = "";

    /// <summary>"upsert" | "delete".</summary>
    public string Operation { get; set; } = "";

    /// <summary>Identidad estable (GUID) de la entidad afectada.</summary>
    public Guid ClientEntityId { get; set; }

    /// <summary>Versión que el cliente tenía al editar (detección de conflicto).</summary>
    public long? BaseVersion { get; set; }

    /// <summary>Momento del cambio en el cliente (desempate last-write-wins).</summary>
    public DateTime ClientTimestamp { get; set; }

    /// <summary>Contenido de la entidad (se deserializa según EntityType).</summary>
    public JsonElement? Payload { get; set; }
}

public class CarpetaSyncPayload
{
    public string Titulo { get; set; } = "";
    public bool RequiereAutenticacion { get; set; }
    public int Posicion { get; set; }
    public int CriterioDeOrden { get; set; } = 1;
    public Guid? CarpetaPadreClientId { get; set; }
    public int? PropositoCarpeta { get; set; }
}

public class EscritoSyncPayload
{
    public string? Titulo { get; set; }
    public string? Cuerpo { get; set; }
    public Guid CarpetaClientId { get; set; }
    public bool EstaEnPapelera { get; set; }
    public DateTime? FechaHoraCreacion { get; set; }
}

public class HabitoSyncPayload
{
    public string Nombre { get; set; } = "";
    public int Tipo { get; set; }
    public bool Activo { get; set; } = true;
    public int Posicion { get; set; }
    public int? MetaMinutos { get; set; }
}

public class ItemObjetivoSyncPayload
{
    public string Texto { get; set; } = "";
    public bool Completado { get; set; }
    public int Posicion { get; set; }
    public DateTime? FechaCompletado { get; set; }

    /// <summary>Identifica (o crea) la lista dueña por su clave natural (tipo + período).</summary>
    public int ListaTipo { get; set; }
    public string ListaClavePeriodo { get; set; } = "";
}

public class RegistroHabitoSyncPayload
{
    public Guid HabitoClientId { get; set; }
    public DateTime Fecha { get; set; }
    public bool? ValorBooleano { get; set; }
    public int? ValorNumerico { get; set; }
}
