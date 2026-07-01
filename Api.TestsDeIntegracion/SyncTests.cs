using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Core.DTOs.Sync;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class SyncTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public SyncTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    private async Task<Carpeta> CrearCarpetaEnDB()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var carpeta = new Carpeta { Id = 0, Titulo = "Carpeta " + Guid.NewGuid(), CriterioDeOrdenId = 1 };
        db.Carpetas.Add(carpeta);
        await db.SaveChangesAsync();
        return carpeta;
    }

    private static SyncOpDTO Op(string entityType, string operation, Guid clientEntityId, object payload,
        long? baseVersion = null, DateTime? clientTimestamp = null)
        => new()
        {
            ClientOpId = Guid.NewGuid(),
            EntityType = entityType,
            Operation = operation,
            ClientEntityId = clientEntityId,
            BaseVersion = baseVersion,
            ClientTimestamp = clientTimestamp ?? DateTime.UtcNow,
            Payload = JsonSerializer.SerializeToElement(payload, JsonOpts),
        };

    private async Task<SyncPushResultDTO> Push(params SyncOpDTO[] ops)
    {
        var dto = new SyncPushDTO { DeviceId = Guid.NewGuid(), Operaciones = ops.ToList() };
        var response = await _client.PostAsJsonAsync("/api/Sync/aplicar", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<SyncPushResultDTO>();
        Assert.NotNull(resultado);
        return resultado!;
    }

    private async Task<SyncPullDTO> Pull(long desde)
    {
        var response = await _client.GetAsync($"/api/Sync/cambios?desde={desde}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultado = await response.Content.ReadFromJsonAsync<SyncPullDTO>();
        Assert.NotNull(resultado);
        return resultado!;
    }

    [Fact]
    public async Task Estampado_AlCrearEntidad_AsignaClientIdYVersionMayorACero()
    {
        var carpeta = await CrearCarpetaEnDB();

        Assert.NotEqual(Guid.Empty, carpeta.ClientId);
        Assert.True(carpeta.Version > 0);
    }

    [Fact]
    public async Task Pull_TraeEntidadesConCursorQueAvanza()
    {
        var carpeta = await CrearCarpetaEnDB();

        var pull = await Pull(0);

        Assert.Contains(pull.Carpetas, c => c.ClientId == carpeta.ClientId);
        Assert.True(pull.Cursor >= carpeta.Version);

        // Un pull incremental desde el cursor no debe re-traer lo ya visto.
        var incremental = await Pull(pull.Cursor);
        Assert.DoesNotContain(incremental.Carpetas, c => c.ClientId == carpeta.ClientId);
    }

    [Fact]
    public async Task Push_CreaCarpetaYEscritoOffline_ResuelveDependenciaPorClientId()
    {
        var carpetaClientId = Guid.NewGuid();
        var escritoClientId = Guid.NewGuid();

        // El escrito referencia la carpeta por su ClientId, aunque la carpeta
        // todavía no existe en el servidor (ambas se crearon offline).
        var resultado = await Push(
            Op("escrito", "upsert", escritoClientId, new EscritoSyncPayload
            {
                Titulo = "Nota offline",
                Cuerpo = "Contenido",
                CarpetaClientId = carpetaClientId,
            }),
            Op("carpeta", "upsert", carpetaClientId, new CarpetaSyncPayload
            {
                Titulo = "Carpeta offline",
                CriterioDeOrden = 1,
            })
        );

        var resCarpeta = resultado.Resultados.Single(r => r.ClientEntityId == carpetaClientId);
        var resEscrito = resultado.Resultados.Single(r => r.ClientEntityId == escritoClientId);

        Assert.Equal(EstadoSyncOp.Aplicado, resCarpeta.Estado);
        Assert.Equal(EstadoSyncOp.Aplicado, resEscrito.Estado);
        Assert.NotNull(resCarpeta.ServerId);
        Assert.NotNull(resEscrito.ServerId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escrito = await db.Escritos.AsNoTracking().SingleAsync(e => e.ClientId == escritoClientId);
        var carpeta = await db.Carpetas.AsNoTracking().SingleAsync(c => c.ClientId == carpetaClientId);
        Assert.Equal(carpeta.Id, escrito.CarpetaId);
    }

    [Fact]
    public async Task Push_EsIdempotente_ConMismaClientOpId()
    {
        var clientId = Guid.NewGuid();
        var op = Op("carpeta", "upsert", clientId, new CarpetaSyncPayload { Titulo = "Idempotente", CriterioDeOrden = 1 });

        var primera = await Push(op);
        Assert.Equal(EstadoSyncOp.Aplicado, primera.Resultados.Single().Estado);

        // Reenvío exacto de la misma operación (simula un retry por timeout).
        var segunda = await Push(op);
        Assert.Equal(EstadoSyncOp.Duplicado, segunda.Resultados.Single().Estado);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cantidad = await db.Carpetas.CountAsync(c => c.ClientId == clientId);
        Assert.Equal(1, cantidad);
    }

    [Fact]
    public async Task Push_Update_LWW_RechazaCambioViejoYAceptaNuevo()
    {
        var clientId = Guid.NewGuid();
        var creacion = await Push(Op("carpeta", "upsert", clientId,
            new CarpetaSyncPayload { Titulo = "Original", CriterioDeOrden = 1 }));
        var versionBase = creacion.Resultados.Single().Version!.Value;

        // Cambio "viejo": timestamp anterior a la última edición del servidor y
        // base desactualizada -> debe perder (rechazado) y devolver el estado server.
        var viejo = await Push(Op("carpeta", "upsert", clientId,
            new CarpetaSyncPayload { Titulo = "Viejo", CriterioDeOrden = 1 },
            baseVersion: versionBase - 1,
            clientTimestamp: DateTime.UtcNow.AddMinutes(-10)));
        Assert.Equal(EstadoSyncOp.Rechazado, viejo.Resultados.Single().Estado);

        // Cambio "nuevo": timestamp futuro -> gana (aplicado).
        var nuevo = await Push(Op("carpeta", "upsert", clientId,
            new CarpetaSyncPayload { Titulo = "Nuevo", CriterioDeOrden = 1 },
            baseVersion: versionBase - 1,
            clientTimestamp: DateTime.UtcNow.AddMinutes(10)));
        Assert.Equal(EstadoSyncOp.Aplicado, nuevo.Resultados.Single().Estado);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var carpeta = await db.Carpetas.AsNoTracking().SingleAsync(c => c.ClientId == clientId);
        Assert.Equal("Nuevo", carpeta.Titulo);
    }

    [Fact]
    public async Task Push_SinConflicto_CuandoBaseVersionCoincide()
    {
        var clientId = Guid.NewGuid();
        var creacion = await Push(Op("carpeta", "upsert", clientId,
            new CarpetaSyncPayload { Titulo = "Original", CriterioDeOrden = 1 }));
        var versionBase = creacion.Resultados.Single().Version!.Value;

        // Aunque el timestamp sea viejo, si la base coincide con la del servidor no
        // hay cambio concurrente -> se aplica igual.
        var update = await Push(Op("carpeta", "upsert", clientId,
            new CarpetaSyncPayload { Titulo = "Editado", CriterioDeOrden = 1 },
            baseVersion: versionBase,
            clientTimestamp: DateTime.UtcNow.AddMinutes(-10)));

        Assert.Equal(EstadoSyncOp.Aplicado, update.Resultados.Single().Estado);
    }

    [Fact]
    public async Task Push_Delete_GeneraTombstoneVisibleEnPull()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escritoClientId = Guid.NewGuid();
        await Push(Op("escrito", "upsert", escritoClientId, new EscritoSyncPayload
        {
            Titulo = "A borrar",
            CarpetaClientId = carpeta.ClientId,
        }));

        var pullPrevio = await Pull(0);

        var borrado = await Push(Op("escrito", "delete", escritoClientId, new { }));
        Assert.Equal(EstadoSyncOp.Aplicado, borrado.Resultados.Single().Estado);

        var pull = await Pull(pullPrevio.Cursor);
        Assert.Contains(pull.Eliminados, t => t.ClientId == escritoClientId && t.TipoEntidad == nameof(Escrito));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Escritos.AsNoTracking().AnyAsync(e => e.ClientId == escritoClientId));
    }

    [Fact]
    public async Task Push_UpsertExistente_ActualizaEnLugarDeDuplicar()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escritoClientId = Guid.NewGuid();

        await Push(Op("escrito", "upsert", escritoClientId, new EscritoSyncPayload
        {
            Titulo = "v1",
            CarpetaClientId = carpeta.ClientId,
        }));

        await Push(Op("escrito", "upsert", escritoClientId, new EscritoSyncPayload
        {
            Titulo = "v2",
            Cuerpo = "cuerpo nuevo",
            CarpetaClientId = carpeta.ClientId,
        }, clientTimestamp: DateTime.UtcNow.AddMinutes(5)));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escritos = await db.Escritos.AsNoTracking().Where(e => e.ClientId == escritoClientId).ToListAsync();
        Assert.Single(escritos);
        Assert.Equal("v2", escritos[0].Titulo);
        Assert.Equal("cuerpo nuevo", escritos[0].Cuerpo);
    }
}
