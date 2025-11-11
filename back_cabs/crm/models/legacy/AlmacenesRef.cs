// =====================================================================================
// ENTIDAD LEGACY ALMACENES REFERENCIA - AlmacenesRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para almacenes del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.almacenes
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.almacenes
// - Uno a Muchos con catalog.numeros_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para almacenes del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("almacenes_ref", Schema = "legacy")]
    public class AlmacenesRef
    {
        /// <summary>
        /// ID del almacén en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admAlmacenes.CIDALMACEN del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_almacen_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyAlmacenId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de almacenes locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Almacen> Almacenes { get; set; }

        /// <summary>
        /// Colección de números de serie asociados a este almacén legacy
        /// </summary>
        // Pendiente: public virtual ICollection<NumeroSerie> NumerosSerie { get; set; }
    }
}
