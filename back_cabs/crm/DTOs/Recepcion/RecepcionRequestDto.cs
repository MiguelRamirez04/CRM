// =====================================================================================
// DTO REQUEST REGISTRO RECEPTION - RecepcionRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de entrada para el endpoint de registro de Recepcion
// Contiene todas las propiedades necesarias que el cliente debe enviar
// para crear un nuevo usuario en el sistema.
//
// CUÁNDO USARLO:
// - Endpoint POST /api/recepcion/registro
// - Validación de datos de entrada
// - Mapeo a entidad de dominio
// - Documentación Swagger de request
//
// CÓMO USARLO:
// [HttpPost("registro")]
// public async Task<IActionResult> Registrar([FromBody] UsuarioRegistroRequestDto request)
// {

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using back_cabs.CRM.Validation;
using back_cabs.CRM.enums;
//     // Procesar registro
// }
//
// =====================================================================================


using System.Diagnostics.CodeAnalysis;


namespace back_cabs.CRM.DTOs.Recepcion
{
    /// <summary>
    /// DTO para la solicitud de creación de una nueva Orden de Trabajo (POST).
    /// El ID es autogenerado por la BD, por lo tanto, no se incluye.
    /// </summary>
    public class OrdenTrabajoCreacionRequestDto
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
        ///   Modalidad de la orden (presencial o remoto.).
        /// </summary>
        [Required(ErrorMessage = "La modalidad es obligatoria.")]
        [StringLength(50)]
        [JsonPropertyName("modalidad")]
        public string Modalidad { get; set; } = string.Empty;

        /// <summary>
        ///     Tipo de orden(cotizacion o servicio)
        /// </summary>
        [Required(ErrorMessage = "El tipo de orden es obligatorio.")]
        [StringLength(50)]
        [JsonPropertyName("tipoOrden")]
        public string TipoOrden { get; set; } = string.Empty;

        /// <summary>
        ///   Referencia al cliente (ID) asociado a esta orden.
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
        ///     Niveles de prioridad sobre la asesoria
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
        ///     Campo para registrar la ubicacion del servicio
        /// </summary>
        [StringLength(300)]
        [JsonPropertyName("ubicacionText")]
        public string? UbicacionText { get; set; }

        /// <summary>
        ///     Estado de factura sobre el servicio.
        /// </summary>
        [JsonPropertyName("estadoFacturado")]
        public string? EstadoFacturado { get; set; } = "NO";

        /// <summary>
        ///     Campo para confirmar facturacion del servicio.
        /// </summary>
        [JsonPropertyName("RequiereFactura")]
        public bool RequiereFactura { get; set; } = false;

        /// <summary>
        ///     Campo para registrar folio de factura.
        /// </summary>
        [JsonPropertyName("FacturaFolio")]
        public int? FacturaFolio { get; set; }

        // Nota: CreadoEn y ActualizadoEn se generan automáticamente por el sistema
        // No se incluyen en el DTO de creación

        /// <summary>
        ///  Campo para establecer el costo real de la asesoria (opcional en creación)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El costo real debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoReal")]
        public decimal? CostoReal { get; set; }

        /// <summary>
        ///     Campo para establecer el costo aproximado de la asesoria
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El costo estimado debe ser mayor o igual a 0.")]
        [JsonPropertyName("costoEstimado")]
        public decimal? CostoEstimado { get; set; }
        
        /// <summary>
        ///     Campo para registrar el id del usuario(asesor) que va a atender la asesoria.
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario que va a atender la orden es necesario.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser mayor a 0.")]
        [JsonPropertyName("creadoPorUserId")]
        public int CreadoPorUserId { get; set; }


    }


// # 2. DTO de Modificación (PUT): `OrdenTrabajoActualizacionRequestDto`

// En el contexto de la API, el `Id` de la orden a modificar **se suele pasar en el segmento de la URL** (`PUT /api/ordenes/{id}`). Por lo tanto, el DTO de actualización en el cuerpo (`[FromBody]`) **no necesita incluir el `Id`**.



    /// <summary>
    /// DTO para la solicitud de actualización de una Orden de Trabajo (PUT).
    /// Los campos son anulables (tipos referencia o '?' en tipos valor) para permitir actualizaciones parciales.
    /// El ID de la orden se espera en la URL, no en el cuerpo.
    /// </summary>
    public class OrdenTrabajoActualizacionRequestDto
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


    // ## 3. DTO de Visualización (GET): `OrdenTrabajoResponseDto`

    // Este DTO **debe incluir el `Id`** y todas las fechas generadas por el sistema (`CreadoEn`, `ActualizadoEn`), ya que son datos que se envían al cliente para que pueda identificar la orden y ver su historial.


    /// <summary>
    /// DTO para la respuesta de visualización de una Orden de Trabajo (GET).
    /// Usa 'record' para inmutabilidad y 'init' para asegurar que los valores solo se establecen durante la construcción.
    /// </summary>
    public record OrdenTrabajoResponseDto
    {
        /// <summary>
        /// Identificador único de la orden (generado por la BD).
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; init; } // El ID es esencial para la respuesta

        /// <summary>
        /// Campo de notas y/o observaciones.
        /// </summary>
        [JsonPropertyName("notas")]
        public string? Notas { get; init; }

        /// <summary>
        /// Campo de fecha de inicio para la asesoria.
        /// </summary>
        [JsonPropertyName("citaProgramadaInicio")]
        public DateTime? CitaProgramadaInicio { get; init; }

        /// <summary>
        /// Campo de fecha de fin para la asesoria.
        /// </summary>
        [JsonPropertyName("citaProgramadaFin")]
        public DateTime? CitaProgramadaFin { get; init; }

        /// <summary>
        /// Campo de modalidad para la asesoria.
        /// </summary>
        [JsonPropertyName("modalidad")]
        public string? Modalidad { get; init; }

        /// <summary>
        /// Campo de tipo de orden a realizar(asesoria o cotizacion).
        /// </summary>
        [JsonPropertyName("tipoOrden")]
        public string? TipoOrden { get; init; }

        /// <summary>
        /// Indica si es un cliente nuevo (true) o un cliente existente (false)
        /// </summary>
        [JsonPropertyName("nuevoCliente")]
        public bool NuevoCliente { get; init; }
        
        /// <summary>
        /// Nombre del cliente (solo si nuevoCliente = true)
        /// </summary>
        [JsonPropertyName("nombreCliente")]
        public string? NombreCliente { get; init; }
        
        /// <summary>
        /// Identificador del cliente en la base de datos (solo si nuevoCliente = false)
        /// </summary>
        [JsonPropertyName("clienteId")]
        public int? ClienteId { get; init; }

        /// <summary>
        /// Campo de prioridad para la realizacion de la asesoria.
        /// </summary>
        [JsonPropertyName("prioridad")]
        public int? Prioridad { get; init; }

        /// <summary>
        /// Campo de estado de la asesoria.
        /// </summary>
        [JsonPropertyName("estado")]
        public string? Estado { get; init; }
        
        /// <summary>
        /// Descripción legible del estado de la orden.
        /// </summary>
        [JsonPropertyName("estadoDescripcion")]
        public string? EstadoDescripcion { get; init; }

        /// <summary>
        /// Campo de ubicacion para realizar la asesoria.
        /// </summary>
        [JsonPropertyName("ubicacionText")]
        public string? UbicacionText { get; init; }

        /// <summary>
        /// Campo de estado sobre la facturacion de la asesoria.
        /// </summary>
        [JsonPropertyName("estadoFacturado")]
        public string? EstadoFacturado { get; init; }

        /// <summary>
        /// Campo de estado sobre si requiere facturacion de la asesoria.
        /// </summary>
        [JsonPropertyName("requiereFactura")]
        public bool? RequiereFactura { get; init; }

        /// <summary>
        /// Campo de costo final de la asesoria.
        /// </summary>
        [JsonPropertyName("costoReal")]
        public decimal? CostoReal { get; init; }

        /// <summary>
        /// Campo de costo aproximado de la asesoria.
        /// </summary>
        [JsonPropertyName("costoEstimado")]
        public decimal? CostoEstimado { get; init; }

        /// <summary>
        /// Marca de tiempo de cuándo fue creada la orden (generada por el sistema).
        /// </summary>
        [JsonPropertyName("creadoEn")]
        public DateTime CreadoEn { get; init; }

        /// <summary>
        /// Marca de tiempo de la última modificación.
        /// </summary>
        [JsonPropertyName("actualizadoEn")]
        public DateTime? ActualizadoEn { get; init; }

        /// <summary>
        /// ID del usuario que creó la orden
        /// </summary>
        [JsonPropertyName("creadoPorUserId")]
        public int CreadoPorUserId { get; init; }
        
        /// <summary>
        /// ID del usuario asignado a la orden
        /// </summary>
        [JsonPropertyName("asignadaAUserId")]
        public int? AsignadaAUserId { get; init; }
    }
}









