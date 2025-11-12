// =====================================================================================
// SERVICE ALMACEN - AlmacenService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IAlmacenService para lógica de negocio de almacenes.
// Maneja mapeo entre entidades y DTOs, validaciones de negocio.
//
// DEPENDENCIAS:
// - IAlmacenRepository: Para acceso a datos
// - ILogger: Para logging de operaciones
//
// =====================================================================================

using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Implementación de IAlmacenService para lógica de negocio de almacenes.
    /// Maneja mapeo DTO, validaciones y reglas de negocio.
    /// </summary>
    public class AlmacenService : IAlmacenService
    {
        private readonly IAlmacenRepository _repository;
        private readonly ILogger<AlmacenService> _logger;

        public AlmacenService(
            IAlmacenRepository repository,
            ILogger<AlmacenService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE BUSINESS LOGIC
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los almacenes activos del catálogo mapeados a DTOs
        /// </summary>
        /// <returns>Lista de almacenes activos como DTOs de respuesta</returns>
        public async Task<IEnumerable<AlmacenResponseDto>> GetAllAlmacenesAsync()
        {
            try
            {
                var almacenes = await _repository.GetAllAsync();
                var dtos = almacenes.Select(MapToDto).ToList();

                _logger.LogInformation("Obtenidos {Count} almacenes activos para respuesta API", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener todos los almacenes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un almacén por ID mapeado a DTO
        /// </summary>
        /// <param name="id">ID del almacén</param>
        /// <returns>Almacén como DTO de respuesta o null si no existe</returns>
        public async Task<AlmacenResponseDto?> GetAlmacenByIdAsync(int id)
        {
            try
            {
                var almacen = await _repository.GetByIdAsync(id);

                if (almacen == null)
                {
                    _logger.LogWarning("Almacén no encontrado con ID: {Id}", id);
                    return null;
                }

                var dto = MapToDto(almacen);
                _logger.LogDebug("Almacén encontrado y mapeado: {NombreAlmacen} (ID: {Id})", dto.NombreAlmacen, dto.Id);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener almacén por ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 IMPLEMENTACIÓN DE BUSINESS VALIDATIONS
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si un almacén existe y está activo
        /// </summary>
        /// <param name="id">ID del almacén a validar</param>
        /// <returns>true si existe y está activo</returns>
        public async Task<bool> ValidateAlmacenExistsAsync(int id)
        {
            try
            {
                var exists = await _repository.ExistsAsync(id);

                if (!exists)
                {
                    _logger.LogWarning("Validación fallida: almacén no existe o no está activo (ID: {Id})", id);
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de almacén ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🛠️ MÉTODOS PRIVADOS DE MAPEO
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mapea una entidad Almacen a AlmacenResponseDto
        /// </summary>
        /// <param name="almacen">Entidad de almacén a mapear</param>
        /// <returns>DTO de respuesta mapeado</returns>
        private static AlmacenResponseDto MapToDto(Almacen almacen)
        {
            return new AlmacenResponseDto
            {
                Id = almacen.Id,
                LegacyAlmacenId = almacen.LegacyAlmacenId,
                CodigoAlmacen = almacen.CodigoAlmacen,
                NombreAlmacen = almacen.NombreAlmacen,
                FechaAlta = almacen.FechaAlta,
                Clasificacion1 = almacen.Clasificacion1,
                Activo = almacen.Activo
            };
        }

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - GetAlmacenesByClasificacionAsync
        // - GetAlmacenesActivosAsync
        // - ValidarAlmacenParaMovimientoAsync
    }
}