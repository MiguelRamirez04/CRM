using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.models.Recepcion;
using back_cabs.CRM.models.Sales;
using back_cabs.CRM.models.Administracion;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.models;
<<<<<<< .merge_file_Jq0WSv
using back_cabs.CRM.models.legacy;
=======
>>>>>>> .merge_file_lizUYh
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
    /// Tokens de recuperación de contraseña
    /// </summary>
    public DbSet<RecuperacionPasswordToken> RecuperacionPasswordTokens { get; set; } = null!;

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
    /// Gastos de viáticos (comentado para evitar conflicto con GastoViatico)
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
    /// Gastos de viáticos
    /// </summary>
    public DbSet<GastoViatico> GastosViaticos { get; set; } = null!;

    /// <summary>
    /// Notificaciones del sistema
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

        // Configuración de la entidad OrdenTrabajo (ops_ordenes_trabajo)
        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.ToTable("ops_ordenes_trabajo");
            entity.HasKey(e => e.Id);
            
            // Configurar columnas con mapeo exacto a SQL Server
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.NuevoCliente).HasColumnName("nuevo_cliente");
            entity.Property(e => e.NombreCliente).HasColumnName("nombre_cliente").HasMaxLength(120);
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id").IsRequired(false); // Permite NULL para clientes nuevos
            entity.Property(e => e.CreadoPorUserId).HasColumnName("creado_por_user_id").IsRequired();
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

        // Configuración de la entidad Cotizacion (sales_cotizaciones)
        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.ToTable("sales_cotizaciones");
            entity.HasKey(e => e.Id);
            
            // Configurar columna calculada PERSISTED en BD
            entity.Property(e => e.Total)
                .HasComputedColumnSql("[subtotal]+[impuestos_total]", stored: true);
            
            // Configurar columnas decimales
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.ImpuestosTotal).HasColumnName("impuestos_total").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.Descuento).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CostoCapacitacion).HasColumnName("costo_capacitacion").HasColumnType("decimal(12,2)");
            
            // Configurar campos de texto
            entity.Property(e => e.Estado).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired().HasDefaultValue("NUEVA");
            entity.Property(e => e.Observaciones).HasColumnType("varchar(500)").HasMaxLength(500);
            entity.Property(e => e.Cliente).HasColumnType("varchar(255)").HasMaxLength(255);
            entity.Property(e => e.Rfc).HasColumnType("varchar(13)").HasMaxLength(13);
            entity.Property(e => e.Folio).HasColumnType("varchar(50)").HasMaxLength(50);
            entity.Property(e => e.DescripcionServicio).HasColumnName("descrpcion_servicio").HasColumnType("varchar(1000)").HasMaxLength(1000);
            
            // Configurar fechas
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasColumnType("datetime2(0)").IsRequired();
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasColumnType("datetime2(0)");
            
            // Configurar campos de capacitación
            entity.Property(e => e.HorasCapacitacion).HasColumnName("horas_capacitacion");
            entity.Property(e => e.PaquetesCapacitacion).HasColumnName("paquetes_capacitacion");
            
            // Configurar campos de contacto
            entity.Property(e => e.Telefono).HasColumnType("bigint");
            entity.Property(e => e.Correo).HasColumnType("varchar(150)").HasMaxLength(150);
            
            // Índices para optimización
            entity.HasIndex(e => e.OrdenId).HasDatabaseName("IX_cotizaciones_orden");
            entity.HasIndex(e => e.Estado).HasDatabaseName("IX_cotizaciones_estado");
            entity.HasIndex(e => e.CreadoEn).HasDatabaseName("IX_cotizaciones_fecha");
            entity.HasIndex(e => e.Cliente).HasDatabaseName("IX_cotizaciones_cliente");
        });

        // Configuración de la entidad Vehiculo (fleet_vehiculos)
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.TipoVehiculo).HasColumnName("tipo_vehiculo").HasMaxLength(50);
            entity.Property(e => e.Transmision).HasColumnName("transmision").HasMaxLength(20);
            entity.Property(e => e.EsDeEmpresa).HasColumnName("es_de_empresa").IsRequired().HasDefaultValue(true);
            entity.Property(e => e.Placas).HasColumnName("placas").HasMaxLength(20);
            entity.Property(e => e.Activo).HasColumnName("activo").IsRequired().HasDefaultValue(true);
            entity.Property(e => e.Observaciones).HasColumnName("observaciones").HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.NombreVehiculo).HasColumnName("nombre_vehiculo").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Kilometraje).HasColumnName("kilometraje");

            entity.HasIndex(e => e.Placas).IsUnique().HasFilter("[placas] IS NOT NULL");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.TipoVehiculo).HasColumnName("tipo_vehiculo").HasMaxLength(50);
            entity.Property(e => e.EsDeEmpresa).HasColumnName("es_de_empresa").IsRequired(true);
            entity.Property(e => e.Transmision).HasColumnName("transmision").HasMaxLength(20);
            entity.Property(e => e.Placas).HasColumnName("placas").HasMaxLength(20);
            entity.Property(e => e.Activo).HasColumnName("activo").IsRequired(true);
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");

            // ✅ IMPORTANTE: Deshabilitar OUTPUT clause porque la tabla tiene triggers
            // Esto es necesario para que EF Core funcione con triggers de auditoría
            entity.ToTable(tb => tb.UseSqlOutputClause(false));
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
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.OrdenId, e.CreadoEn });
            entity.HasIndex(e => e.EvaluadorId);
            entity.HasIndex(e => e.ClienteId).HasFilter("[cliente_id] IS NOT NULL");

            //DJ aqui vas weon no se te olvide
            entity.Property(e => e.Objetivo).HasMaxLength(200);
            entity.Property(e => e.ComentariosGenerales).HasColumnName("comentarios_generales");
            entity.Property(e => e.ScoreCalidadTotal).HasColumnName("score_calidad_total");
            entity.Property(e => e.RequiereSeguimiento).HasColumnName("requiere_seguimiento").IsRequired(true);
            entity.Property(e => e.SeguimientoNotas).HasColumnName("seguimiento_notas");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasColumnType("DATETIME2(0)").IsRequired().HasDefaultValueSql("GETUTCDATE()");
            
        });

        // Configuración de la entidad EvaluacionDetalle (evaluaciones_detalles)
        modelBuilder.Entity<EvaluacionDetalle>(entity =>
        {
            entity.ToTable("evaluacion_detalles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.EvaluacionId).HasColumnName("evaluacion_id").IsRequired();
            entity.Property(e => e.Fase).HasColumnName("fase").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Sugerencias).HasColumnName("sugerencias");
            entity.Property(e => e.ScoreFase).HasColumnName("score_fase");
            entity.Property(e => e.EvidenciaNota).HasColumnName("evidencia_nota");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasColumnType("DATETIME2(0)").IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Lugar).HasColumnName("lugar").IsRequired();

            //Llaves foraneas de la tabla
            entity.HasOne(e => e.Evaluacion)
                .WithMany()
                .HasForeignKey(e => e.EvaluacionId)
                .HasConstraintName("FK_eval_detalle_eval")
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para optimización
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
            entity.ToTable("reparaciones", "dbo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrdenId).HasColumnName("orden_id").IsRequired();
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id").IsRequired();
            entity.Property(e => e.DispositivoTipo).HasColumnName("dispositivo_tipo").IsRequired();
            entity.Property(e => e.Marca).HasColumnName("marca");
            entity.Property(e => e.Modelo).HasColumnName("modelo");
            entity.Property(e => e.AccesoriosRecibidos).HasColumnName("accesorios_recibidos");
            entity.Property(e => e.AccesoriosRecibidos).HasColumnName("accesorios_recibidos");
            entity.Property(e => e.DescripcionFalla).HasColumnName("descripcion_falla").IsRequired();
            entity.Property(e => e.Diagnostico).HasColumnName("diagnostico");
            entity.Property(e => e.SolucionAplicada).HasColumnName("solucion_aplicada");
            entity.Property(e => e.Resultado).HasColumnName("resultado").IsRequired();
            entity.Property(e => e.CausaIrreparable).HasColumnName("causa_irreparable");
            entity.Property(e => e.RespaldoDatosAutorizado).HasColumnName("respaldo_datos_autorizado").IsRequired();
            entity.Property(e => e.CostoManoObra).HasColumnName("costo_mano_obra").HasColumnType("DECIMAL(12,2)").IsRequired();
            entity.Property(e => e.CostoRefaccionesCompra).HasColumnName("costo_refacciones_compra").HasColumnType("DECIMAL(12,2)").IsRequired();
            entity.Property(e => e.CostoRefaccionesPublico).HasColumnName("costo_refacciones_publico").HasColumnType("DECIMAL(12,2)").IsRequired();
            entity.Property(e => e.CostoTotalCompra).HasColumnName("costo_total_compra").ValueGeneratedOnAddOrUpdate().HasComputedColumnSql("[costo_mano_obra] + [costo_refacciones_compra]").HasColumnType("DECIMAL(12,2)").IsRequired();
            entity.Property(e => e.CostoTotalPublico).HasColumnName("costo_total_publico").ValueGeneratedOnAddOrUpdate().HasComputedColumnSql("[costo_mano_obra] + [costo_refacciones_publico]").HasColumnType("DECIMAL(12,2)").IsRequired();
            entity.Property(e => e.MargenEstimado).HasColumnName("margen_estimado").ValueGeneratedOnAddOrUpdate().HasComputedColumnSql("[costo_refacciones_publico] - [costo_refacciones_compra]").HasColumnType("DECIMAL(5,2)");
            entity.Property(e => e.GarantiaDias).HasColumnName("garantia_dias");
            entity.Property(e => e.FechaLlegada).HasColumnName("fecha_llegada").HasColumnType("DATETIME2(0)").IsRequired();
            entity.Property(e => e.EmpezadoEn).HasColumnName("empezado_en").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.EntregadoEn).HasColumnName("entregado_en").HasColumnType("DATETIME2(0)");
            entity.Property(e => e.TipoEntrega).HasColumnName("tipo_entrega").IsRequired();
            entity.Property(e => e.UbicacionAlmacenamiento).HasColumnName("ubicacion_almacenamiento");
            entity.Property(e => e.Notas).HasColumnName("notas");

            //Llaves foraneas de la tabla
            /*
            entity.HasOne(e => e.Orden)
                .WithMany()
                .HasForeignKey(e => e.OrdenId)
                .HasConstraintName("FK_rep_orden")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tecnico)
                .WithMany()
                .HasForeignKey(e => e.TecnicoId)
                .HasConstraintName("FK_rep_tecnico")
                .OnDelete(DeleteBehavior.Cascade);
            */
            // Índices para optimización
            entity.HasIndex(e => e.OrdenId);
            entity.HasIndex(e => e.TecnicoId);
            entity.HasIndex(e => new { e.Resultado, e.FechaLlegada });
            entity.HasIndex(e => e.FechaLlegada);
        });

        // Configuración de la entidad ReparacionComponente (reparacion_componentes)
        modelBuilder.Entity<ReparacionComponente>(entity =>
        {
            entity.ToTable("reparacion_componentes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().IsRequired();
            entity.Property(e => e.ReparacionId).HasColumnName("reparacion_id").IsRequired();
            entity.Property(e => e.Componente).HasColumnName("componente").IsRequired();
            entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
            entity.Property(e => e.Proveedor).HasColumnName("proveedor").HasMaxLength(160);
            entity.Property(e => e.GarantiaMeses).HasColumnName("garantia_meses").IsRequired();
            entity.Property(e => e.CostoUnitarioCompra).HasColumnName("costo_unitario_compra").HasColumnType("DECIMAL(12,2)");
            entity.Property(e => e.CostoUnitarioPublico).HasColumnName("costo_unitario_publico").HasColumnType("DECIMAL(12,2)");
            entity.Property(e => e.SubtotalCompra).HasColumnName("subtotal_compra").HasColumnType("DECIMAL(13,2)").ValueGeneratedOnAddOrUpdate().HasComputedColumnSql("[cantidad] * [costo_unitario_compra]").IsRequired();
            entity.Property(e => e.SubtotalPublico).HasColumnName("subtotal_publico").HasColumnType("DECIMAL(13,2)").ValueGeneratedOnAddOrUpdate().HasComputedColumnSql("[cantidad] * [costo_unitario_publico]").IsRequired();
            entity.Property(e => e.Notas).HasColumnName("notas").HasMaxLength(500);



            entity.HasIndex(e => e.ReparacionId);
            entity.HasIndex(e => e.Componente);
        });

        // Configuración de la entidad ReparacionFoto (reparacion_fotos)
        modelBuilder.Entity<ReparacionFoto>(entity =>
        {
            entity.ToTable("reparacion_fotos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd().IsRequired();
            entity.Property(e => e.ReparacionId).HasColumnName("reparacion_id").IsRequired();
            entity.Property(e => e.DocumentoId).HasColumnName("documento_id").IsRequired();
            entity.Property(e => e.Etapa).HasColumnName("etapa");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasColumnType("DATETIME2(0)").IsRequired().HasDefaultValueSql("GETUTCDATE()");



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