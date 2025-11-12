// =====================================================================================
// DOCUMENTO MODELO REPOSITORY - DocumentoModeloRepository.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa el acceso a datos para documentos modelo utilizando Entity Framework Core.
// Proporciona consultas optimizadas para lectura de documentos modelo.
//
// OPTIMIZACIONES:
// - AsNoTracking() para queries read-only (mejor performance)
// - Filtro WHERE Activo = true en todas las consultas
// - EF.Functions.Like() para búsquedas case-insensitive
// - Ordenamiento por Descripcion
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
    /// Repositorio para operaciones de lectura de documentos modelo
    /// Implementa consultas optimizadas con EF Core
    /// </summary>
    public class DocumentoModeloRepository : IDocumentoModeloRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<DocumentoModeloRepository> _logger;

        public DocumentoModeloRepository(
            ReadOnlyContext readContext,
            ILogger<DocumentoModeloRepository> logger)
        {
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────
        // 📖 MÉTODOS DE CONSULTA
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene todos los documentos modelo activos del catálogo
        /// </summary>
        public async Task<IEnumerable<DocumentoModelo>> GetAllDocumentoModelosAsync()
        {
            try
            {
                _logger.LogDebug("🔍 Consultando todos los documentos modelo activos en BD");

                var documentosModelo = await _readContext.DocumentosModelo
                    .AsNoTracking() // ✅ Optimización: no tracking para read-only
                    .Where(dm => dm.Activo) // ✅ Solo documentos modelo activos
                    .OrderBy(dm => dm.Descripcion) // ✅ Ordenar por descripción
                    .ToListAsync();

                _logger.LogInformation("✅ Obtenidos {Count} documentos modelo activos de la BD", documentosModelo.Count);
                return documentosModelo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todos los documentos modelo desde BD");
                throw;
            }
        }

        /// <summary>
        /// Busca documentos modelo por descripción utilizando filtro LIKE
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        public async Task<IEnumerable<DocumentoModelo>> SearchDocumentoModelosByFilterAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("⚠️ Término de búsqueda vacío, retornando lista vacía");
                    return Enumerable.Empty<DocumentoModelo>();
                }

                // Normalizar término de búsqueda
                var normalizedTerm = $"%{searchTerm.Trim()}%";

                _logger.LogDebug("🔍 Buscando documentos modelo con término: '{SearchTerm}'", searchTerm);

                var documentosModelo = await _readContext.DocumentosModelo
                    .AsNoTracking() // ✅ Optimización: no tracking para read-only
                    .Where(dm => dm.Activo && // ✅ Solo documentos modelo activos
                        EF.Functions.Like(dm.Descripcion ?? "", normalizedTerm)) // ✅ Buscar en descripción
                    .OrderBy(dm => dm.Descripcion) // ✅ Ordenar por descripción
                    .ToListAsync();

                _logger.LogInformation("✅ Búsqueda completada: {Count} documentos modelo encontrados para '{SearchTerm}'",
                    documentosModelo.Count, searchTerm);

                return documentosModelo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos modelo con término: '{SearchTerm}'", searchTerm);
                throw;
            }
        }
    }
}
