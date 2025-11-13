// =====================================================================================
// INTERFAZ SERVICIO NÚMERO SERIE - INumeroSerieService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato para el servicio de números de serie.
// Capa de lógica de negocio entre controlador y repositorio.
//
// RESPONSABILIDADES:
// - Gestión de cache Redis (Cache-Aside Pattern)
// - Mapeo de entidades a DTOs
// - Lógica de negocio adicional si es necesaria
//
// MÉTODOS:
// - GetNumeroSeriePaginatedAsync(): Obtiene todos con paginación y cache 30min
// - SearchNumeroSeriePaginatedAsync(searchTerm): Búsqueda paginada con cache 60min
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz del servicio para números de serie
    /// Define operaciones de negocio con integración de cache Redis
    /// </summary>
    public interface INumeroSerieService
    {
        /// <summary>
        /// Obtiene números de serie activos paginados con cache Redis
        /// TTL: 30 minutos por página
        /// Cache Key: numeros_serie:page:{page}:size:{pageSize}
        /// </summary>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <returns>Respuesta paginada con metadata</returns>
        Task<PaginatedResponseDto<NumeroSerieResponseDto>> GetNumeroSeriePaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Busca números de serie por término con paginación y cache Redis
        /// TTL: 60 minutos por página
        /// Cache Key: numeros_serie:search:{searchTerm}:page:{page}:size:{pageSize}
        /// </summary>
        /// <param name="searchTerm">Término a buscar</param>
        /// <param name="page">Número de página (1-based)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <returns>Respuesta paginada con metadata</returns>
        Task<PaginatedResponseDto<NumeroSerieResponseDto>> SearchNumeroSeriePaginatedAsync(string searchTerm, int page, int pageSize);
    }
}
