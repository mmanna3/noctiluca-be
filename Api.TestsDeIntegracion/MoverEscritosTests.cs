using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class MoverEscritosTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public MoverEscritosTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    private async Task<(int carpeta1Id, int carpeta2Id, int escrito1Id, int escrito2Id)> SeedData()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var carpeta1 = new Carpeta { Id = 0, Titulo = "Carpeta " + Guid.NewGuid(), CriterioDeOrdenId = 1 };
        var carpeta2 = new Carpeta { Id = 0, Titulo = "Carpeta " + Guid.NewGuid(), CriterioDeOrdenId = 1 };
        db.Carpetas.Add(carpeta1);
        db.Carpetas.Add(carpeta2);
        await db.SaveChangesAsync();

        var escrito1 = new Escrito
        {
            Id = 0,
            Titulo = "Escrito 1",
            CarpetaId = carpeta1.Id,
            FechaHoraCreacion = DateTime.Now
        };
        var escrito2 = new Escrito
        {
            Id = 0,
            Titulo = "Escrito 2",
            CarpetaId = carpeta1.Id,
            FechaHoraCreacion = DateTime.Now
        };
        db.Escritos.Add(escrito1);
        db.Escritos.Add(escrito2);
        await db.SaveChangesAsync();

        return (carpeta1.Id, carpeta2.Id, escrito1.Id, escrito2.Id);
    }

    [Fact]
    public async Task MoverEscritoAOtraCarpeta_OK()
    {
        var (_, carpeta2Id, escrito1Id, _) = await SeedData();

        var dto = new MoverEscritosDTO
        {
            EscritoIds = [escrito1Id],
            CarpetaDestinoId = carpeta2Id
        };

        var response = await _client.PutAsJsonAsync("/api/escrito/mover", dto);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escrito = await db.Escritos.AsNoTracking().SingleAsync(e => e.Id == escrito1Id);
        Assert.Equal(carpeta2Id, escrito.CarpetaId);
    }

    [Fact]
    public async Task MoverVariosEscritosAOtraCarpeta_OK()
    {
        var (_, carpeta2Id, escrito1Id, escrito2Id) = await SeedData();

        var dto = new MoverEscritosDTO
        {
            EscritoIds = [escrito1Id, escrito2Id],
            CarpetaDestinoId = carpeta2Id
        };

        var response = await _client.PutAsJsonAsync("/api/escrito/mover", dto);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escritos = await db.Escritos
            .AsNoTracking()
            .Where(e => e.Id == escrito1Id || e.Id == escrito2Id)
            .ToListAsync();

        Assert.All(escritos, e => Assert.Equal(carpeta2Id, e.CarpetaId));
    }

    [Fact]
    public async Task MoverEscrito_CarpetaInexistente_Error()
    {
        var (_, _, escrito1Id, _) = await SeedData();

        var dto = new MoverEscritosDTO
        {
            EscritoIds = [escrito1Id],
            CarpetaDestinoId = 99999
        };

        var response = await _client.PutAsJsonAsync("/api/escrito/mover", dto);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task MoverEscrito_EscritoInexistente_Error()
    {
        var (_, carpeta2Id, _, _) = await SeedData();

        var dto = new MoverEscritosDTO
        {
            EscritoIds = [99999],
            CarpetaDestinoId = carpeta2Id
        };

        var response = await _client.PutAsJsonAsync("/api/escrito/mover", dto);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
