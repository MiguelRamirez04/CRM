using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Administracion
{
    /// <summary>
    /// Entidad que representa un producto o servicio del catálogo
    /// </summary>
    [Table("catalog_productos_servicio_ref")]
    public class Catalog_Productos_Servicio_Ref
    {
        /// <summary>
        /// Identificador único del producto/servicio (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del producto o servicio
        /// </summary>
        [Required]
        [StringLength(200)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de producto/servicio
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Unidad de medida
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("unidad")]
        public string Unidad { get; set; } = string.Empty;

        /// <summary>
        /// Precio de lista
        /// </summary>
        [Required]
        [Column("precio_lista", TypeName = "decimal(12,2)")]
        public decimal PrecioLista { get; set; }

        /// <summary>
        /// ID del producto en el sistema legacy
        /// </summary>
        [Column("legacy_product_id")]
        public int? LegacyProductId { get; set; }
    }
}