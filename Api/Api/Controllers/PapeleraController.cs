using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PapeleraController : ControllerBase
{
    private readonly IPapeleraCore _papeleraCore;

    public PapeleraController(IPapeleraCore papeleraCore)
    {
        _papeleraCore = papeleraCore;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<EscritoDTO>>> GetEscritosEnPapelera()
    {
        var escritosEnPapelera = await _papeleraCore.ObtenerEscritosEnPapelera();
        return Ok(escritosEnPapelera);
    }
    
    [Authorize(Roles = "Administrador")]
    [HttpPost("poner-en-papelera")]
    public async Task<ActionResult> PonerEnPapelera(int id)
    {
        var resultado = await _papeleraCore.PonerEnPapelera(id);
        if (!resultado)
            return NotFound("Escrito no encontrado");

        return NoContent();
    }
}
