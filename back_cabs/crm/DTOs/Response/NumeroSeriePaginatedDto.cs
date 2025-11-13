// =====================================================================================
// PAGINATED RESULT DTO - NumeroSeriePaginatedDto.cs
// =====================================================================================
//
// DTO específico para respuestas paginadas de NumeroSerie
//
// =====================================================================================

using CRM.DTOs.Response;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// Alias para compatibilidad con PaginatedResponseDto existente
    /// </summary>
    public class NumeroSeriePaginatedDto : PaginatedResponseDto<NumeroSerieResponseDto>
    {
        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public NumeroSeriePaginatedDto(List<NumeroSerieResponseDto> data, int page, int pageSize, int totalRecords)
        {
            Items = data;
            Pagina = page;
            ResultadosPorPagina = pageSize;
            TotalItems = totalRecords;
        }

        /// <summary>
        /// Constructor vacío
        /// </summary>
        public NumeroSeriePaginatedDto()
        {
        }
    }
}
