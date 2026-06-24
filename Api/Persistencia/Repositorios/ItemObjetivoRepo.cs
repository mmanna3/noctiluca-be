using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ItemObjetivoRepo : IItemObjetivoRepo
{
    private readonly AppDbContext _context;

    public ItemObjetivoRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ItemObjetivo?> ObtenerPorIdConTracking(int id)
    {
        return await _context.Set<ItemObjetivo>()
            .SingleOrDefaultAsync(i => i.Id == id);
    }

    public void Crear(ItemObjetivo item)
    {
        _context.Set<ItemObjetivo>().Add(item);
    }

    public void Eliminar(ItemObjetivo item)
    {
        _context.Set<ItemObjetivo>().Remove(item);
    }
}
