using Api.Core.Logica;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Api.Core.Servicios.Interfaces;
using Api.Persistencia._Config;
using Api.Persistencia.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api._Config;

public static class InyeccionDeDependenciasConfig
{
    public static WebApplicationBuilder Configurar(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBDVirtual, BDVirtual>();
        
        builder.Services.AddScoped<ICarpetaRepo, CarpetaRepo>();
        builder.Services.AddScoped<ICarpetaCore, CarpetaCore>();
        
        builder.Services.AddScoped<IEscritoRepo, EscritoRepo>();
        builder.Services.AddScoped<IEscritoCore, EscritoCore>();
        builder.Services.AddScoped<IPapeleraCore, PapeleraCore>();

        builder.Services.AddScoped<IHabitoRepo, HabitoRepo>();
        builder.Services.AddScoped<IRegistroHabitoRepo, RegistroHabitoRepo>();
        builder.Services.AddScoped<IHabitoCore, HabitoCore>();

        builder.Services.AddScoped<IListaObjetivoRepo, ListaObjetivoRepo>();
        builder.Services.AddScoped<IItemObjetivoRepo, ItemObjetivoRepo>();
        builder.Services.AddScoped<IObjetivoCore, ObjetivoCore>();

        builder.Services.AddScoped<ISyncCore, SyncCore>();
        
        builder.Services.AddScoped<IAuthService, AuthCore>();

        builder.Services.AddScoped<AppPaths, AppPathsWebApp>();
        builder.Services.AddScoped<IBackupCore, BackupCore>();
        builder.Services.AddScoped<IGoogleDriveCore, GoogleDriveCore>();
        
        // Configurar la autenticación JWT
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Obtener la clave secreta de la configuración o usar una clave por defecto
                string claveSecreta = builder.Configuration.GetSection("AppSettings:Token").Value ?? "clave_secreta_por_defecto_para_desarrollo_con_longitud_suficiente_para_hmac_sha512";
                
                // Asegurar que la clave tenga al menos 64 bytes (512 bits) para HMAC-SHA512
                if (Encoding.UTF8.GetByteCount(claveSecreta) < 64)
                {
                    // Extender la clave hasta alcanzar al menos 64 bytes
                    claveSecreta = claveSecreta.PadRight(64, '_');
                }
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        
        return builder;
    }
}