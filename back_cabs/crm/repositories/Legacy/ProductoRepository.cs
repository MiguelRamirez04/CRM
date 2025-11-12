// =====================================================================================
// PRODUCTO REPOSITORY - ProductoRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa el acceso a datos para productos utilizando Entity Framework Core.
// Proporciona consultas optimizadas para lectura de productos.
//
// OPTIMIZACIONES:
// - AsNoTracking() para queries read-only (mejor performance)
// - Filtro WHERE Activo = true en todas las consultas
// - EF.Functions.Like() para búsquedas case-insensitive
// - Ordenamiento por CodigoProducto
//
// CONTEXTO:
// - Usa ReadOnlyContext (CQRS pattern para queries)
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
    /// Repositorio para operaciones de lectura de productos
    /// Implementa consultas optimizadas con EF Core
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<ProductoRepository> _logger;

        public ProductoRepository(
            ReadOnlyContext readContext,
            ILogger<ProductoRepository> logger)
        {
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 MÉTODOS DE CONSULTA
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los productos activos del catálogo
        /// </summary>
        public async Task<IEnumerable<Producto>> GetAllProductosAsync()
        {
            try
            {
                _logger.LogDebug("🔍 Consultando todos los productos activos en BD");

                var productos = await _readContext.Productos
                    .AsNoTracking() // ✅ Optimización: no tracking para read-only
                    .Where(p => p.Activo) // ✅ Solo productos activos
                    .OrderBy(p => p.CodigoProducto) // ✅ Ordenar por código
                    .ToListAsync();

                _logger.LogInformation("✅ Obtenidos {Count} productos activos de la BD", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todos los productos desde BD");
                throw;
            }
        }

        /// <summary>
        /// Busca productos por código o nombre utilizando filtro LIKE
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        public async Task<IEnumerable<Producto>> SearchProductosByFilterAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("⚠️ Término de búsqueda vacío, retornando lista vacía");
                    return Enumerable.Empty<Producto>();
                }

                // Normalizar término de búsqueda
                var normalizedTerm = $"%{searchTerm.Trim()}%";

                _logger.LogDebug("🔍 Buscando productos con término: '{SearchTerm}'", searchTerm);

                var productos = await _readContext.Productos
                    .AsNoTracking() // ✅ Optimización: no tracking para read-only
                    .Where(p => p.Activo && // ✅ Solo productos activos
                        (EF.Functions.Like(p.CodigoProducto ?? "", normalizedTerm) || // ✅ Buscar en código
                         EF.Functions.Like(p.NombreProducto ?? "", normalizedTerm)))   // ✅ Buscar en nombre
                    .OrderBy(p => p.CodigoProducto) // ✅ Ordenar por código
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda completada: {Count} productos encontrados para '{SearchTerm}'", 
                    productos.Count, searchTerm);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar productos con término: '{SearchTerm}'", searchTerm);
                throw;
            }
        }
    }
}
