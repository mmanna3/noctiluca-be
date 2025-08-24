using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class NotaRepo : RepositorioABM<Nota>, INotaRepo
{
    public NotaRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override void AntesDeModificar(Nota entidadAnterior, Nota entidadNueva)
    {
        entidadNueva.CarpetaId = entidadAnterior.CarpetaId;
        entidadNueva.FechaHora = DateTime.Now;
    }
    
    protected override void AntesDeCrear(Nota entidad)
    {
        entidad.FechaHora = DateTime.Now;
    }
}