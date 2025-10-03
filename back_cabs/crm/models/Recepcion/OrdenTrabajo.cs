// =====================================================================================
// ENTIDAD ORDEN TRABAJO - OrdenTrabajo.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para órdenes de trabajo del módulo de Recepción.
// Representa la tabla ops_ordenes_trabajo en la base de datos con todas sus propiedades.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Gestión de órdenes de asesoría y cotizaciones
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Recepcion
{
    /// <summary>
    /// Entidad que representa una orden de trabajo (asesoría o cotización)
    /// </summary>
    [Table("ops_ordenes_trabajo")]
    public class OrdenTrabajo
    {
        /// <summary>
        /// Identificador único de la orden (autoincremental)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Notas o descripción detallada de la orden
        /// </summary>
        [Column(TypeName = "VARCHAR(MAX)")]
        public string? Notas { get; set; }

        /// <summary>
        /// Fecha y hora de inicio de la cita programada
        /// </summary>
        public DateTime? CitaProgramadaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de fin de la cita programada
        /// </summary>
        public DateTime? CitaProgramadaFin { get; set; }

        /// <summary>
        /// Modalidad de la orden (Presencial, Virtual)
        /// </summary>
        [StringLength(50)]
        public string? Modalidad { get; set; }

        /// <summary>
        /// Tipo de orden (Cotizacion, Asesoria)
        /// </summary>
        [StringLength(50)]
        public string? TipoOrden { get; set; }

        /// <summary>
        /// Referencia a cotizaciones relacionadas
        /// </summary>
        [StringLength(255)]
        public string? Cotizaciones { get; set; }

        /// <summary>
        /// ID del cliente legacy (sistema anterior)
        /// </summary>
        [Column("legacy_client_id")]
        public int? LegacyClientId { get; set; }

        /// <summary>
        /// Referencia al cliente en el sistema actual
        /// </summary>
        [Column("cliente_ref_id")]
        public int? ClienteRefId { get; set; }

        /// <summary>
        /// Nivel de prioridad de la orden (1-5)
        /// </summary>
        public int? Prioridad { get; set; }

        /// <summary>
        /// Estado de la orden (true = activa, false = inactiva)
        /// </summary>
        public bool? Estado { get; set; }

        /// <summary>
        /// Ubicación en texto de donde se realizará la asesoría
        /// </summary>
        [Column(TypeName = "VARCHAR(MAX)")]
        public string? UbicacionText { get; set; }

        /// <summary>
        /// Estado de facturación de la orden
        /// </summary>
        [StringLength(50)]
        public string? EstadoFacturado { get; set; }

        /// <summary>
        /// Indica si la orden requiere factura
        /// </summary>
        public bool? RequiereFactura { get; set; }

        /// <summary>
        /// Folio de la factura (si ya fue facturado)
        /// </summary>
        public int? FacturaFolio { get; set; }

        /// <summary>
        /// Fecha de creación de la orden
        /// </summary>
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [Column("actualizado_en")]
        public DateTime? ActualizadoEn { get; set; }

        /// <summary>
        /// Costo real de la orden (después de realizada)
        /// </summary>
        public int? CostoReal { get; set; }

        /// <summary>
        /// Costo estimado de la orden (antes de realizarla)
        /// </summary>
        public int? CostoEstimado { get; set; }

        /// <summary>
        /// ID del usuario (asesor) asignado a la orden
        /// </summary>
        [Column("id_usuario")]
        public int id_usuario { get; set; }

        // Propiedades de navegación para Entity Framework
        
        /// <summary>
        /// Cliente asociado a esta orden
        /// </summary>
        [ForeignKey("ClienteRefId")]
        public virtual Cliente? Cliente { get; set; }
    }
}
