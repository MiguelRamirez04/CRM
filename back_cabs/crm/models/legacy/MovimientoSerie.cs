// =====================================================================================
// ENTIDAD CATALOG MOVIMIENTOS SERIE - MovimientoSerie.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de movimientos de serie del catálogo local.
// Representa la relación entre movimientos de documento y números de serie específicos.
//
// PROPÓSITO:
// - Rastrear qué números de serie se movieron en cada partida
// - Control de trazabilidad de productos serializados
// - Historial de movimientos por número de serie
//
// RELACIONES:
// - Muchos a Uno con legacy.movimientos_ref (FK: LegacyMovimientoId)
// - Muchos a Uno con legacy.numeros_serie_ref (FK: LegacySerieId)
//
// USO:
// - Ventas de productos con número de serie
// - Traspasos de equipos entre almacenes
// - Rastreo de garantías y servicios
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de relación entre movimientos y números de serie
    /// Permite rastrear productos específicos en transacciones
    /// </summary>
    [Table("movimientos_serie", Schema = "catalog")]
    public class MovimientoSerie
    {
        // ═══════════════════════════════════════════════════════════════
        // CLAVE PRIMARIA
        // ═══════════════════════════════════════════════════════════════

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLAVES FORÁNEAS LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del movimiento asociado en el sistema legacy
        /// </summary>
        [Column("legacy_movimiento_id")]
        public int? LegacyMovimientoId { get; set; }

        /// <summary>
        /// ID del número de serie asociado en el sistema legacy
        /// </summary>
        [Column("legacy_serie_id")]
        public int? LegacySerieId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Fecha del movimiento de serie
        /// Generalmente coincide con la fecha del movimiento padre
        /// </summary>
        [Column("fecha")]
        public DateTime? Fecha { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al movimiento en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyMovimientoId))]
        public virtual MovimientosRef? MovimientosRef { get; set; }

        /// <summary>
        /// Referencia al número de serie en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacySerieId))]
        public virtual NumerosSerieRef? NumerosSerieRef { get; set; }
    }
}
