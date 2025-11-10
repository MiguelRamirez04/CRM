using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request;

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

    [MaxLength(100)]
    public string NombreVehiculo { get; set; } = string.Empty;

    [Required]
    public int Kilometraje { get; set; }
}
