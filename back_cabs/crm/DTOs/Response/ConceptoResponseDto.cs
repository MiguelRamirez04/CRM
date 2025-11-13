// =====================================================================================
// CONCEPTO RESPONSE DTO - ConceptoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO de respuesta para conceptos basado en la vista VwConceptosCompletos.
// Mapea datos de catálogo local y referencias legacy.
//
// PROPIEDADES:
// - Datos locales: Id, CodigoConcepto, NombreConcepto, Naturaleza, TipoFolio, CreaCliente, Activo
// - Datos legacy: LegacyConceptoId, LegacyDocumentoModeloId, DocumentoModeloLegacy, CodigoLegacy, NombreLegacy, NaturalezaLegacy, TipoFolioLegacy
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para conceptos
    /// Basado en la vista VwConceptosCompletos con datos locales y legacy
    /// </summary>
    public class ConceptoResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // DATOS LOCALES (catalog.conceptos)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del concepto en el sistema local
        /// </summary>
        [Required]
        public int ConceptoId { get; set; }

        /// <summary>
        /// Código único del concepto
        /// </summary>
        public string? CodigoConcepto { get; set; }

        /// <summary>
        /// Nombre descriptivo del concepto
        /// </summary>
        public string? NombreConcepto { get; set; }

        /// <summary>
        /// Naturaleza del concepto
        /// 1 = Cargo, 2 = Abono
        /// </summary>
        public int? Naturaleza { get; set; }

        /// <summary>
        /// Tipo de folio
        /// 1 = Alfanumérico, 2 = Numérico, etc.
        /// </summary>
        public int? TipoFolio { get; set; }

        /// <summary>
        /// Indica si el concepto crea automáticamente un cliente
        /// 0 = No crea, 1 = Sí crea
        /// </summary>
        public int? CreaCliente { get; set; }

        /// <summary>
        /// Indica si el concepto está activo
        /// </summary>
        [Required]
        public bool Activo { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS LEGACY (legacy.conceptos_ref + COMPAC)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del concepto en el sistema legacy
        /// </summary>
        public int? LegacyConceptoId { get; set; }

        /// <summary>
        /// ID del documento modelo asociado en el sistema legacy
        /// </summary>
        public int? LegacyDocumentoModeloId { get; set; }

        /// <summary>
        /// Documento modelo legacy (de COMPAC)
        /// </summary>
        public int? DocumentoModeloLegacy { get; set; }

        /// <summary>
        /// Código del concepto en sistema legacy
        /// </summary>
        public string? CodigoLegacy { get; set; }

        /// <summary>
        /// Nombre del concepto en sistema legacy
        /// </summary>
        public string? NombreLegacy { get; set; }

        /// <summary>
        /// Naturaleza del concepto en sistema legacy
        /// </summary>
        public int? NaturalezaLegacy { get; set; }

        /// <summary>
        /// Tipo de folio en sistema legacy
        /// </summary>
        public int? TipoFolioLegacy { get; set; }
    }
}
