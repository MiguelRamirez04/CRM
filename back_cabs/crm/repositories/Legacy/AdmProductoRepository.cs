// =====================================================================================
// REPOSITORY ADM PRODUCTO - AdmProductoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa acceso a datos de productos legacy (admProductos) con Entity Framework Core.
// Proporciona consultas paginadas y búsquedas optimizadas por código/nombre.
//
// PROPÓSITO:
// - Acceso eficiente a BD legacy adCABS2016
// - Paginación con Skip/Take (50 registros por página)
// - Búsqueda por código y nombre de producto
// - AsNoTracking para optimización de solo lectura
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para acceso a productos legacy (admProductos de adCABS2016)
    /// Solo lectura con paginación optimizada
    /// </summary>
    public class AdmProductoRepository : IAdmProductoRepository
    {
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmProductoRepository> _logger;

        public AdmProductoRepository(LegacyCompacReadOnlyContext context, ILogger<AdmProductoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: GET ALL PAGINADO
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los productos con paginación
        /// Usa AsNoTracking para optimización (solo lectura)
        /// </summary>
        public async Task<(List<AdmProducto> Data, int TotalRecords)> GetAllPaginatedAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Consultando productos legacy página {Page} tamaño {PageSize}", page, pageSize);

                // Contar total de registros
                var totalRecords = await _context.AdmProductos.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await _context.AdmProductos
                    .AsNoTracking()
                    .OrderBy(p => p.CCodigoProducto)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Consulta exitosa: {Count} de {Total} productos legacy (página {Page})",
                    data.Count, totalRecords, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al consultar productos legacy paginados (página {Page}, tamaño {PageSize})",
                    page, pageSize);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODO: BÚSQUEDA PAGINADA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca productos por código o nombre con paginación
        /// Búsqueda case-insensitive en CCODIGOPRODUCTO y CNOMBREPRODUCTO
        /// </summary>
        public async Task<(List<AdmProducto> Data, int TotalRecords)> SearchPaginatedAsync(
            string searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                _logger.LogDebug("🔍 Buscando productos legacy: '{SearchTerm}' página {Page} tamaño {PageSize}",
                    searchTerm, page, pageSize);

                var normalizedTerm = searchTerm.Trim().ToLower();

                // Query base con filtros
                var query = _context.AdmProductos
                    .AsNoTracking()
                    .Where(p =>
                        p.CCodigoProducto.ToLower().Contains(normalizedTerm) ||
                        p.CNombreProducto.ToLower().Contains(normalizedTerm)
                    );

                // Contar total de registros filtrados
                var totalRecords = await query.CountAsync();

                // Calcular skip
                var skip = (page - 1) * pageSize;

                // Obtener datos paginados
                var data = await query
                    .OrderBy(p => p.CCodigoProducto)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda exitosa: {Count} de {Total} productos legacy encontrados ('{SearchTerm}', página {Page})",
                    data.Count, totalRecords, searchTerm, page);

                return (data, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar productos legacy ('{SearchTerm}', página {Page}, tamaño {PageSize})",
                    searchTerm, page, pageSize);
                throw;
            }
        }
    }
}