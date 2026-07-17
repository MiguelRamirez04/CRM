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
using back_cabs.CRM.Core.Validation;
using back_cabs.CRM.enums;

namespace CRM.DTOs.Request
{
    /// <summary>
    /// DTO para solicitud de registro de nuevo usuario
    /// </summary>
    public class UsuarioRegistroRequestDto
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        /// <example>Juan Carlos</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        /// <example>Pérez García</example>
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        /// <example>5512345678</example>
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Range(0, 999999999998, ErrorMessage = "El teléfono debe ser un número válido de 10 dígitos")]
        [JsonPropertyName("telefono")]
        public long Telefono { get; set; }

        /// <summary>
        /// Email único del usuario (será usado para login)
        /// </summary>
        /// <example>juan.perez@empresa.com</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
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
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        [JsonPropertyName("confirmarContrasena")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario en el sistema
        /// </summary>
        /// <example>Conductor</example>
        [Required(ErrorMessage = "El rol es obligatorio")]
        [EnumStringValue(typeof(RolUsuario))]
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de transmisión que puede manejar (opcional)
    /// </summary>
    /// <example>Manual</example>
    [StringLength(50, ErrorMessage = "La transmisión habilitada no puede exceder 50 caracteres")]
    [EnumStringValue(typeof(TipoTransmision))]
    [JsonPropertyName("transmisionHabilitada")]
    public string? TransmisionHabilitada { get; set; }

    /// <summary>
    /// Indica si el usuario está activo (puede usar licencia de conducir)
    /// </summary>
    /// <example>true</example>
    [JsonPropertyName("activo")]
    public bool? Activo { get; set; } = true;
    }
}