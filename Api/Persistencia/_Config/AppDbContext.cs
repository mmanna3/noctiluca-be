using Api.Core.Entidades;
using Api.Core.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        // Sincronización offline-first
        builder.Entity<Carpeta>().HasIndex(c => c.ClientId).IsUnique();
        builder.Entity<Escrito>().HasIndex(e => e.ClientId).IsUnique();
        builder.Entity<Tombstone>().HasIndex(t => t.Version);
        builder.Entity<SyncOpLog>().HasIndex(s => s.ClientOpId).IsUnique();

        builder.Entity<ContadorSync>().HasData(
            new ContadorSync { Id = 1, UltimoValor = 0 }
        );
    }
    
    public DbSet<Carpeta> Carpetas { get; set; } = null!;
    public DbSet<Escrito> Escritos { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Habito> Habitos { get; set; } = null!;
    public DbSet<RegistroHabito> RegistrosHabito { get; set; } = null!;
    public DbSet<ListaObjetivo> ListasObjetivo { get; set; } = null!;
    public DbSet<ItemObjetivo> ItemsObjetivo { get; set; } = null!;
    public DbSet<Tombstone> Tombstones { get; set; } = null!;
    public DbSet<ContadorSync> ContadoresSync { get; set; } = null!;
    public DbSet<SyncOpLog> SyncOpLogs { get; set; } = null!;

    public override int SaveChanges()
    {
        EstamparEntidadesSincronizables();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EstamparEntidadesSincronizables();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Antes de persistir: genera tombstones para las bajas, asigna ClientId a las
    /// altas que no lo traigan, protege el ClientId de las modificaciones y estampa
    /// una Version monotónica (del <see cref="ContadorSync"/>) y ActualizadoEn en cada
    /// entidad sincronizable tocada. Todo dentro del mismo SaveChanges para que sea atómico.
    /// </summary>
    private void EstamparEntidadesSincronizables()
    {
        var eliminadas = ChangeTracker.Entries<EntidadSincronizable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in eliminadas)
        {
            Tombstones.Add(new Tombstone
            {
                Id = 0,
                TipoEntidad = entry.Metadata.ClrType.Name,
                ClientId = entry.Entity.ClientId,
                EliminadoEn = DateTime.UtcNow,
            });
        }

        var modificadas = ChangeTracker.Entries<EntidadSincronizable>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var tombstonesNuevos = ChangeTracker.Entries<Tombstone>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        if (modificadas.Count == 0 && tombstonesNuevos.Count == 0)
            return;

        var contador = ContadoresSync.Find(1);
        if (contador == null)
        {
            contador = new ContadorSync { Id = 1, UltimoValor = 0 };
            ContadoresSync.Add(contador);
        }

        var ahora = DateTime.UtcNow;

        foreach (var entry in modificadas)
        {
            ProtegerClientId(entry);

            if (entry.Entity.ClientId == Guid.Empty)
                entry.Entity.ClientId = Guid.NewGuid();

            contador.UltimoValor++;
            entry.Entity.Version = contador.UltimoValor;
            entry.Entity.ActualizadoEn = ahora;
        }

        foreach (var tombstone in tombstonesNuevos)
        {
            contador.UltimoValor++;
            tombstone.Version = contador.UltimoValor;
        }
    }

    /// <summary>
    /// Evita que una modificación que no incluya el ClientId (por ejemplo un PUT
    /// clásico que aún no manda el campo) lo pise con Guid.Empty: si el valor
    /// original en base era válido, lo restaura.
    /// </summary>
    private static void ProtegerClientId(EntityEntry<EntidadSincronizable> entry)
    {
        if (entry.State != EntityState.Modified)
            return;

        var propiedad = entry.Property(nameof(EntidadSincronizable.ClientId));
        if (entry.Entity.ClientId == Guid.Empty && propiedad.OriginalValue is Guid original && original != Guid.Empty)
            entry.Entity.ClientId = original;
    }
}