// =====================================================================================
// REPOSITORY ALMACEN - AlmacenRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa IAlmacenRepository para operaciones de acceso a datos de almacenes.
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
    /// Implementación de IAlmacenRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Almacenes del catálogo legacy.
    /// </summary>
    public class AlmacenRepository : IAlmacenRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<AlmacenRepository> _logger;

        public AlmacenRepository(
            ReadOnlyContext readContext,
            ILogger<AlmacenRepository> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // 📖 IMPLEMENTACIÓN DE QUERIES (Lectura)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene todos los almacenes activos del catálogo legacy
        /// </summary>
        /// <returns>Lista de almacenes activos ordenados por nombre</returns>
        public async Task<IEnumerable<Almacen>> GetAllAsync()
        {
            try
            {
                var almacenes = await _readContext.Almacenes
                    .AsNoTracking()
                    .Where(a => a.Activo == true) // Solo almacenes activos
                    .OrderBy(a => a.NombreAlmacen)
                    .ToListAsync();

                _logger.LogDebug("Obtenidos {Count} almacenes activos", almacenes.Count);
                return almacenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los almacenes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un almacén por su ID del catálogo legacy
        /// </summary>
        /// <param name="id">ID del almacén</param>
        /// <returns>Almacén encontrado o null si no existe</returns>
        public async Task<Almacen?> GetByIdAsync(int id)
        {
            try
            {
                var almacen = await _readContext.Almacenes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id && a.Activo == true);

                if (almacen != null)
                {
                    _logger.LogDebug("Almacén encontrado: {NombreAlmacen} (ID: {Id})", almacen.NombreAlmacen, almacen.Id);
                }
                else
                {
                    _logger.LogDebug("Almacén no encontrado con ID: {Id}", id);
                }

                return almacen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almacén por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un almacén con el ID especificado en el catálogo legacy
        /// </summary>
        /// <param name="id">ID del almacén a verificar</param>
        /// <returns>true si existe y está activo, false en caso contrario</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Almacenes
                    .AnyAsync(a => a.Id == id && a.Activo == true);

                _logger.LogDebug("Verificación de existencia para almacén ID {Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de almacén ID: {Id}", id);
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