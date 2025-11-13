// =====================================================================================
// REPOSITORIO NÚMERO SERIE - NumeroSerieRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa el acceso a datos para números de serie usando Entity Framework Core.
// Consulta la vista VwNumerosSerieCompletos con optimizaciones de rendimiento.
//
// OPTIMIZACIONES:
// - AsNoTracking(): Sin seguimiento de cambios (solo lectura)
// - Where(Activo = true): Filtrado en BD, no en memoria
// - EF.Functions.Like: Búsqueda case-insensitive en SQL
// - OrderBy: Resultados ordenados consistentemente
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para operaciones de lectura de números de serie
    /// Usa ReadOnlyContext para consultas optimizadas sin tracking
    /// </summary>
    public class NumeroSerieRepository : INumeroSerieRepository
    {
        private readonly ReadOnlyContext _context;
        private readonly ILogger<NumeroSerieRepository> _logger;

        public NumeroSerieRepository(ReadOnlyContext context, ILogger<NumeroSerieRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene números de serie activos paginados
        /// </summary>
        public async Task<(List<VwNumerosSerieCompletos> Data, int TotalRecords)> GetNumeroSeriePaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogDebug("Consultando números de serie paginados - Página: {Page}, Tamaño: {PageSize}", page, pageSize);

                // Obtener total de registros
                var totalRecords = await _context.VwNumerosSerieCompletos
                    .AsNoTracking()
                    .Where(ns => ns.Activo)
                    .CountAsync();

                // Obtener datos paginados
                var skip = (page - 1) * pageSize;
                var numerosSerie = await _context.VwNumerosSerieCompletos
                    .AsNoTracking()
                    .Where(ns => ns.Activo)
                    .OrderBy(ns => ns.NumeroSerieId)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Página {Page} de {TotalPages} retornó {Count} de {TotalRecords} números de serie", 
                    page, Math.Ceiling(totalRecords / (double)pageSize), numerosSerie.Count, totalRecords);

                return (numerosSerie, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar números de serie paginados - Página: {Page}", page);
                throw;
            }
        }

        /// <summary>
        /// Busca números de serie por término con paginación
        /// </summary>
        public async Task<(List<VwNumerosSerieCompletos> Data, int TotalRecords)> SearchNumeroSeriePaginatedAsync(string searchTerm, int page, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Término de búsqueda vacío, retornando lista vacía");
                    return (new List<VwNumerosSerieCompletos>(), 0);
                }

                var normalizedTerm = $"%{searchTerm.Trim()}%";
                _logger.LogDebug("Buscando números de serie paginados - Término: {SearchTerm}, Página: {Page}, Tamaño: {PageSize}", 
                    searchTerm, page, pageSize);

                // Obtener total de registros que coinciden
                var totalRecords = await _context.VwNumerosSerieCompletos
                    .AsNoTracking()
                    .Where(ns => ns.Activo &&
                           EF.Functions.Like(ns.NumeroSerie, normalizedTerm))
                    .CountAsync();

                // Obtener datos paginados
                var skip = (page - 1) * pageSize;
                var numerosSerie = await _context.VwNumerosSerieCompletos
                    .AsNoTracking()
                    .Where(ns => ns.Activo &&
                           EF.Functions.Like(ns.NumeroSerie, normalizedTerm))
                    .OrderBy(ns => ns.NumeroSerieId)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Búsqueda '{SearchTerm}' - Página {Page} de {TotalPages} retornó {Count} de {TotalRecords} números de serie", 
                    searchTerm, page, Math.Ceiling(totalRecords / (double)pageSize), numerosSerie.Count, totalRecords);

                return (numerosSerie, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar números de serie paginados - Término: '{SearchTerm}', Página: {Page}", searchTerm, page);
                throw;
            }
        }
    }
}

