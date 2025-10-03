using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.Recepcion;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.contexts;

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

    /// <summary>
    /// Usuarios del sistema de autenticación
    /// </summary>
    public DbSet<UsuarioAuth> UsuariosAuth { get; set; } = null!;

    /// <summary>
    /// Clientes del sistema CRM
    /// </summary>
    public DbSet<Cliente> Clientes { get; set; } = null!;

    /// <summary>
    /// Órdenes de trabajo del módulo de Recepción
    /// </summary>
    public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la entidad UsuarioAuth
        modelBuilder.Entity<UsuarioAuth>(entity =>
        {
            entity.ToTable("Auth_usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd(); // IDENTITY autoincremental
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefono).IsRequired(false);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.Rol).IsRequired(false);
            entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.LicenciaConducir).HasMaxLength(50);
            entity.Property(e => e.TransmisionHabilitada).HasMaxLength(50);
            entity.Property(e => e.CreadoEn).IsRequired().HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ActualizadoEn).IsRequired(false);
        });

        // Configuración de la entidad Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RFC).HasMaxLength(13);
            entity.HasIndex(e => e.RFC);
        });

        // Configuración de la entidad OrdenTrabajo
        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Modalidad).HasMaxLength(50);
            entity.Property(e => e.TipoOrden).HasMaxLength(50);
            entity.Property(e => e.Cotizaciones).HasMaxLength(255);
            
            // Relación con Cliente
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.OrdenesTrabajo)
                  .HasForeignKey(e => e.ClienteRefId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Índices para mejorar rendimiento en consultas frecuentes
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.CitaProgramadaInicio);
            entity.HasIndex(e => e.id_usuario);
        });
    }
}