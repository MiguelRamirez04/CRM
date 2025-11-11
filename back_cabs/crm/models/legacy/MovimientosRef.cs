// =====================================================================================
// ENTIDAD LEGACY MOVIMIENTOS REFERENCIA - MovimientosRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para movimientos del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.movimientos
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.movimientos
// - Uno a Muchos con catalog.movimientos_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para movimientos del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("movimientos_ref", Schema = "legacy")]
    public class MovimientosRef
    {
        /// <summary>
        /// ID del movimiento en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admMovimientos.CIDMOVIMIENTO del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_movimiento_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyMovimientoId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de movimientos locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }

        /// <summary>
        /// Colección de movimientos de serie asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<MovimientoSerie> MovimientosSerie { get; set; }
    }
}
