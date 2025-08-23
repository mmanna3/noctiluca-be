using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class NotaRepo : RepositorioABM<Nota>, INotaRepo
{
    public NotaRepo(AppDbContext context) : base(context)
    {
    }
}