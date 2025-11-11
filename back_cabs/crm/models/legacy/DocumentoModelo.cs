// =====================================================================================
// ENTIDAD CATALOG DOCUMENTOS MODELO - DocumentoModelo.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de documentos modelo del catálogo local.
// Representa plantillas o tipos de documentos (facturas, remisiones, pedidos, etc.)
//
// PROPÓSITO:
// - Definir tipos de documentos del sistema
// - Configurar comportamiento de afectación de existencias
// - Mantener referencia con sistema legacy
//
// RELACIONES:
// - Muchos a Uno con legacy.documentos_modelo_ref (FK: LegacyDocumentoModeloId)
// - Uno a Muchos con catalog.conceptos
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de documentos modelo
    /// Define tipos y plantillas de documentos del sistema
    /// </summary>
    [Table("documentos_modelo", Schema = "catalog")]
    public class DocumentoModelo
    {
        // ═══════════════════════════════════════════════════════════════
        // CLAVE PRIMARIA
        // ═══════════════════════════════════════════════════════════════

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLAVE FORÁNEA LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del documento modelo en el sistema legacy
        /// </summary>
        [Column("legacy_documento_modelo_id")]
        public int? LegacyDocumentoModeloId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Descripción del tipo de documento
        /// Ejemplo: "Factura", "Remisión", "Pedido", "Devolución"
        /// </summary>
        [Column("descripcion")]
        [MaxLength(50)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Naturaleza del documento
        /// 1 = Cargo (aumenta saldo)
        /// 2 = Abono (disminuye saldo)
        /// </summary>
        [Column("naturaleza")]
        public int? Naturaleza { get; set; }

        /// <summary>
        /// Indica cómo afecta la existencia
        /// 1 = Aumenta existencia
        /// 2 = Disminuye existencia
        /// 0 = No afecta
        /// </summary>
        [Column("afecta_existencia")]
        public int? AfectaExistencia { get; set; }

        /// <summary>
        /// Número de folio actual para este tipo de documento
        /// Se incrementa automáticamente al crear documentos
        /// </summary>
        [Column("no_folio")]
        public double? NoFolio { get; set; }

        /// <summary>
        /// ID del concepto asumido por defecto para este modelo
        /// </summary>
        [Column("concepto_asumido_id")]
        public int? ConceptoAsumidoId { get; set; }

        /// <summary>
        /// Indica si el documento modelo está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al documento modelo en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoModeloId))]
        public virtual DocumentosModeloRef? DocumentosModeloRef { get; set; }

        /// <summary>
        /// Colección de conceptos asociados a este documento modelo
        /// </summary>
        // Pendiente: public virtual ICollection<Concepto> Conceptos { get; set; }

        /// <summary>
        /// Colección de movimientos asociados a este documento modelo
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
