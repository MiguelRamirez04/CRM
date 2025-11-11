// =====================================================================================
// ENTIDAD CATALOG DOCUMENTOS - Documento.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de documentos del catálogo local con datos completos.
// Representa documentos comerciales (facturas, pedidos, remisiones, etc.)
//
// PROPÓSITO:
// - Gestionar documentos comerciales del sistema
// - Registro de transacciones con clientes
// - Control de saldos y afectaciones contables
// - Mantener referencia con sistema legacy
//
// RELACIONES:
// - Muchos a Uno con legacy.documentos_ref (FK: LegacyDocumentoId)
// - Muchos a Uno con legacy.documentos_modelo_ref (FK: LegacyDocumentoModeloId)
// - Muchos a Uno con legacy.conceptos_ref (FK: LegacyConceptoId)
// - Muchos a Uno con legacy.clientes_ref (FK: LegacyClienteId)
// - Muchos a Uno con legacy.agentes_ref (FK: LegacyAgenteId)
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de documentos comerciales
    /// Representa facturas, pedidos, remisiones y otros documentos
    /// </summary>
    [Table("documentos", Schema = "catalog")]
    public class Documento
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
        /// ID del documento en el sistema legacy
        /// </summary>
        [Column("legacy_documento_id")]
        public int? LegacyDocumentoId { get; set; }

        /// <summary>
        /// ID del documento modelo asociado en el sistema legacy
        /// </summary>
        [Column("legacy_documento_modelo_id")]
        public int? LegacyDocumentoModeloId { get; set; }

        /// <summary>
        /// ID del concepto asociado en el sistema legacy
        /// </summary>
        [Column("legacy_concepto_id")]
        public int? LegacyConceptoId { get; set; }

        /// <summary>
        /// ID del cliente asociado en el sistema legacy
        /// </summary>
        [Column("legacy_cliente_id")]
        public long? LegacyClienteId { get; set; }

        /// <summary>
        /// ID del agente asociado en el sistema legacy
        /// </summary>
        [Column("legacy_agente_id")]
        public int? LegacyAgenteId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // INFORMACIÓN DEL DOCUMENTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Serie del documento (ej: "A", "B", "F001")
        /// </summary>
        [Column("serie_documento")]
        [MaxLength(11)]
        public string? SerieDocumento { get; set; }

        /// <summary>
        /// Número de folio del documento
        /// </summary>
        [Column("folio")]
        public double? Folio { get; set; }

        /// <summary>
        /// Fecha de emisión del documento
        /// </summary>
        [Column("fecha")]
        public DateTime? Fecha { get; set; }

        /// <summary>
        /// Razón social del cliente en el documento
        /// </summary>
        [Column("razon_social")]
        [MaxLength(60)]
        public string? RazonSocial { get; set; }

        /// <summary>
        /// RFC del cliente en el documento
        /// </summary>
        [Column("rfc")]
        [MaxLength(20)]
        public string? Rfc { get; set; }

        /// <summary>
        /// Fecha de vencimiento del documento (para crédito)
        /// </summary>
        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Fecha de entrega o recepción física
        /// </summary>
        [Column("fecha_entrega_recepcion")]
        public DateTime? FechaEntregaRecepcion { get; set; }

        /// <summary>
        /// Referencia o número de orden del cliente
        /// </summary>
        [Column("referencia")]
        [MaxLength(20)]
        public string? Referencia { get; set; }

        /// <summary>
        /// Observaciones generales del documento
        /// </summary>
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE CONTROL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Naturaleza del documento (1=Cargo, 2=Abono)
        /// </summary>
        [Column("naturaleza")]
        public int? Naturaleza { get; set; }

        /// <summary>
        /// ID del documento origen (en caso de ser derivado)
        /// </summary>
        [Column("documento_origen_id")]
        public int? DocumentoOrigenId { get; set; }

        /// <summary>
        /// Indica si usa cliente (0=No, 1=Sí)
        /// </summary>
        [Column("usa_cliente")]
        public int? UsaCliente { get; set; }

        /// <summary>
        /// Indica si ya fue afectado contablemente
        /// </summary>
        [Column("afectado")]
        public int? Afectado { get; set; }

        /// <summary>
        /// Indica si el documento fue impreso
        /// </summary>
        [Column("impreso")]
        public int? Impreso { get; set; }

        /// <summary>
        /// Indica si el documento está cancelado
        /// </summary>
        [Column("cancelado")]
        public int? Cancelado { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // IMPORTES Y CÁLCULOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Importe neto (subtotal sin impuestos)
        /// </summary>
        [Column("neto")]
        [Precision(18, 4)]
        public decimal? Neto { get; set; }

        /// <summary>
        /// Importe de impuesto 1 (generalmente IVA)
        /// </summary>
        [Column("impuesto1")]
        [Precision(18, 4)]
        public decimal? Impuesto1 { get; set; }

        /// <summary>
        /// Descuento aplicado al documento
        /// </summary>
        [Column("descuento_mov")]
        [Precision(18, 4)]
        public decimal? DescuentoMov { get; set; }

        /// <summary>
        /// Importe total del documento
        /// </summary>
        [Column("total")]
        [Precision(18, 4)]
        public decimal? Total { get; set; }

        /// <summary>
        /// Saldo pendiente del documento
        /// </summary>
        [Column("pendiente")]
        [Precision(18, 4)]
        public decimal? Pendiente { get; set; }

        /// <summary>
        /// Total de unidades en el documento
        /// </summary>
        [Column("total_unidades")]
        [Precision(18, 4)]
        public decimal? TotalUnidades { get; set; }

        /// <summary>
        /// Indica si el documento está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al documento en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoId))]
        public virtual DocumentosRef? DocumentosRef { get; set; }

        /// <summary>
        /// Referencia al documento modelo en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoModeloId))]
        public virtual DocumentosModeloRef? DocumentosModeloRef { get; set; }

        /// <summary>
        /// Referencia al concepto en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyConceptoId))]
        public virtual ConceptosRef? ConceptosRef { get; set; }

        /// <summary>
        /// Referencia al cliente en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyClienteId))]
        public virtual ClientesRef? ClientesRef { get; set; }

        /// <summary>
        /// Referencia al agente en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyAgenteId))]
        public virtual AgentesRef? AgentesRef { get; set; }

        /// <summary>
        /// Colección de movimientos (partidas) de este documento
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
