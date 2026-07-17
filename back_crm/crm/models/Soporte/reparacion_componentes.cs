// =====================================================================================
// ENTIDAD REPARACIÓN COMPONENTE - ReparacionComponente.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad para los componentes o refacciones utilizados en una reparación.
// Representa la tabla 'reparacion_componentes' en la base de datos.
//
// CUÁNDO USARLO:
// - Para registrar las piezas y sus costos asociados a una reparación específica.
// - Mapeo con Entity Framework.
// - Cálculo de costos de refacciones.
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Soporte
{
    /// <summary>
    /// Entidad que representa un componente o refacción utilizado en una reparación.
    /// Mapea a la tabla 'reparacion_componentes' en la base de datos.
    /// </summary>
    [Table("reparacion_componentes")]
    public class ReparacionComponente
    {
        /// <summary>
        /// Identificador único del componente de la reparación (autoincremental).
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la reparación a la que pertenece este componente.
        /// </summary>
        [Required]
        [Column("reparacion_id")]
        public int ReparacionId { get; set; }

        /// <summary>
        /// Propiedad de navegación para la reparación asociada.
        /// </summary>
        [ForeignKey("ReparacionId")]
        public virtual Reparacion Reparacion { get; set; } = null!;

        /// <summary>
        /// Descripción del componente (ej. "SSD 512GB", "Pantalla 14 pulgadas").
        /// </summary>
        [Required(ErrorMessage = "La descripción del componente es obligatoria.")]
        [StringLength(160)]
        [Column("componente")]
        public string Componente { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de unidades de este componente utilizadas.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Column("cantidad")]
        public int Cantidad { get; set; } = 1;

        /// <summary>
        /// Proveedor donde se adquirió el componente.
        /// </summary>
        [StringLength(160)]
        [Column("proveedor")]
        public string? Proveedor { get; set; }

        /// <summary>
        /// Meses de garantía ofrecidos por el proveedor para este componente.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "La garantía no puede ser negativa.")]
        [Column("garantia_meses")]
        public int? GarantiaMeses { get; set; }

        /// <summary>
        /// Costo unitario de compra del componente al proveedor.
        /// </summary>
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El costo de compra no puede ser negativo.")]
        [Column("costo_unitario_compra", TypeName = "decimal(12, 2)")]
        public decimal? CostoUnitarioCompra { get; set; }

        /// <summary>
        /// Precio unitario de venta del componente al cliente final.
        /// </summary>
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El costo al público no puede ser negativo.")]
        [Column("costo_unitario_publico", TypeName = "decimal(12, 2)")]
        public decimal? CostoUnitarioPublico { get; set; }

        /// <summary>
        /// Subtotal a precio de compra (cantidad * costo_unitario_compra).
        /// Es una columna calculada en la base de datos.
        /// </summary>
        [Column("subtotal_compra", TypeName = "decimal(13, 2)")] // El tipo puede variar según el cálculo
        public decimal SubtotalCompra { get; private set; }

        /// <summary>
        /// Subtotal a precio de venta al público (cantidad * costo_unitario_publico).
        /// Es una columna calculada en la base de datos.
        /// </summary>
        [Column("subtotal_publico", TypeName = "decimal(13, 2)")] // El tipo puede variar según el cálculo
        public decimal SubtotalPublico { get; private set; }

        /// <summary>
        /// Notas adicionales sobre el componente.
        /// </summary>
        [StringLength(500)]
        [Column("notas")]
        public string? Notas { get; set; }
    }
}