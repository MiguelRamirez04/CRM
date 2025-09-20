using Microsoft.EntityFrameworkCore;

namespace CRM.Contexts;

/// <summary>
/// Contexto básico para operaciones de solo lectura (GET queries)
/// </summary>
public class ReadOnlyContext : DbContext
{
    public ReadOnlyContext(DbContextOptions<ReadOnlyContext> options) : base(options)
    {
        // Configuraciones específicas para solo lectura
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuraciones se agregarán cuando se definan los modelos
        base.OnModelCreating(modelBuilder);
    }
}