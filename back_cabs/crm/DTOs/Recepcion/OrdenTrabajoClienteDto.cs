// =====================================================================================
// DTO CLIENTE ORDEN TRABAJO - OrdenTrabajoClienteDto.cs
// =====================================================================================    
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define DTOs específicos para el manejo del modelo híbrido de clientes en órdenes de trabajo.
// Permite manejar tanto clientes nuevos como clientes legacy de manera estructurada.
//
// CUÁNDO USARLO:
// - Al seleccionar clientes para crear órdenes de trabajo
// - Al implementar búsqueda de clientes en el frontend
// - Para validar los datos de cliente en la creación de órdenes
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.DTOs.Recepcion
{
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