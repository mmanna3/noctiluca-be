using Api.Core.DTOs.Sync;

namespace Api.Core.Servicios.Interfaces;

public interface ISyncCore
{
    /// <summary>Devuelve los cambios (altas/modificaciones/bajas) con Version mayor a <paramref name="desde"/>.</summary>
    Task<SyncPullDTO> Pull(long desde);

    /// <summary>Aplica un lote de operaciones del cliente resolviendo conflictos con last-write-wins.</summary>
    Task<SyncPushResultDTO> Push(SyncPushDTO dto);
}
