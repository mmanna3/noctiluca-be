using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IEscritoRepo : IRepositorioABM<Escrito>
{
    Task<Escrito?> ObtenerPorIdConTracking(int id);
}