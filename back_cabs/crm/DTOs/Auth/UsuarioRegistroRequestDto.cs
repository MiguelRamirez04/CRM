// =====================================================================================
// DTO REQUEST REGISTRO USUARIO - UsuarioRegistroRequestDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de entrada para el endpoint de registro de usuarios.
// Contiene todas las propiedades necesarias que el cliente debe enviar
// para crear un nuevo usuario en el sistema.
//
// CUÁNDO USARLO:
// - Endpoint POST /api/auth/registro
// - Validación de datos de entrada
// - Mapeo a entidad de dominio
// - Documentación Swagger de request
//
// CÓMO USARLO:
// [HttpPost("registro")]
// public async Task<IActionResult> Registrar([FromBody] UsuarioRegistroRequestDto request)
// {
//     // Procesar registro
// }
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace back_cabs.CRM.DTOs.Auth
{
    /// <summary>
    /// DTO para solicitud de registro de nuevo usuario
    /// </summary>
    public class UsuarioRegistroRequestDto
    {
        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        /// <example>Juan Carlos Pérez García</example>
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres")]
        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Email único del usuario (será usado para login)
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario (mínimo 8 caracteres)
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de contraseña (debe coincidir con contraseña)
        /// </summary>
        /// <example>MiContraseña123!</example>
        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [JsonPropertyName("confirmarContrasena")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario en el sistema
        /// </summary>
        /// <example>Soporte</example>
        [Required(ErrorMessage = "El rol es obligatorio")]
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario tiene licencia de conducir válida
        /// </summary>
        /// <example>true</example>
        [JsonPropertyName("licenciaConducir")]
        public bool LicenciaConducir { get; set; } = false;

        /// <summary>
        /// Tipo de transmisión que puede manejar (Ninguna, Automatico, Manual, Ambas)
        /// </summary>
        /// <example>Ambas</example>
        [JsonPropertyName("transmisionHabilitada")]
        public string TransmisionHabilitada { get; set; } = "Ninguna";
    }
}