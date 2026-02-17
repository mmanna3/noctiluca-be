using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class EscritoController : ABMController<EscritoDTO, IEscritoCore>
    {
        public EscritoController(IEscritoCore core) : base(core)
        {
        }

        [HttpPut("mover")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> MoverEscritos(MoverEscritosDTO dto)
        {
            await Core.MoverACarpeta(dto);
            return NoContent();
        }
    }
}
