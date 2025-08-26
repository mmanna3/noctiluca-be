using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IPapeleraCore
{
    Task<IEnumerable<EscritoDTO>> ObtenerEscritosEnPapelera();
    Task<bool> PonerEnPapelera(int escritoId);
}
