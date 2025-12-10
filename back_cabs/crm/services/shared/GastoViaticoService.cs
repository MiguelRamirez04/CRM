using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.Interfaces;
using CRM.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.Services.Shared
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio de viáticos
    /// Implementa las operaciones CRUD mínimas necesarias para la API.
    /// </summary>
    public class GastoViaticoService : IGastoViaticoService
    {
        private readonly IGastoViaticoRepository _repository;
        private readonly ILogger<GastoViaticoService> _logger;

        public GastoViaticoService(IGastoViaticoRepository repository, ILogger<GastoViaticoService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GastoViaticoResponseDto> CreateViaticoAsync(GastoViaticoCreateRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.MontoTotal < 0) throw new ArgumentException("El monto total no puede ser negativo", nameof(dto.MontoTotal));
            if (string.IsNullOrWhiteSpace(dto.Gastos)) throw new ArgumentException("El campo Gastos es requerido", nameof(dto.Gastos));

            var model = new GastoViatico
            {
                OrdenId = dto.OrdenId,
                VehiculoId = dto.VehiculoId, // ✅ Nuevo campo
                TieneFactura = dto.TieneFactura,
                Descripcion = dto.Descripcion,
                ProveedorNombre = dto.ProveedorNombre,
                Fecha = dto.Fecha,
                KmRecorridos = dto.KmRecorridos,
                Gastos = dto.Gastos,
                MontoTotal = dto.MontoTotal,
                LugarDestino = dto.LugarDestino
            };

            try
            {
                var created = await _repository.CreateViaticoAsync(model);
                _logger.LogInformation("Viático creado con ID {Id}", created.Id);
                return MapViaticoToResponseDto(created);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar viático en la base de datos");
                throw;
            }
        }

        public async Task<PaginatedResponseDto<GastoViaticoResponseDto>> GetViaticosAsync(int? ordenId = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int pageNumber = 1, int pageSize = 10)
        {
            var (items, total) = await _repository.GetViaticosFilteredAsync(ordenId, fechaDesde, fechaHasta, pageNumber, pageSize);
            var dtoItems = items.Select(MapViaticoToResponseDto).ToList();
            return new PaginatedResponseDto<GastoViaticoResponseDto>
            {
                Items = dtoItems,
                Pagina = pageNumber,
                ResultadosPorPagina = pageSize,
                TotalItems = total
            };
        }

        public async Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id)
        {
            // ✅ Usar método con Include para cargar vehículo
            var viatico = await _repository.GetViaticoConVehiculoAsync(id);
            if (viatico == null) return null;
            return MapViaticoToResponseDto(viatico);
        }

        public async Task<GastoViaticoResponseDto?> UpdateViaticoAsync(int id, GastoViaticoUpdateRequestDto dto)
        {
            var viatico = await _repository.GetViaticoByIdForUpdateAsync(id);
            if (viatico == null) return null;

            // Actualizar campos permitidos
            viatico.OrdenId = dto.OrdenId;
            viatico.VehiculoId = dto.VehiculoId; // ✅ Nuevo campo
            viatico.TieneFactura = dto.TieneFactura;
            viatico.Descripcion = dto.Descripcion;
            viatico.ProveedorNombre = dto.ProveedorNombre;
            viatico.Fecha = dto.Fecha;
            viatico.KmRecorridos = dto.KmRecorridos;
            viatico.Gastos = dto.Gastos;
            viatico.MontoTotal = dto.MontoTotal;
            viatico.LugarDestino = dto.LugarDestino;

            await _repository.SaveChangesAsync();
            _logger.LogInformation("Viático {Id} actualizado", id);
            
            return MapViaticoToResponseDto(viatico);
        }

        private GastoViaticoResponseDto MapViaticoToResponseDto(GastoViatico v)
        {
            return new GastoViaticoResponseDto
            {
                Id = v.Id,
                OrdenId = v.OrdenId,
                VehiculoId = v.VehiculoId, // ✅ Nuevo campo
                VehiculoNombre = v.Vehiculo?.NombreVehiculo, // ✅ Datos del vehículo
                VehiculoPlacas = v.Vehiculo?.Placas, // ✅ Datos del vehículo
                TieneFactura = v.TieneFactura,
                Descripcion = v.Descripcion,
                ProveedorNombre = v.ProveedorNombre,
                Fecha = v.Fecha,
                KmRecorridos = v.KmRecorridos,
                Gastos = v.Gastos ?? string.Empty,
                MontoTotal = v.MontoTotal,
                LugarDestino = v.LugarDestino
            };
        }
    }
}