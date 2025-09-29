// =====================================================================================
// SERVICIO USUARIO AUTH - UsuarioAuthService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa la lógica de negocio para el registro y gestión de usuarios.
// Incluye validación, hashing de contraseñas, persistencia y generación de tokens.
//
// CUÁNDO USARLO:
// - Registro de nuevos usuarios
// - Validación de reglas de negocio
// - Operaciones relacionadas con autenticación
// - Mapeo entre DTOs y entidades
//
// CÓMO USARLO:
// var service = new UsuarioAuthService(_writeContext, _readContext, _jwtService, _logger);
// var resultado = await service.RegistrarUsuarioAsync(dto);
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Auth;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.validators.Auth;
using back_cabs.middleware;
using back_cabs.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Auth
{
    /// <summary>
    /// Servicio para gestión de usuarios y autenticación
    /// </summary>
    public class UsuarioAuthService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ServicioJwt _servicioJwt;
        private readonly ILogger<UsuarioAuthService> _logger;
        private readonly UsuarioRegistroValidator _validator;

        public UsuarioAuthService(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ServicioJwt servicioJwt,
            ILogger<UsuarioAuthService> logger,
            UsuarioRegistroValidator validator)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _servicioJwt = servicioJwt ?? throw new ArgumentNullException(nameof(servicioJwt));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="request">Datos del usuario a registrar</param>
        /// <returns>Respuesta con información del usuario registrado y token JWT</returns>
        /// <exception cref="FluentValidation.ValidationException">Cuando los datos no son válidos</exception>
        /// <exception cref="InvalidOperationException">Cuando ocurre un error en el proceso</exception>
        public async Task<RegistroExitosoResponseDto> RegistrarUsuarioAsync(UsuarioRegistroRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de registro para email: {Email}", request.Email);

                // PASO 1: VALIDAR DATOS DE ENTRADA
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validación fallida para email {Email}: {Errores}", 
                        request.Email, 
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                // PASO 2: VERIFICAR UNICIDAD DEL EMAIL (doble verificación)
                var emailExiste = await _readContext.UsuariosAuth
                    .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
                
                if (emailExiste)
                {
                    _logger.LogWarning("Intento de registro con email duplicado: {Email}", request.Email);
                    throw new InvalidOperationException("Ya existe un usuario registrado con este email");
                }

                // PASO 3: CREAR HASH SEGURO DE LA CONTRASEÑA
                var contrasenaHash = ApiUtilities.GenerateSha256Hash(request.Contrasena);
                _logger.LogDebug("Hash de contraseña generado para usuario: {Email}", request.Email);

                // PASO 4: CREAR ENTIDAD DE USUARIO
                var nuevoUsuario = new UsuarioAuth
                {
                    Id = Guid.NewGuid(),
                    NombreCompleto = request.NombreCompleto.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    ContrasenaHash = contrasenaHash,
                    Rol = request.Rol,
                    LicenciaConducir = request.LicenciaConducir,
                    TransmisionHabilitada = request.TransmisionHabilitada,
                    Activo = true,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                // PASO 5: GUARDAR EN BASE DE DATOS
                _writeContext.UsuariosAuth.Add(nuevoUsuario);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado exitosamente: {UserId} - {Email}", 
                    nuevoUsuario.Id, nuevoUsuario.Email);

                // PASO 6: GENERAR TOKEN JWT PARA AUTO-LOGIN
                var claims = new List<System.Security.Claims.Claim>
                {
                    new("sub", nuevoUsuario.Id.ToString()),
                    new("email", nuevoUsuario.Email),
                    new("rol", nuevoUsuario.Rol),
                    new("nombre", nuevoUsuario.NombreCompleto)
                };

                var token = _servicioJwt.GenerarTokenAcceso(claims, TimeSpan.FromHours(24));
                var expiracion = DateTime.UtcNow.AddHours(24); // Token válido por 24 horas

                _logger.LogDebug("Token JWT generado para usuario: {UserId}", nuevoUsuario.Id);

                // PASO 7: MAPEAR A DTO DE RESPUESTA
                var usuarioDto = MapearAUsuarioResponseDto(nuevoUsuario);

                // PASO 8: CREAR RESPUESTA FINAL
                var respuesta = new RegistroExitosoResponseDto
                {
                    Usuario = usuarioDto,
                    Token = token,
                    ExpiraEn = expiracion,
                    Mensaje = "Usuario registrado exitosamente",
                    Exitoso = true
                };

                _logger.LogInformation("Registro completado exitosamente para usuario: {UserId}", nuevoUsuario.Id);
                return respuesta;
            }
            catch (FluentValidation.ValidationException)
            {
                // Re-lanzar excepciones de validación sin logging adicional
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el registro de usuario: {Email}", request.Email);
                throw new InvalidOperationException("Ocurrió un error interno durante el registro. Intente nuevamente.", ex);
            }
        }

        /// <summary>
        /// Mapea una entidad UsuarioAuth a UsuarioResponseDto
        /// </summary>
        /// <param name="usuario">Entidad a mapear</param>
        /// <returns>DTO con información del usuario</returns>
        private static UsuarioResponseDto MapearAUsuarioResponseDto(UsuarioAuth usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                CreadoEn = usuario.CreadoEn,
                LicenciaConducir = usuario.LicenciaConducir,
                TransmisionHabilitada = usuario.TransmisionHabilitada,
                PuedeUsarVehiculo = usuario.PuedeUsarVehiculo
            };
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        public async Task<UsuarioAuth?> ObtenerUsuarioPorEmailAsync(string email)
        {
            try
            {
                return await _readContext.UsuariosAuth
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un email ya está registrado
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <returns>True si el email existe</returns>
        public async Task<bool> EmailExisteAsync(string email)
        {
            try
            {
                return await _readContext.UsuariosAuth
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Valida las credenciales de un usuario (email y contraseña)
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="contrasena">Contraseña en texto plano</param>
        /// <returns>Usuario si las credenciales son válidas, null si no</returns>
        public async Task<UsuarioAuth?> ValidarCredencialesAsync(string email, string contrasena)
        {
            try
            {
                _logger.LogInformation($"Validando credenciales para usuario: {email}");

                // Buscar usuario por email
                var usuario = await _readContext.UsuariosAuth
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {email}");
                    return null;
                }

                // Validar contraseña con hash
                var contrasenaHash = ApiUtilities.GenerateSha256Hash(contrasena);
                if (usuario.ContrasenaHash != contrasenaHash)
                {
                    _logger.LogWarning($"Contraseña inválida para usuario: {email}");
                    return null;
                }

                _logger.LogInformation($"Credenciales válidas para usuario: {email}");
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validando credenciales para usuario: {email}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID único
        /// </summary>
        /// <param name="id">ID único del usuario (Guid)</param>
        /// <returns>Usuario si existe, null si no se encuentra</returns>
        public async Task<UsuarioAuth?> ObtenerUsuarioPorIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario por ID: {UserId}", id);

                var usuario = await _readContext.UsuariosAuth
                    .FirstOrDefaultAsync(u => u.Id == id && u.Activo);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {UserId}", id);
                    return null;
                }

                _logger.LogInformation("Usuario encontrado: {UserId} - {Email}", usuario.Id, usuario.Email);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="nuevaContrasena">Nueva contraseña en texto plano</param>
        /// <returns>True si se actualizó correctamente, False si no</returns>
        public async Task<bool> ActualizarContrasenaAsync(Guid userId, string nuevaContrasena)
        {
            try
            {
                _logger.LogInformation("Actualizando contraseña para usuario: {UserId}", userId);

                // Validar que la nueva contraseña no esté vacía
                if (string.IsNullOrWhiteSpace(nuevaContrasena))
                {
                    _logger.LogWarning("Nueva contraseña no puede estar vacía para usuario: {UserId}", userId);
                    return false;
                }

                // Buscar el usuario
                var usuario = await _writeContext.UsuariosAuth
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Activo);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para actualizar contraseña: {UserId}", userId);
                    return false;
                }

                // Generar hash de la nueva contraseña
                var nuevoHash = ApiUtilities.GenerateSha256Hash(nuevaContrasena);
                
                // Verificar que la nueva contraseña sea diferente a la actual
                if (usuario.ContrasenaHash == nuevoHash)
                {
                    _logger.LogWarning("La nueva contraseña es igual a la actual para usuario: {UserId}", userId);
                    return false;
                }

                // Actualizar la contraseña y fecha de modificación
                usuario.ContrasenaHash = nuevoHash;
                usuario.ActualizarFechaModificacion();

                // Guardar cambios en la base de datos
                var filasAfectadas = await _writeContext.SaveChangesAsync();
                
                if (filasAfectadas > 0)
                {
                    _logger.LogInformation("Contraseña actualizada exitosamente para usuario: {UserId} - {Email}", 
                        usuario.Id, usuario.Email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se pudo actualizar la contraseña para usuario: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar contraseña para usuario: {UserId}", userId);
                throw;
            }
        }
    }
}