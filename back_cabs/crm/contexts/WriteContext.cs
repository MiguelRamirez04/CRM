using Microsoft.EntityFrameworkCore;

namespace CRM.Contexts;

/// <summary>
/// Contexto básico para operaciones de escritura (POST, PUT, DELETE)
/// </summary>
public class WriteContext : DbContext
{
    public WriteContext(DbContextOptions<WriteContext> options) : base(options)
    {
        // Configuraciones específicas para escritura
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        ChangeTracker.AutoDetectChangesEnabled = true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuraciones se agregarán cuando se definan los modelos
        base.OnModelCreating(modelBuilder);
    }
}