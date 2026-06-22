using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class SubCarpetasTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public SubCarpetasTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    private async Task<Carpeta> CrearCarpetaEnDB(string titulo, int? carpetaPadreId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var carpeta = new Carpeta
        {
            Id = 0,
            Titulo = titulo,
            CriterioDeOrdenId = 1,
            CarpetaPadreId = carpetaPadreId
        };
        db.Carpetas.Add(carpeta);
        await db.SaveChangesAsync();
        return carpeta;
    }

    [Fact]
    public async Task CrearSubcarpeta_DentroDeCarpetaRaiz_OK()
    {
        var carpetaRaiz = await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());

        var dto = new CarpetaDTO
        {
            Titulo = "Subcarpeta " + Guid.NewGuid(),
            CarpetaPadreId = carpetaRaiz.Id
        };

        var response = await _client.PostAsJsonAsync("/api/Carpeta", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var subcarpeta = await db.Carpetas.AsNoTracking().SingleAsync(c => c.Titulo == dto.Titulo);
        Assert.Equal(carpetaRaiz.Id, subcarpeta.CarpetaPadreId);
    }

    [Fact]
    public async Task CrearSubcarpeta_DentroDeSubcarpeta_Error()
    {
        var carpetaRaiz = await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());
        var subcarpeta = await CrearCarpetaEnDB("Sub " + Guid.NewGuid(), carpetaRaiz.Id);

        var dto = new CarpetaDTO
        {
            Titulo = "Sub-sub " + Guid.NewGuid(),
            CarpetaPadreId = subcarpeta.Id
        };

        var response = await _client.PostAsJsonAsync("/api/Carpeta", dto);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task EliminarCarpetaConSubcarpetas_Error()
    {
        var carpetaRaiz = await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());
        await CrearCarpetaEnDB("Sub " + Guid.NewGuid(), carpetaRaiz.Id);

        var response = await _client.DeleteAsync($"/api/Carpeta/{carpetaRaiz.Id}");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerCarpeta_IncluyeSubcarpetas()
    {
        var carpetaRaiz = await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());
        await CrearCarpetaEnDB("Sub1 " + Guid.NewGuid(), carpetaRaiz.Id);
        await CrearCarpetaEnDB("Sub2 " + Guid.NewGuid(), carpetaRaiz.Id);

        var response = await _client.GetAsync($"/api/Carpeta/{carpetaRaiz.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var carpetaDTO = await response.Content.ReadFromJsonAsync<CarpetaDTO>();
        Assert.NotNull(carpetaDTO);
        Assert.Equal(2, carpetaDTO.CantidadDeSubCarpetas);
    }

    [Fact]
    public async Task ListarCarpetas_DevuelveSoloRaices()
    {
        var carpetaRaiz = await CrearCarpetaEnDB("Raiz " + Guid.NewGuid());
        await CrearCarpetaEnDB("Sub " + Guid.NewGuid(), carpetaRaiz.Id);

        var response = await _client.GetAsync("/api/Carpeta");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var carpetas = await response.Content.ReadFromJsonAsync<List<CarpetaDTO>>();
        Assert.NotNull(carpetas);

        var subcarpetasEnLista = carpetas.Where(c => c.CarpetaPadreId.HasValue).ToList();
        Assert.Empty(subcarpetasEnLista);
    }

    [Fact]
    public async Task CrearSubcarpeta_CarpetaPadreInexistente_Error()
    {
        var dto = new CarpetaDTO
        {
            Titulo = "Subcarpeta " + Guid.NewGuid(),
            CarpetaPadreId = 99999
        };

        var response = await _client.PostAsJsonAsync("/api/Carpeta", dto);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
