namespace Api.Core.DTOs.Sync;

/// <summary>
/// Respuesta del change-feed. Trae las entidades creadas/modificadas y las bajas
/// (tombstones) cuya Version es mayor al cursor pedido, más el nuevo
/// <see cref="Cursor"/> para el próximo pull.
/// </summary>
public class SyncPullDTO
{
    public long Cursor { get; set; }

    public bool HayMas { get; set; }

    public List<CarpetaDTO> Carpetas { get; set; } = new();

    public List<EscritoDTO> Escritos { get; set; } = new();

    public List<TombstoneDTO> Eliminados { get; set; } = new();
}

public class TombstoneDTO
{
    public string TipoEntidad { get; set; } = "";

    public Guid ClientId { get; set; }

    public long Version { get; set; }

    public DateTime EliminadoEn { get; set; }
}
