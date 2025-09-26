using back_cabs.CRM.models.Auth;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.contexts;

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

    /// <summary>
    /// Usuarios del sistema de autenticación
    /// </summary>
    public DbSet<UsuarioAuth> UsuariosAuth { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la entidad UsuarioAuth
        modelBuilder.Entity<UsuarioAuth>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.NombreCompleto).IsRequired();
            entity.Property(e => e.Rol).IsRequired();
            entity.Property(e => e.ContrasenaHash).IsRequired();
        });
    }
}