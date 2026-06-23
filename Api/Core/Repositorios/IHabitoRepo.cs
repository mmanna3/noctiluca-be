using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IHabitoRepo : IRepositorioABM<Habito>
{
    Task<IEnumerable<Habito>> ListarActivos();
    Task<int> ContarActivos();
    Task<int> ContarActivosExcluyendo(int idExcluido);
    Task<int> ContarRegistros(int habitoId);
    Task<Habito?> ObtenerPorIdConRegistros(int id);
}
