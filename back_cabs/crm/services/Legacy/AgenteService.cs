// =====================================================================================
// SERVICE AGENTE - AgenteService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IAgenteService para lógica de negocio de agentes.
// Maneja mapeo entre entidades y DTOs, validaciones de negocio.
//
// DEPENDENCIAS:
// - IAgenteRepository: Para acceso a datos
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
    /// Implementación de IAgenteService para lógica de negocio de agentes.
    /// Maneja mapeo DTO, validaciones y reglas de negocio.
    /// </summary>
    public class AgenteService : IAgenteService
    {
        private readonly IAgenteRepository _repository;
        private readonly ILogger<AgenteService> _logger;

        public AgenteService(
            IAgenteRepository repository,
            ILogger<AgenteService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE BUSINESS LOGIC
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los agentes activos del catálogo mapeados a DTOs
        /// </summary>
        /// <returns>Lista de agentes activos como DTOs de respuesta</returns>
        public async Task<IEnumerable<AgenteResponseDto>> GetAllAgentesAsync()
        {
            try
            {
                var agentes = await _repository.GetAllAsync();
                var dtos = agentes.Select(MapToDto).ToList();

                _logger.LogInformation("Obtenidos {Count} agentes activos para respuesta API", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener todos los agentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un agente por ID mapeado a DTO
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente como DTO de respuesta o null si no existe</returns>
        public async Task<AgenteResponseDto?> GetAgenteByIdAsync(int id)
        {
            try
            {
                var agente = await _repository.GetByIdAsync(id);

                if (agente == null)
                {
                    _logger.LogWarning("Agente no encontrado con ID: {Id}", id);
                    return null;
                }

                var dto = MapToDto(agente);
                _logger.LogDebug("Agente encontrado y mapeado: {NombreAgente} (ID: {Id})", dto.NombreAgente, dto.Id);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en lógica de negocio al obtener agente por ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🔍 IMPLEMENTACIÓN DE BUSINESS VALIDATIONS
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida si un agente existe y está activo
        /// </summary>
        /// <param name="id">ID del agente a validar</param>
        /// <returns>true si existe y está activo</returns>
        public async Task<bool> ValidateAgenteExistsAsync(int id)
        {
            try
            {
                var exists = await _repository.ExistsAsync(id);

                if (!exists)
                {
                    _logger.LogWarning("Validación fallida: agente no existe o no está activo (ID: {Id})", id);
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar existencia de agente ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // 🛠️ MÉTODOS PRIVADOS DE MAPEO
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mapea una entidad Agente a AgenteResponseDto
        /// </summary>
        /// <param name="agente">Entidad de agente a mapear</param>
        /// <returns>DTO de respuesta mapeado</returns>
        private static AgenteResponseDto MapToDto(Agente agente)
        {
            return new AgenteResponseDto
            {
                Id = agente.Id,
                LegacyAgenteId = agente.LegacyAgenteId,
                CodigoAgente = agente.CodigoAgente,
                NombreAgente = agente.NombreAgente,
                FechaAlta = agente.FechaAlta,
                TipoAgente = agente.TipoAgente,
                TextoExtra2 = agente.TextoExtra2,
                Activo = agente.Activo
            };
        }

        // ---------------------------------------------------------------------
        // ✏️ BUSINESS OPERATIONS - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones complejas de negocio se agregarían aquí cuando sean necesarias
        // - GetAgentesByTipoAsync
        // - GetAgentesActivosAsync
        // - ValidarAgenteParaAsignacionAsync
    }
}