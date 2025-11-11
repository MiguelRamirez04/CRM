// =====================================================================================
// ENTIDAD LEGACY DOCUMENTOS MODELO REFERENCIA - DocumentosModeloRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para documentos modelo del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.documentos_modelo
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.documentos_modelo
// - Uno a Muchos con catalog.conceptos
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para documentos modelo del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("documentos_modelo_ref", Schema = "legacy")]
    public class DocumentosModeloRef
    {
        /// <summary>
        /// ID del documento modelo en el sistema legacy (Primary Key)
        /// Mantiene sincronización con sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_documento_modelo_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyDocumentoModeloId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de documentos modelo locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<DocumentoModelo> DocumentosModelo { get; set; }

        /// <summary>
        /// Colección de conceptos asociados a este documento modelo legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Concepto> Conceptos { get; set; }

        /// <summary>
        /// Colección de movimientos asociados a este documento modelo legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
