// =====================================================================================
// ATRIBUTOS VALIDACION - ValidationAttributes.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define atributos de validación personalizados para las clases DTO.
// Permite realizar validaciones condicionales y específicas para el dominio.
//
// CUÁNDO USARLO:
// - Cuando necesites validación condicional en DTOs
// - Cuando requieras lógica de validación personalizada
// - Para mejorar la calidad de datos de entrada en APIs
//
// CÓMO USARLO:
// [RequiredIf("OtroPropiedad", true, ErrorMessage = "Este campo es requerido cuando OtroPropiedad es true")]
// public string? Propiedad { get; set; }
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.Validation
{
    /// <summary>
    /// Atributo para validación condicional: requiere un campo sólo cuando otra propiedad cumple cierta condición
    /// </summary>
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependencyProperty;
        private readonly object _targetValue;

        /// <summary>
        /// Crea una instancia de RequiredIfAttribute
        /// </summary>
        /// <param name="dependencyProperty">Nombre de la propiedad de la que depende</param>
        /// <param name="targetValue">Valor que debe tener la propiedad dependiente para activar la validación</param>
        public RequiredIfAttribute(string dependencyProperty, object targetValue)
        {
            _dependencyProperty = dependencyProperty;
            _targetValue = targetValue;
        }

        /// <summary>
        /// Método de validación personalizado
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Obtener la propiedad dependiente usando reflection
            var dependencyPropertyInfo = validationContext.ObjectType.GetProperty(_dependencyProperty);

            if (dependencyPropertyInfo == null)
            {
                return new ValidationResult($"Propiedad dependiente {_dependencyProperty} no encontrada");
            }

            // Obtener el valor de la propiedad dependiente
            var dependencyValue = dependencyPropertyInfo.GetValue(validationContext.ObjectInstance);

            // Validar según la condición
            if (Equals(dependencyValue, _targetValue))
            {
                // La condición se cumple, validar que el valor no sea null
                if (value == null || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue)))
                {
                    var errorMessage = ErrorMessage ?? $"{validationContext.DisplayName} es obligatorio cuando {_dependencyProperty} es {_targetValue}";
                    return new ValidationResult(errorMessage);
                }
            }

            // Si la condición no se cumple o el valor es válido
            return ValidationResult.Success;
        }
    }
}