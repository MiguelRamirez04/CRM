// =====================================================================================
// ENTIDAD CATALOG ALMACENES - Almacen.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de almacenes del catálogo local con datos completos.
// Representa ubicaciones físicas o lógicas para almacenamiento de productos.
//
// PROPÓSITO:
// - Gestionar catálogo de almacenes/bodegas
// - Control de inventarios por ubicación
// - Mantener referencia con sistema legacy
//
// RELACIONES:
// - Muchos a Uno con legacy.almacenes_ref (FK: LegacyAlmacenId)
// - Uno a Muchos con catalog.numeros_serie
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de almacenes
    /// Almacena información completa de almacenes/bodegas del sistema
    /// </summary>
    [Table("almacenes", Schema = "catalog")]
    public class Almacen
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
        /// ID del almacén en el sistema legacy
        /// </summary>
        [Column("legacy_almacen_id")]
        public int? LegacyAlmacenId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Código único del almacén
        /// </summary>
        [Column("codigo_almacen")]
        [MaxLength(30)]
        public string? CodigoAlmacen { get; set; }

        /// <summary>
        /// Nombre descriptivo del almacén
        /// </summary>
        [Column("nombre_almacen")]
        [MaxLength(60)]
        public string? NombreAlmacen { get; set; }

        /// <summary>
        /// Fecha de alta del almacén en el sistema
        /// </summary>
        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Clasificación del almacén (tipo, categoría)
        /// </summary>
        [Column("clasificacion1")]
        public int? Clasificacion1 { get; set; }

        /// <summary>
        /// Indica si el almacén está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al almacén en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyAlmacenId))]
        public virtual AlmacenesRef? AlmacenesRef { get; set; }

        /// <summary>
        /// Colección de números de serie almacenados en este almacén
        /// </summary>
        // Pendiente: public virtual ICollection<NumeroSerie> NumerosSerie { get; set; }
    }
}
