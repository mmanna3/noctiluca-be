using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class HabitoRepo : RepositorioABM<Habito>, IHabitoRepo
{
    public HabitoRepo(AppDbContext context) : base(context)
    {
    }

    protected override IQueryable<Habito> Set()
    {
        return Context.Set<Habito>()
            .Include(x => x.Registros)
            .AsQueryable();
    }

    public override async Task<IEnumerable<Habito>> Listar()
    {
        return await Context.Set<Habito>()
            .AsNoTracking()
            .OrderBy(h => h.Posicion)
            .ThenBy(h => h.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habito>> ListarActivos()
    {
        return await Context.Set<Habito>()
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Posicion)
            .ThenBy(h => h.Id)
            .ToListAsync();
    }

    public async Task<int> ContarActivos()
    {
        return await Context.Set<Habito>()
            .CountAsync(h => h.Activo);
    }

    public async Task<int> ContarActivosExcluyendo(int idExcluido)
    {
        return await Context.Set<Habito>()
            .CountAsync(h => h.Activo && h.Id != idExcluido);
    }

    public async Task<int> ContarRegistros(int habitoId)
    {
        return await Context.Set<RegistroHabito>()
            .CountAsync(r => r.HabitoId == habitoId);
    }

    public async Task<Habito?> ObtenerPorIdConRegistros(int id)
    {
        return await Set()
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == id);
    }
}
