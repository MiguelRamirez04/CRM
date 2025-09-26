using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using back_cabs.CRM.enums;
using back_cabs.CRM.services.Auth;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly UsuarioAuthService _authService;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger, UsuarioAuthService authService)
    {
        _configuration = configuration;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validar credenciales con base de datos
            var usuario = await _authService.ValidarCredencialesAsync(request.Email, request.Password);
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

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken) || !IsValidRefreshToken(refreshToken))
            {
                return Unauthorized(new { message = "Token de refresco inválido" });
            }

            var userId = GetUserIdFromRefreshToken(refreshToken);
            var user = GetUserById(userId); // TODO: Obtener de BD

            if (user == null)
            {
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            var tokens = GenerateTokens(user);

            // Actualizar cookies HttpOnly
            SetRefreshTokenCookie(tokens.RefreshToken);
            SetAccessTokenCookie(tokens.AccessToken);

            _logger.LogInformation($"Token refrescado para usuario {user.Email}");

            return Ok(new
            {
                expiresIn = 1800 // 30 minutos
                // NO devolver tokens en la respuesta
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresh token");
            return Unauthorized(new { message = "Error al refrescar token" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Limpiar cookies HttpOnly
            ClearAuthCookies();

            _logger.LogInformation("Usuario cerró sesión");

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el logout");
            return StatusCode(500, new { message = "Error al cerrar sesión" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var accessToken = Request.Cookies["AccessToken"];
            
            if (string.IsNullOrEmpty(accessToken) || !IsValidAccessToken(accessToken))
            {
                return Unauthorized(new { message = "Token de acceso inválido" });
            }

            var userId = GetUserIdFromAccessToken(accessToken);
            var user = GetUserById(userId); // TODO: Obtener de BD

            if (user == null)
            {
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            return Ok(new
            {
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role,
                    permissions = user.Permissions
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario actual");
            return Unauthorized(new { message = "Error de autenticación" });
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var accessToken = Request.Cookies["AccessToken"];
            
            if (string.IsNullOrEmpty(accessToken) || !IsValidAccessToken(accessToken))
            {
                return Unauthorized();
            }

            var userId = GetUserIdFromAccessToken(accessToken);
            
            // TODO: Validar contraseña actual y cambiar por la nueva
            if (!ValidateCurrentPassword(userId, request.OldPassword))
            {
                return BadRequest(new { message = "Contraseña actual incorrecta" });
            }

            // TODO: Cambiar contraseña en BD
            UpdateUserPassword(userId, request.NewPassword);

            _logger.LogInformation($"Usuario {userId} cambió su contraseña");

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
            return StatusCode(500, new { message = "Error al cambiar contraseña" });
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

        var key = Encoding.UTF8.GetBytes(secretKey);
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
            var key = Encoding.UTF8.GetBytes(secretKey);

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
        return token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
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

