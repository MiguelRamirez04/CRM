using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Legacy
{
    /// <summary>
    /// Servicio para AdmDocumentos (Cotizaciones)
    /// </summary>
    public class AdmDocumentoService : IAdmDocumentoService
    {
        private readonly IAdmDocumentoRepository _repository;
        private readonly LegacyCompacReadOnlyContext _context;
        private readonly ILogger<AdmDocumentoService> _logger;

        public AdmDocumentoService(
            IAdmDocumentoRepository repository,
            LegacyCompacReadOnlyContext context,
            ILogger<AdmDocumentoService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        public async Task<(List<AdmDocumentoResponseDto> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter)
        {
            try
            {
                // Validar y normalizar filtros
                filter.Page = filter.Page < 1 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize < 1 ? 50 : (filter.PageSize > 100 ? 100 : filter.PageSize);

                _logger.LogInformation(
                    "🔍 Buscando documentos. Página: {Page}, Tamaño: {PageSize}, Fecha: {FechaInicio}-{FechaFin}, RazonSocial: {RazonSocial}",
                    filter.Page, filter.PageSize, filter.FechaInicio, filter.FechaFin, filter.RazonSocial);

                var (documentos, totalRegistros) = await _repository.SearchPaginatedAsync(filter);

                // Obtener IDs únicos para consultas relacionadas (solo agentes)
                var agentesIds = documentos.Select(d => d.CIdAgente).Distinct().Where(id => id > 0).ToList();

                // Cargar solo agentes (único campo relacionado necesario)
                var agentes = await CargarAgentesAsync(agentesIds);

                // Si se solicitan movimientos, cargarlos
                Dictionary<int, List<AdmMovimientoResponseDto>> movimientosPorDocumento = new();
                
                if (filter.IncluirMovimientos)
                {
                    var documentosIds = documentos.Select(d => d.CIdDocumento).ToList();
                    movimientosPorDocumento = await CargarMovimientosPorDocumentosAsync(documentosIds);
                }

                // Mapear a DTOs
                var result = documentos.Select(d =>
                {
                    var dto = MapToDto(d, agentes);
                    
                    if (filter.IncluirMovimientos && movimientosPorDocumento.TryGetValue(d.CIdDocumento, out var movs))
                    {
                        dto.Movimientos = movs;
                    }
                    
                    return dto;
                }).ToList();

                _logger.LogInformation(
                    "✅ Búsqueda de documentos completada. Total: {Total}, Retornados: {Count}",
                    totalRegistros, result.Count);

                return (result, totalRegistros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        public async Task<AdmDocumentoResponseDto?> GetByIdWithMovimientosAsync(int idDocumento)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo documento {IdDocumento} con movimientos", idDocumento);

                var documento = await _repository.GetByIdWithMovimientosAsync(idDocumento);
                
                if (documento == null)
                {
                    _logger.LogWarning("⚠️ Documento {IdDocumento} no encontrado", idDocumento);
                    return null;
                }

                // Cargar solo agentes
                var agentes = await CargarAgentesAsync(new List<int> { documento.CIdAgente });

                // Cargar movimientos con productos
                var movimientosPorDocumento = await CargarMovimientosPorDocumentosAsync(new List<int> { idDocumento });

                var result = MapToDto(documento, agentes);
                
                if (movimientosPorDocumento.TryGetValue(idDocumento, out var movimientos))
                {
                    result.Movimientos = movimientos;
                }

                _logger.LogInformation(
                    "✅ Documento {IdDocumento} obtenido con {MovCount} movimientos",
                    idDocumento, result.Movimientos.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        #region Métodos privados de carga de datos relacionados

        private async Task<Dictionary<int, models.legacy.AdmAgente>> CargarAgentesAsync(List<int> ids)
        {
            if (!ids.Any()) return new Dictionary<int, models.legacy.AdmAgente>();

            // Cargar todos los agentes y filtrar en memoria para evitar OPENJSON
            var allAgentes = await _context.AdmAgentes
                .AsNoTracking()
                .ToListAsync();
            
            return allAgentes
                .Where(a => ids.Contains(a.CIdAgente))
                .ToDictionary(a => a.CIdAgente);
        }

        private async Task<Dictionary<int, List<AdmMovimientoResponseDto>>> CargarMovimientosPorDocumentosAsync(List<int> documentosIds)
        {
            if (!documentosIds.Any()) return new Dictionary<int, List<AdmMovimientoResponseDto>>();

            // Para evitar OPENJSON, cargamos movimientos iterando por cada documento
            // Dado que es paginado (máx 50-100 docs), es aceptable
            var todosMovimientos = new List<models.legacy.AdmMovimiento>();
            
            foreach (var docId in documentosIds)
            {
                var movs = await _context.AdmMovimientos
                    .AsNoTracking()
                    .Where(m => m.CIdDocumento == docId)
                    .OrderBy(m => m.CNumeroMovimiento)
                    .ToListAsync();
                
                todosMovimientos.AddRange(movs);
            }

            // Obtener IDs únicos de entidades relacionadas
            var productosIds = todosMovimientos.Select(m => m.CIdProducto).Distinct().Where(id => id > 0).ToList();
            var almacenesIds = todosMovimientos.Select(m => m.CIdAlmacen).Distinct().Where(id => id > 0).ToList();

            // Cargar productos y almacenes - evitar Contains() para prevenir OPENJSON
            Dictionary<int, models.legacy.AdmProducto> productos;
            if (productosIds.Any())
            {
                var allProductos = await _context.AdmProductos.AsNoTracking().ToListAsync();
                productos = allProductos
                    .Where(p => productosIds.Contains(p.CIdProducto))
                    .ToDictionary(p => p.CIdProducto);
            }
            else
            {
                productos = new Dictionary<int, models.legacy.AdmProducto>();
            }

            Dictionary<int, models.legacy.AdmAlmacen> almacenes;
            if (almacenesIds.Any())
            {
                var allAlmacenes = await _context.AdmAlmacenes.AsNoTracking().ToListAsync();
                almacenes = allAlmacenes
                    .Where(a => almacenesIds.Contains(a.CIdAlmacen))
                    .ToDictionary(a => a.CIdAlmacen);
            }
            else
            {
                almacenes = new Dictionary<int, models.legacy.AdmAlmacen>();
            }

            // Agrupar movimientos por documento
            return todosMovimientos
                .GroupBy(m => m.CIdDocumento)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => MapMovimientoToDto(m, productos, almacenes)).ToList()
                );
        }

        #endregion

        #region Métodos de mapeo

        private AdmDocumentoResponseDto MapToDto(
            models.legacy.AdmDocumento entity,
            Dictionary<int, models.legacy.AdmAgente> agentes)
        {
            agentes.TryGetValue(entity.CIdAgente, out var agente);

            return new AdmDocumentoResponseDto
            {
                SerieDocumento = entity.CSerieDocumento,
                Folio = entity.CFolio,
                Fecha = entity.CFecha,
                RazonSocial = entity.CRazonSocial,
                FechaVencimiento = entity.CFechaVencimiento,
                FechaProntoPago = entity.CFechaProntoPago,
                FechaEntregaRecepcion = entity.CFechaEntregaRecepcion,

                // Agente (nombre completo)
                Agente = agente?.CNombreAgente,

                // Movimientos se cargan por separado
                Movimientos = new List<AdmMovimientoResponseDto>()
            };
        }

        private AdmMovimientoResponseDto MapMovimientoToDto(
            models.legacy.AdmMovimiento entity,
            Dictionary<int, models.legacy.AdmProducto> productos,
            Dictionary<int, models.legacy.AdmAlmacen> almacenes)
        {
            productos.TryGetValue(entity.CIdProducto, out var producto);
            almacenes.TryGetValue(entity.CIdAlmacen, out var almacen);

            return new AdmMovimientoResponseDto
            {
                IdMovimiento = entity.CIdMovimiento,
                NumeroMovimiento = entity.CNumeroMovimiento,

                // Producto
                IdProducto = entity.CIdProducto,
                CodigoProducto = producto?.CCodigoProducto,
                NombreProducto = producto?.CNombreProducto,
                DescripcionProducto = producto?.CDescripcionProducto,

                // Almacén
                IdAlmacen = entity.CIdAlmacen,
                CodigoAlmacen = almacen?.CCodigoAlmacen,
                NombreAlmacen = almacen?.CNombreAlmacen,

                // Unidades
                Unidades = entity.CUnidades,
                UnidadesCapturadas = entity.CUnidadesCapturadas,
                IdUnidad = entity.CIdUnidad,

                // Precios y costos
                Precio = entity.CPrecio,
                PrecioCapturado = entity.CPrecioCapturado,
                CostoCapturado = entity.CCostoCapturado,
                PorcentajeDescuento = entity.CPorcentajeDescuento1,
                DescuentoLinea = entity.CDescuento1,

                // Impuestos
                Impuesto1 = entity.CImpuesto1,
                Impuesto2 = entity.CImpuesto2,
                Impuesto3 = entity.CImpuesto3,
                Retencion1 = entity.CRetencion1,
                Retencion2 = entity.CRetencion2,

                // Totales
                Neto = entity.CNeto,
                Total = entity.CTotal,

                // Referencia y observaciones
                Referencia = entity.CReferencia,
                Observaciones = entity.CObservaMov,

                // Estado
                Afectado = entity.CAfectaExistencia,
                Venta = 0 // No existe campo CVenta en AdmMovimiento
            };
        }

        #endregion
    }
}
