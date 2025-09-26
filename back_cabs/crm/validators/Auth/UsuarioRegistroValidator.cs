// =====================================================================================
// VALIDADOR REGISTRO USUARIO - UsuarioRegistroValidator.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa las reglas de validación para el registro de usuarios.
// Incluye validaciones de negocio específicas como unicidad de email,
// complejidad de contraseña, y consistencia de datos.
//
// CUÁNDO USARLO:
// - Antes de procesar el registro de usuario
// - En el servicio de autenticación
// - Para validación automática en controladores
// - En pruebas unitarias de validación
//
// CÓMO USARLO:
// var validator = new UsuarioRegistroValidator(_context);
// var result = await validator.ValidateAsync(dto);
// if (!result.IsValid) return BadRequest(result.Errors);
//
// =====================================================================================

using FluentValidation;
using back_cabs.CRM.DTOs.Auth;
using back_cabs.CRM.enums;
using back_cabs.CRM.contexts;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.validators.Auth
{
    /// <summary>
    /// Validador para el registro de usuarios
    /// </summary>
    public class UsuarioRegistroValidator : AbstractValidator<UsuarioRegistroRequestDto>
    {
        private readonly ReadOnlyContext _readContext;

        public UsuarioRegistroValidator(ReadOnlyContext readContext)
        {
            _readContext = readContext;
            
            ConfigurarReglasValidacion();
        }

        /// <summary>
        /// Configura todas las reglas de validación
        /// </summary>
        private void ConfigurarReglasValidacion()
        {
            // Validación de nombre completo
            RuleFor(x => x.NombreCompleto)
                .NotEmpty()
                .WithMessage("El nombre completo es obligatorio")
                .Length(3, 200)
                .WithMessage("El nombre debe tener entre 3 y 200 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El nombre solo puede contener letras y espacios");

            // Validación de email
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El email es obligatorio")
                .EmailAddress()
                .WithMessage("Debe ser un email válido")
                .MaximumLength(255)
                .WithMessage("El email no puede exceder 255 caracteres")
                .MustAsync(async (email, cancellation) => await SerEmailUnico(email))
                .WithMessage("Ya existe un usuario registrado con este email");

            // Validación de contraseña
            RuleFor(x => x.Contrasena)
                .NotEmpty()
                .WithMessage("La contraseña es obligatoria")
                .MinimumLength(8)
                .WithMessage("La contraseña debe tener al menos 8 caracteres")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("La contraseña debe contener al menos: 1 minúscula, 1 mayúscula, 1 número y 1 carácter especial");

            // Validación de confirmación de contraseña
            RuleFor(x => x.ConfirmarContrasena)
                .NotEmpty()
                .WithMessage("La confirmación de contraseña es obligatoria")
                .Equal(x => x.Contrasena)
                .WithMessage("Las contraseñas no coinciden");

            // Validación de rol
            RuleFor(x => x.Rol)
                .NotEmpty()
                .WithMessage("El rol es obligatorio")
                .Must(SerRolValido)
                .WithMessage($"El rol debe ser uno de: {string.Join(", ", Enum.GetNames<RolUsuario>())}");

            // Validación de transmisión habilitada
            RuleFor(x => x.TransmisionHabilitada)
                .Must(SerTransmisionValida)
                .WithMessage($"La transmisión debe ser una de: {string.Join(", ", Enum.GetNames<TipoTransmision>())}");

            // Validación de consistencia: si no tiene licencia, no puede tener transmisión habilitada
            RuleFor(x => x)
                .Must(TenerConsistenciaLicenciaTransmision)
                .WithMessage("Si no tiene licencia de conducir, la transmisión debe ser 'Ninguna'")
                .OverridePropertyName(nameof(UsuarioRegistroRequestDto.TransmisionHabilitada));
        }

        /// <summary>
        /// Verifica si el email es único en el sistema
        /// </summary>
        private async Task<bool> SerEmailUnico(string email)
        {
            try
            {
                var existe = await _readContext.UsuariosAuth
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
                return !existe;
            }
            catch (Exception)
            {
                // En caso de error de BD, permitir la validación y que sea manejada en el servicio
                return true;
            }
        }

        /// <summary>
        /// Verifica si el rol es válido
        /// </summary>
        private static bool SerRolValido(string rol)
        {
            return Enum.TryParse<RolUsuario>(rol, true, out _);
        }

        /// <summary>
        /// Verifica si el tipo de transmisión es válido
        /// </summary>
        private static bool SerTransmisionValida(string transmision)
        {
            return Enum.TryParse<TipoTransmision>(transmision, true, out _);
        }

        /// <summary>
        /// Verifica consistencia entre licencia y transmisión
        /// </summary>
        private static bool TenerConsistenciaLicenciaTransmision(UsuarioRegistroRequestDto dto)
        {
            // Si no tiene licencia, la transmisión debe ser "Ninguna"
            if (!dto.LicenciaConducir)
            {
                return dto.TransmisionHabilitada.Equals("Ninguna", StringComparison.OrdinalIgnoreCase);
            }

            // Si tiene licencia, puede tener cualquier transmisión excepto "Ninguna"
            return !dto.TransmisionHabilitada.Equals("Ninguna", StringComparison.OrdinalIgnoreCase);
        }
    }
}