using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class CarpetaRepo : RepositorioABM<Carpeta>, ICarpetaRepo
{
    public CarpetaRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Carpeta> Set()
    {
        return Context.Set<Carpeta>()
            .Include(x => x.Escritos.Where(e => !e.EstaEnPapelera))
            .Include(x => x.CriterioDeOrden)
            .AsQueryable();
    }
    
    public override async Task<Carpeta?> ObtenerPorId(int id)
    {
        var carpeta = await Context.Set<Carpeta>()
            .Include(x => x.CriterioDeOrden)
            .Include(x => x.Escritos.Where(e => !e.EstaEnPapelera))
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);
            
        if (carpeta != null)
        {
            // Ordenar los escritos según el criterio de orden
            carpeta.Escritos = OrdenarEscritos(carpeta.Escritos, carpeta.CriterioDeOrdenId);
        }
        
        return carpeta;
    }
    
    public async Task<Carpeta?> ObtenerPorIdConTracking(int id)
    {
        var carpeta = await Context.Set<Carpeta>()
            .Include(x => x.CriterioDeOrden)
            .Include(x => x.Escritos.Where(e => !e.EstaEnPapelera))
            .SingleOrDefaultAsync(x => x.Id == id);
            
        if (carpeta != null)
        {
            // Ordenar los escritos según el criterio de orden
            carpeta.Escritos = OrdenarEscritos(carpeta.Escritos, carpeta.CriterioDeOrdenId);
        }
        
        return carpeta;
    }
    
    private ICollection<Escrito> OrdenarEscritos(ICollection<Escrito> escritos, int criterioDeOrdenId)
    {
        return criterioDeOrdenId switch
        {
            1 => escritos.OrderByDescending(e => e.FechaHoraCreacion).ToList(), // CreacionDesc
            2 => escritos.OrderByDescending(e => e.FechaHoraEdicion).ToList(),  // EdicionDesc
            3 => escritos.OrderBy(e => e.Titulo).ToList(),                      // AZ
            4 => escritos.OrderBy(e => e.FechaHoraCreacion).ToList(),           // CreacionAsc
            5 => escritos.OrderBy(e => e.FechaHoraEdicion).ToList(),            // EdicionAsc
            6 => escritos.OrderByDescending(e => e.Titulo).ToList(),            // ZA
            _ => escritos.OrderByDescending(e => e.FechaHoraCreacion).ToList()  // Default: CreacionDesc
        };
    }
}