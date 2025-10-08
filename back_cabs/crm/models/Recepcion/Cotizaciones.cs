using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Sales
{
    [Table("cotizaciones")]
    public class Cotizacion
    {
        [Key]
        public int Id { get; set; }

        public int? OrdenId { get; set; }
        public int? IntakeLegacyId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; } = 0;

        [Column(TypeName = "decimal(12,2)")]
        public decimal ImpuestosTotal { get; set; } = 0;

        [NotMapped]
        public decimal Total => Subtotal + ImpuestosTotal;

        [StringLength(20)]
        public string Estado { get; set; } = "NUEVA";

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime? ActualizadoEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        public int? ValidezDias { get; set; }
    }
}