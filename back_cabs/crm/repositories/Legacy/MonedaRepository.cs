// =====================================================================================
// REPOSITORY MONEDA - MonedaRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IMonedaRepository para operaciones de acceso a datos de monedas.
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
    /// Implementación de IMonedaRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Monedas del catálogo legacy.
    /// </summary>
    public class MonedaRepository : IMonedaRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<MonedaRepository> _logger;

        public MonedaRepository(
            ReadOnlyContext readContext,
            ILogger<MonedaRepository> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todas las monedas activas del catálogo legacy
        /// </summary>
        /// <returns>Lista de monedas activas ordenadas por nombre</returns>
        public async Task<IEnumerable<Moneda>> GetAllAsync()
        {
            try
            {
                var monedas = await _readContext.Monedas
                    .AsNoTracking()
                    .Where(m => m.Activo == true) // Solo monedas activas
                    .OrderBy(m => m.NombreMoneda)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} monedas activas", monedas.Count);
                return monedas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las monedas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una moneda por su ID del catálogo legacy
        /// </summary>
        /// <param name="id">ID de la moneda</param>
        /// <returns>Moneda encontrada o null si no existe</returns>
        public async Task<Moneda?> GetByIdAsync(int id)
        {
            try
            {
                var moneda = await _readContext.Monedas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id && m.Activo == true);

                if (moneda != null)
                {
                    _logger.LogDebug("Moneda encontrada: {NombreMoneda} (ID: {Id})", moneda.NombreMoneda, moneda.Id);
                }
                else
                {
                    _logger.LogDebug("Moneda no encontrada con ID: {Id}", id);
                }

                return moneda;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener moneda por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una moneda con el ID especificado en el catálogo legacy
        /// </summary>
        /// <param name="id">ID de la moneda a verificar</param>
        /// <returns>true si existe y está activa, false en caso contrario</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Monedas
                    .AnyAsync(m => m.Id == id && m.Activo == true);

                _logger.LogDebug("Verificación de existencia para moneda ID {Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de moneda ID: {Id}", id);
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