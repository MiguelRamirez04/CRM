// =====================================================================================
// DTO CLIENTES COMPLETOS PAGINADO - ClientesCompletosPaginadoDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el DTO que representa la salida del procedimiento almacenado 
// usp_GetClientesCompletos_Paginado. Este DTO se usa para la comunicación 
// entre la API y los clientes.
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Response;

/// <summary>
/// DTO para representar los resultados paginados del procedimiento almacenado de clientes
/// </summary>
public class ClientesCompletosPaginadoDto
{
    public int ClienteId { get; set; }
    public string? NombreComercial { get; set; }
    public string? RFC { get; set; }
    public bool Activo { get; set; }
    public int? LegacyClientId { get; set; }
    public string? Calle { get; set; }
    public string? NumeroExterior { get; set; }
    public string? Colonia { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? Pais { get; set; }
    public string? TelefonoPrincipal { get; set; }
    public string? EmailPrincipal { get; set; }
}

/// <summary>
/// Parámetros para la consulta de clientes paginados
/// </summary>
public class ClientesCompletosPaginadoRequestDto
{
    [MaxLength(200)]
    public string? NombreBusqueda { get; set; }
    
    [MaxLength(13)]
    public string? RFCBusqueda { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;
    
    [Range(1, 100)]
    public int ResultadosPorPagina { get; set; } = 20;
}

/// <summary>
/// Respuesta paginada para la consulta de clientes
/// </summary>
public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Pagina { get; set; }
    public int ResultadosPorPagina { get; set; }
    public int? TotalItems { get; set; }
    public int? TotalPaginas => TotalItems.HasValue ? (int)Math.Ceiling((double)TotalItems.Value / ResultadosPorPagina) : null;
}