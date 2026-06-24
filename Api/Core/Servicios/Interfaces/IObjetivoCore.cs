using Api.Core.DTOs;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IObjetivoCore
{
    Task<ListaObjetivoDTO> ObtenerOCrearListaDia(DateTime fecha);
    Task<ListaObjetivoDTO?> ObtenerLista(TipoListaObjetivoEnum tipo, string clavePeriodo);
    Task<ListaObjetivoDTO> ObtenerListaPorId(int id);
    Task<HistoricoObjetivoPaginadoDTO> ObtenerHistorico(
        TipoListaObjetivoEnum tipo,
        int pagina,
        int tamano);
    Task<ItemObjetivoDTO> CrearItem(CrearItemObjetivoDTO dto);
    Task<ItemObjetivoDTO> EditarItem(int id, EditarItemObjetivoDTO dto);
    Task<ItemObjetivoDTO> ToggleCompletado(int id);
    Task EliminarItem(int id);
}
