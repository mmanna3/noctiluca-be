using Api.Core.Entidades;
using Api.Core.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia._Config;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        // var connectionString = Database.GetDbConnection().ConnectionString;
        // Console.WriteLine($"Using Connection String: {connectionString}");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Administrador" }
        );
        
        builder.Entity<Usuario>()
            .Property(u => u.RolId)
            .HasDefaultValue(1);
        
        builder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
        
        /*builder.Entity<Usuario>().HasData(
            new Usuario 
            { 
                Id = 1, 
                NombreUsuario = "mana", 
                Password = AuthCore.HashPassword("contraseña"),
                RolId = 1
            }
        );*/
        
        builder.Entity<CriterioDeOrden>().HasData(
            new CriterioDeOrden { Id = 1, Criterio = "Creación Desc" },
            new CriterioDeOrden { Id = 2, Criterio = "Edición Desc" },
            new CriterioDeOrden { Id = 3, Criterio = "A-Z" },
            new CriterioDeOrden { Id = 4, Criterio = "Creación Asc" },
            new CriterioDeOrden { Id = 5, Criterio = "Edición Asc" },
            new CriterioDeOrden { Id = 6, Criterio = "Z-A" }
        );
        
        builder.Entity<Carpeta>()
            .Property(u => u.CriterioDeOrdenId)
            .HasDefaultValue(1);
    }
    
    public DbSet<Carpeta> Carpetas { get; set; } = null!;
    public DbSet<Escrito> Escritos { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
}