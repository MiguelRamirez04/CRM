// =====================================================================================
// SERVICE MONEDA - MonedaService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IMonedaService para lógica de negocio de monedas.
// Maneja mapeo entre entidades y DTOs, validaciones de negocio.
//
// DEPENDENCIAS:
// - IMonedaRepository: Para acceso a datos
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
    /// Implementación de IMonedaService para lógica de negocio de monedas.
    /// Maneja mapeo DTO, validaciones y reglas de negocio.
    /// </summary>
    public class MonedaService : IMonedaService
    {
        private readonly IMonedaRepository _repository;
        private readonly ILogger<MonedaService> _logger;

        public MonedaService(
            IMonedaRepository repository,
            ILogger<MonedaService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE BUSINESS LOGIC
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todas las monedas activas del catálogo mapeadas a DTOs
        /// </summary>
        /// <returns>Lista de monedas activas como DTOs de respuesta</returns>
        public async Task<IEnumerable<MonedaResponseDto>> GetAllMonedasAsync()
        {
            try
            {
                var monedas = await _repository.GetAllAsync();
                var dtos = monedas.Select(MapToDto).ToList();

                _logger.LogInformation("Obtenidas {Count} monedas activas para respuesta API", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener todas las monedas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una moneda por ID mapeada a DTO
        /// </summary>
        /// <param name="id">ID de la moneda</param>
        /// <returns>Moneda como DTO de respuesta o null si no existe</returns>
        public async Task<MonedaResponseDto?> GetMonedaByIdAsync(int id)
        {
            try
            {
                var moneda = await _repository.GetByIdAsync(id);

                if (moneda == null)
                {
                    _logger.LogWarning("Moneda no encontrada con ID: {Id}", id);
                    return null;
                }

                var dto = MapToDto(moneda);
                _logger.LogDebug("Moneda encontrada y mapeada: {NombreMoneda} (ID: {Id})", dto.NombreMoneda, dto.Id);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener moneda por ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 IMPLEMENTACIÓN DE BUSINESS VALIDATIONS
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si una moneda existe y está activa
        /// </summary>
        /// <param name="id">ID de la moneda a validar</param>
        /// <returns>true si existe y está activa</returns>
        public async Task<bool> ValidateMonedaExistsAsync(int id)
        {
            try
            {
                var exists = await _repository.ExistsAsync(id);

                if (!exists)
                {
                    _logger.LogWarning("Validación fallida: moneda no existe o no está activa (ID: {Id})", id);
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de moneda ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🛠️ MÉTODOS PRIVADOS DE MAPEO
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mapea una entidad Moneda a MonedaResponseDto
        /// </summary>
        /// <param name="moneda">Entidad de moneda a mapear</param>
        /// <returns>DTO de respuesta mapeado</returns>
        private static MonedaResponseDto MapToDto(Moneda moneda)
        {
            return new MonedaResponseDto
            {
                Id = moneda.Id,
                LegacyMonedaId = moneda.LegacyMonedaId,
                NombreMoneda = moneda.NombreMoneda,
                SimboloMoneda = moneda.SimboloMoneda,
                ClaveSat = moneda.ClaveSat,
                Decimales = moneda.Decimales,
                Activo = moneda.Activo
            };
        }

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - ConvertirMonedaAsync
        // - ObtenerTipoCambioAsync
        // - ValidarMonedaParaTransaccionAsync
    }
}