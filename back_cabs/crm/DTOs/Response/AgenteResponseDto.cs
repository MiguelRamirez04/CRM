// =====================================================================================
// DTO RESPUESTA AGENTE - AgenteResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta para la API de agentes.
// Contiene los datos que se devuelven al frontend.
//
// PROPIEDADES:
// - Datos básicos del agente (id, código, nombre, etc.)
// - Información de configuración (tipo, fecha alta, activo)
// - Referencia al sistema legacy
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para operaciones de agentes
    /// </summary>
    public class AgenteResponseDto
    {
        /// <summary>
        /// ID único del agente en el sistema local
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del agente en el sistema legacy (opcional)
        /// </summary>
        public int? LegacyAgenteId { get; set; }

        /// <summary>
        /// Código único del agente en el sistema
        /// </summary>
        public string? CodigoAgente { get; set; }

        /// <summary>
        /// Nombre completo del agente
        /// </summary>
        public string? NombreAgente { get; set; }

        /// <summary>
        /// Fecha de alta del agente en el sistema
        /// </summary>
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Tipo de agente (1=Vendedor, 2=Cobrador, etc.)
        /// </summary>
        public int? TipoAgente { get; set; }

        /// <summary>
        /// Campo extra de texto para información adicional
        /// </summary>
        public string? TextoExtra2 { get; set; }

        /// <summary>
        /// Indica si el agente está activo
        /// </summary>
        public bool Activo { get; set; }
    }
}