using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.models.Recepcion;
using back_cabs.CRM.models.Sales;
using back_cabs.CRM.models.Administracion;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.models;
using back_cabs.CRM.models.legacy;
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

    /// <summary>
    /// Órdenes de trabajo del módulo de Recepción
    /// </summary>
    public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; } = null!;

    /// <summary>
    /// Cotizaciones del módulo de Recepción
    /// </summary>
    public DbSet<Cotizacion> Cotizaciones { get; set; } = null!;

    /// <summary>
    /// Vehículos de la flota
    /// </summary>
    public DbSet<Vehiculo> Vehiculos { get; set; } = null!;

    /// <summary>
    /// Catálogo de clientes
    /// </summary>
    public DbSet<Catalog_Clientes> CatalogClientes { get; set; } = null!;

    /// <summary>
    /// Catálogo de productos y servicios
    /// </summary>
    public DbSet<Catalog_Productos_Servicio_Ref> CatalogProductosServicios { get; set; } = null!;

    /// <summary>
    /// Gastos de viáticos (comentado para evitar conflicto)
    /// </summary>
    // public DbSet<Finance_Gastos_Viaticos> GastosViaticos { get; set; } = null!;

    /// <summary>
    /// Evaluaciones de servicio
    /// </summary>
    public DbSet<Evaluacion> Evaluaciones { get; set; } = null!;

    /// <summary>
    /// Detalles de evaluaciones
    /// </summary>
    public DbSet<EvaluacionDetalle> EvaluacionesDetalles { get; set; } = null!;

    /// <summary>
    /// Fotos de evaluaciones
    /// </summary>
    public DbSet<EvaluacionFoto> EvaluacionesFotos { get; set; } = null!;

    /// <summary>
    /// Reparaciones de dispositivos
    /// </summary>
    public DbSet<Reparacion> Reparaciones { get; set; } = null!;

    /// <summary>
    /// Componentes utilizados en reparaciones
    /// </summary>
    public DbSet<ReparacionComponente> ReparacionesComponentes { get; set; } = null!;

    /// <summary>
    /// Fotos de reparaciones
    /// </summary>
    public DbSet<ReparacionFoto> ReparacionesFotos { get; set; } = null!;

    /// <summary>
    /// Ejecuciones de órdenes de trabajo
    /// </summary>
    public DbSet<EjecucionOrden> EjecucionesOrden { get; set; } = null!;

    /// <summary>
    /// Documentos y archivos del sistema
    /// </summary>
    public DbSet<FilesDocumento> Documentos { get; set; } = null!;

    /// <summary>
    /// Vista completa de clientes con datos de sistema legacy (solo lectura)
    /// </summary>
    public DbSet<VwClientesCompletos> ClientesCompletos { get; set; } = null!;

    /// <summary>
    /// Gastos de viáticos (solo lectura)
    /// </summary>
    public DbSet<GastoViatico> GastosViaticos { get; set; } = null!;

    /// <summary>
    /// Notificaciones del sistema (solo lectura)
    /// </summary>
    public DbSet<Notificacion> Notificaciones { get; set; } = null!;

    /// <summary>
    /// Catálogo de monedas del sistema
    /// </summary>
    public DbSet<Moneda> Monedas { get; set; } = null!;

    /// <summary>
    /// Referencias a monedas del sistema legacy
    /// </summary>
    public DbSet<MonedasRef> MonedasRef { get; set; } = null!;

    /// <summary>
    /// Catálogo de agentes del sistema
    /// </summary>
    public DbSet<Agente> Agentes { get; set; } = null!;

    /// <summary>
    /// Referencias a agentes del sistema legacy
    /// </summary>
    public DbSet<AgentesRef> AgentesRef { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración simplificada de UsuarioAuth - usa los atributos del modelo
        modelBuilder.Entity<UsuarioAuth>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configuración para la VISTA de solo lectura VwClientesCompletos
        modelBuilder.Entity<VwClientesCompletos>(entity =>
        {
            entity.HasNoKey(); // Es una vista sin clave primaria
            entity.ToView("VwClientesCompletos"); // Mapea a la vista en la BD
            
            // Mapeo explícito de columnas - actualizado según los errores de log
            entity.Property(e => e.ClienteId).HasColumnName("id");
            entity.Property(e => e.NombreComercial).HasColumnName("nombre");
            entity.Property(e => e.RFC).HasColumnName("rfc");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.LegacyClientId).HasColumnName("id_cliente_legacy");
            entity.Property(e => e.Calle).HasColumnName("calle");
            entity.Property(e => e.NumeroExterior).HasColumnName("numero");
            entity.Property(e => e.Colonia).HasColumnName("colonia");
            entity.Property(e => e.CodigoPostal).HasColumnName("cp");
            entity.Property(e => e.Ciudad).HasColumnName("ciudad");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.Pais).HasColumnName("pais");
            entity.Property(e => e.TelefonoPrincipal).HasColumnName("telefono");
            entity.Property(e => e.EmailPrincipal).HasColumnName("email");
        });

        // Configuración de la entidad OrdenTrabajo (ops_ordenes_trabajo)
        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.ToTable("ops_ordenes_trabajo");
            entity.HasKey(e => e.Id);

            // Configurar columnas con mapeo exacto a SQL Server
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.NuevoCliente).HasColumnName("nuevo_cliente");
            entity.Property(e => e.NombreCliente).HasColumnName("nombre_cliente").HasMaxLength(120);
            entity.Property(e => e.CreadoPorUserId).HasColumnName("creado_por_user_id");
            entity.Property(e => e.AsignadaAUserId).HasColumnName("asignada_a_user_id");
            entity.Property(e => e.Notas).HasColumnName("notas").HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.CitaProgramadaInicio).HasColumnName("cita_programada_inicio").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.CitaProgramadaFin).HasColumnName("cita_programada_fin").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.Modalidad).HasColumnName("modalidad").HasMaxLength(50);
            entity.Property(e => e.TipoOrden).HasColumnName("tipo_orden").HasMaxLength(50);
            entity.Property(e => e.Prioridad).HasColumnName("prioridad");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired().HasDefaultValue("CAPTURADA");
            entity.Property(e => e.UbicacionText).HasColumnName("ubicacion_text").HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.RequiereFactura).HasColumnName("requiere_factura").IsRequired().HasDefaultValue(false);
            entity.Property(e => e.EstadoFacturado).HasColumnName("estado_facturado").HasMaxLength(50);
            entity.Property(e => e.FacturaFolio).HasColumnName("factura_folio").HasMaxLength(50);
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasColumnType("DATETIME2(0)").IsRequired().HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.CostoReal).HasColumnName("costo_real").HasColumnType("DECIMAL(12,2)");
            entity.Property(e => e.CostoEstimado).HasColumnName("costo_estimado").HasColumnType("DECIMAL(12,2)");

            // Relaciones con Foreign Keys (solo usuarios, no hay modelo Cliente)
            entity.HasOne(e => e.CreadoPor)
                .WithMany()
                .HasForeignKey(e => e.CreadoPorUserId)
                .HasConstraintName("FK_ordenes_trabajo_creado_por")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AsignadaA)
                .WithMany()
                .HasForeignKey(e => e.AsignadaAUserId)
                .HasConstraintName("FK_ordenes_trabajo_asignada_a")
                .OnDelete(DeleteBehavior.SetNull);

            // Índices para optimización
            entity.HasIndex(e => new { e.ClienteId, e.CreadoEn }).HasDatabaseName("IX_ordenes_cliente");
            entity.HasIndex(e => new { e.Estado, e.AsignadaAUserId }).HasDatabaseName("IX_ordenes_estado_asignado");
            entity.HasIndex(e => e.CitaProgramadaInicio);
        });

        // Configuración de la entidad Cotizacion (sales_cotizaciones) - SOLO LECTURA
        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.ToTable("sales_cotizaciones");
            entity.HasKey(e => e.Id);
            
            // Configurar columna calculada PERSISTED en BD
            entity.Property(e => e.Total)
                .HasComputedColumnSql("[subtotal]+[impuestos_total]", stored: true);
            
            // Índices para optimización de consultas
            entity.HasIndex(e => e.OrdenId).HasDatabaseName("IX_cotizaciones_orden");
            entity.HasIndex(e => e.Estado).HasDatabaseName("IX_cotizaciones_estado");
            entity.HasIndex(e => e.CreadoEn).HasDatabaseName("IX_cotizaciones_fecha");
            entity.HasIndex(e => e.Cliente).HasDatabaseName("IX_cotizaciones_cliente");
        });

        // Configuración de la entidad Vehiculo (fleet_vehiculos)
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasIndex(e => e.Placas).IsUnique().HasFilter("[placas] IS NOT NULL");
        });

        // Configuración de la entidad Catalog_Clientes (catalog_clientes)
        modelBuilder.Entity<Catalog_Clientes>(entity =>
        {
            entity.HasIndex(e => e.LegacyClientId).IsUnique().HasFilter("[legacy_client_id] IS NOT NULL");
            entity.HasIndex(e => e.Email).HasFilter("[email] IS NOT NULL");
            entity.HasIndex(e => e.RFC).IsUnique().HasFilter("[rfc] IS NOT NULL");
        });

        // Configuración de la entidad Catalog_Productos_Servicio_Ref (catalog_productos_servicio_ref)
        modelBuilder.Entity<Catalog_Productos_Servicio_Ref>(entity =>
        {
            entity.HasIndex(e => e.Nombre);
            entity.HasIndex(e => e.LegacyProductId).IsUnique().HasFilter("[legacy_product_id] IS NOT NULL");
        });

        // Configuración de la entidad Finance_Gastos_Viaticos (comentada para evitar conflicto)
        // modelBuilder.Entity<Finance_Gastos_Viaticos>(entity =>
        // {
        //     entity.HasIndex(e => new { e.CordenId, e.Fecha });
        //     entity.HasIndex(e => e.Fecha);
        // });

        // Configuración de la entidad Evaluacion (evaluaciones)
        modelBuilder.Entity<Evaluacion>(entity =>
        {
            entity.HasIndex(e => new { e.OrdenId, e.CreadoEn });
            entity.HasIndex(e => e.EvaluadorId);
            entity.HasIndex(e => e.ClienteId).HasFilter("[cliente_id] IS NOT NULL");
        });

        // Configuración de la entidad EvaluacionDetalle (evaluaciones_detalles)
        modelBuilder.Entity<EvaluacionDetalle>(entity =>
        {
            entity.HasIndex(e => e.EvaluacionId);
            entity.HasIndex(e => new { e.EvaluacionId, e.Fase });
        });

        // Configuración de la entidad EvaluacionFoto (evaluaciones_fotos)
        modelBuilder.Entity<EvaluacionFoto>(entity =>
        {
            entity.HasIndex(e => e.DetalleId);
            entity.HasIndex(e => e.DocumentoId);
        });

        // Configuración de la entidad Reparacion (reparaciones)
        modelBuilder.Entity<Reparacion>(entity =>
        {
            entity.HasIndex(e => e.OrdenId);
            entity.HasIndex(e => e.TecnicoId);
            entity.HasIndex(e => new { e.Resultado, e.FechaLlegada });
            entity.HasIndex(e => e.FechaLlegada);
        });

        // Configuración de la entidad ReparacionComponente (reparacion_componentes)
        modelBuilder.Entity<ReparacionComponente>(entity =>
        {
            entity.HasIndex(e => e.ReparacionId);
            entity.HasIndex(e => e.Componente);
        });

        // Configuración de la entidad ReparacionFoto (reparacion_fotos)
        modelBuilder.Entity<ReparacionFoto>(entity =>
        {
            entity.HasIndex(e => e.ReparacionId);
            entity.HasIndex(e => e.DocumentoId);
            entity.HasIndex(e => new { e.ReparacionId, e.Etapa });
        });

        // Configuración de la entidad EjecucionOrden (ops_ejecuciones_orden)
        modelBuilder.Entity<EjecucionOrden>(entity =>
        {
            entity.ToTable("ops_ejecuciones_orden");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrdenId).HasColumnName("orden_id").IsRequired();
            entity.Property(e => e.TipoEjecucion).HasColumnName("tipo_ejecucion").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id").IsRequired();
            entity.Property(e => e.HrInicio).HasColumnName("hr_inicio").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.HrFin).HasColumnName("hr_fin").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.Comentarios).HasColumnName("comentarios").HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.VehiculoId).HasColumnName("vehiculo_id");
            entity.Property(e => e.KmInicial).HasColumnName("km_inicial");
            entity.Property(e => e.KmFinal).HasColumnName("km_final");
            entity.Property(e => e.Herramientas).HasColumnName("herramientas").HasMaxLength(100);
            entity.Property(e => e.CodigoSesion).HasColumnName("codigo_sesion").HasMaxLength(100);
            entity.Property(e => e.ContrasenaSesion).HasColumnName("contrasena_sesion").HasMaxLength(100);

            // Relaciones con Foreign Keys
            entity.HasOne(e => e.Orden)
                .WithMany()
                .HasForeignKey(e => e.OrdenId)
                .HasConstraintName("FK_ejecuciones_orden_orden")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tecnico)
                .WithMany()
                .HasForeignKey(e => e.TecnicoId)
                .HasConstraintName("FK_ejecuciones_orden_tecnico")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vehiculo)
                .WithMany()
                .HasForeignKey(e => e.VehiculoId)
                .HasConstraintName("FK_ejecuciones_orden_vehiculo")
                .OnDelete(DeleteBehavior.SetNull);

            // Índices para optimización
            entity.HasIndex(e => e.OrdenId).HasDatabaseName("IX_ejecuciones_orden_orden");
            entity.HasIndex(e => e.TecnicoId).HasDatabaseName("IX_ejecuciones_orden_tecnico");
            entity.HasIndex(e => new { e.OrdenId, e.TipoEjecucion, e.HrInicio }).HasDatabaseName("IX_ejecuciones_orden_tipo");
            entity.HasIndex(e => e.HrInicio).HasDatabaseName("IX_ejecuciones_orden_hr_inicio");
        });

        // Configuración de la entidad FilesDocumento (files_documentos)
        modelBuilder.Entity<FilesDocumento>(entity =>
        {
            entity.HasIndex(e => e.CreadoPorUsuarioId);
            entity.HasIndex(e => new { e.EntidadTipo, e.EntidadId });
            entity.HasIndex(e => e.RutaAlmacenamiento).IsUnique();
            entity.HasIndex(e => e.NombreArchivo);
        });



        // Configuración para GastoViatico (finance_gastos_viaticos)
        modelBuilder.Entity<GastoViatico>(entity =>
        {
            entity.ToTable("finance_gastos_viaticos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrdenId).HasColumnName("orden_id");
            entity.Property(e => e.TieneFactura).HasColumnName("tiene_factura");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.ProveedorNombre).HasColumnName("proveedor_nombre");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.KmRecorridos).HasColumnName("km_recorridos");
            entity.Property(e => e.Gastos).HasColumnName("gastos");
            entity.Property(e => e.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(12,2)");
            entity.Property(e => e.LugarDestino).HasColumnName("lugar_destino");

            // Índices
            entity.HasIndex(e => e.OrdenId);
            entity.HasIndex(e => e.Fecha);
        });

    }
}