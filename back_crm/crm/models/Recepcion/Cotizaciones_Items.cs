using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Sales
{
    [Table("cotizacion_items")]
    public class CotizacionItem
    {
        [Key]
        public int Id { get; set; }

        public int CotizacionId { get; set; }
        public int? ProductoServicioRefId { get; set; }
        public int? LegacyProductoId { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? TotalLinea { get; set; }
    }
}