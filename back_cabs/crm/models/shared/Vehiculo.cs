using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Fleet;

[Table("vehiculos", Schema = "fleet")]
public class Vehiculo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo_vehiculo")]
    [MaxLength(50)]
    public string? TipoVehiculo { get; set; }

    [Column("transmision")]
    [MaxLength(20)]
    public string? Transmision { get; set; }

    [Column("es_de_empresa")]
    [Required]
    public bool EsDeEmpresa { get; set; } = true;

    [Column("placas")]
    [MaxLength(20)]
    public string? Placas { get; set; }

    [Column("activo")]
    [Required]
    public bool Activo { get; set; } = true;

    [Column("observaciones")]
    public string? Observaciones { get; set; }
}
