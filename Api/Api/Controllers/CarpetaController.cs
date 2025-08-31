using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class CarpetaController : ABMController<CarpetaDTO, ICarpetaCore>
    {
        public CarpetaController(ICarpetaCore core) : base(core)
        {
        }
        
        [HttpPut("{id}/criterio-orden")]
        public async Task<IActionResult> ActualizarCriterioDeOrden(int id, [FromBody] int criterioDeOrdenId)
        {
            await Core.ActualizarCriterioDeOrden(id, criterioDeOrdenId);
            return Ok();
        }
    }
}
