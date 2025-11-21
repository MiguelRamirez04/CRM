using System;
using System.ComponentModel.DataAnnotations;

<<<<<<< HEAD
namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para la creación y actualización de una Cotización.
    /// Contiene los datos que el cliente debe proporcionar.
    /// </summary>
    public class CotizacionRequestDto
    {
        // --- Llaves Foráneas (IDs) ---
        [Required]
        public int DocumentoDeId { get; set; }
        [Required]
        public int ConceptoDocumentoId { get; set; }
        [Required]
        public int ClienteProveedorId { get; set; }
        [Required]
        public int AgenteId { get; set; }
        public int? DocumentoOrigenId { get; set; }
=======
namespace CRM.DTOs.Request;

/// <summary>
/// DTO para crear o actualizar una cotización.
/// Alineado con la tabla sales_cotizaciones en SQL Server.
/// </summary>
public class CotizacionCreateRequestDto
{
    // OrdenId es opcional - puede crearse cotización sin orden de trabajo asociada
    public int? OrdenId { get; set; }
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c

    public int? IntakeLegacyId { get; set; }

<<<<<<< HEAD
        // --- Datos Descriptivos ---
        [StringLength(60)]
        public string? RazonSocial { get; set; }
        [StringLength(20)]
        public string? Rfc { get; set; }
        [StringLength(20)]
        public string? Referencia { get; set; }
        public string? Observaciones { get; set; }

        // --- Banderas y Estados ---
        public int Naturaleza { get; set; }
        public int UsaCliente { get; set; }
        public int Afectado { get; set; }
        public int Impreso { get; set; }
        public int Cancelado { get; set; }

        // --- Importes ---
        public double Neto { get; set; }
        public double Impuesto1 { get; set; }
        public double DescuentoMovimiento { get; set; }
        public double Total { get; set; }
        public double TotalUnidades { get; set; }
    }
=======
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
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
}
