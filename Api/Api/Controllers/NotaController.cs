using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class NotaController : ABMController<NotaDTO, INotaCore>
    {
        public NotaController(INotaCore core) : base(core)
        {
        }
    }
}
