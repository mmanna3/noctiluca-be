using Api.Core.DTOs.Sync;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncCore _core;

    public SyncController(ISyncCore core)
    {
        _core = core;
    }

    /// <summary>Change-feed: trae todo lo que cambió desde el cursor indicado.</summary>
    [HttpGet("cambios")]
    [Authorize(Roles = "Administrador,Consulta")]
    public async Task<ActionResult<SyncPullDTO>> Cambios([FromQuery] long desde = 0)
    {
        var resultado = await _core.Pull(desde);
        return Ok(resultado);
    }

    /// <summary>Aplica el lote de operaciones acumuladas en el outbox del cliente.</summary>
    [HttpPost("aplicar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<SyncPushResultDTO>> Aplicar([FromBody] SyncPushDTO dto)
    {
        var resultado = await _core.Push(dto);
        return Ok(resultado);
    }
}
