// =====================================================================================
// ENTIDAD LEGACY NÚMEROS SERIE REFERENCIA - NumerosSerieRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para números de serie del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.numeros_serie
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.numeros_serie
// - Uno a Muchos con catalog.movimientos_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para números de serie del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("numeros_serie_ref", Schema = "legacy")]
    public class NumerosSerieRef
    {
        /// <summary>
        /// ID del número de serie en el sistema legacy (Primary Key)
        /// Mantiene sincronización con sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_serie_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacySerieId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de números de serie locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<NumeroSerie> NumerosSerie { get; set; }

        /// <summary>
        /// Colección de movimientos de serie asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<MovimientoSerie> MovimientosSerie { get; set; }
    }
}
