using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IHabitoCore : ICoreABM<HabitoDTO>
{
    Task<TrackerDiaDTO> ObtenerTracker(DateTime fecha);
    Task UpsertRegistro(UpsertRegistroHabitoDTO dto);
    Task<ResumenSemanalDTO> ObtenerResumenSemanal(DateTime fecha);
}
