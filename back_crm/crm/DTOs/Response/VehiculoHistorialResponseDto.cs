namespace CRM.DTOs.Response;

public class VehiculoHistorialResponseDto
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public string CampoModificado { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
    public string TipoCambio { get; set; } = string.Empty;
}
