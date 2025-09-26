// =====================================================================================
// DTO RESPONSE USUARIO - UsuarioResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta para operaciones con usuarios.
// Contiene la información del usuario que se retorna al cliente,
// excluyendo datos sensibles como contraseñas.
//
// CUÁNDO USARLO:
// - Respuesta de registro exitoso
// - Respuesta de login
// - Listado de usuarios
// - Información de perfil de usuario
//
// CÓMO USARLO:
// return Ok(new UsuarioResponseDto 
// { 
//     Id = usuario.Id,
//     NombreCompleto = usuario.NombreCompleto
// });
//
// =====================================================================================

using System.Text.Json.Serialization;

namespace back_cabs.CRM.DTOs.Auth
{
    /// <summary>
    /// DTO de respuesta con información del usuario
    /// </summary>
    public class UsuarioResponseDto
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        /// <example>f47ac10b-58cc-4372-a567-0e02b2c3d479</example>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        /// <example>Juan Carlos Pérez García</example>
        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario en el sistema
        /// </summary>
        /// <example>Soporte</example>
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("activo")]
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha de creación del usuario
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        [JsonPropertyName("creadoEn")]
        public DateTime CreadoEn { get; set; }

        /// <summary>
        /// Indica si tiene licencia de conducir
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("licenciaConducir")]
        public bool LicenciaConducir { get; set; }

        /// <summary>
        /// Tipo de transmisión habilitada
        /// </summary>
        /// <example>Ambas</example>
        [JsonPropertyName("transmisionHabilitada")]
        public string TransmisionHabilitada { get; set; } = string.Empty;

        /// <summary>
        /// Indica si puede usar vehículos de empresa
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("puedeUsarVehiculo")]
        public bool PuedeUsarVehiculo { get; set; }
    }
}