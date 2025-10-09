// =====================================================================================
// DTO REQUEST REGISTRO ORDEN TRABAJO - OrdenTrabajoRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de entrada para el endpoint de registro de Orden de Trabajo
// Contiene todas las propiedades necesarias que el cliente debe enviar
// para crear o actualizar una orden de trabajo.
//
// CUÁNDO USARLO:
// - Endpoint POST /api/ordenes/registro
// - Endpoint PUT /api/ordenes/{id}
// - Validación de datos de entrada
// - Mapeo a entidad de dominio
// - Documentación Swagger de request
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using back_cabs.CRM.Validation;
using back_cabs.CRM.enums;

namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para la solicitud de creación de una nueva Orden de Trabajo (POST).
    /// El ID es autogenerado por la BD, por lo tanto, no se incluye.
    /// </summary>
    public class OrdenTrabajoRequestDto
    {
        /// <summary>
        /// Notas o descripción detallada de la orden.
        /// </summary>
        [StringLength(500, ErrorMessage = "Las notas no deben exceder los 500 caracteres.")]
        [JsonPropertyName("notas")]
        public string? Notas { get; set; }

        /// <summary>
        /// Fecha y hora de inicio de la cita programada.
        /// </summary>
        [Required(ErrorMessage = "La fecha de inicio de la cita es obligatoria.")]
        [JsonPropertyName("citaProgramadaInicio")]
        public DateTime CitaProgramadaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de fin de la cita programada.
        /// </summary>
        [JsonPropertyName("citaProgramadaFin")]
        public DateTime? CitaProgramadaFin { get; set; }

        /// <summary>
        /// Modalidad de la orden (presencial o remoto.).
        /// </summary>
        [Required(ErrorMessage = "La modalidad es obligatoria.")]
        [StringLength(50)]
        [JsonPropertyName("modalidad")]
        public string Modalidad { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de orden (cotizacion o servicio)
        /// </summary>
        [Required(ErrorMessage = "El tipo de orden es obligatorio.")]
        [StringLength(50)]
        [JsonPropertyName("tipoOrden")]
        public string TipoOrden { get; set; } = string.Empty;

        /// <summary>
        /// Referencia al cliente (ID) asociado a esta orden.
        /// </summary>
        [StringLength(100)]
        [JsonPropertyName("cotizaciones")]
        public string? Cotizaciones { get; set; }

        /// <summary>
        /// Indica si el cliente es nuevo (true) o es un cliente existente (false)
        /// </summary>
        [JsonPropertyName("nuevoCliente")]
        public bool NuevoCliente { get; set; }

        /// <summary>
        /// Nombre del cliente (obligatorio si nuevoCliente = true)
        /// </summary>
        [StringLength(120)]
        [JsonPropertyName("nombreCliente")]
        [RequiredIf("NuevoCliente", true, ErrorMessage = "El nombre del cliente es obligatorio para clientes nuevos")]
        public string? NombreCliente { get; set; }

        /// <summary>
        /// ID del cliente registrado de la base existente (obligatorio si nuevoCliente = false)
        /// </summary>
        [JsonPropertyName("clienteId")]
        [RequiredIf("NuevoCliente", false, ErrorMessage = "El ID del cliente es obligatorio para clientes existentes")]
        public int? ClienteId { get; set; }

        /// <summary>
        /// Niveles de prioridad sobre la asesoria
        /// </summary>
        [Range(1, 5, ErrorMessage = "La prioridad debe estar entre 1 y 5.")]
        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; } = 3;

        /// <summary>
        /// Estado de la orden (ej. CAPTURADA, ASIGNADA, EN_CURSO, etc.)
        /// </summary>
        [Required(ErrorMessage = "Se requiere establecer el estado de la orden")]
        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "CAPTURADA";

        /// <summary>
        /// Campo para registrar la ubicacion del servicio
        /// </summary>
        [StringLength(300)]
        [JsonPropertyName("ubicacionText")]
        public string? UbicacionText { get; set; }

        /// <summary>
        /// Estado de factura sobre el servicio.
        /// </summary>
        [JsonPropertyName("estadoFacturado")]
        public string? EstadoFacturado { get; set; } = "NO";

        /// <summary>
        /// Campo para confirmar facturacion del servicio.
        /// </summary>
        [JsonPropertyName("RequiereFactura")]
        public bool RequiereFactura { get; set; } = false;

        /// <summary>
        /// Campo para registrar folio de factura.
        /// </summary>
        [JsonPropertyName("FacturaFolio")]
        public int? FacturaFolio { get; set; }

        // Nota: CreadoEn y ActualizadoEn se generan automáticamente por el sistema
        // No se incluyen en el DTO de creación

        /// <summary>
        /// Campo para establecer el costo real de la asesoria (opcional en creación)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El costo real debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoReal")]
        public decimal? CostoReal { get; set; }

        /// <summary>
        /// Campo para establecer el costo aproximado de la asesoria
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El costo estimado debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoEstimado")]
        public decimal? CostoEstimado { get; set; }

        /// <summary>
        /// Campo para registrar el id del usuario(asesor) que va a atender la asesoria.
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario que va a atender la orden es necesario.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser mayor a 0.")]
        [JsonPropertyName("creadoPorUserId")]
        public int CreadoPorUserId { get; set; }
    }

    /// <summary>
    /// DTO para la solicitud de actualización de una Orden de Trabajo (PUT).
    /// Los campos son anulables (tipos referencia o '?' en tipos valor) para permitir actualizaciones parciales.
    /// El ID de la orden se espera en la URL, no en el cuerpo.
    /// </summary>
    public class OrdenTrabajoUpdateRequestDto
    {
        [StringLength(500)]
        [JsonPropertyName("notas")]
        public string? Notas { get; set; }

        [JsonPropertyName("citaProgramadaInicio")]
        public DateTime? CitaProgramadaInicio { get; set; }

        [JsonPropertyName("citaProgramadaFin")]
        public DateTime? CitaProgramadaFin { get; set; }

        [Range(1, 5)]
        [JsonPropertyName("prioridad")]
        public int? Prioridad { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("estadoFacturado")]
        public string? EstadoFacturado { get; set; }

        [JsonPropertyName("RequiereFactura")]
        public bool? RequiereFactura { get; set; }

        [JsonPropertyName("FacturaFolio")]
        public int? FacturaFolio { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo real debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoReal")]
        public decimal? CostoReal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo estimado debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoEstimado")]
        public decimal? CostoEstimado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser mayor a 0.")]
        [JsonPropertyName("creadoPorUserId")]
        public int? CreadoPorUserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario asignado debe ser mayor a 0.")]
        [JsonPropertyName("asignadaAUserId")]
        public int? AsignadaAUserId { get; set; }
    }
}