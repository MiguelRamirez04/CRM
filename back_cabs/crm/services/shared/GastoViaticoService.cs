using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Asegúrate de tener los usings correctos
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.Interfaces; // <-- Importante
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Para DbUpdateException

namespace back_cabs.CRM.Services.Shared
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio de viáticos.
    /// Orquesta validaciones y llama al repositorio para acceso a datos.
    /// </summary>
    // Fíjate en la implementación de la interfaz
    public class GastoViaticoService : IGastoViaticoService
    {
        // Se van los DbContext, entra el Repositorio
        private readonly IGastoViaticoRepository _repository;
        private readonly ILogger<GastoViaticoService> _logger;

        public GastoViaticoService(
            IGastoViaticoRepository repository, // <-- Inyectamos el repo
            ILogger<GastoViaticoService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GastoViaticoResponseDto> CreateViaticoAsync(GastoViaticoCreateRequestDto dto)
        {
            _logger.LogInformation("Iniciando creación de viático para usuario {UsuarioId}", dto.UsuarioId);

            // 1. Lógica de negocio/Validación (Esto se queda en el servicio)
            if (dto.TipoViatico == TipoViatico.ORDEN && !dto.OrdenId.HasValue)
                throw new ArgumentException("El campo OrdenId es obligatorio para viáticos por orden.", nameof(dto.OrdenId));

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("Debe incluir al menos un detalle de gasto.", nameof(dto.Detalles));

            // Llamamos al repositorio para la validación de BD
            var usuarioExiste = await _repository.UsuarioExistsAsync(dto.UsuarioId);
            if (!usuarioExiste)
            {
                _logger.LogWarning("Usuario con ID {UsuarioId} no existe.", dto.UsuarioId);
                throw new ArgumentException($"Usuario con ID {dto.UsuarioId} no existe.", nameof(dto.UsuarioId));
            }

            // 2. Mapeo (Esto se queda en el servicio)
            var viaticoModel = new GastoViatico
            {
                TipoViatico = dto.TipoViatico,
                OrdenId = dto.OrdenId,
                UsuarioId = dto.UsuarioId,
                TieneFactura = dto.TieneFactura,
                Descripcion = dto.Descripcion,
                ProveedorNombre = dto.ProveedorNombre,
                Fecha = dto.Fecha,
                LugarDestino = dto.LugarDestino,
                Observaciones = dto.Observaciones,
                Detalles = dto.Detalles.Select(d => new GastoViaticoDetalle
                {
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            };

            try
            {
                // 3. Llamada al Repositorio (Guardado)
                var viaticoCreado = await _repository.CreateViaticoAsync(viaticoModel);

                _logger.LogInformation($"Viático creado exitosamente con ID {viaticoCreado.Id}");
                
                // 4. Mapeo de respuesta (Esto se queda en el servicio)
                return MapViaticoToResponseDto(viaticoCreado);
            }
            catch (Exception ex) // Captura la excepción relanzada por el repo
            {
                _logger.LogError(ex, "Error al crear viático para usuario {UsuarioId}", dto.UsuarioId);
                throw new DbUpdateException("Error al guardar el viático en la base de datos.", ex);
            }
        }

        public async Task<PaginatedResponseDto<GastoViaticoResponseDto>> GetViaticosAsync(
            int? usuarioId = null, TipoViatico? tipoViatico = null, EstadoGasto? estadoGasto = null,
            DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? ordenId = null,
            int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation("Consultando viáticos con filtros...");

            // 1. Llamada al Repositorio (Lectura)
            // (Nota: El filtro 'ordenId' no estaba en tu lógica de query original, lo mantengo así)
            var (viaticos, totalCount) = await _repository.GetViaticosFilteredAsync(
                usuarioId, tipoViatico, estadoGasto, fechaDesde, fechaHasta, pageNumber, pageSize);

            // 2. Mapeo de respuesta
            var resultDto = viaticos.Select(MapViaticoToResponseDto).ToList();

            _logger.LogInformation("Se encontraron {Count} viáticos", resultDto.Count);
            return new PaginatedResponseDto<GastoViaticoResponseDto>
            {
                Items = resultDto,
                TotalItems = totalCount,
                Pagina = pageNumber,
                ResultadosPorPagina = pageSize
            };
        }

        public async Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id)
        {
            _logger.LogInformation("Consultando viático con ID {ViaticoId}", id);

            // 1. Llamada al Repositorio
            var viatico = await _repository.GetViaticoByIdReadOnlyAsync(id);

            if (viatico == null)
            {
                _logger.LogWarning("Viático con ID {ViaticoId} no encontrado", id);
                return null;
            }

            // 2. Mapeo de respuesta
            return MapViaticoToResponseDto(viatico);
        }

        public async Task UpdateEstadoAsync(int id, EstadoGasto estado)
        {
            _logger.LogInformation("Actualizando estado de viático {ViaticoId} a {Estado}", id, estado);

            // 1. Llamada al Repositorio (para escritura)
            var viatico = await _repository.GetViaticoByIdForUpdateAsync(id);
            if (viatico == null)
                throw new KeyNotFoundException($"Viático con ID {id} no encontrado.");

            // 2. Lógica de negocio (Validación)
            if (viatico.EstadoGasto == EstadoGasto.PAGADO && estado != EstadoGasto.PAGADO)
                throw new ArgumentException("No se puede cambiar el estado de un viático pagado.", nameof(estado));

            // 3. Modificación del modelo
            viatico.EstadoGasto = estado;

            // 4. Llamada al Repositorio (Guardado)
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Estado de viático {ViaticoId} actualizado exitosamente", id);
        }

        /// <summary>
        /// Método privado de ayuda para mapear el modelo al DTO de respuesta.
        /// </summary>
        private GastoViaticoResponseDto MapViaticoToResponseDto(GastoViatico viatico)
        {
            return new GastoViaticoResponseDto
            {
                Id = viatico.Id,
                TipoViatico = viatico.TipoViatico,
                OrdenId = viatico.OrdenId,
                UsuarioId = viatico.UsuarioId,
                TieneFactura = viatico.TieneFactura,
                Descripcion = viatico.Descripcion,
                ProveedorNombre = viatico.ProveedorNombre,
                Fecha = viatico.Fecha,
                FechaRegistro = viatico.FechaRegistro,
                KmRecorridos = viatico.KmRecorridos,
                LugarDestino = viatico.LugarDestino,
                EstadoGasto = viatico.EstadoGasto,
                DocumentoId = viatico.DocumentoId,
                Observaciones = viatico.Observaciones,
                Detalles = viatico.Detalles.Select(d => new GastoViaticoResponseDto.GastoViaticoDetalleResponseDto
                {
                    Id = d.Id,
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            };
        }
    }
}