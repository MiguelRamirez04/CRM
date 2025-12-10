using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Modelo para gastos de viáticos - Coincide con tabla finance_gastos_viaticos
    /// </summary>
    [Table("finance_gastos_viaticos")]
    public class GastoViatico
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo (OPCIONAL - viáticos pueden ser independientes)
        /// </summary>
        [Column("orden_id")]
        public int? OrdenId { get; set; }

        /// <summary>
        /// ID del vehículo usado para el viático (OPCIONAL)
        /// </summary>
        [Column("vehiculo_id")]
        public int? VehiculoId { get; set; }

        [Required]
        [Column("tiene_factura")]
        public bool TieneFactura { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("proveedor_nombre")]
        [StringLength(200)]
        public string? ProveedorNombre { get; set; }

        [Required]
        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column("km_recorridos")]
        public int? KmRecorridos { get; set; }

        [Required]
        [Column("gastos")]
        [StringLength(200)]
        public string Gastos { get; set; } = string.Empty;

        [Required]
        [Column("monto_total", TypeName = "decimal(12, 2)")]
        public decimal MontoTotal { get; set; }

        [Column("lugar_destino")]
        [StringLength(200)]
        public string? LugarDestino { get; set; }

        // =====================================================================
        // PROPIEDADES DE NAVEGACIÓN
        // =====================================================================

        /// <summary>
        /// Vehículo utilizado para el viático (opcional)
        /// Permite cargar con Include() para evitar N+1 queries
        /// </summary>
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }
    }
}
