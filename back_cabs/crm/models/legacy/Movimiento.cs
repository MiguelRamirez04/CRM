// =====================================================================================
// ENTIDAD CATALOG MOVIMIENTOS - Movimiento.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de movimientos del catálogo local con datos completos.
// Representa partidas o líneas de detalle dentro de documentos comerciales.
//
// PROPÓSITO:
// - Gestionar partidas de documentos (productos/servicios en factura)
// - Control de afectaciones a inventario
// - Cálculo de precios, descuentos e impuestos
// - Mantener referencia con sistema legacy
//
// RELACIONES:
// - Muchos a Uno con legacy.movimientos_ref (FK: LegacyMovimientoId)
// - Muchos a Uno con legacy.documentos_ref (FK: LegacyDocumentoId)
// - Muchos a Uno con legacy.productos_ref (FK: LegacyProductoId)
// - Muchos a Uno con legacy.documentos_modelo_ref (FK: LegacyDocumentoModeloId)
// - Uno a Muchos con catalog.movimientos_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de movimientos (partidas de documentos)
    /// Representa líneas de detalle en documentos comerciales
    /// </summary>
    [Table("movimientos", Schema = "catalog")]
    public class Movimiento
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
        /// ID del movimiento en el sistema legacy
        /// </summary>
        [Column("legacy_movimiento_id")]
        public int? LegacyMovimientoId { get; set; }

        /// <summary>
        /// ID del documento padre en el sistema legacy
        /// </summary>
        [Column("legacy_documento_id")]
        public int? LegacyDocumentoId { get; set; }

        /// <summary>
        /// ID del producto asociado en el sistema legacy
        /// </summary>
        [Column("legacy_producto_id")]
        public int? LegacyProductoId { get; set; }

        /// <summary>
        /// ID del documento modelo en el sistema legacy
        /// </summary>
        [Column("legacy_documento_modelo_id")]
        public int? LegacyDocumentoModeloId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // INFORMACIÓN DEL MOVIMIENTO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Número consecutivo del movimiento dentro del documento
        /// </summary>
        [Column("numero_movimiento")]
        public double? NumeroMovimiento { get; set; }

        /// <summary>
        /// Fecha del movimiento
        /// </summary>
        [Column("fecha")]
        public DateTime? Fecha { get; set; }

        /// <summary>
        /// Referencia específica del movimiento
        /// </summary>
        [Column("referencia")]
        [MaxLength(20)]
        public string? Referencia { get; set; }

        /// <summary>
        /// Observaciones específicas del movimiento
        /// </summary>
        [Column("observa_mov")]
        public string? ObservaMov { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CANTIDADES Y UNIDADES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Cantidad de unidades (después de conversión)
        /// </summary>
        [Column("unidades")]
        [Precision(18, 4)]
        public decimal? Unidades { get; set; }

        /// <summary>
        /// Cantidad de unidades capturadas originalmente
        /// </summary>
        [Column("unidades_capturadas")]
        [Precision(18, 4)]
        public decimal? UnidadesCapturadas { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PRECIOS Y COSTOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Precio unitario capturado
        /// </summary>
        [Column("precio_capturado")]
        [Precision(18, 4)]
        public decimal? PrecioCapturado { get; set; }

        /// <summary>
        /// Costo específico del producto en este movimiento
        /// </summary>
        [Column("costo_especifico")]
        [Precision(18, 4)]
        public decimal? CostoEspecifico { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // IMPORTES Y CÁLCULOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Importe neto (sin impuestos)
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
        /// Porcentaje de impuesto 1
        /// </summary>
        [Column("porcentaje_impuesto1")]
        [Precision(18, 4)]
        public decimal? PorcentajeImpuesto1 { get; set; }

        /// <summary>
        /// Importe de descuento 1
        /// </summary>
        [Column("descuento1")]
        [Precision(18, 4)]
        public decimal? Descuento1 { get; set; }

        /// <summary>
        /// Porcentaje de descuento 1
        /// </summary>
        [Column("porcentaje_descuento1")]
        [Precision(18, 4)]
        public decimal? PorcentajeDescuento1 { get; set; }

        /// <summary>
        /// Importe total del movimiento
        /// </summary>
        [Column("total")]
        [Precision(18, 4)]
        public decimal? Total { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE CONTROL
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Indica cómo afecta la existencia
        /// 1 = Aumenta, 2 = Disminuye, 0 = No afecta
        /// </summary>
        [Column("afecta_existencia")]
        public int? AfectaExistencia { get; set; }

        /// <summary>
        /// Indica si ya afectó saldos
        /// </summary>
        [Column("afectado_saldos")]
        public int? AfectadoSaldos { get; set; }

        /// <summary>
        /// Indica si ya afectó inventario
        /// </summary>
        [Column("afectado_inventario")]
        public int? AfectadoInventario { get; set; }

        /// <summary>
        /// ID del movimiento origen (en caso de ser derivado)
        /// </summary>
        [Column("movto_origen_id")]
        public int? MovtoOrigenId { get; set; }

        /// <summary>
        /// Indica si el movimiento está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al movimiento en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyMovimientoId))]
        public virtual MovimientosRef? MovimientosRef { get; set; }

        /// <summary>
        /// Referencia al documento padre en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoId))]
        public virtual DocumentosRef? DocumentosRef { get; set; }

        /// <summary>
        /// Referencia al producto en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyProductoId))]
        public virtual ProductosRef? ProductosRef { get; set; }

        /// <summary>
        /// Referencia al documento modelo en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyDocumentoModeloId))]
        public virtual DocumentosModeloRef? DocumentosModeloRef { get; set; }

        /// <summary>
        /// Colección de movimientos de serie asociados
        /// </summary>
        // Pendiente: public virtual ICollection<MovimientoSerie> MovimientosSerie { get; set; }
    }
}
