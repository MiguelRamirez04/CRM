using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request;

public class CotizacionCreateRequestDto
{
    [Required]
    public int? OrdenId { get; set; }

    public int? IntakeLegacyId { get; set; }

    [Required]
    public int? CreadaPor { get; set; }

    [Required]
    [Range(0, 999999999.99)]
    public decimal Subtotal { get; set; }

    [Range(0, 999999999.99)]
    public decimal ImpuestosTotal { get; set; } = 0;

    [Range(0, 999999999.99)]
    public decimal? Descuento { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "NUEVA";

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [StringLength(255)]
    public string? Cliente { get; set; }

    [StringLength(13)]
    public string? Rfc { get; set; }

    [StringLength(50)]
    public string? Folio { get; set; }

    [StringLength(1000)]
    public string? DescripcionServicio { get; set; }

    public int? ValidezDias { get; set; }
}