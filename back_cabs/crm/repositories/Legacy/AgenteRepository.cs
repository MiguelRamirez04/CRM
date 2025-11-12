// =====================================================================================
// REPOSITORY AGENTE - AgenteRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IAgenteRepository para operaciones de acceso a datos de agentes.
// Usa Entity Framework Core con el patrón CQRS (ReadOnlyContext para lecturas).
//
// DEPENDENCIAS:
// - ReadOnlyContext: Para operaciones de lectura
// - ILogger: Para logging de operaciones
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Implementación de IAgenteRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Agentes del catálogo legacy.
    /// </summary>
    public class AgenteRepository : IAgenteRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<AgenteRepository> _logger;

        public AgenteRepository(
            ReadOnlyContext readContext,
            ILogger<AgenteRepository> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los agentes activos del catálogo legacy
        /// </summary>
        /// <returns>Lista de agentes activos ordenados por nombre</returns>
        public async Task<IEnumerable<Agente>> GetAllAsync()
        {
            try
            {
                var agentes = await _readContext.Agentes
                    .AsNoTracking()
                    .Where(a => a.Activo == true) // Solo agentes activos
                    .OrderBy(a => a.NombreAgente)
                    .ToListAsync();

                _logger.LogDebug("Obtenidos {Count} agentes activos", agentes.Count);
                return agentes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los agentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un agente por su ID del catálogo legacy
        /// </summary>
        /// <param name="id">ID del agente</param>
        /// <returns>Agente encontrado o null si no existe</returns>
        public async Task<Agente?> GetByIdAsync(int id)
        {
            try
            {
                var agente = await _readContext.Agentes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo == true);

                if (agente != null)
                {
                    _logger.LogDebug("Agente encontrado: {NombreAgente} (ID: {Id})", agente.NombreAgente, agente.Id);
                }
                else
                {
                    _logger.LogDebug("Agente no encontrado con ID: {Id}", id);
                }

                return agente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agente por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un agente con el ID especificado en el catálogo legacy
        /// </summary>
        /// <param name="id">ID del agente a verificar</param>
        /// <returns>true si existe y está activo, false en caso contrario</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Agentes
                    .AnyAsync(a => a.Id == id && a.Activo == true);

                _logger.LogDebug("Verificación de existencia para agente ID {Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de agente ID: {Id}", id);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // ✏️ COMMANDS (Escritura) - Futuras implementaciones
        // ---------------------------------------------------------------------

        // Métodos para operaciones de escritura se agregarían aquí cuando sean necesarias
        // - CreateAsync
        // - UpdateAsync
        // - DeleteAsync
    }
}