namespace Crm.Api.DTOs.Common;

/// <summary>
/// DTO específico para errores de validación
/// </summary>
public record ValidationErrorDto
{
    /// <summary>
    /// Campo que falló la validación
    /// </summary>
    public string Field { get; init; } = string.Empty;
    
    /// <summary>
    /// Mensaje de error específico del campo
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Código de error de validación
    /// </summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>
    /// Valor que causó el error
    /// </summary>
    public object? AttemptedValue { get; init; }

    /// <summary>
    /// Crea un error de validación
    /// </summary>
    public static ValidationErrorDto Create(string field, string message, string code = "VALIDATION_ERROR", object? attemptedValue = null)
        => new()
        {
            Field = field,
            Message = message,
            Code = code,
            AttemptedValue = attemptedValue
        };
}

/// <summary>
/// Respuesta de error que contiene múltiples errores de validación
/// </summary>
public record ValidationErrorResponseDto
{
    /// <summary>
    /// Indica que es un error de validación
    /// </summary>
    public bool Success => false;
    
    /// <summary>
    /// Mensaje general del error
    /// </summary>
    public string Message { get; init; } = "Errores de validación";
    
    /// <summary>
    /// Lista de errores de validación por campo
    /// </summary>
    public IEnumerable<ValidationErrorDto> Errors { get; init; } = Array.Empty<ValidationErrorDto>();
    
    /// <summary>
    /// Timestamp del error
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total de errores de validación
    /// </summary>
    public int ErrorCount => Errors.Count();

    /// <summary>
    /// Crea una respuesta de errores de validación
    /// </summary>
    public static ValidationErrorResponseDto Create(IEnumerable<ValidationErrorDto> errors, string? message = null)
        => new()
        {
            Message = message ?? $"Se encontraron {errors.Count()} errores de validación",
            Errors = errors
        };

    /// <summary>
    /// Crea una respuesta de error de validación único
    /// </summary>
    public static ValidationErrorResponseDto CreateSingle(string field, string message, string code = "VALIDATION_ERROR")
        => new()
        {
            Message = "Error de validación",
            Errors = new[] { ValidationErrorDto.Create(field, message, code) }
        };

    /// <summary>
    /// Convierte a diccionario para compatibilidad con ModelState
    /// </summary>
    public Dictionary<string, string[]> ToDictionary()
        => Errors
            .GroupBy(e => e.Field)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );
}
