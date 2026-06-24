using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ObjetivoController : ControllerBase
{
    private readonly IObjetivoCore _core;

    public ObjetivoController(IObjetivoCore core)
    {
        _core = core;
    }

    [HttpGet("dia")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ListaObjetivoDTO>> ObtenerListaDia([FromQuery] DateTime fecha)
    {
        var resultado = await _core.ObtenerOCrearListaDia(fecha);
        return Ok(resultado);
    }

    [HttpGet("lista")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ListaObjetivoDTO>> ObtenerLista(
        [FromQuery] TipoListaObjetivoEnum tipo,
        [FromQuery] string clavePeriodo)
    {
        var resultado = await _core.ObtenerLista(tipo, clavePeriodo);
        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpGet("lista/{id}")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ListaObjetivoDTO>> ObtenerListaPorId(int id)
    {
        var resultado = await _core.ObtenerListaPorId(id);
        return Ok(resultado);
    }

    [HttpGet("historico")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<HistoricoObjetivoPaginadoDTO>> ObtenerHistorico(
        [FromQuery] TipoListaObjetivoEnum tipo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        var resultado = await _core.ObtenerHistorico(tipo, pagina, tamano);
        return Ok(resultado);
    }

    [HttpPost("item")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ItemObjetivoDTO>> CrearItem([FromBody] CrearItemObjetivoDTO dto)
    {
        var resultado = await _core.CrearItem(dto);
        return Ok(resultado);
    }

    [HttpPut("item/{id}")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ItemObjetivoDTO>> EditarItem(int id, [FromBody] EditarItemObjetivoDTO dto)
    {
        var resultado = await _core.EditarItem(id, dto);
        return Ok(resultado);
    }

    [HttpPut("item/{id}/completado")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ItemObjetivoDTO>> ToggleCompletado(int id)
    {
        var resultado = await _core.ToggleCompletado(id);
        return Ok(resultado);
    }

    [HttpDelete("item/{id}")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<IActionResult> EliminarItem(int id)
    {
        await _core.EliminarItem(id);
        return Ok();
    }
}
