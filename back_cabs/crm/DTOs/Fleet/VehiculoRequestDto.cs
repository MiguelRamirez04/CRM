using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Fleet;

public class VehiculoRequestDto
{
    [MaxLength(50)]
    public string? TipoVehiculo { get; set; }

    [MaxLength(20)]
    public string? Transmision { get; set; }

    public bool EsDeEmpresa { get; set; } = true;

    [MaxLength(20)]
    public string? Placas { get; set; }

    public bool Activo { get; set; } = true;

    public string? Observaciones { get; set; }
}
