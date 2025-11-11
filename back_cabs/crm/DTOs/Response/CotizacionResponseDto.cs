namespace CRM.DTOs.Response;

public class CotizacionResponseDto
{
    public int Id { get; set; }
    public int? OrdenId { get; set; }
    public int? IntakeLegacyId { get; set; }
    public int? CreadaPor { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ImpuestosTotal { get; set; }
    public decimal? Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "NUEVA";
    public string? Observaciones { get; set; }
    public string? Cliente { get; set; }
    public string? Rfc { get; set; }
    public string? Folio { get; set; }
    public string? DescripcionServicio { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public DateTime CreadoEn { get; set; }
    public int? ValidezDias { get; set; }
}