namespace Crm.Api.DTOs.Common;

/// <summary>
/// DTO para respuestas paginadas
/// </summary>
/// <typeparam name="T">Tipo de los elementos de la lista</typeparam>
public record PaginatedDto<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    
    /// <summary>
    /// Número de página actual (base 1)
    /// </summary>
    public int PageNumber { get; init; }
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; init; }
    
    /// <summary>
    /// Total de elementos en todas las páginas
    /// </summary>
    public int TotalCount { get; init; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// Número de la primera página
    /// </summary>
    public int FirstPage => 1;
    
    /// <summary>
    /// Número de la última página
    /// </summary>
    public int LastPage => TotalPages;
    
    /// <summary>
    /// Metadatos adicionales de paginación
    /// </summary>
    public PaginationMetadata Metadata => new()
    {
        PageNumber = PageNumber,
        PageSize = PageSize,
        TotalCount = TotalCount,
        TotalPages = TotalPages,
        HasPreviousPage = HasPreviousPage,
        HasNextPage = HasNextPage
    };

    /// <summary>
    /// Crea una respuesta paginada
    /// </summary>
    public static PaginatedDto<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
        => new()
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

    /// <summary>
    /// Crea una respuesta paginada vacía
    /// </summary>
    public static PaginatedDto<T> Empty(int pageNumber = 1, int pageSize = 10)
        => new()
        {
            Items = Array.Empty<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
}

/// <summary>
/// Metadatos de paginación
/// </summary>
public record PaginationMetadata
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
