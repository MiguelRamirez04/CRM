namespace CRM.DTOs.Response;

public class VehiculoResponseDto
{
    public int Id { get; set; }
    public string? TipoVehiculo { get; set; }
    public string? Transmision { get; set; }
    public bool EsDeEmpresa { get; set; }
    public string? Placas { get; set; }
    public bool Activo { get; set; }
    public string? Observaciones { get; set; }
    public string NombreVehiculo { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public DateTime CreadoEn { get; set; }
    public int? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public int? ActualizadoPorUsuarioId { get; set; }
    public string? HistorialCambios { get; set; }
}
