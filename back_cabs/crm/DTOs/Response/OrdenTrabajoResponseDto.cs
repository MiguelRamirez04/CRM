// =====================================================================================
// DTO RESPONSE ORDEN TRABAJO - OrdenTrabajoResponseDto.cs
// =====================================================================================
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CRM.DTOs.Response
{
    /// <summary>
    /// DTO para la respuesta de visualización de una Orden de Trabajo (GET).
    /// Usa 'record' para inmutabilidad y 'init' para asegurar que los valores solo se establecen durante la construcción.
    /// </summary>
    public record OrdenTrabajoResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
        [JsonPropertyName("notas")]
        public string? Notas { get; init; }
        [JsonPropertyName("citaProgramadaInicio")]
        public DateTime? CitaProgramadaInicio { get; init; }
        [JsonPropertyName("citaProgramadaFin")]
        public DateTime? CitaProgramadaFin { get; init; }
        [JsonPropertyName("modalidad")]
        public string? Modalidad { get; init; }
        [JsonPropertyName("tipoOrden")]
        public string? TipoOrden { get; init; }
        [JsonPropertyName("nuevoCliente")]
        public bool NuevoCliente { get; init; }
        [JsonPropertyName("nombreCliente")]
        public string? NombreCliente { get; init; }
        [JsonPropertyName("clienteId")]
        public int? ClienteId { get; init; }
        [JsonPropertyName("prioridad")]
        public int? Prioridad { get; init; }
        [JsonPropertyName("estado")]
        public string? Estado { get; init; }
        [JsonPropertyName("estadoDescripcion")]
        public string? EstadoDescripcion { get; init; }
        [JsonPropertyName("ubicacionText")]
        public string? UbicacionText { get; init; }
        [JsonPropertyName("estadoFacturado")]
        public string? EstadoFacturado { get; init; }
        [JsonPropertyName("requiereFactura")]
        public bool? RequiereFactura { get; init; }
        [JsonPropertyName("costoReal")]
        public decimal? CostoReal { get; init; }
        [JsonPropertyName("costoEstimado")]
        public decimal? CostoEstimado { get; init; }
        [JsonPropertyName("creadoEn")]
        public DateTime CreadoEn { get; init; }
        [JsonPropertyName("actualizadoEn")]
        public DateTime? ActualizadoEn { get; init; }
        [JsonPropertyName("creadoPorUserId")]
        public int CreadoPorUserId { get; init; }
        [JsonPropertyName("asignadaAUserId")]
        public int? AsignadaAUserId { get; init; }
    }

    /// <summary>
    /// DTO para la búsqueda y selección de clientes en órdenes de trabajo.
    /// Permite buscar por nombre o RFC.
    /// </summary>
    public class ClienteBusquedaRequestDto
    {
        /// <summary>
        /// Texto de búsqueda para nombre o RFC
        /// </summary>
        [JsonPropertyName("busqueda")]
        [StringLength(200)]
        public string? TextoBusqueda { get; set; }
        
        /// <summary>
        /// Tipo de búsqueda: "nombre", "rfc", o null para búsqueda general
        /// </summary>
        [JsonPropertyName("tipoBusqueda")]
        public string? TipoBusqueda { get; set; }
        
        /// <summary>
        /// Número de página para paginación
        /// </summary>
        [Range(1, int.MaxValue)]
        [JsonPropertyName("pagina")]
        public int Pagina { get; set; } = 1;
        
        /// <summary>
        /// Resultados por página
        /// </summary>
        [Range(1, 50)]
        [JsonPropertyName("resultadosPorPagina")]
        public int ResultadosPorPagina { get; set; } = 5;
    }

    /// <summary>
    /// DTO para la información resumida de un cliente.
    /// Se usa para mostrar datos básicos en listas y selección.
    /// </summary>
    public class ClienteResumenDto
    {
        /// <summary>
        /// ID del cliente en la base de datos
        /// </summary>
        [JsonPropertyName("clienteId")]
        public int ClienteId { get; set; }
        
        /// <summary>
        /// Nombre comercial del cliente
        /// </summary>
        [JsonPropertyName("nombreComercial")]
        public string? NombreComercial { get; set; }
        
        /// <summary>
        /// RFC del cliente
        /// </summary>
        [JsonPropertyName("rfc")]
        public string? RFC { get; set; }
        
        /// <summary>
        /// ID legacy del cliente
        /// </summary>
        [JsonPropertyName("legacyClientId")]
        public int? LegacyClientId { get; set; }
    }
}
