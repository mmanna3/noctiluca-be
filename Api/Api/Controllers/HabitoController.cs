using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

public class HabitoController : ABMController<HabitoDTO, IHabitoCore>
{
    public HabitoController(IHabitoCore core) : base(core)
    {
    }

    [HttpGet("tracker")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<TrackerDiaDTO>> ObtenerTracker([FromQuery] DateTime fecha)
    {
        var resultado = await Core.ObtenerTracker(fecha);
        return Ok(resultado);
    }

    [HttpPut("registro")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<IActionResult> UpsertRegistro([FromBody] UpsertRegistroHabitoDTO dto)
    {
        await Core.UpsertRegistro(dto);
        return Ok();
    }

    [HttpGet("resumen-semanal")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<ResumenSemanalDTO>> ObtenerResumenSemanal([FromQuery] DateTime fecha)
    {
        var resultado = await Core.ObtenerResumenSemanal(fecha);
        return Ok(resultado);
    }
}
