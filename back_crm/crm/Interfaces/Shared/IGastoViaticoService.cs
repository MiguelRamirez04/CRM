using System;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gastos de viáticos
    /// </summary>
    public interface IGastoViaticoService
    {
        Task<GastoViaticoResponseDto> CreateViaticoAsync(GastoViaticoCreateRequestDto dto);
        Task<PaginatedResponseDto<GastoViaticoResponseDto>> GetViaticosAsync(
            int? ordenId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id);
        Task<GastoViaticoResponseDto?> UpdateViaticoAsync(int id, GastoViaticoUpdateRequestDto dto);
    }
}
