using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IItemObjetivoRepo
{
    Task<ItemObjetivo?> ObtenerPorIdConTracking(int id);
    void Crear(ItemObjetivo item);
    void Eliminar(ItemObjetivo item);
}
