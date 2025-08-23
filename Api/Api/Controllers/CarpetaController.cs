using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class CarpetaController : ABMController<CarpetaDTO, ICarpetaCore>
    {
        public CarpetaController(ICarpetaCore core) : base(core)
        {
        }
    }
}
