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
            .Include(x => x.Escritos)
            .AsQueryable();
    }
}