using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class EscritoRepo : RepositorioABM<Escrito>, IEscritoRepo
{
    public EscritoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Escrito> Set()
    {
        return Context.Set<Escrito>().Include(x => x.Carpeta);
    }
    
    protected override void AntesDeModificar(Escrito entidadAnterior, Escrito entidadNueva)
    {
        entidadNueva.CarpetaId = entidadAnterior.CarpetaId;
        entidadNueva.FechaHoraCreacion = entidadAnterior.FechaHoraCreacion;
        entidadNueva.FechaHoraEdicion = DateTime.Now;
    }
    
    protected override void AntesDeCrear(Escrito entidad)
    {
        entidad.FechaHoraCreacion = DateTime.Now;
    }
}