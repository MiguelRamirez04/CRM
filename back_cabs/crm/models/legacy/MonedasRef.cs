// =====================================================================================
// ENTIDAD LEGACY MONEDAS REFERENCIA - MonedasRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para monedas del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.monedas
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.monedas (una referencia legacy puede tener múltiples registros locales)
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para monedas del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("monedas_ref", Schema = "legacy")]
    public class MonedasRef
    {
        /// <summary>
        /// ID de la moneda en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admMonedas.CIDMONEDA del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_moneda_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // No es autoincremental, viene del legacy
        public int LegacyMonedaId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de monedas locales asociadas a esta referencia legacy
        /// Relación: Una referencia legacy puede tener múltiples registros en catalog.monedas
        /// </summary>
        public virtual ICollection<Moneda> Monedas { get; set; } = new List<Moneda>();
    }
}
