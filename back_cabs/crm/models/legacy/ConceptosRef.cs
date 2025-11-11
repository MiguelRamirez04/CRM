// =====================================================================================
// ENTIDAD LEGACY CONCEPTOS REFERENCIA - ConceptosRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para conceptos del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.conceptos
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.conceptos
// - Uno a Muchos con catalog.documentos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para conceptos del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("conceptos_ref", Schema = "legacy")]
    public class ConceptosRef
    {
        /// <summary>
        /// ID del concepto en el sistema legacy (Primary Key)
        /// Mantiene sincronización con sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_concepto_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyConceptoId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de conceptos locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Concepto> Conceptos { get; set; }

        /// <summary>
        /// Colección de documentos asociados a este concepto legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Documento> Documentos { get; set; }
    }
}
