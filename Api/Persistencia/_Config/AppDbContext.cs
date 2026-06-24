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

        builder.Entity<Carpeta>()
            .HasOne(c => c.CarpetaPadre)
            .WithMany(c => c.SubCarpetas)
            .HasForeignKey(c => c.CarpetaPadreId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RegistroHabito>()
            .HasOne(r => r.Habito)
            .WithMany(h => h.Registros)
            .HasForeignKey(r => r.HabitoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RegistroHabito>()
            .HasIndex(r => new { r.HabitoId, r.Fecha })
            .IsUnique();

        builder.Entity<ListaObjetivo>()
            .HasIndex(l => new { l.Tipo, l.ClavePeriodo })
            .IsUnique();

        builder.Entity<ItemObjetivo>()
            .HasOne(i => i.ListaObjetivo)
            .WithMany(l => l.Items)
            .HasForeignKey(i => i.ListaObjetivoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    public DbSet<Carpeta> Carpetas { get; set; } = null!;
    public DbSet<Escrito> Escritos { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Habito> Habitos { get; set; } = null!;
    public DbSet<RegistroHabito> RegistrosHabito { get; set; } = null!;
    public DbSet<ListaObjetivo> ListasObjetivo { get; set; } = null!;
    public DbSet<ItemObjetivo> ItemsObjetivo { get; set; } = null!;
}