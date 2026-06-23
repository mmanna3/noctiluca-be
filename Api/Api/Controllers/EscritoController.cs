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

        [HttpGet("buscar")]
        [Authorize(Roles = "Administrador,Consulta")]
        public async Task<ActionResult<IEnumerable<EscritoDTO>>> Buscar([FromQuery] string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 3)
                return BadRequest("El texto de búsqueda debe tener al menos 3 caracteres");

            var resultados = await Core.Buscar(texto.Trim());
            return Ok(resultados);
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
