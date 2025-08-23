using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class CarpetaRepo : RepositorioABM<Carpeta>, ICarpetaRepo
{
    public CarpetaRepo(AppDbContext context) : base(context)
    {
    }
}