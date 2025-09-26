// =====================================================================================
// CONTROLADOR AUTH - AuthController.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los endpoints HTTP para operaciones de autenticación y gestión de usuarios.
// Incluye registro, login, y otras operaciones relacionadas con la autenticación.
//
// CUÁNDO USARLO:
// - Registro de nuevos usuarios (POST /api/auth/registro)
// - Login de usuarios existentes
// - Operaciones de gestión de autenticación
// - Endpoints públicos de autenticación
//
// CÓMO USARLO:
// Los endpoints se exponen automáticamente en:
// POST /api/auth/registro - Registrar nuevo usuario
// Documentación completa disponible en Swagger UI
//
// =====================================================================================

using back_cabs.CRM.DTOs.Auth;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.Auth;
using back_cabs.middleware;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace back_cabs.CRM.controllers.Auth
{
    /// <summary>
    /// Controlador para operaciones de autenticación y gestión de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioAuthService _usuarioAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioAuthService usuarioAuthService,
            ILogger<AuthController> logger)
        {
            _usuarioAuthService = usuarioAuthService ?? throw new ArgumentNullException(nameof(usuarioAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="request">Datos del usuario a registrar</param>
        /// <returns>Información del usuario registrado con token JWT</returns>
        /// <response code="201">Usuario registrado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos o email duplicado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(RegistroExitosoResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando registro de usuario para email: {Email}", request?.Email);

                // Validación básica del request
                if (request == null)
                {
                    _logger.LogWarning("Request de registro recibido como null");
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Los datos del usuario son requeridos",
                        "Request body cannot be null"));
                }

                // Procesar el registro
                var resultado = await _usuarioAuthService.RegistrarUsuarioAsync(request);

                _logger.LogInformation("Usuario registrado exitosamente: {UserId} - {Email}", 
                    resultado.Usuario.Id, resultado.Usuario.Email);

                // Retornar respuesta exitosa con código 201 Created
                return CreatedAtAction(
                    nameof(RegistrarUsuario),
                    new { id = resultado.Usuario.Id },
                    resultado);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Errores de validación en registro: {Errores}", 
                    string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

                // Crear respuesta estructurada con errores de validación
                var erroresValidacion = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Errores de validación en los datos proporcionados",
                    System.Text.Json.JsonSerializer.Serialize(erroresValidacion)));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                _logger.LogWarning("Intento de registro con email duplicado: {Email}", request?.Email);

                return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorValidacion,
                    "Email duplicado",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante registro de usuario: {Email}", request?.Email);

                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error interno del servidor",
                    "Ocurrió un error inesperado durante el proceso de registro"));
            }
        }

        /// <summary>
        /// Obtiene información sobre los roles disponibles en el sistema
        /// </summary>
        /// <returns>Lista de roles válidos con sus descripciones</returns>
        /// <response code="200">Lista de roles obtenida exitosamente</response>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public IActionResult ObtenerRoles()
        {
            try
            {
                var roles = Enum.GetValues<RolUsuario>()
                    .Select(rol => new
                    {
                        Valor = rol.ToString(),
                        Descripcion = rol.ObtenerDescripcion(),
                        EsAdministrativo = rol.EsAdministrativo(),
                        PuedeGestionarClientes = rol.PuedeGestionarClientes(),
                        PuedeRealizarSoporte = rol.PuedeRealizarSoporte()
                    })
                    .ToList();

                return Ok(new
                {
                    Roles = roles,
                    Total = roles.Count,
                    Mensaje = "Roles obtenidos exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del sistema");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener roles",
                    "No se pudieron cargar los roles del sistema"));
            }
        }

        /// <summary>
        /// Obtiene información sobre los tipos de transmisión disponibles
        /// </summary>
        /// <returns>Lista de tipos de transmisión válidos con sus descripciones</returns>
        /// <response code="200">Lista de transmisiones obtenida exitosamente</response>
        [HttpGet("transmisiones")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public IActionResult ObtenerTiposTransmision()
        {
            try
            {
                var transmisiones = Enum.GetValues<TipoTransmision>()
                    .Select(tipo => new
                    {
                        Valor = tipo.ToString(),
                        Descripcion = tipo.ObtenerDescripcion(),
                        PuedeManearAutomatico = tipo.PuedeManearAutomatico(),
                        PuedeManearManual = tipo.PuedeManearManual()
                    })
                    .ToList();

                return Ok(new
                {
                    Transmisiones = transmisiones,
                    Total = transmisiones.Count,
                    Mensaje = "Tipos de transmisión obtenidos exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de transmisión");
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error al obtener transmisiones",
                    "No se pudieron cargar los tipos de transmisión"));
            }
        }

        /// <summary>
        /// Verifica si un email ya está registrado en el sistema
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <returns>Indica si el email existe o no</returns>
        /// <response code="200">Verificación completada</response>
        /// <response code="400">Email inválido</response>
        [HttpGet("verificar-email")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> VerificarEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(UtilidadesManejoErrores.CreateErrorResponse(
                        TipoError.ErrorValidacion,
                        "Email requerido",
                        "El email es obligatorio para la verificación"));
                }

                var existe = await _usuarioAuthService.EmailExisteAsync(email);

                return Ok(new
                {
                    Email = email,
                    Existe = existe,
                    Disponible = !existe,
                    Mensaje = existe ? "Email ya registrado" : "Email disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar email: {Email}", email);
                return StatusCode(500, UtilidadesManejoErrores.CreateErrorResponse(
                    TipoError.ErrorServidorInterno,
                    "Error en verificación",
                    "No se pudo verificar la disponibilidad del email"));
            }
        }
    }
}