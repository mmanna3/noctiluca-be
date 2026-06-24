using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ListaObjetivoRepo : IListaObjetivoRepo
{
    private readonly AppDbContext _context;

    public ListaObjetivoRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ListaObjetivo?> ObtenerPorTipoYClave(TipoListaObjetivoEnum tipo, string clavePeriodo)
    {
        return await _context.Set<ListaObjetivo>()
            .Include(l => l.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Tipo == tipo && l.ClavePeriodo == clavePeriodo);
    }

    public async Task<ListaObjetivo?> ObtenerPorIdConItems(int id)
    {
        return await _context.Set<ListaObjetivo>()
            .Include(l => l.Items)
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == id);
    }

    public async Task<ListaObjetivo?> ObtenerPorIdConItemsConTracking(int id)
    {
        if (id <= 0)
            return null;

        return await _context.Set<ListaObjetivo>()
            .Include(l => l.Items)
            .SingleOrDefaultAsync(l => l.Id == id);
    }

    public void Crear(ListaObjetivo lista)
    {
        _context.Set<ListaObjetivo>().Add(lista);
    }

    public async Task<(IEnumerable<ListaObjetivo> items, int total)> ListarHistorico(
        TipoListaObjetivoEnum tipo,
        int pagina,
        int tamano)
    {
        var query = _context.Set<ListaObjetivo>()
            .Include(l => l.Items)
            .AsNoTracking()
            .Where(l => l.Tipo == tipo && l.Items.Any());

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.FechaInicio)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}
