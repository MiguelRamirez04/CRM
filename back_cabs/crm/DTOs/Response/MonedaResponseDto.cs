// =====================================================================================
// DTO RESPUESTA MONEDA - MonedaResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta para la API de monedas.
// Contiene los datos que se devuelven al frontend.
//
// PROPIEDADES:
// - Datos básicos de la moneda (id, nombre, símbolo, etc.)
// - Información de configuración (decimales, activo)
// - Referencia al sistema legacy
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para operaciones de monedas
    /// </summary>
    public class MonedaResponseDto
    {
        /// <summary>
        /// ID único de la moneda en el sistema local
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la moneda en el sistema legacy (opcional)
        /// </summary>
        public int? LegacyMonedaId { get; set; }

        /// <summary>
        /// Nombre completo de la moneda
        /// </summary>
        public string? NombreMoneda { get; set; }

        /// <summary>
        /// Símbolo de la moneda
        /// </summary>
        public string? SimboloMoneda { get; set; }

        /// <summary>
        /// Clave SAT para facturación electrónica
        /// </summary>
        public string? ClaveSat { get; set; }

        /// <summary>
        /// Número de decimales para cálculos
        /// </summary>
        public int? Decimales { get; set; }

        /// <summary>
        /// Indica si la moneda está activa
        /// </summary>
        public bool Activo { get; set; }
    }
}