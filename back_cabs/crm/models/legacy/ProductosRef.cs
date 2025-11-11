// =====================================================================================
// ENTIDAD LEGACY PRODUCTOS REFERENCIA - ProductosRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para productos del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.productos
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.productos
// - Uno a Muchos con catalog.numeros_serie
// - Uno a Muchos con catalog.movimientos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para productos del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("productos_ref", Schema = "legacy")]
    public class ProductosRef
    {
        /// <summary>
        /// ID del producto en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admProductos.CIDPRODUCTO del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_producto_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyProductoId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de productos locales asociados a esta referencia legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Producto> Productos { get; set; }

        /// <summary>
        /// Colección de números de serie asociados a este producto legacy
        /// </summary>
        // Pendiente: public virtual ICollection<NumeroSerie> NumerosSerie { get; set; }

        /// <summary>
        /// Colección de movimientos asociados a este producto legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Movimiento> Movimientos { get; set; }
    }
}
