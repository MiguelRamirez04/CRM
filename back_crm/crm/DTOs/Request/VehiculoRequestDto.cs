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

/// <summary>
/// DTO específico para actualizar vehículos.
/// Solo permite modificar: kilometraje (obligatorio), placas (opcional), observaciones (opcional) y activo (opcional)
/// </summary>
public class VehiculoUpdateDto
{
    [Required]
    public int Kilometraje { get; set; }

    [MaxLength(20)]
    public string? Placas { get; set; }

    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }
}
