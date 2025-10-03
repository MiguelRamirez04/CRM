// =====================================================================================
// ENTIDAD ORDEN TRABAJO - OrdenTrabajo.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para órdenes de trabajo del módulo de Recepción.
// Representa la tabla ops.ordenes_trabajo en la base de datos SQL Server.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Gestión de órdenes de trabajo y asesorías
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Auth;

namespace back_cabs.CRM.models.Recepcion
{
    /// <summary>
    /// Entidad que representa una orden de trabajo
    /// Mapea a: ops.ordenes_trabajo en SQL Server
    /// </summary>
    [Table("ordenes_trabajo", Schema = "ops")]
    public class OrdenTrabajo
    {
        /// <summary>
        /// Identificador único de la orden (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ID del cliente asociado a la orden
        /// Mapea a: cliente_id (INT NOT NULL)
        /// </summary>
        [Required]
        [Column("cliente_id")]
        public int ClienteId { get; set; }

        /// <summary>
        /// ID del usuario que creó la orden
        /// Mapea a: creado_por_user_id (INT NOT NULL)
        /// </summary>
        [Required]
        [Column("creado_por_user_id")]
        public int CreadoPorUserId { get; set; }

        /// <summary>
        /// ID del usuario asignado a la orden
        /// Mapea a: asignada_a_user_id (INT NULL)
        /// </summary>
        [Column("asignada_a_user_id")]
        public int? AsignadaAUserId { get; set; }

        /// <summary>
        /// Notas o descripción detallada de la orden
        /// Mapea a: notas (NVARCHAR(MAX) NULL)
        /// </summary>
        [Column("notas", TypeName = "NVARCHAR(MAX)")]
        public string? Notas { get; set; }

        /// <summary>
        /// Fecha y hora de inicio de la cita programada
        /// Mapea a: cita_programada_inicio (DATETIME2(0) NULL)
        /// </summary>
        [Column("cita_programada_inicio", TypeName = "DATETIME2(0)")]
        public DateTime? CitaProgramadaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de fin de la cita programada
        /// Mapea a: cita_programada_fin (DATETIME2(0) NULL)
        /// </summary>
        [Column("cita_programada_fin", TypeName = "DATETIME2(0)")]
        public DateTime? CitaProgramadaFin { get; set; }

        /// <summary>
        /// Modalidad de la orden (ej: Presencial, Virtual, Remoto)
        /// Mapea a: modalidad (VARCHAR(50) NULL)
        /// </summary>
        [StringLength(50)]
        [Column("modalidad")]
        public string? Modalidad { get; set; }

        /// <summary>
        /// Tipo de orden (ej: Asesoria, Instalacion, Mantenimiento)
        /// Mapea a: tipo_orden (VARCHAR(50) NULL)
        /// </summary>
        [StringLength(50)]
        [Column("tipo_orden")]
        public string? TipoOrden { get; set; }

        /// <summary>
        /// Nivel de prioridad de la orden (1-5)
        /// Mapea a: prioridad (INT NULL)
        /// </summary>
        [Column("prioridad")]
        public int? Prioridad { get; set; }

        /// <summary>
        /// Estado de la orden
        /// Valores: CAPTURADA, ASIGNADA, EN_CURSO, COMPLETADA, POR_FACTURAR, FACTURADA, CERRADA
        /// Mapea a: estado (VARCHAR(20) NOT NULL)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "CAPTURADA";

        /// <summary>
        /// Ubicación en texto donde se realizará el servicio
        /// Mapea a: ubicacion_text (NVARCHAR(MAX) NULL)
        /// </summary>
        [Column("ubicacion_text", TypeName = "NVARCHAR(MAX)")]
        public string? UbicacionText { get; set; }

        /// <summary>
        /// Indica si la orden requiere factura
        /// Mapea a: requiere_factura (BIT NOT NULL DEFAULT 0)
        /// </summary>
        [Required]
        [Column("requiere_factura")]
        public bool RequiereFactura { get; set; } = false;

        /// <summary>
        /// Estado de facturación de la orden
        /// Mapea a: estado_facturado (VARCHAR(50) NULL)
        /// </summary>
        [StringLength(50)]
        [Column("estado_facturado")]
        public string? EstadoFacturado { get; set; }

        /// <summary>
        /// Folio de la factura (si ya fue facturado)
        /// Mapea a: factura_folio (VARCHAR(50) NULL)
        /// </summary>
        [StringLength(50)]
        [Column("factura_folio")]
        public string? FacturaFolio { get; set; }

        /// <summary>
        /// Fecha de creación de la orden
        /// Mapea a: creado_en (DATETIME2(0) NOT NULL DEFAULT GETDATE())
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// Mapea a: actualizado_en (DATETIME2(0) NULL)
        /// </summary>
        [Column("actualizado_en", TypeName = "DATETIME2(0)")]
        public DateTime? ActualizadoEn { get; set; }

        /// <summary>
        /// Costo real de la orden (después de realizada)
        /// Mapea a: costo_real (DECIMAL(12,2) NULL)
        /// </summary>
        [Column("costo_real", TypeName = "DECIMAL(12,2)")]
        public decimal? CostoReal { get; set; }

        /// <summary>
        /// Costo estimado de la orden (antes de realizarla)
        /// Mapea a: costo_estimado (DECIMAL(12,2) NULL)
        /// </summary>
        [Column("costo_estimado", TypeName = "DECIMAL(12,2)")]
        public decimal? CostoEstimado { get; set; }

        // Propiedades de navegación para Entity Framework
        
        /// <summary>
        /// Cliente asociado a esta orden
        /// </summary>
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        /// <summary>
        /// Usuario que creó la orden
        /// </summary>
        [ForeignKey("CreadoPorUserId")]
        public virtual UsuarioAuth? CreadoPor { get; set; }

        /// <summary>
        /// Usuario asignado a la orden
        /// </summary>
        [ForeignKey("AsignadaAUserId")]
        public virtual UsuarioAuth? AsignadaA { get; set; }

        /// <summary>
        /// Actualiza la fecha de modificación al momento actual
        /// </summary>
        public void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }
    }
}
