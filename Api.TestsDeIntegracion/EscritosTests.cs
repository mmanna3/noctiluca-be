using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class EscritosTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public EscritosTests(NoctilucaWebApplicationFactory factory)
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
    public async Task CrearEscrito_OK()
    {
        var carpeta = await CrearCarpetaEnDB();
        var dto = new EscritoDTO { Titulo = "Escrito " + Guid.NewGuid(), CarpetaId = carpeta.Id };

        var response = await _client.PostAsJsonAsync("/api/Escrito", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var escritoCreado = await response.Content.ReadFromJsonAsync<EscritoDTO>();
        Assert.NotNull(escritoCreado);
        Assert.Equal(dto.Titulo, escritoCreado.Titulo);
        Assert.True(escritoCreado.Id > 0);
        Assert.Equal(carpeta.Id, escritoCreado.CarpetaId);
    }

    [Fact]
    public async Task CrearEscrito_SinTitulo_UsaFechaHoraComoTitulo()
    {
        var carpeta = await CrearCarpetaEnDB();
        var dto = new EscritoDTO { Titulo = "", CarpetaId = carpeta.Id };

        var response = await _client.PostAsJsonAsync("/api/Escrito", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var escritoCreado = await response.Content.ReadFromJsonAsync<EscritoDTO>();
        Assert.NotNull(escritoCreado);
        Assert.True(escritoCreado.Id > 0);

        // El controller devuelve el DTO original; el título auto-generado está en la DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var escritoEnDB = await db.Escritos.AsNoTracking().SingleAsync(e => e.Id == escritoCreado.Id);
        Assert.False(string.IsNullOrWhiteSpace(escritoEnDB.Titulo));
        // El título auto-generado tiene el formato dd-MM-yy HH:mm
        Assert.Matches(@"^\d{2}-\d{2}-\d{2} \d{2}:\d{2}$", escritoEnDB.Titulo);
    }

    [Fact]
    public async Task ObtenerEscrito_PorId_OK()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escrito = await CrearEscritoEnDB(carpeta.Id, "Escrito " + Guid.NewGuid());

        var response = await _client.GetAsync($"/api/Escrito/{escrito.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var escritoDTO = await response.Content.ReadFromJsonAsync<EscritoDTO>();
        Assert.NotNull(escritoDTO);
        Assert.Equal(escrito.Id, escritoDTO.Id);
        Assert.Equal(escrito.Titulo, escritoDTO.Titulo);
    }

    [Fact]
    public async Task EliminarEscrito_OK()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escrito = await CrearEscritoEnDB(carpeta.Id, "Escrito " + Guid.NewGuid());

        var response = await _client.DeleteAsync($"/api/Escrito/{escrito.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eliminado = await db.Escritos.AsNoTracking().SingleOrDefaultAsync(e => e.Id == escrito.Id);
        Assert.Null(eliminado);
    }

    [Fact]
    public async Task PonerEnPapelera_OK()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escrito = await CrearEscritoEnDB(carpeta.Id, "Escrito " + Guid.NewGuid());

        var response = await _client.PostAsync($"/api/Papelera/poner-en-papelera?id={escrito.Id}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var enPapelera = await db.Escritos.AsNoTracking().SingleAsync(e => e.Id == escrito.Id);
        Assert.True(enPapelera.EstaEnPapelera);
    }

    [Fact]
    public async Task ModificarEscrito_CambiaTitulo_OK()
    {
        var carpeta = await CrearCarpetaEnDB();
        var escrito = await CrearEscritoEnDB(carpeta.Id, "Original " + Guid.NewGuid());
        var tituloNuevo = "Modificado " + Guid.NewGuid();

        var dto = new EscritoDTO
        {
            Id = escrito.Id,
            Titulo = tituloNuevo,
            CarpetaId = carpeta.Id,
            FechaHoraCreacion = escrito.FechaHoraCreacion
        };

        var response = await _client.PutAsJsonAsync($"/api/Escrito/{escrito.Id}", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modificado = await db.Escritos.AsNoTracking().SingleAsync(e => e.Id == escrito.Id);
        Assert.Equal(tituloNuevo, modificado.Titulo);
    }
}
