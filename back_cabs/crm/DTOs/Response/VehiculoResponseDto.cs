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
}
