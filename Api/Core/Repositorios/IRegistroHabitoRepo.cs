using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IRegistroHabitoRepo : IRepositorioABM<RegistroHabito>
{
    Task<RegistroHabito?> ObtenerPorHabitoYFecha(int habitoId, DateTime fecha);
    Task<IEnumerable<RegistroHabito>> ListarPorHabitosYFecha(IEnumerable<int> habitoIds, DateTime fecha);
    Task<IEnumerable<RegistroHabito>> ListarPorHabitosYRango(IEnumerable<int> habitoIds, DateTime fechaInicio, DateTime fechaFin);
    Task<bool> ExisteAlgunoParaHabito(int habitoId);
}
