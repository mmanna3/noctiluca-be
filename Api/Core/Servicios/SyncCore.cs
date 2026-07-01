using System.Text.Json;
using Api.Core.DTOs;
using Api.Core.DTOs.Sync;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Core.Servicios;

public class SyncCore : ISyncCore
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public SyncCore(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<SyncPullDTO> Pull(long desde)
    {
        var carpetas = await _db.Carpetas.AsNoTracking()
            .Where(c => c.Version > desde)
            .OrderBy(c => c.Version)
            .ToListAsync();

        var escritos = await _db.Escritos.AsNoTracking()
            .Include(e => e.Carpeta)
            .Where(e => e.Version > desde)
            .OrderBy(e => e.Version)
            .ToListAsync();

        var habitos = await _db.Habitos.AsNoTracking()
            .Where(h => h.Version > desde)
            .OrderBy(h => h.Version)
            .ToListAsync();

        var registros = await _db.RegistrosHabito.AsNoTracking()
            .Include(r => r.Habito)
            .Where(r => r.Version > desde)
            .OrderBy(r => r.Version)
            .ToListAsync();

        var listas = await _db.ListasObjetivo.AsNoTracking()
            .Where(l => l.Version > desde)
            .OrderBy(l => l.Version)
            .ToListAsync();

        var items = await _db.ItemsObjetivo.AsNoTracking()
            .Include(i => i.ListaObjetivo)
            .Where(i => i.Version > desde)
            .OrderBy(i => i.Version)
            .ToListAsync();

        var tombstones = await _db.Tombstones.AsNoTracking()
            .Where(t => t.Version > desde)
            .OrderBy(t => t.Version)
            .ToListAsync();

        var cursor = desde;
        if (carpetas.Count > 0) cursor = Math.Max(cursor, carpetas.Max(c => c.Version));
        if (escritos.Count > 0) cursor = Math.Max(cursor, escritos.Max(e => e.Version));
        if (habitos.Count > 0) cursor = Math.Max(cursor, habitos.Max(h => h.Version));
        if (registros.Count > 0) cursor = Math.Max(cursor, registros.Max(r => r.Version));
        if (listas.Count > 0) cursor = Math.Max(cursor, listas.Max(l => l.Version));
        if (items.Count > 0) cursor = Math.Max(cursor, items.Max(i => i.Version));
        if (tombstones.Count > 0) cursor = Math.Max(cursor, tombstones.Max(t => t.Version));

        return new SyncPullDTO
        {
            Cursor = cursor,
            HayMas = false,
            Carpetas = _mapper.Map<List<CarpetaDTO>>(carpetas),
            Escritos = _mapper.Map<List<EscritoDTO>>(escritos),
            Habitos = _mapper.Map<List<HabitoDTO>>(habitos),
            RegistrosHabito = registros.Select(r =>
            {
                var dto = _mapper.Map<RegistroHabitoDTO>(r);
                dto.HabitoClientId = r.Habito?.ClientId;
                return dto;
            }).ToList(),
            ListasObjetivo = _mapper.Map<List<ListaObjetivoDTO>>(listas),
            ItemsObjetivo = items.Select(i =>
            {
                var dto = _mapper.Map<ItemObjetivoDTO>(i);
                dto.ListaTipo = i.ListaObjetivo?.Tipo;
                dto.ListaClavePeriodo = i.ListaObjetivo?.ClavePeriodo;
                return dto;
            }).ToList(),
            Eliminados = tombstones.Select(t => new TombstoneDTO
            {
                TipoEntidad = t.TipoEntidad,
                ClientId = t.ClientId,
                Version = t.Version,
                EliminadoEn = t.EliminadoEn,
            }).ToList(),
        };
    }

    public async Task<SyncPushResultDTO> Push(SyncPushDTO dto)
    {
        var resultados = new List<SyncOpResultDTO>();

        foreach (var op in OrdenarPorDependencias(dto.Operaciones))
        {
            var resultado = await ProcesarOperacion(op);
            resultados.Add(resultado);
        }

        var cursor = await _db.ContadoresSync
            .AsNoTracking()
            .Select(c => c.UltimoValor)
            .FirstOrDefaultAsync();

        return new SyncPushResultDTO { Cursor = cursor, Resultados = resultados };
    }

    /// <summary>
    /// Orden causal dentro del lote: primero altas de carpetas (raíces antes que
    /// subcarpetas) y hábitos, después escritos, ítems de objetivo y registros de
    /// hábito (que dependen de que su hábito exista) y por último las bajas.
    /// </summary>
    private static IEnumerable<SyncOpDTO> OrdenarPorDependencias(IEnumerable<SyncOpDTO> ops)
    {
        var lista = ops.ToList();

        int Rango(SyncOpDTO op)
        {
            if (op.Operation == "delete") return 9;
            if (op.EntityType == "carpeta")
            {
                var payload = DeserializarCarpeta(op);
                return payload?.CarpetaPadreClientId == null ? 0 : 1;
            }
            if (op.EntityType == "habito") return 1;
            if (op.EntityType == "registroHabito") return 3;
            return 2; // escrito / itemObjetivo upsert
        }

        return lista.OrderBy(Rango);
    }

    private async Task<SyncOpResultDTO> ProcesarOperacion(SyncOpDTO op)
    {
        var previo = await _db.SyncOpLogs.AsNoTracking()
            .FirstOrDefaultAsync(l => l.ClientOpId == op.ClientOpId);
        if (previo != null)
            return ResultadoDuplicado(op, previo);

        SyncOpResultDTO resultado;
        try
        {
            resultado = (op.EntityType, op.Operation) switch
            {
                ("carpeta", "upsert") => await UpsertCarpeta(op),
                ("escrito", "upsert") => await UpsertEscrito(op),
                ("habito", "upsert") => await UpsertHabito(op),
                ("itemObjetivo", "upsert") => await UpsertItemObjetivo(op),
                ("registroHabito", "upsert") => await UpsertRegistroHabito(op),
                (_, "delete") => await Eliminar(op),
                _ => Error(op, $"Operación no soportada: {op.EntityType}/{op.Operation}"),
            };
        }
        catch (Exception ex)
        {
            resultado = Error(op, ex.Message);
        }

        await RegistrarOperacion(op, resultado);
        return resultado;
    }

    private async Task<SyncOpResultDTO> UpsertCarpeta(SyncOpDTO op)
    {
        var payload = DeserializarCarpeta(op)
            ?? throw new InvalidOperationException("Payload de carpeta inválido");

        int? carpetaPadreId = null;
        if (payload.CarpetaPadreClientId is Guid padreClientId)
        {
            var padre = await _db.Carpetas.FirstOrDefaultAsync(c => c.ClientId == padreClientId);
            if (padre == null)
                return Error(op, "No se encontró la carpeta padre referenciada");
            carpetaPadreId = padre.Id;
        }

        var existente = await _db.Carpetas.FirstOrDefaultAsync(c => c.ClientId == op.ClientEntityId);

        if (existente == null)
        {
            var nueva = new Carpeta
            {
                Id = 0,
                ClientId = op.ClientEntityId,
                Titulo = payload.Titulo,
                RequiereAutenticacion = payload.RequiereAutenticacion,
                Posicion = payload.Posicion,
                CriterioDeOrdenId = payload.CriterioDeOrden,
                CarpetaPadreId = carpetaPadreId,
                PropositoCarpeta = (PropositoCarpetaEnum?)payload.PropositoCarpeta,
            };
            _db.Carpetas.Add(nueva);
            await _db.SaveChangesAsync();
            return Aplicado(op, nueva.Id, nueva.Version);
        }

        if (HayConflictoYPierdeElCliente(op, existente))
            return Rechazado(op, existente.Id, existente.Version);

        existente.Titulo = payload.Titulo;
        existente.RequiereAutenticacion = payload.RequiereAutenticacion;
        existente.Posicion = payload.Posicion;
        existente.CriterioDeOrdenId = payload.CriterioDeOrden;
        existente.CarpetaPadreId = carpetaPadreId;
        existente.PropositoCarpeta = (PropositoCarpetaEnum?)payload.PropositoCarpeta;
        await _db.SaveChangesAsync();
        return Aplicado(op, existente.Id, existente.Version);
    }

    private async Task<SyncOpResultDTO> UpsertEscrito(SyncOpDTO op)
    {
        var payload = op.Payload?.Deserialize<EscritoSyncPayload>(JsonOpts)
            ?? throw new InvalidOperationException("Payload de escrito inválido");

        var carpeta = await _db.Carpetas.FirstOrDefaultAsync(c => c.ClientId == payload.CarpetaClientId);
        if (carpeta == null)
            return Error(op, "No se encontró la carpeta del escrito");

        var existente = await _db.Escritos.FirstOrDefaultAsync(e => e.ClientId == op.ClientEntityId);

        if (existente == null)
        {
            var nuevo = new Escrito
            {
                Id = 0,
                ClientId = op.ClientEntityId,
                Titulo = string.IsNullOrWhiteSpace(payload.Titulo)
                    ? DateTime.Now.ToString("dd-MM-yy HH:mm")
                    : payload.Titulo,
                Cuerpo = payload.Cuerpo,
                CarpetaId = carpeta.Id,
                EstaEnPapelera = payload.EstaEnPapelera,
                FechaHoraCreacion = payload.FechaHoraCreacion ?? DateTime.Now,
                FechaHoraEdicion = op.ClientTimestamp,
            };
            _db.Escritos.Add(nuevo);
            await _db.SaveChangesAsync();
            return Aplicado(op, nuevo.Id, nuevo.Version);
        }

        if (HayConflictoYPierdeElCliente(op, existente))
            return Rechazado(op, existente.Id, existente.Version);

        existente.Titulo = payload.Titulo ?? existente.Titulo;
        existente.Cuerpo = payload.Cuerpo;
        existente.CarpetaId = carpeta.Id;
        existente.EstaEnPapelera = payload.EstaEnPapelera;
        existente.FechaHoraEdicion = op.ClientTimestamp;
        await _db.SaveChangesAsync();
        return Aplicado(op, existente.Id, existente.Version);
    }

    private async Task<SyncOpResultDTO> UpsertHabito(SyncOpDTO op)
    {
        var payload = op.Payload?.Deserialize<HabitoSyncPayload>(JsonOpts)
            ?? throw new InvalidOperationException("Payload de hábito inválido");

        var existente = await _db.Habitos.FirstOrDefaultAsync(h => h.ClientId == op.ClientEntityId);

        if (existente == null)
        {
            var nuevo = new Habito
            {
                Id = 0,
                ClientId = op.ClientEntityId,
                Nombre = payload.Nombre,
                Tipo = (TipoHabitoEnum)payload.Tipo,
                Activo = payload.Activo,
                Posicion = payload.Posicion,
                MetaMinutos = payload.MetaMinutos,
            };
            _db.Habitos.Add(nuevo);
            await _db.SaveChangesAsync();
            return Aplicado(op, nuevo.Id, nuevo.Version);
        }

        if (HayConflictoYPierdeElCliente(op, existente))
            return Rechazado(op, existente.Id, existente.Version);

        existente.Nombre = payload.Nombre;
        existente.Tipo = (TipoHabitoEnum)payload.Tipo;
        existente.Activo = payload.Activo;
        existente.Posicion = payload.Posicion;
        existente.MetaMinutos = payload.MetaMinutos;
        await _db.SaveChangesAsync();
        return Aplicado(op, existente.Id, existente.Version);
    }

    private async Task<SyncOpResultDTO> UpsertItemObjetivo(SyncOpDTO op)
    {
        var payload = op.Payload?.Deserialize<ItemObjetivoSyncPayload>(JsonOpts)
            ?? throw new InvalidOperationException("Payload de ítem de objetivo inválido");

        var lista = await ObtenerOCrearLista((TipoListaObjetivoEnum)payload.ListaTipo, payload.ListaClavePeriodo);

        var existente = await _db.ItemsObjetivo.FirstOrDefaultAsync(i => i.ClientId == op.ClientEntityId);

        if (existente == null)
        {
            var nuevo = new ItemObjetivo
            {
                Id = 0,
                ClientId = op.ClientEntityId,
                ListaObjetivoId = lista.Id,
                Texto = payload.Texto,
                Completado = payload.Completado,
                Posicion = payload.Posicion,
                FechaCompletado = payload.FechaCompletado,
            };
            _db.ItemsObjetivo.Add(nuevo);
            await _db.SaveChangesAsync();
            return Aplicado(op, nuevo.Id, nuevo.Version);
        }

        if (HayConflictoYPierdeElCliente(op, existente))
            return Rechazado(op, existente.Id, existente.Version);

        existente.Texto = payload.Texto;
        existente.Completado = payload.Completado;
        existente.Posicion = payload.Posicion;
        existente.FechaCompletado = payload.FechaCompletado;
        existente.ListaObjetivoId = lista.Id;
        await _db.SaveChangesAsync();
        return Aplicado(op, existente.Id, existente.Version);
    }

    private async Task<SyncOpResultDTO> UpsertRegistroHabito(SyncOpDTO op)
    {
        var payload = op.Payload?.Deserialize<RegistroHabitoSyncPayload>(JsonOpts)
            ?? throw new InvalidOperationException("Payload de registro de hábito inválido");

        var habito = await _db.Habitos.FirstOrDefaultAsync(h => h.ClientId == payload.HabitoClientId);
        if (habito == null)
            return Error(op, "No se encontró el hábito del registro");

        var fecha = payload.Fecha.Date;
        // Clave natural (HabitoId, Fecha): un registro por hábito y día.
        var existente = await _db.RegistrosHabito
            .FirstOrDefaultAsync(r => r.HabitoId == habito.Id && r.Fecha == fecha);

        if (existente == null)
        {
            var nuevo = new RegistroHabito
            {
                Id = 0,
                ClientId = op.ClientEntityId,
                HabitoId = habito.Id,
                Fecha = fecha,
                ValorBooleano = payload.ValorBooleano,
                ValorNumerico = payload.ValorNumerico,
            };
            _db.RegistrosHabito.Add(nuevo);
            await _db.SaveChangesAsync();
            return Aplicado(op, nuevo.Id, nuevo.Version);
        }

        if (HayConflictoYPierdeElCliente(op, existente))
            return Rechazado(op, existente.Id, existente.Version);

        existente.ValorBooleano = payload.ValorBooleano;
        existente.ValorNumerico = payload.ValorNumerico;
        await _db.SaveChangesAsync();
        return Aplicado(op, existente.Id, existente.Version);
    }

    /// <summary>
    /// Resuelve la lista de objetivos por su clave natural (tipo + período); la crea
    /// si todavía no existe. Es idempotente: el índice único (Tipo, ClavePeriodo)
    /// garantiza una sola lista por período aunque varios ítems lleguen juntos.
    /// </summary>
    private async Task<ListaObjetivo> ObtenerOCrearLista(TipoListaObjetivoEnum tipo, string clavePeriodo)
    {
        var existente = await _db.ListasObjetivo
            .FirstOrDefaultAsync(l => l.Tipo == tipo && l.ClavePeriodo == clavePeriodo);
        if (existente != null)
            return existente;

        var (inicio, fin) = ObjetivoPeriodoUtil.ObtenerRangoDesdeClave(tipo, clavePeriodo);
        var nueva = new ListaObjetivo
        {
            Id = 0,
            Tipo = tipo,
            ClavePeriodo = clavePeriodo,
            FechaInicio = inicio,
            FechaFin = fin,
            FechaCreacion = DateTime.Now,
        };
        _db.ListasObjetivo.Add(nueva);
        await _db.SaveChangesAsync();
        return nueva;
    }

    private async Task<SyncOpResultDTO> Eliminar(SyncOpDTO op)
    {
        switch (op.EntityType)
        {
            case "carpeta":
                var carpeta = await _db.Carpetas.FirstOrDefaultAsync(c => c.ClientId == op.ClientEntityId);
                if (carpeta != null)
                {
                    _db.Carpetas.Remove(carpeta);
                    await _db.SaveChangesAsync();
                }
                return Aplicado(op, carpeta?.Id, null);

            case "escrito":
                var escrito = await _db.Escritos.FirstOrDefaultAsync(e => e.ClientId == op.ClientEntityId);
                if (escrito != null)
                {
                    _db.Escritos.Remove(escrito);
                    await _db.SaveChangesAsync();
                }
                return Aplicado(op, escrito?.Id, null);

            case "itemObjetivo":
                var item = await _db.ItemsObjetivo.FirstOrDefaultAsync(i => i.ClientId == op.ClientEntityId);
                if (item != null)
                {
                    _db.ItemsObjetivo.Remove(item);
                    await _db.SaveChangesAsync();
                }
                return Aplicado(op, item?.Id, null);

            case "habito":
                var habito = await _db.Habitos.FirstOrDefaultAsync(h => h.ClientId == op.ClientEntityId);
                if (habito != null)
                {
                    _db.Habitos.Remove(habito);
                    await _db.SaveChangesAsync();
                }
                return Aplicado(op, habito?.Id, null);

            default:
                return Error(op, $"Tipo no soportado para delete: {op.EntityType}");
        }
    }

    /// <summary>
    /// Last-write-wins: si la versión base que trae el cliente coincide con la del
    /// servidor, no hubo cambio concurrente y se aplica. Si difiere, gana el cambio
    /// con timestamp más nuevo; si el del cliente es más viejo, se rechaza y se le
    /// devuelve el estado del servidor para que sobrescriba local.
    /// </summary>
    private static bool HayConflictoYPierdeElCliente(SyncOpDTO op, EntidadSincronizable existente)
    {
        var sinCambioConcurrente = op.BaseVersion.HasValue && op.BaseVersion.Value == existente.Version;
        if (sinCambioConcurrente)
            return false;

        return op.ClientTimestamp < existente.ActualizadoEn;
    }

    private async Task RegistrarOperacion(SyncOpDTO op, SyncOpResultDTO resultado)
    {
        _db.SyncOpLogs.Add(new SyncOpLog
        {
            Id = 0,
            ClientOpId = op.ClientOpId,
            ProcesadoEn = DateTime.UtcNow,
            ResultadoJson = JsonSerializer.Serialize(resultado, JsonOpts),
        });
        await _db.SaveChangesAsync();
    }

    private static CarpetaSyncPayload? DeserializarCarpeta(SyncOpDTO op)
        => op.Payload?.Deserialize<CarpetaSyncPayload>(JsonOpts);

    private static SyncOpResultDTO Aplicado(SyncOpDTO op, int? serverId, long? version) => new()
    {
        ClientOpId = op.ClientOpId,
        ClientEntityId = op.ClientEntityId,
        Estado = EstadoSyncOp.Aplicado,
        ServerId = serverId,
        Version = version,
    };

    private static SyncOpResultDTO Rechazado(SyncOpDTO op, int? serverId, long? version) => new()
    {
        ClientOpId = op.ClientOpId,
        ClientEntityId = op.ClientEntityId,
        Estado = EstadoSyncOp.Rechazado,
        ServerId = serverId,
        Version = version,
        Mensaje = "El servidor tenía una versión más nueva (last-write-wins)",
    };

    private static SyncOpResultDTO Error(SyncOpDTO op, string mensaje) => new()
    {
        ClientOpId = op.ClientOpId,
        ClientEntityId = op.ClientEntityId,
        Estado = EstadoSyncOp.Error,
        Mensaje = mensaje,
    };

    private static SyncOpResultDTO ResultadoDuplicado(SyncOpDTO op, SyncOpLog previo)
    {
        var resultado = previo.ResultadoJson != null
            ? JsonSerializer.Deserialize<SyncOpResultDTO>(previo.ResultadoJson, JsonOpts)
            : null;

        resultado ??= new SyncOpResultDTO { ClientOpId = op.ClientOpId, ClientEntityId = op.ClientEntityId };
        resultado.Estado = EstadoSyncOp.Duplicado;
        return resultado;
    }
}
