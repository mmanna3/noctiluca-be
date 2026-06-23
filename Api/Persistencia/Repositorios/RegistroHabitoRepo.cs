using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class RegistroHabitoRepo : RepositorioABM<RegistroHabito>, IRegistroHabitoRepo
{
    public RegistroHabitoRepo(AppDbContext context) : base(context)
    {
    }

    public async Task<RegistroHabito?> ObtenerPorHabitoYFecha(int habitoId, DateTime fecha)
    {
        var fechaNormalizada = fecha.Date;
        return await Context.Set<RegistroHabito>()
            .SingleOrDefaultAsync(r => r.HabitoId == habitoId && r.Fecha == fechaNormalizada);
    }

    public async Task<IEnumerable<RegistroHabito>> ListarPorHabitosYFecha(IEnumerable<int> habitoIds, DateTime fecha)
    {
        var ids = habitoIds.ToList();
        var fechaNormalizada = fecha.Date;
        return await Context.Set<RegistroHabito>()
            .AsNoTracking()
            .Where(r => ids.Contains(r.HabitoId) && r.Fecha == fechaNormalizada)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegistroHabito>> ListarPorHabitosYRango(
        IEnumerable<int> habitoIds,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var ids = habitoIds.ToList();
        var inicio = fechaInicio.Date;
        var fin = fechaFin.Date;
        return await Context.Set<RegistroHabito>()
            .AsNoTracking()
            .Where(r => ids.Contains(r.HabitoId) && r.Fecha >= inicio && r.Fecha <= fin)
            .ToListAsync();
    }

    public async Task<bool> ExisteAlgunoParaHabito(int habitoId)
    {
        return await Context.Set<RegistroHabito>()
            .AnyAsync(r => r.HabitoId == habitoId);
    }
}
