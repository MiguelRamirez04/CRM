// =====================================================================================
// ENTIDAD LEGACY DOCUMENTOS REFERENCIA - DocumentosRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para documentos del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.documentos
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.documentos
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para documentos del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("documentos_ref", Schema = "legacy")]
    public class DocumentosRef
    {
        /// <summary>
        /// ID del documento en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admDocumentos.CIDDOCUMENTO del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_documento_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyDocumentoId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de documentos locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Documento> Documentos { get; set; }

        /// <summary>
        /// Colección de movimientos asociados a este documento legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
