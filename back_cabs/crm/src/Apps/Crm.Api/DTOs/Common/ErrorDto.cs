namespace Crm.Api.DTOs.Common;

/// <summary>
/// DTO para representar errores de la API
/// </summary>
public record ErrorDto
{
    /// <summary>
    /// Código de error único
    /// </summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>
    /// Mensaje descriptivo del error
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Detalles adicionales del error
    /// </summary>
    public string? Details { get; init; }
    
    /// <summary>
    /// Campo específico que causó el error (para errores de validación)
    /// </summary>
    public string? Field { get; init; }
    
    /// <summary>
    /// Timestamp del error
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Crea un error de validación
    /// </summary>
    public static ErrorDto Validation(string field, string message)
        => new()
        {
            Code = "VALIDATION_ERROR",
            Message = message,
            Field = field
        };

    /// <summary>
    /// Crea un error de recurso no encontrado
    /// </summary>
    public static ErrorDto NotFound(string resource, object id)
        => new()
        {
            Code = "NOT_FOUND",
            Message = $"{resource} con ID {id} no encontrado"
        };

    /// <summary>
    /// Crea un error de autorización
    /// </summary>
    public static ErrorDto Unauthorized(string? details = null)
        => new()
        {
            Code = "UNAUTHORIZED",
            Message = "No autorizado para realizar esta operación",
            Details = details
        };

    /// <summary>
    /// Crea un error genérico
    /// </summary>
    public static ErrorDto General(string message, string? code = null)
        => new()
        {
            Code = code ?? "GENERAL_ERROR",
            Message = message
        };
}
