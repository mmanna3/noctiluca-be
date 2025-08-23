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
    }
    
    public DbSet<Carpeta> Carpetas { get; set; } = null!;
    public DbSet<Nota> Notas { get; set; } = null!;
}