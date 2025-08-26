using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class EscritoController : ABMController<EscritoDTO, IEscritoCore>
    {
        public EscritoController(IEscritoCore core) : base(core)
        {
        }
    }
}
