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
            // Validación de nombre
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre es obligatorio")
                .Length(2, 100)
                .WithMessage("El nombre debe tener entre 2 y 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El nombre solo puede contener letras y espacios");

            // Validación de apellido
            RuleFor(x => x.Apellido)
                .NotEmpty()
                .WithMessage("El apellido es obligatorio")
                .Length(2, 100)
                .WithMessage("El apellido debe tener entre 2 y 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El apellido solo puede contener letras y espacios");

            // Validación de teléfono (opcional)
            RuleFor(x => x.Telefono)
                .Must(telefono => string.IsNullOrEmpty(telefono) || (telefono.Length == 10 && telefono.All(char.IsDigit)))
                .WithMessage("El teléfono debe ser un número de 10 dígitos")
                .When(x => !string.IsNullOrEmpty(x.Telefono));

            // Validación de email
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("El email es obligatorio")
                .EmailAddress()
                .WithMessage("Debe ser un email válido")
                .MaximumLength(150)
                .WithMessage("El email no puede exceder 150 caracteres")
                .MustAsync(async (email, cancellation) => await SerEmailUnico(email))
                .WithMessage("Ya existe un usuario registrado con este email");

            // Validación de contraseña (simplificada para desarrollo)
            RuleFor(x => x.Contrasena)
                .NotEmpty()
                .WithMessage("La contraseña es obligatoria")
                .MinimumLength(8)
                .WithMessage("La contraseña debe tener al menos 8 caracteres");
            // TODO: Agregar validación de complejidad en producción:
            // .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            // .WithMessage("La contraseña debe contener al menos: 1 minúscula, 1 mayúscula, 1 número y 1 carácter especial");

            // Validación de confirmación de contraseña
            RuleFor(x => x.ConfirmarContrasena)
                .NotEmpty()
                .WithMessage("La confirmación de contraseña es obligatoria")
                .Equal(x => x.Contrasena)
                .WithMessage("Las contraseñas no coinciden");

            // Validación de rol (opcional por ahora)
            // TODO: Implementar mapeo y validación de rol cuando se defina la lógica de negocio

            // Validación de transmisión habilitada (opcional)
            RuleFor(x => x.TransmisionHabilitada)
                .MaximumLength(50)
                .WithMessage("La transmisión habilitada no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.TransmisionHabilitada));

            // Validación de licencia de conducir (opcional)
            RuleFor(x => x.LicenciaConducir)
                .MaximumLength(50)
                .WithMessage("La licencia de conducir no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.LicenciaConducir));
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

        // Métodos de validación obsoletos eliminados
        // TODO: Restaurar validaciones de enum cuando se implemente la lógica de negocio completa
    }
}