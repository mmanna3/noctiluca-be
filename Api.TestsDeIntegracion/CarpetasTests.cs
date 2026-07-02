using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class CarpetasTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public CarpetasTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    private async Task<Carpeta> CrearCarpetaEnDB(string titulo)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var carpeta = new Carpeta { Id = 0, Titulo = titulo, CriterioDeOrdenId = 1 };
        db.Carpetas.Add(carpeta);
        await db.SaveChangesAsync();
        return carpeta;
    }

    private async Task<Escrito> CrearEscritoEnDB(int carpetaId, string titulo)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escrito = new Escrito
        {
            Id = 0,
            Titulo = titulo,
            CarpetaId = carpetaId,
            FechaHoraCreacion = DateTime.Now
        };
        db.Escritos.Add(escrito);
        await db.SaveChangesAsync();
        return escrito;
    }

    [Fact]
    public async Task CrearCarpeta_OK()
    {
        var dto = new CarpetaDTO { Titulo = "Nueva " + Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/Carpeta", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var carpetaCreada = await response.Content.ReadFromJsonAsync<CarpetaDTO>();
        Assert.NotNull(carpetaCreada);
        Assert.Equal(dto.Titulo, carpetaCreada.Titulo);
        Assert.True(carpetaCreada.Id > 0);
    }

    [Fact]
    public async Task ObtenerCarpeta_PorId_OK()
    {
        var carpeta = await CrearCarpetaEnDB("Carpeta " + Guid.NewGuid());

        var response = await _client.GetAsync($"/api/Carpeta/{carpeta.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var carpetaDTO = await response.Content.ReadFromJsonAsync<CarpetaDTO>();
        Assert.NotNull(carpetaDTO);
        Assert.Equal(carpeta.Titulo, carpetaDTO.Titulo);
        Assert.Equal(carpeta.Id, carpetaDTO.Id);
    }

    [Fact]
    public async Task ObtenerCarpetas_ListaSoloRaices()
    {
        await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());

        var response = await _client.GetAsync("/api/Carpeta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<CarpetaDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, c => Assert.Null(c.CarpetaPadreId));
    }

    [Fact]
    public async Task EliminarCarpeta_SinContenido_OK()
    {
        var carpeta = await CrearCarpetaEnDB("Carpeta " + Guid.NewGuid());

        var response = await _client.DeleteAsync($"/api/Carpeta/{carpeta.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eliminada = await db.Carpetas.AsNoTracking().SingleOrDefaultAsync(c => c.Id == carpeta.Id);
        Assert.Null(eliminada);
    }

    [Fact]
    public async Task EliminarCarpeta_ConEscritos_Error()
    {
        var carpeta = await CrearCarpetaEnDB("Carpeta " + Guid.NewGuid());
        await CrearEscritoEnDB(carpeta.Id, "Escrito");

        var response = await _client.DeleteAsync($"/api/Carpeta/{carpeta.Id}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ModificarCarpeta_CambiaTitulo_OK()
    {
        var carpeta = await CrearCarpetaEnDB("Original " + Guid.NewGuid());
        var tituloNuevo = "Modificado " + Guid.NewGuid();
        var dto = new CarpetaDTO { Id = carpeta.Id, Titulo = tituloNuevo };

        var response = await _client.PutAsJsonAsync($"/api/Carpeta/{carpeta.Id}", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modificada = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Id == carpeta.Id);
        Assert.Equal(tituloNuevo, modificada.Titulo);
    }

    [Fact]
    public async Task ActualizarCriterioDeOrden_OK()
    {
        var carpeta = await CrearCarpetaEnDB("Carpeta " + Guid.NewGuid());

        var response = await _client.PutAsJsonAsync($"/api/Carpeta/{carpeta.Id}/criterio-orden", 3);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actualizada = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Id == carpeta.Id);
        Assert.Equal(3, actualizada.CriterioDeOrdenId);
    }

    [Fact]
    public async Task ActualizarPosiciones_OK()
    {
        var carpeta1 = await CrearCarpetaEnDB("A " + Guid.NewGuid());
        var carpeta2 = await CrearCarpetaEnDB("B " + Guid.NewGuid());

        var dto = new ActualizarPosicionesDTO
        {
            Posiciones =
            [
                new PosicionCarpetaDTO { IdDeCarpeta = carpeta1.Id, Posicion = 2 },
                new PosicionCarpetaDTO { IdDeCarpeta = carpeta2.Id, Posicion = 1 }
            ]
        };

        var response = await _client.PutAsJsonAsync("/api/Carpeta/actualizar-posiciones", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var c1 = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Id == carpeta1.Id);
        var c2 = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Id == carpeta2.Id);
        Assert.Equal(2, c1.Posicion);
        Assert.Equal(1, c2.Posicion);
    }

    [Fact]
    public async Task ModificarCarpetaRaiz_PuedeSerPrivada_OK()
    {
        var carpeta = await CrearCarpetaEnDB("Privada " + Guid.NewGuid());
        var dto = new CarpetaDTO { Id = carpeta.Id, Titulo = carpeta.Titulo, RequiereAutenticacion = true };

        var response = await _client.PutAsJsonAsync($"/api/Carpeta/{carpeta.Id}", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modificada = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Id == carpeta.Id);
        Assert.True(modificada.RequiereAutenticacion);
    }

    [Fact]
    public async Task ModificarSubcarpeta_ComoPrivada_Error()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var raiz = new Carpeta { Id = 0, Titulo = "Raiz " + Guid.NewGuid(), CriterioDeOrdenId = 1 };
        db.Carpetas.Add(raiz);
        await db.SaveChangesAsync();
        var sub = new Carpeta
        {
            Id = 0,
            Titulo = "Sub " + Guid.NewGuid(),
            CriterioDeOrdenId = 1,
            CarpetaPadreId = raiz.Id,
        };
        db.Carpetas.Add(sub);
        await db.SaveChangesAsync();

        var dto = new CarpetaDTO
        {
            Id = sub.Id,
            Titulo = sub.Titulo,
            CarpetaPadreId = raiz.Id,
            RequiereAutenticacion = true,
        };

        var response = await _client.PutAsJsonAsync($"/api/Carpeta/{sub.Id}", dto);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
