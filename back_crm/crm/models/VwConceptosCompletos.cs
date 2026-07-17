// =====================================================================================
// VISTA CONCEPTOS COMPLETOS - VwConceptosCompletos.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Entidad que mapea la vista VwConceptosCompletos.
// Une datos de catalog.conceptos con legacy.conceptos_ref y COMPAC.admConceptos.
//
// CAMPOS:
// - Locales: ConceptoId, CodigoConcepto, NombreConcepto, Naturaleza, TipoFolio, CreaCliente, Activo
// - Legacy: LegacyConceptoId, LegacyDocumentoModeloId, DocumentoModeloLegacy, CodigoLegacy, NombreLegacy, NaturalezaLegacy, TipoFolioLegacy
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models
{
    /// <summary>
    /// Vista que combina datos de conceptos locales y legacy
    /// Une catalog.conceptos con legacy.conceptos_ref y COMPAC.admConceptos
    /// </summary>
    [Table("VwConceptosCompletos")]
    public class VwConceptosCompletos
    {
        // ═══════════════════════════════════════════════════════════════
        // DATOS LOCALES (catalog.conceptos)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del concepto en el sistema local
        /// </summary>
        [Key]
        [Column("ConceptoId")]
        public int ConceptoId { get; set; }

        /// <summary>
        /// Código único del concepto
        /// </summary>
        [Column("CodigoConcepto")]
        public string? CodigoConcepto { get; set; }

        /// <summary>
        /// Nombre descriptivo del concepto
        /// </summary>
        [Column("NombreConcepto")]
        public string? NombreConcepto { get; set; }

        /// <summary>
        /// Naturaleza del concepto
        /// 1 = Cargo, 2 = Abono
        /// </summary>
        [Column("Naturaleza")]
        public int? Naturaleza { get; set; }

        /// <summary>
        /// Tipo de folio
        /// 1 = Alfanumérico, 2 = Numérico, etc.
        /// </summary>
        [Column("TipoFolio")]
        public int? TipoFolio { get; set; }

        /// <summary>
        /// Indica si el concepto crea automáticamente un cliente
        /// 0 = No crea, 1 = Sí crea
        /// </summary>
        [Column("CreaCliente")]
        public int? CreaCliente { get; set; }

        /// <summary>
        /// Indica si el concepto está activo
        /// </summary>
        [Column("Activo")]
        public bool Activo { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS LEGACY (legacy.conceptos_ref + COMPAC)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del concepto en el sistema legacy
        /// </summary>
        [Column("LegacyConceptoId")]
        public int? LegacyConceptoId { get; set; }

        /// <summary>
        /// ID del documento modelo asociado en el sistema legacy
        /// </summary>
        [Column("LegacyDocumentoModeloId")]
        public int? LegacyDocumentoModeloId { get; set; }

        /// <summary>
        /// Documento modelo legacy (de COMPAC)
        /// </summary>
        [Column("DocumentoModeloLegacy")]
        public int? DocumentoModeloLegacy { get; set; }

        /// <summary>
        /// Código del concepto en sistema legacy
        /// </summary>
        [Column("CodigoLegacy")]
        public string? CodigoLegacy { get; set; }

        /// <summary>
        /// Nombre del concepto en sistema legacy
        /// </summary>
        [Column("NombreLegacy")]
        public string? NombreLegacy { get; set; }

        /// <summary>
        /// Naturaleza del concepto en sistema legacy
        /// </summary>
        [Column("NaturalezaLegacy")]
        public int? NaturalezaLegacy { get; set; }

        /// <summary>
        /// Tipo de folio en sistema legacy
        /// </summary>
        [Column("TipoFolioLegacy")]
        public int? TipoFolioLegacy { get; set; }
    }
}
