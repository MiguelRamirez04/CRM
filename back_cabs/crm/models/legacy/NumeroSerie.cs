// =====================================================================================
// ENTIDAD CATALOG NÚMEROS SERIE - NumeroSerie.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de números de serie del catálogo local.
// Representa el control individual de productos identificables por serie.
//
// PROPÓSITO:
// - Control de inventario por número de serie
// - Rastreo de productos individuales
// - Gestión de estados de equipos/productos
//
// RELACIONES:
// - Muchos a Uno con legacy.numeros_serie_ref (FK: LegacySerieId)
// - Muchos a Uno con legacy.productos_ref (FK: LegacyProductoId)
// - Muchos a Uno con legacy.almacenes_ref (FK: LegacyAlmacenId)
// - Uno a Muchos con catalog.movimientos_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de números de serie
    /// Control individual de productos identificables
    /// </summary>
    [Table("numeros_serie", Schema = "catalog")]
    public class NumeroSerie
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
        /// ID del número de serie en el sistema legacy
        /// </summary>
        [Column("legacy_serie_id")]
        public int? LegacySerieId { get; set; }

        /// <summary>
        /// ID del producto asociado en el sistema legacy
        /// </summary>
        [Column("legacy_producto_id")]
        public int? LegacyProductoId { get; set; }

        /// <summary>
        /// ID del almacén donde se encuentra en el sistema legacy
        /// </summary>
        [Column("legacy_almacen_id")]
        public int? LegacyAlmacenId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Número de serie único del producto
        /// </summary>
        [Column("numero_serie")]
        [MaxLength(30)]
        public string? NumeroSerieValor { get; set; }

        /// <summary>
        /// Estado actual del número de serie
        /// 1 = Disponible, 2 = Vendido, 3 = En reparación, etc.
        /// </summary>
        [Column("estado")]
        public int? Estado { get; set; }

        /// <summary>
        /// Estado anterior del número de serie
        /// Útil para rastrear cambios de estado
        /// </summary>
        [Column("estado_anterior")]
        public int? EstadoAnterior { get; set; }

        /// <summary>
        /// Costo específico de este número de serie
        /// </summary>
        [Column("costo")]
        [Precision(18, 4)]
        public decimal? Costo { get; set; }

        /// <summary>
        /// Fecha y hora del último cambio de estado
        /// </summary>
        [Column("fecha_timestamp")]
        public DateTime? FechaTimestamp { get; set; }

        /// <summary>
        /// Indica si el número de serie está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al número de serie en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacySerieId))]
        public virtual NumerosSerieRef? NumerosSerieRef { get; set; }

        /// <summary>
        /// Referencia al producto en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyProductoId))]
        public virtual ProductosRef? ProductosRef { get; set; }

        /// <summary>
        /// Referencia al almacén en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyAlmacenId))]
        public virtual AlmacenesRef? AlmacenesRef { get; set; }

        /// <summary>
        /// Colección de movimientos de serie asociados
        /// </summary>
        // Pendiente: public virtual ICollection<MovimientoSerie> MovimientosSerie { get; set; }
    }
}
