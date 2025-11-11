// =====================================================================================
// ENTIDAD CATALOG PRODUCTOS - Producto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de productos del catálogo local con datos completos.
// Representa artículos, servicios o bienes comercializables.
//
// PROPÓSITO:
// - Gestionar catálogo de productos/servicios
// - Control de precios y costos
// - Mantener referencia con sistema legacy
// - Cumplimiento fiscal (Clave SAT)
//
// RELACIONES:
// - Muchos a Uno con legacy.productos_ref (FK: LegacyProductoId)
// - Uno a Muchos con catalog.numeros_serie
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de productos
    /// Almacena información completa de productos/servicios del sistema
    /// </summary>
    [Table("productos", Schema = "catalog")]
    public class Producto
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
        /// ID del producto en el sistema legacy
        /// </summary>
        [Column("legacy_producto_id")]
        public int? LegacyProductoId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Código único del producto (SKU)
        /// </summary>
        [Column("codigo_producto")]
        [MaxLength(30)]
        public string? CodigoProducto { get; set; }

        /// <summary>
        /// Nombre descriptivo del producto
        /// </summary>
        [Column("nombre_producto")]
        [MaxLength(60)]
        public string? NombreProducto { get; set; }

        /// <summary>
        /// Tipo de producto (1=Producto, 2=Servicio, 3=Paquete, etc.)
        /// </summary>
        [Column("tipo_producto")]
        public int? TipoProducto { get; set; }

        /// <summary>
        /// Fecha de alta del producto en el sistema
        /// </summary>
        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Descripción detallada del producto
        /// </summary>
        [Column("descripcion_producto")]
        public string? DescripcionProducto { get; set; }

        /// <summary>
        /// Método de costeo (1=Promedio, 2=PEPS, 3=UEPS, etc.)
        /// </summary>
        [Column("metodo_costeo")]
        public int? MetodoCosteo { get; set; }

        /// <summary>
        /// Estado del producto (1=Activo, 2=Descontinuado, etc.)
        /// </summary>
        [Column("status_producto")]
        public int? StatusProducto { get; set; }

        /// <summary>
        /// ID de la unidad base de medida
        /// </summary>
        [Column("unidad_base_id")]
        public int? UnidadBaseId { get; set; }

        /// <summary>
        /// ID de la unidad no convertible
        /// </summary>
        [Column("unidad_no_convertible_id")]
        public int? UnidadNoConvertibleId { get; set; }

        /// <summary>
        /// Precio de venta principal del producto
        /// </summary>
        [Column("precio1")]
        [Precision(18, 4)]
        public decimal? Precio1 { get; set; }

        /// <summary>
        /// Clave SAT del producto para facturación electrónica
        /// </summary>
        [Column("clave_sat")]
        [MaxLength(8)]
        public string? ClaveSat { get; set; }

        /// <summary>
        /// Subtipo o categoría del producto
        /// </summary>
        [Column("subtipo")]
        public int? Subtipo { get; set; }

        /// <summary>
        /// Indica si el producto está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al producto en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyProductoId))]
        public virtual ProductosRef? ProductosRef { get; set; }

        /// <summary>
        /// Colección de números de serie de este producto
        /// </summary>
        // Pendiente: public virtual ICollection<NumeroSerie> NumerosSerie { get; set; }

        /// <summary>
        /// Colección de movimientos de este producto
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
