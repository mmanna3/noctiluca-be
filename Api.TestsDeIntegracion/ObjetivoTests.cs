using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.TestsDeIntegracion;

public class ObjetivoTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NoctilucaWebApplicationFactory _factory;

    public ObjetivoTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task ObtenerListaDia_GetOrCreate_DevuelveListaVacia()
    {
        var fecha = new DateTime(2026, 6, 24);

        var response = await _client.GetAsync($"/api/Objetivo/dia?fecha={fecha:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<ListaObjetivoDTO>();
        Assert.NotNull(lista);
        Assert.Equal(TipoListaObjetivoEnum.Dia, lista!.Tipo);
        Assert.Equal("2026-06-24", lista.ClavePeriodo);
        Assert.Empty(lista.Items);
    }

    [Fact]
    public async Task CrearItem_EnListaDia_PersisteYApareceEnHistorico()
    {
        var fecha = new DateTime(2026, 7, 1);
        var diaResponse = await _client.GetAsync($"/api/Objetivo/dia?fecha={fecha:yyyy-MM-dd}");
        var lista = await diaResponse.Content.ReadFromJsonAsync<ListaObjetivoDTO>();
        Assert.NotNull(lista);

        var crearDto = new CrearItemObjetivoDTO
        {
            ListaObjetivoId = lista!.Id,
            Texto = "Objetivo de prueba",
        };

        var crearResponse = await _client.PostAsJsonAsync("/api/Objetivo/item", crearDto);
        Assert.Equal(HttpStatusCode.OK, crearResponse.StatusCode);

        var historicoResponse = await _client.GetAsync("/api/Objetivo/historico?tipo=1&pagina=1&tamano=20");
        Assert.Equal(HttpStatusCode.OK, historicoResponse.StatusCode);
        var historico = await historicoResponse.Content.ReadFromJsonAsync<HistoricoObjetivoPaginadoDTO>();
        Assert.NotNull(historico);
        Assert.Contains(historico!.Items, h => h.ClavePeriodo == "2026-07-01");
    }

    [Fact]
    public async Task ToggleCompletado_CambiaEstado()
    {
        using var scope = _factory.Services.CreateScope();
        var core = scope.ServiceProvider.GetRequiredService<IObjetivoCore>();

        var lista = await core.ObtenerOCrearListaDia(new DateTime(2026, 7, 2));
        var item = await core.CrearItem(new CrearItemObjetivoDTO
        {
            ListaObjetivoId = lista.Id,
            Texto = "Toggle test",
        });

        var toggled = await core.ToggleCompletado(item.Id);
        Assert.True(toggled.Completado);
        Assert.NotNull(toggled.FechaCompletado);

        var toggledAgain = await core.ToggleCompletado(item.Id);
        Assert.False(toggledAgain.Completado);
        Assert.Null(toggledAgain.FechaCompletado);
    }

    [Fact]
    public async Task ListaDia_ConMasDeSieteItems_IncluyeAdvertencia()
    {
        using var scope = _factory.Services.CreateScope();
        var core = scope.ServiceProvider.GetRequiredService<IObjetivoCore>();

        var lista = await core.ObtenerOCrearListaDia(new DateTime(2026, 7, 3));
        for (var i = 1; i <= 8; i++)
        {
            await core.CrearItem(new CrearItemObjetivoDTO
            {
                ListaObjetivoId = lista.Id,
                Texto = $"Item {i}",
            });
        }

        var actualizada = await core.ObtenerOCrearListaDia(new DateTime(2026, 7, 3));
        Assert.NotNull(actualizada.AdvertenciaLimite);
        Assert.Contains("7", actualizada.AdvertenciaLimite);
    }

    [Fact]
    public async Task Historico_NoIncluyeListasVacias()
    {
        using var scope = _factory.Services.CreateScope();
        var core = scope.ServiceProvider.GetRequiredService<IObjetivoCore>();

        await core.ObtenerOCrearListaDia(new DateTime(2026, 8, 10));

        var historico = await core.ObtenerHistorico(TipoListaObjetivoEnum.Dia, 1, 50);
        Assert.DoesNotContain(historico.Items, h => h.ClavePeriodo == "2026-08-10");
    }
}
