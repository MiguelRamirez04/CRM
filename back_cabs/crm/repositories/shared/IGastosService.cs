using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using CRM.DTOs.Response;

namespace back_cabs.CRM.Interfaces
{
    // Esta es tu interfaz original, CORREGIDA y renombrada
    public interface IGastoViaticoService
    {
        Task<GastoViaticoResponseDto> CreateViaticoAsync(GastoViaticoCreateRequestDto dto);

        Task<PaginatedResponseDto<GastoViaticoResponseDto>> GetViaticosAsync(
            int? usuarioId = null,
            TipoViatico? tipoViatico = null,
            EstadoGasto? estadoGasto = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? ordenId = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id);

        Task UpdateEstadoAsync(int id, EstadoGasto estado);
    }
}