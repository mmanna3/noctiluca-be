using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IListaObjetivoRepo
{
    Task<ListaObjetivo?> ObtenerPorTipoYClave(TipoListaObjetivoEnum tipo, string clavePeriodo);
    Task<ListaObjetivo?> ObtenerPorIdConItems(int id);
    Task<ListaObjetivo?> ObtenerPorIdConItemsConTracking(int id);
    void Crear(ListaObjetivo lista);
    Task<(IEnumerable<ListaObjetivo> items, int total)> ListarHistorico(
        TipoListaObjetivoEnum tipo,
        int pagina,
        int tamano);
}
