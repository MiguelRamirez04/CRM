// =====================================================================================
// ENTIDAD CATALOG CONCEPTOS - Concepto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de conceptos del catálogo local.
// Representa tipos específicos de operaciones dentro de los documentos modelo.
//
// PROPÓSITO:
// - Definir conceptos de operación (venta, compra, traspaso, etc.)
// - Configurar comportamiento de folios y creación de clientes
// - Mantener referencia con sistema legacy y documento modelo
//
// RELACIONES:
// - Muchos a Uno con legacy.conceptos_ref (FK: LegacyConceptoId)
// - Muchos a Uno con legacy.documentos_modelo_ref (FK: LegacyDocumentoModeloId)
// - Uno a Muchos con catalog.documentos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de conceptos
    /// Define operaciones específicas dentro de documentos modelo
    /// </summary>
    [Table("conceptos", Schema = "catalog")]
    public class Concepto
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
        /// ID del concepto en el sistema legacy
        /// </summary>
        [Column("legacy_concepto_id")]
        public int? LegacyConceptoId { get; set; }

        /// <summary>
        /// ID del documento modelo asociado en el sistema legacy
        /// </summary>
        [Column("legacy_documento_modelo_id")]
        public int? LegacyDocumentoModeloId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Código único del concepto
        /// </summary>
        [Column("codigo_concepto")]
        [MaxLength(30)]
        public string? CodigoConcepto { get; set; }

        /// <summary>
        /// Nombre descriptivo del concepto
        /// </summary>
        [Column("nombre_concepto")]
        [MaxLength(60)]
        public string? NombreConcepto { get; set; }

        /// <summary>
        /// Naturaleza del concepto
        /// 1 = Cargo, 2 = Abono
        /// </summary>
        [Column("naturaleza")]
        public int? Naturaleza { get; set; }

        /// <summary>
        /// Tipo de folio
        /// 1 = Alfanumérico, 2 = Numérico, etc.
        /// </summary>
        [Column("tipo_folio")]
        public int? TipoFolio { get; set; }

        /// <summary>
        /// Indica si el concepto crea automáticamente un cliente
        /// 0 = No crea, 1 = Sí crea
        /// </summary>
        [Column("crea_cliente")]
        public int? CreaCliente { get; set; }

        /// <summary>
        /// Indica si el concepto está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al concepto en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyConceptoId))]
        public virtual ConceptosRef? ConceptosRef { get; set; }

        /// <summary>
        /// Referencia al documento modelo en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoModeloId))]
        public virtual DocumentosModeloRef? DocumentosModeloRef { get; set; }

        /// <summary>
        /// Colección de documentos asociados a este concepto
        /// </summary>
        // Pendiente: public virtual ICollection<Documento> Documentos { get; set; }
    }
}
