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

using back_cabs.CRM.DTOs.Auth;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.Auth;
using back_cabs.middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

                var user = new User
                {
                    Id = usuario.Id.ToString(),
                    Email = usuario.Email,
                    Name = usuario.NombreCompleto,
                    Role = usuario.Rol.ToLower(),
                    Permissions = GetPermissionsByRole(Enum.Parse<RolUsuario>(usuario.Rol))
                };
                var tokens = GenerateTokens(user);

                // Configurar cookie HttpOnly con el refresh token
                SetRefreshTokenCookie(tokens.RefreshToken);

                // Configurar cookie HttpOnly con el access token (opcional, para máxima seguridad)
                SetAccessTokenCookie(tokens.AccessToken);

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
                    token = tokens.AccessToken, // JWT para usar en Swagger
                    refreshToken = tokens.RefreshToken,
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
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        public Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var accessToken = Request.Cookies["AccessToken"];
                
                if (string.IsNullOrEmpty(accessToken) || !IsValidAccessToken(accessToken))
                {
                    return Task.FromResult<IActionResult>(Unauthorized(new { message = "Token de acceso inválido" }));
                }

                var userId = GetUserIdFromAccessToken(accessToken);
                var user = GetUserById(userId); // TODO: Obtener de BD

                if (user == null)
                {
                    return Task.FromResult<IActionResult>(Unauthorized(new { message = "Usuario no encontrado" }));
                }

                return Task.FromResult<IActionResult>(Ok(new
                {
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.Name,
                        role = user.Role,
                        permissions = user.Permissions
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return Task.FromResult<IActionResult>(Unauthorized(new { message = "Error de autenticación" }));
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
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.InternalServerError)]
        public Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var accessToken = Request.Cookies["AccessToken"];
                
                if (string.IsNullOrEmpty(accessToken) || !IsValidAccessToken(accessToken))
                {
                    return Task.FromResult<IActionResult>(Unauthorized());
                }

                var userId = GetUserIdFromAccessToken(accessToken);
                
                // TODO: Validar contraseña actual y cambiar por la nueva
                if (!ValidateCurrentPassword(userId, request.OldPassword))
                {
                    return Task.FromResult<IActionResult>(BadRequest(new { message = "Contraseña actual incorrecta" }));
                }

                // TODO: Cambiar contraseña en BD
                UpdateUserPassword(userId, request.NewPassword);

                _logger.LogInformation($"Usuario {userId} cambió su contraseña");

                return Task.FromResult<IActionResult>(Ok(new { message = "Contraseña cambiada exitosamente" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return Task.FromResult<IActionResult>(StatusCode(500, new { message = "Error al cambiar contraseña" }));
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
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
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

        #endregion

        #region TODO: Métodos de Base de Datos (reemplazar con implementación real)

        private string[] GetPermissionsByRole(RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.Administrador => new[] { "administracion.read", "administracion.write", "recepcion.read", "recepcion.write", "soporte.read", "soporte.write" },
                RolUsuario.Recepcion => new[] { "recepcion.read", "recepcion.write", "soporte.read" },
                RolUsuario.Soporte => new[] { "soporte.read", "soporte.write" },
                _ => Array.Empty<string>()
            };
        }

        private User GetUserById(string id)
        {
            // TODO: Obtener de BD
            return new User
            {
                Id = id,
                Email = "admin@test.com",
                Name = "Usuario Admin",
                Role = "admin",
                Permissions = new[] { "administracion.read", "administracion.write", "recepcion.read", "soporte.read" }
            };
        }

        private bool ValidateCurrentPassword(string userId, string password)
        {
            // TODO: Validar con hash en BD
            return password == "123456";
        }

        private void UpdateUserPassword(string userId, string newPassword)
        {
            // TODO: Hash y guardar nueva contraseña en BD
        }

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