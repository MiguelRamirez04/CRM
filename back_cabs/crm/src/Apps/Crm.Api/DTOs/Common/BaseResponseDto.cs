namespace Crm.Api.DTOs.Common;

/// <summary>
/// Respuesta base estándar para todos los endpoints de la API
/// </summary>
/// <typeparam name="T">Tipo de datos de la respuesta</typeparam>
public record BaseResponseDto<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Datos de la respuesta (null si hay error)
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Detalles del error (null si es exitoso)
    /// </summary>
    public ErrorDto? Error { get; init; }
    
    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static BaseResponseDto<T> CreateSuccess(T data, string message = "Operación exitosa")
        => new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static BaseResponseDto<T> CreateError(ErrorDto error)
        => new()
        {
            Success = false,
            Message = error.Message,
            Error = error
        };

    /// <summary>
    /// Crea una respuesta de error simple
    /// </summary>
    public static BaseResponseDto<T> CreateError(string message, string? code = null)
        => new()
        {
            Success = false,
            Message = message,
            Error = ErrorDto.General(message, code)
        };
}
