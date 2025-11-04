using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request;

/// <summary>
/// DTO para crear o actualizar una cotización.
/// Alineado con la tabla sales_cotizaciones en SQL Server.
/// </summary>
public class CotizacionCreateRequestDto
{
    // OrdenId es opcional - puede crearse cotización sin orden de trabajo asociada
    public int? OrdenId { get; set; }

    public int? IntakeLegacyId { get; set; }

    [Required(ErrorMessage = "El subtotal es obligatorio")]
    [Range(0, 999999999.99, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
    public decimal Subtotal { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Los impuestos deben ser mayor o igual a 0")]
    public decimal ImpuestosTotal { get; set; } = 0;

    [Range(0, 999999999.99, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal? Descuento { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
    public string Estado { get; set; } = "NUEVA";

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    [StringLength(255, ErrorMessage = "El nombre del cliente no puede exceder 255 caracteres")]
    public string? Cliente { get; set; }

    [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
    [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", ErrorMessage = "El RFC no tiene un formato válido")]
    public string? Rfc { get; set; }

    [StringLength(50, ErrorMessage = "El folio no puede exceder 50 caracteres")]
    public string? Folio { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción del servicio no puede exceder 1000 caracteres")]
    public string? DescripcionServicio { get; set; }

    [Range(1, 365, ErrorMessage = "La validez debe estar entre 1 y 365 días")]
    public int? ValidezDias { get; set; }

    // ===================================================================
    // CAMPOS DE CAPACITACIÓN
    // ===================================================================

    [Range(0, 999, ErrorMessage = "Las horas de capacitación deben estar entre 0 y 999")]
    public int? HorasCapacitacion { get; set; }

    [Range(0, 999, ErrorMessage = "Los paquetes de capacitación deben estar entre 0 y 999")]
    public int? PaquetesCapacitacion { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "El costo de capacitación debe ser mayor o igual a 0")]
    public decimal? CostoCapacitacion { get; set; }

    // ===================================================================
    // CAMPOS DE CONTACTO
    // ===================================================================

    /// <summary>
    /// Teléfono de contacto (5 a 15 dígitos).
    /// Ejemplo: 6178907616 (10-12 dígitos típico en México)
    /// </summary>
    [Range(10000, 999999999999999, ErrorMessage = "El teléfono debe tener entre 5 y 15 dígitos")]
    public long? Telefono { get; set; }

    /// <summary>
    /// Correo electrónico de contacto.
    /// Máximo 150 caracteres.
    /// </summary>
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
    public string? Correo { get; set; }
}