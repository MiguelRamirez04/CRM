using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Administracion
{
    /// <summary>
    /// Entidad que representa un gasto de viáticos
    /// </summary>
    [Table("finance_gastos_viaticos")]
    public class Finance_Gastos_Viaticos
    {
        /// <summary>
        /// Identificador único del gasto (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo relacionada
        /// </summary>
        [Required]
        [Column("corden_id")]
        public int CordenId { get; set; }

        /// <summary>
        /// Indica si el gasto tiene factura
        /// </summary>
        [Required]
        [Column("tiene_factura")]
        public bool TieneFactura { get; set; } = false;

        /// <summary>
        /// Descripción del gasto
        /// </summary>
        [StringLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        [StringLength(200)]
        [Column("proveedor_nombre")]
        public string? ProveedorNombre { get; set; }

        /// <summary>
        /// Fecha del gasto
        /// </summary>
        [Required]
        [Column("fecha", TypeName = "DATETIME2(0)")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kilómetros recorridos
        /// </summary>
        [Column("km_recorridos")]
        public int? KmRecorridos { get; set; }

        /// <summary>
        /// Detalle de los gastos (JSON o descripción)
        /// </summary>
        [Required]
        [Column("gastos")]
        public string Gastos { get; set; } = string.Empty;

        /// <summary>
        /// Monto total del gasto
        /// </summary>
        [Required]
        [Column("monto_total", TypeName = "decimal(12,2)")]
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Lugar de destino
        /// </summary>
        [StringLength(200)]
        [Column("lugar_destino")]
        public string? LugarDestino { get; set; }
    }
}