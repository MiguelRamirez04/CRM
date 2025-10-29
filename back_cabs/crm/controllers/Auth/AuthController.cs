//file:back_cabs/CRM/controllers/Auth/AuthController.cs
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

using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.Auth;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;

        public AuthController(
            UsuarioAuthService usuarioAuthService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _usuarioAuthService = usuarioAuthService ?? throw new ArgumentNullException(nameof(usuarioAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y información del usuario autenticado</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar credenciales con base de datos
                var usuario = await _usuarioAuthService.ValidarCredencialesAsync(request.Email, request.Password);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Determinar rol (por defecto Recepcion si no está definido)
                var rolUsuario = !string.IsNullOrEmpty(usuario.Rol) && Enum.TryParse<RolUsuario>(usuario.Rol, out var rol)
                    ? rol
                    : RolUsuario.RECEPCION;

                var user = new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                    Role = rolUsuario.ToString(), // Mantener en MAYÚSCULAS (RECEPCION, SOPORTE, ADMINISTRACION)
                    Permissions = GetPermissionsByRole(rolUsuario)
                };
                var tokens = GenerateTokens(user);

                // Configurar cookie HttpOnly con el access token
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Secure = Request.IsHttps, // true en producción
                    SameSite = SameSiteMode.Lax // O Strict si el frontend está en el mismo dominio
                };
                Response.Cookies.Append("AuthToken", tokens.AccessToken, cookieOptions);

                _logger.LogInformation($"Usuario {user.Email} inició sesión exitosamente");

                return Ok(new
                {
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.Name,
                        role = user.Role,
                        permissions = user.Permissions
                    },
                    // El token se envía en la cookie, pero lo devolvemos para facilitar pruebas en Swagger
                    token = tokens.AccessToken, 
                    expiresIn = 1800 // 30 minutos en segundos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Refresca el token de acceso usando el refresh token
        /// </summary>
        /// <returns>Nuevo token de acceso</returns>
        /// <response code="200">Token refrescado exitosamente</response>
        /// <response code="401">Refresh token inválido</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        public Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                
                if (string.IsNullOrEmpty(refreshToken) || !IsValidRefreshToken(refreshToken))
                {
                    return Task.FromResult<IActionResult>(Unauthorized(new { message = "Token de refresco inválido" }));
                }

                var userId = GetUserIdFromRefreshToken(refreshToken);
                var user = GetUserById(userId); // TODO: Obtener de BD

                if (user == null)
                {
                    return Task.FromResult<IActionResult>(Unauthorized(new { message = "Usuario no encontrado" }));
                }

                var tokens = GenerateTokens(user);

                // Actualizar cookies HttpOnly
                SetRefreshTokenCookie(tokens.RefreshToken);
                SetAccessTokenCookie(tokens.AccessToken);

                _logger.LogInformation($"Token refrescado para usuario {user.Email}");

                return Task.FromResult<IActionResult>(Ok(new
                {
                    expiresIn = 1800 // 30 minutos
                    // NO devolver tokens en la respuesta
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el refresh token");
                return Task.FromResult<IActionResult>(Unauthorized(new { message = "Error al refrescar token" }));
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        /// <returns>Confirmación de logout</returns>
        /// <response code="200">Logout exitoso</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> Logout()
        {
            try
            {
                // Limpiar cookies HttpOnly
                ClearAuthCookies();

                _logger.LogInformation("Usuario cerró sesión");

                return Task.FromResult<IActionResult>(Ok(new { message = "Sesión cerrada exitosamente" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
                return Task.FromResult<IActionResult>(StatusCode(500, new { message = "Error al cerrar sesión" }));
            }
        }

        /// <summary>
        /// Obtiene información del usuario actualmente autenticado
        /// </summary>
        /// <returns>Información del usuario actual</returns>
        /// <response code="200">Usuario obtenido exitosamente</response>
        /// <response code="401">Token inválido o no proporcionado</response>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Usar la información del usuario ya autenticado por el middleware
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Usuario autenticado pero sin claim NameIdentifier");
                    return Unauthorized(new { message = "Token de acceso inválido - falta ID de usuario" });
                }

                // Obtener los datos reales del usuario desde la base de datos usando el ID
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("ID de usuario inválido en claims: {UserId}", userIdClaim);
                    return Unauthorized(new { message = "ID de usuario inválido en el token" });
                }

                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado en BD para ID: {UserId}", userId);
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation("Usuario {Email} obtuvo su información exitosamente", usuario.Email);

                // Determinar rol (por defecto Recepcion si no está definido)
                var rolUsuario = !string.IsNullOrEmpty(usuario.Rol) && Enum.TryParse<RolUsuario>(usuario.Rol, out var rol)
                    ? rol
                    : RolUsuario.RECEPCION;

                return Ok(new
                {
                    user = new
                    {
                        id = usuario.Id,
                        email = usuario.Email,
                        name = usuario.NombreCompleto,
                        role = rolUsuario.ToString(), // Mantener en MAYÚSCULAS
                        permissions = GetPermissionsByRole(rolUsuario),
                        fechaRegistro = usuario.CreadoEn,
                        tipoTransmision = usuario.TransmisionHabilitada
                    }
                });
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Error de formato al procesar claims de usuario");
                return Unauthorized(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos (opcional, default: false)</param>
        /// <returns>Lista de todos los usuarios</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("usuarios")]
        [Authorize(Roles = "ADMINISTRACION")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllUsuarios([FromQuery] bool incluirInactivos = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios (incluir inactivos: {IncluirInactivos})", incluirInactivos);

                var usuarios = await _usuarioAuthService.ObtenerTodosLosUsuariosAsync(incluirInactivos);

                return Ok(new
                {
                    success = true,
                    data = usuarios,
                    count = usuarios.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error al obtener los usuarios",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene usuarios filtrados por rol (para selectores de técnicos, etc.)
        /// </summary>
        /// <param name="rol">Rol a filtrar (SOPORTE, ADMINISTRACION, RECEPCION)</param>
        /// <param name="incluirInactivos">Si incluye usuarios inactivos (opcional, default: false)</param>
        /// <returns>Lista de usuarios del rol especificado</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente</response>
        /// <response code="400">Rol inválido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("usuarios/rol/{rol}")]
        [Authorize(Roles = "ADMINISTRACION, RECEPCION, SOPORTE")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsuariosPorRol(string rol, [FromQuery] bool incluirInactivos = false)
        {
            try
            {
                // Validar que el rol sea válido
                var rolesValidos = new[] { "SOPORTE", "ADMINISTRACION", "RECEPCION" };
                if (!rolesValidos.Contains(rol.ToUpper()))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Rol inválido. Roles válidos: {string.Join(", ", rolesValidos)}"
                    });
                }

                _logger.LogInformation("Obteniendo usuarios por rol: {Rol} (incluir inactivos: {IncluirInactivos})", 
                    rol, incluirInactivos);

                var usuarios = await _usuarioAuthService.ObtenerUsuariosPorRolAsync(rol, incluirInactivos);

                return Ok(new
                {
                    success = true,
                    data = usuarios,
                    count = usuarios.Count(),
                    rol = rol.ToUpper()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por rol: {Rol}", rol);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = $"Error al obtener usuarios con rol {rol}",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene técnicos disponibles (SOPORTE y ADMINISTRACION activos)
        /// Endpoint simplificado para selectores en formularios
        /// </summary>
        /// <returns>Lista de técnicos disponibles</returns>
        /// <response code="200">Lista de técnicos obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("tecnicos")]
        [Authorize(Roles = "ADMINISTRACION, RECEPCION, SOPORTE")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTecnicos()
        {
            try
            {
                _logger.LogInformation("Obteniendo técnicos disponibles (SOPORTE y ADMINISTRACION)");

                // Obtener usuarios de SOPORTE
                var soporte = await _usuarioAuthService.ObtenerUsuariosPorRolAsync("SOPORTE", incluirInactivos: false);
                
                // Obtener usuarios de ADMINISTRACION
                var administracion = await _usuarioAuthService.ObtenerUsuariosPorRolAsync("ADMINISTRACION", incluirInactivos: false);

                // Combinar ambas listas
                var tecnicos = soporte.Concat(administracion)
                    .OrderBy(u => u.Nombre)
                    .ThenBy(u => u.Apellido)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = tecnicos,
                    count = tecnicos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener técnicos disponibles");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error al obtener técnicos disponibles",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        /// <param name="request">Contraseña actual y nueva contraseña</param>
        /// <returns>Confirmación del cambio de contraseña</returns>
        /// <response code="200">Contraseña cambiada exitosamente</response>
        /// <response code="400">Contraseña actual incorrecta</response>
        /// <response code="401">Token inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Validar el request
                if (request == null || string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "Contraseña actual y nueva contraseña son requeridas" });
                }

                // Extraer el ID del usuario desde el JWT Bearer token
                var userId = await GetCurrentUserIdFromJwtAsync();
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Token de acceso inválido o no proporcionado" });
                }

                // Convertir userId string a int
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return Unauthorized(new { message = "ID de usuario inválido en el token" });
                }

                // Obtener el usuario por ID
                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userIdInt);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Validar la contraseña actual usando el servicio de autenticación
                var credencialesValidas = await _usuarioAuthService.ValidarCredencialesAsync(usuario.Email, request.OldPassword);
                if (credencialesValidas == null)
                {
                    _logger.LogWarning("Intento de cambio de contraseña con contraseña actual incorrecta para usuario: {UserId}", userId);
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                }

                // Actualizar la contraseña usando el servicio
                var actualizado = await _usuarioAuthService.ActualizarContrasenaAsync(userIdInt, request.NewPassword);
                if (!actualizado)
                {
                    return StatusCode(500, new { message = "Error al actualizar la contraseña" });
                }

                _logger.LogInformation("Usuario {UserId} - {Email} cambió su contraseña exitosamente", userId, usuario.Email);
                
                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (FormatException)
            {
                return Unauthorized(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #region Métodos Privados de Seguridad

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // CRÍTICO: No accesible desde JavaScript
                Secure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production",
                SameSite = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
                    ? SameSiteMode.Strict 
                    : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7), // Refresh token dura 7 días
                Path = "/api/auth" // Solo disponible para rutas de auth
            };

            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }

        private void SetAccessTokenCookie(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // CRÍTICO: No accesible desde JavaScript
                Secure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production",
                SameSite = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
                    ? SameSiteMode.Strict 
                    : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(30), // Access token dura 30 minutos
                Path = "/" // Disponible para toda la aplicación
            };

            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
        }

        private void ClearAuthCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production",
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1) // Expirar inmediatamente
            };

            Response.Cookies.Append("RefreshToken", "", cookieOptions);
            Response.Cookies.Append("AccessToken", "", cookieOptions);
        }

        private (string AccessToken, string RefreshToken) GenerateTokens(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT SecretKey not configured"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // Refresh token simple (en producción usar algo más robusto)
            var refreshToken = Guid.NewGuid().ToString() + "_" + user.Id;

            return (accessToken, refreshToken);
        }

        private bool IsValidRefreshToken(string refreshToken)
        {
            // TODO: Validar contra BD o cache
            return !string.IsNullOrEmpty(refreshToken) && refreshToken.Contains("_");
        }

        private bool IsValidAccessToken(string accessToken)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var key = Encoding.UTF8.GetBytes(secretKey ?? throw new InvalidOperationException("JWT SecretKey not configured"));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true
                };

                tokenHandler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetUserIdFromRefreshToken(string refreshToken)
        {
            return refreshToken.Split('_')[1];
        }

        private string GetUserIdFromAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);
            return token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Extrae el ID del usuario autenticado desde el JWT Bearer token de la request actual
        /// </summary>
        /// <returns>ID del usuario o null si no está autenticado</returns>
        private Task<string?> GetCurrentUserIdFromJwtAsync()
        {
            try
            {
                // Primero intentar obtener el token del header Authorization Bearer
                var authHeader = Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    if (IsValidAccessToken(token))
                    {
                        return Task.FromResult<string?>(GetUserIdFromAccessToken(token));
                    }
                }

                // Si no hay Bearer token, intentar obtener de cookies (fallback)
                var cookieToken = Request.Cookies["AccessToken"];
                if (!string.IsNullOrEmpty(cookieToken) && IsValidAccessToken(cookieToken))
                {
                    return Task.FromResult<string?>(GetUserIdFromAccessToken(cookieToken));
                }

                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al extraer user ID del JWT");
                return Task.FromResult<string?>(null);
            }
        }

        #endregion

        #region TODO: Métodos de Base de Datos (reemplazar con implementación real)

        private string[] GetPermissionsByRole(RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.ADMINISTRACION => new[] { "administracion.read", "administracion.write", "recepcion.read", "recepcion.write", "soporte.read", "soporte.write" },
                RolUsuario.RECEPCION => new[] { "recepcion.read", "recepcion.write", "soporte.read" },
                RolUsuario.SOPORTE => new[] { "soporte.read", "soporte.write" },
                _ => Array.Empty<string>()
            };
        }

        private async Task<User?> GetUserByIdAsync(string id)
        {
            try
            {
                if (!int.TryParse(id, out var userId))
                {
                    return null;
                }

                var usuario = await _usuarioAuthService.ObtenerUsuarioPorIdAsync(userId);
                if (usuario == null)
                {
                    return null;
                }

                // Determinar rol (por defecto Recepcion si no está definido)
                var rolUsuario = !string.IsNullOrEmpty(usuario.Rol) && Enum.TryParse<RolUsuario>(usuario.Rol, out var rol)
                    ? rol
                    : RolUsuario.RECEPCION;

                return new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                    Role = rolUsuario.ToString(), // Mantener en MAYÚSCULAS
                    Permissions = GetPermissionsByRole(rolUsuario)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {UserId}", id);
                return null;
            }
        }

        // Método obsoleto mantenido para compatibilidad con refresh token (será refactorizado)
        private User GetUserById(string id)
        {
            // Este método será reemplazado gradualmente por GetUserByIdAsync
            try
            {
                if (!int.TryParse(id, out var userId))
                {
                    return new User { Id = id, Email = "unknown", Name = "Unknown User", Role = "unknown", Permissions = Array.Empty<string>() };
                }

                // Por ahora devolvemos datos básicos, TODO: hacer asíncrono
                return new User
                {
                    Id = id,
                    Email = "user@temp.com",
                    Name = "Usuario Temporal",
                    Role = "user",
                    Permissions = new[] { "basic.read" }
                };
            }
            catch
            {
                return new User { Id = id, Email = "error", Name = "Error User", Role = "error", Permissions = Array.Empty<string>() };
            }
        }

        // Métodos ValidateCurrentPassword y UpdateUserPassword removidos
        // Ahora se usa _usuarioAuthService.ValidarCredencialesAsync y _usuarioAuthService.ActualizarContrasenaAsync

        #endregion
    }
}

#region DTOs

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
}

#endregion