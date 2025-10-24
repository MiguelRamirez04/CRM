// =====================================================================================
// VALIDADOR DE VEHÍCULO - VehiculoCreateDtoValidator.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las reglas de validación de FluentValidation para el DTO VehiculoCreateDto.
// Valida que todos los datos necesarios para crear un vehículo sean correctos,
// incluyendo placas únicas, año válido, y campos requeridos.
//
// CUÁNDO SE USA:
// - Se ejecuta automáticamente antes de que el controlador procese el request
// - Valida el payload JSON enviado al endpoint POST /api/vehiculos
// - Previene datos inválidos antes de llegar a la lógica de negocio
//
// EJEMPLO DE USO:
// El validador se registra automáticamente y se ejecuta cuando el modelo es VehiculoCreateDto
//
// =====================================================================================

using FluentValidation;
using CRM.DTOs.Request;

namespace back_cabs.CRM.Validators.Fleet;

/// <summary>
/// Validador de FluentValidation para VehiculoRequestDto
/// </summary>
public class VehiculoRequestDtoValidator : AbstractValidator<VehiculoRequestDto>
{
    public VehiculoRequestDtoValidator()
    {
        // ============================================================
        // VALIDACIONES DE TIPO DE VEHÍCULO
        // ============================================================
        
        RuleFor(x => x.TipoVehiculo)
            .MaximumLength(50)
            .WithMessage("El tipo de vehículo no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TipoVehiculo));

        // ============================================================
        // VALIDACIONES DE TRANSMISIÓN
        // ============================================================
        
        RuleFor(x => x.Transmision)
            .MaximumLength(20)
            .WithMessage("La transmisión no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Transmision));

        // ============================================================
        // VALIDACIONES DE PLACAS
        // ============================================================
        
        RuleFor(x => x.Placas)
            .MaximumLength(20)
            .WithMessage("Las placas no pueden exceder 20 caracteres")
            .Matches(@"^[A-Z0-9\-]+$")
            .WithMessage("Las placas solo pueden contener letras mayúsculas, números y guiones")
            .When(x => !string.IsNullOrEmpty(x.Placas));

        // ============================================================
        // VALIDACIONES DE OBSERVACIONES
        // ============================================================
        
        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }
}
