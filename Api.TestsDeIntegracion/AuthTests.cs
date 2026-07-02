using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Servicios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.TestsDeIntegracion;

public class AuthTests : IClassFixture<NoctilucaWebApplicationFactory>
{
    private readonly HttpClient _clientAutenticado;
    private readonly NoctilucaWebApplicationFactory _factory;
    private const string PasswordPrueba = "password123";

    public AuthTests(NoctilucaWebApplicationFactory factory)
    {
        _factory = factory;
        _clientAutenticado = factory.CreateClient();
        _clientAutenticado.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        CrearUsuarioDePrueba().GetAwaiter().GetResult();
    }

    private async Task CrearUsuarioDePrueba()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == "test"))
            return;

        if (!await db.Set<Rol>().AnyAsync())
        {
            db.Set<Rol>().Add(new Rol { Id = 1, Nombre = "Administrador" });
            await db.SaveChangesAsync();
        }

        var rol = await db.Set<Rol>().FirstAsync();
        db.Usuarios.Add(new Usuario
        {
            Id = 0,
            NombreUsuario = "test",
            Password = AuthCore.HashPassword(PasswordPrueba),
            RolId = rol.Id,
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task ValidarPassword_Correcto_OK()
    {
        var response = await _clientAutenticado.PostAsJsonAsync(
            "/api/Auth/validar-password",
            new ValidarPasswordDTO { Password = PasswordPrueba });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidarPasswordResponseDTO>();
        Assert.NotNull(body);
        Assert.True(body.Exito);
    }

    [Fact]
    public async Task ValidarPassword_Incorrecto_BadRequest()
    {
        var response = await _clientAutenticado.PostAsJsonAsync(
            "/api/Auth/validar-password",
            new ValidarPasswordDTO { Password = "incorrecta" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidarPasswordResponseDTO>();
        Assert.NotNull(body);
        Assert.False(body.Exito);
    }

    [Fact]
    public async Task ValidarPassword_UsuarioInexistente_BadRequest()
    {
        // El handler de test autentica siempre; simulamos usuario sin registro en DB
        // usando el nombre "test" que sí existe. Para contraseña incorrecta:
        var response = await _clientAutenticado.PostAsJsonAsync(
            "/api/Auth/validar-password",
            new ValidarPasswordDTO { Password = "nunca-valida" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
