using System.Data;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Legacy
{
    /// <summary>
    /// Repositorio para AdmDocumentos
    /// </summary>
    public class AdmDocumentoRepository : IAdmDocumentoRepository
    {
        private readonly LegacyCompacReadOnlyContext _readContext;
        private readonly LegacyCompacWriteContext _writeContext;
        private readonly ILogger<AdmDocumentoRepository> _logger;

        public AdmDocumentoRepository(
            LegacyCompacReadOnlyContext readContext,
            LegacyCompacWriteContext writeContext,
            ILogger<AdmDocumentoRepository> logger)
        {
            _readContext = readContext;
            _writeContext = writeContext;
            _logger = logger;
        }

        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        public async Task<(List<AdmDocumento> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter)
        {
            try
            {
                var query = _readContext.AdmDocumentos.AsNoTracking();

                // Aplicar filtros
                
                // ⚠️ FILTRO OBLIGATORIO: Solo cotizaciones (Serie "CA")
                query = query.Where(d => d.CSerieDocumento == "CA");

                if (filter.FechaInicio.HasValue)
                {
                    query = query.Where(d => d.CFecha >= filter.FechaInicio.Value);
                }

                if (filter.FechaFin.HasValue)
                {
                    query = query.Where(d => d.CFecha <= filter.FechaFin.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Folio))
                {
                    var folioStr = filter.Folio.Trim();
                    // Intentar convertir a double para búsqueda exacta
                    if (double.TryParse(folioStr, out double folioNum))
                    {
                        query = query.Where(d => d.CFolio == folioNum);
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter.RazonSocial))
                {
                    var razonSocial = filter.RazonSocial.Trim().ToLower();
                    query = query.Where(d => d.CRazonSocial.ToLower().Contains(razonSocial));
                }

                if (filter.FechaVencimientoInicio.HasValue)
                {
                    query = query.Where(d => d.CFechaVencimiento >= filter.FechaVencimientoInicio.Value);
                }

                if (filter.FechaVencimientoFin.HasValue)
                {
                    query = query.Where(d => d.CFechaVencimiento <= filter.FechaVencimientoFin.Value);
                }

                if (filter.IdConcepto.HasValue)
                {
                    query = query.Where(d => d.CIdConceptoDocumento == filter.IdConcepto.Value);
                }

                if (filter.IdAgente.HasValue)
                {
                    query = query.Where(d => d.CIdAgente == filter.IdAgente.Value);
                }

                // Obtener total de registros antes de paginar
                var totalRegistros = await query.CountAsync();

                // Aplicar ordenamiento (más recientes primero)
                query = query.OrderByDescending(d => d.CFecha)
                            .ThenByDescending(d => d.CFolio);

                // Aplicar paginación
                var documentos = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogInformation(
                    "✅ Búsqueda de documentos completada. Total: {Total}, Página: {Page}, Tamaño: {PageSize}",
                    totalRegistros, filter.Page, filter.PageSize);

                return (documentos, totalRegistros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID
        /// </summary>
        public async Task<AdmDocumento?> GetByIdWithMovimientosAsync(int idDocumento)
        {
            try
            {
                var documento = await _readContext.AdmDocumentos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.CIdDocumento == idDocumento);

                return documento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los movimientos de un documento específico
        /// </summary>
        public async Task<List<AdmMovimiento>> GetMovimientosByDocumentoIdAsync(int idDocumento)
        {
            try
            {
                var movimientos = await _readContext.AdmMovimientos
                    .AsNoTracking()
                    .Where(m => m.CIdDocumento == idDocumento)
                    .OrderBy(m => m.CNumeroMovimiento)
                    .ToListAsync();

                return movimientos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener movimientos del documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en la base de datos
        /// </summary>
        public async Task<int> CreateAsync(AdmDocumento documento)
        {
            try
            {
                _logger.LogInformation("📝 Insertando nuevo documento en BD. Serie: {Serie}, Folio: {Folio}", 
                    documento.CSerieDocumento, documento.CFolio);

                // Obtener el próximo ID disponible
                var maxId = await _writeContext.AdmDocumentos.MaxAsync(d => (int?)d.CIdDocumento) ?? 0;
                documento.CIdDocumento = maxId + 1;

                await _writeContext.AdmDocumentos.AddAsync(documento);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("✅ Documento creado exitosamente. ID: {IdDocumento}", documento.CIdDocumento);

                return documento.CIdDocumento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear documento. Serie: {Serie}, Folio: {Folio}", 
                    documento.CSerieDocumento, documento.CFolio);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un documento modelo por su ID
        /// </summary>
        public async Task<bool> ExistsDocumentoModeloAsync(int idDocumentoDe)
        {
            try
            {
                return await _readContext.AdmDocumentosModelo
                    .AsNoTracking()
                    .AnyAsync(dm => dm.CIdDocumentoDe == idDocumentoDe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de documento modelo {IdDocumentoDe}", idDocumentoDe);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un concepto por su ID
        /// </summary>
        public async Task<bool> ExistsConceptoAsync(int idConcepto)
        {
            try
            {
                return await _readContext.AdmConceptos
                    .AsNoTracking()
                    .AnyAsync(c => c.CIdConceptoDocumento == idConcepto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de concepto {IdConcepto}", idConcepto);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un cliente/proveedor por su ID
        /// </summary>
        public async Task<bool> ExistsClienteProveedorAsync(int idClienteProveedor)
        {
            try
            {
                return await _readContext.AdmClientes
                    .AsNoTracking()
                    .AnyAsync(c => c.CIdClienteProveedor == idClienteProveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de cliente/proveedor {IdClienteProveedor}", idClienteProveedor);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un agente por su ID
        /// </summary>
        public async Task<bool> ExistsAgenteAsync(int idAgente)
        {
            try
            {
                return await _readContext.AdmAgentes
                    .AsNoTracking()
                    .AnyAsync(a => a.CIdAgente == idAgente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de agente {IdAgente}", idAgente);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una moneda por su ID
        /// </summary>
        public async Task<bool> ExistsMonedaAsync(int idMoneda)
        {
            try
            {
                return await _readContext.AdmMonedas
                    .AsNoTracking()
                    .AnyAsync(m => m.CIdMoneda == idMoneda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de moneda {IdMoneda}", idMoneda);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA COTIZACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene el folio actual del concepto
        /// </summary>
        public async Task<double> GetFolioActualAsync(int idConcepto)
        {
            try
            {
                var concepto = await _readContext.AdmConceptos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CIdConceptoDocumento == idConcepto);

                if (concepto == null)
                {
                    throw new InvalidOperationException($"No se encontró el concepto con ID {idConcepto}");
                }

                return concepto.CNoFolio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo folio del concepto {IdConcepto}", idConcepto);
                throw;
            }
        }

        /// <summary>
        /// Actualiza el folio del concepto
        /// </summary>
        public async Task UpdateFolioConceptoAsync(int idConcepto, double nuevoFolio)
        {
            try
            {
                var concepto = await _writeContext.AdmConceptos
                    .FirstOrDefaultAsync(c => c.CIdConceptoDocumento == idConcepto);

                if (concepto == null)
                {
                    throw new InvalidOperationException($"No se encontró el concepto con ID {idConcepto}");
                }

                concepto.CNoFolio = nuevoFolio;
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("✅ Folio del concepto {IdConcepto} actualizado a {NuevoFolio}", idConcepto, nuevoFolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error actualizando folio del concepto {IdConcepto}", idConcepto);
                throw;
            }
        }

        /// <summary>
        /// Crea un documento con sus movimientos en una transacción
        /// </summary>
        public async Task<int> CreateDocumentoConMovimientosAsync(AdmDocumento documento, List<AdmMovimiento> movimientos)
        {
            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Generar ID manual para Documento (Legacy no usa Identity)
                var maxIdDocumento = await _writeContext.AdmDocumentos
                    .OrderByDescending(d => d.CIdDocumento)
                    .Select(d => d.CIdDocumento)
                    .FirstOrDefaultAsync();
                
                documento.CIdDocumento = maxIdDocumento + 1;

                // 2. Obtener y actualizar el folio
                var folioActual = await GetFolioActualAsync(documento.CIdConceptoDocumento);
                var nuevoFolio = folioActual + 1;
                documento.CFolio = nuevoFolio;

                // 3. Insertar el documento
                await _writeContext.AdmDocumentos.AddAsync(documento);
                await _writeContext.SaveChangesAsync();

                var documentoId = documento.CIdDocumento;
                _logger.LogInformation("✅ Documento creado con ID {DocumentoId}", documentoId);

                // 4. Generar IDs manuales para Movimientos e Insertar
                var maxIdMovimiento = await _writeContext.AdmMovimientos
                    .OrderByDescending(m => m.CIdMovimiento)
                    .Select(m => m.CIdMovimiento)
                    .FirstOrDefaultAsync();

                foreach (var movimiento in movimientos)
                {
                    maxIdMovimiento++;
                    movimiento.CIdMovimiento = maxIdMovimiento;
                    movimiento.CIdDocumento = documentoId;
                    movimiento.CIdDocumentoDe = documento.CIdDocumentoDe;
                }

                await _writeContext.AdmMovimientos.AddRangeAsync(movimientos);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("✅ {CantidadMovimientos} movimientos creados para documento {DocumentoId}", 
                    movimientos.Count, documentoId);

                // 5. Actualizar el folio en admConceptos
                await UpdateFolioConceptoAsync(documento.CIdConceptoDocumento, nuevoFolio);

                // 6. Commit de la transacción
                await transaction.CommitAsync();

                _logger.LogInformation("✅ Cotización creada exitosamente - ID: {DocumentoId}, Folio: {Folio}", 
                    documentoId, nuevoFolio);

                return documentoId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Error creando documento con movimientos. Transacción revertida.");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un almacén por su ID
        /// </summary>
        public async Task<bool> ExistsAlmacenAsync(int idAlmacen)
        {
            try
            {
                return await _readContext.AdmAlmacenes
                    .AsNoTracking()
                    .AnyAsync(a => a.CIdAlmacen == idAlmacen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verificando existencia de almacén {IdAlmacen}", idAlmacen);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe un producto por su ID
        /// </summary>
        public async Task<bool> ExistsProductoAsync(int idProducto)
        {
            try
            {
                return await _readContext.AdmProductos
                    .AsNoTracking()
                    .AnyAsync(p => p.CIdProducto == idProducto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verificando existencia de producto {IdProducto}", idProducto);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una unidad de medida por su ID
        /// </summary>
        public async Task<bool> ExistsUnidadAsync(int idUnidad)
        {
            try
            {
                return await _readContext.AdmUnidadesMedidaPeso
                    .AsNoTracking()
                    .AnyAsync(u => u.CIdUnidad == idUnidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verificando existencia de unidad {IdUnidad}", idUnidad);
                throw;
            }
        }

        /// <summary>
        /// Cancela un documento (cambia CCANCELADO a 1)
        /// </summary>
        public async Task CancelarDocumentoAsync(int idDocumento, string? motivo, string usuario)
        {
            try
            {
                var documento = await _writeContext.AdmDocumentos
                    .FirstOrDefaultAsync(d => d.CIdDocumento == idDocumento);

                if (documento == null)
                {
                    throw new InvalidOperationException($"No se encontró el documento con ID {idDocumento}");
                }

                if (documento.CCancelado == 1)
                {
                    throw new InvalidOperationException($"El documento {documento.CSerieDocumento}-{documento.CFolio} ya está cancelado");
                }

                // Actualizar campos de cancelación
                documento.CCancelado = 1;
                documento.CUsuario = usuario;
                
                // Agregar motivo en observaciones si se proporcionó
                if (!string.IsNullOrWhiteSpace(motivo))
                {
                    var observacionCancelacion = $"[CANCELADO: {DateTime.Now:yyyy-MM-dd HH:mm}] {motivo}";
                    documento.CObservaciones = string.IsNullOrWhiteSpace(documento.CObservaciones)
                        ? observacionCancelacion
                        : $"{documento.CObservaciones}\\n{observacionCancelacion}";
                }

                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("✅ Documento {IdDocumento} cancelado exitosamente por {Usuario}", 
                    idDocumento, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cancelando documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID (solo encabezado)
        /// </summary>
        public async Task<AdmDocumento?> GetDocumentoByIdAsync(int idDocumento)
        {
            try
            {
                return await _readContext.AdmDocumentos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.CIdDocumento == idDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        /// <summary>
        /// Elimina un documento y sus movimientos asociados
        /// </summary>
        public async Task DeleteDocumentoAsync(int idDocumento)
        {
            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Eliminar movimientos
                var movimientos = await _writeContext.AdmMovimientos
                    .Where(m => m.CIdDocumento == idDocumento)
                    .ToListAsync();
                
                if (movimientos.Any())
                {
                    _writeContext.AdmMovimientos.RemoveRange(movimientos);
                    await _writeContext.SaveChangesAsync();
                }

                // 2. Eliminar documento
                var documento = await _writeContext.AdmDocumentos
                    .FirstOrDefaultAsync(d => d.CIdDocumento == idDocumento);
                
                if (documento != null)
                {
                    _writeContext.AdmDocumentos.Remove(documento);
                    await _writeContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("✅ Documento {IdDocumento} eliminado exitosamente", idDocumento);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Error eliminando documento {IdDocumento}", idDocumento);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA REPORTES Y ESTADÍSTICAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene estadísticas generales de cotizaciones para dashboard
        /// </summary>
        public async Task<EstadisticasGeneralesDto> GetEstadisticasGeneralesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var connection = _readContext.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var estadisticas = new EstadisticasGeneralesDto
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                // 1. Estadísticas de Documentos
                // Usamos Raw SQL para evitar problemas de traducción de EF Core 8 con bases de datos legacy
                // y para asegurar el casting correcto de tipos float/double a decimal
                var sqlDocs = @"
                    SELECT 
                        COUNT(*) as TotalCotizaciones,
                        CAST(COALESCE(SUM(CTOTAL), 0) AS DECIMAL(18,2)) as MontoTotal,
                        CAST(COALESCE(AVG(CTOTAL), 0) AS DECIMAL(18,2)) as MontoPromedio,
                        CAST(COALESCE(MAX(CTOTAL), 0) AS DECIMAL(18,2)) as MontoMaximo,
                        CAST(COALESCE(MIN(CTOTAL), 0) AS DECIMAL(18,2)) as MontoMinimo,
                        SUM(CASE WHEN CCANCELADO = 0 THEN 1 ELSE 0 END) as CotizacionesActivas,
                        SUM(CASE WHEN CCANCELADO = 1 THEN 1 ELSE 0 END) as CotizacionesCanceladas,
                        COUNT(DISTINCT CIDCLIENTEPROVEEDOR) as ClientesUnicos
                    FROM admDocumentos
                    WHERE CSERIEDOCUMENTO = 'CA'
                    AND (@FechaInicio IS NULL OR CFECHA >= @FechaInicio)
                    AND (@FechaFin IS NULL OR CFECHA <= @FechaFin)";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlDocs;
                    
                    var pInicio = command.CreateParameter();
                    pInicio.ParameterName = "@FechaInicio";
                    pInicio.Value = fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value;
                    command.Parameters.Add(pInicio);

                    var pFin = command.CreateParameter();
                    pFin.ParameterName = "@FechaFin";
                    pFin.Value = fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value;
                    command.Parameters.Add(pFin);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            estadisticas.TotalCotizaciones = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            estadisticas.MontoTotal = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            estadisticas.MontoPromedio = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            estadisticas.MontoMaximo = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                            estadisticas.MontoMinimo = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            estadisticas.CotizacionesActivas = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            estadisticas.CotizacionesCanceladas = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                            estadisticas.ClientesUnicos = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                        }
                    }
                }

                // 2. Productos Únicos (Join con Movimientos)
                var sqlProds = @"
                    SELECT COUNT(DISTINCT m.CIDPRODUCTO)
                    FROM admMovimientos m
                    INNER JOIN admDocumentos d ON m.CIDDOCUMENTO = d.CIDDOCUMENTO
                    WHERE d.CSERIEDOCUMENTO = 'CA'
                    AND (@FechaInicio IS NULL OR d.CFECHA >= @FechaInicio)
                    AND (@FechaFin IS NULL OR d.CFECHA <= @FechaFin)
                    AND m.CIDPRODUCTO > 0";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlProds;
                    
                    var pInicio = command.CreateParameter();
                    pInicio.ParameterName = "@FechaInicio";
                    pInicio.Value = fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value;
                    command.Parameters.Add(pInicio);

                    var pFin = command.CreateParameter();
                    pFin.ParameterName = "@FechaFin";
                    pFin.Value = fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value;
                    command.Parameters.Add(pFin);

                    var result = await command.ExecuteScalarAsync();
                    estadisticas.ProductosUnicos = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }

                _logger.LogInformation("✅ Estadísticas generales calculadas: {Total} cotizaciones, {Monto} total",
                    estadisticas.TotalCotizaciones, estadisticas.MontoTotal);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo estadísticas generales");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el top N de clientes con más cotizaciones y montos
        /// </summary>
        public async Task<List<TopClienteDto>> GetTopClientesAsync(int top, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = _readContext.AdmDocumentos
                    .AsNoTracking()
                    .AsSplitQuery() // Optimización: divide queries complejas
                    .Where(d => d.CSerieDocumento == "CA"); // Solo cotizaciones

                // Aplicar filtros de fecha
                if (fechaInicio.HasValue)
                {
                    query = query.Where(d => d.CFecha >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(d => d.CFecha <= fechaFin.Value);
                }

                // Agrupar por cliente y obtener estadísticas
                // OPTIMIZACIÓN: proyección temprana para reducir datos transferidos
                var topClientes = await query
                    .Join(_readContext.AdmClientes.AsNoTracking(),
                        d => d.CIdClienteProveedor,
                        c => c.CIdClienteProveedor,
                        (d, c) => new { 
                            ClienteId = d.CIdClienteProveedor, 
                            CodigoCliente = c.CCodigoCliente,
                            RazonSocial = c.CRazonSocial,
                            Rfc = c.CRfc,
                            Total = d.CTotal,
                            Cancelado = d.CCancelado,
                            Fecha = d.CFecha
                        })
                    .GroupBy(dc => new
                    {
                        dc.ClienteId,
                        dc.CodigoCliente,
                        dc.RazonSocial,
                        dc.Rfc
                    })
                    .Select(g => new
                    {
                        IdCliente = g.Key.ClienteId,
                        CodigoCliente = g.Key.CodigoCliente,
                        RazonSocial = g.Key.RazonSocial,
                        Rfc = g.Key.Rfc,
                        TotalCotizaciones = g.Count(),
                        MontoTotal = g.Sum(dc => dc.Total),
                        CotizacionesActivas = g.Count(dc => dc.Cancelado == 0),
                        UltimaCotizacion = g.Max(dc => dc.Fecha)
                    })
                    .OrderByDescending(c => c.MontoTotal)
                    .Take(top)
                    .ToListAsync();

                // Convertir a DTO con ranking
                var resultado = topClientes.Select((cliente, index) => new TopClienteDto
                {
                    IdCliente = cliente.IdCliente,
                    CodigoCliente = cliente.CodigoCliente ?? string.Empty,
                    RazonSocial = cliente.RazonSocial ?? string.Empty,
                    Rfc = cliente.Rfc ?? string.Empty,
                    TotalCotizaciones = cliente.TotalCotizaciones,
                    MontoTotal = (decimal)cliente.MontoTotal,
                    MontoPromedio = cliente.TotalCotizaciones > 0 
                        ? (decimal)cliente.MontoTotal / cliente.TotalCotizaciones 
                        : 0,
                    CotizacionesActivas = cliente.CotizacionesActivas,
                    UltimaCotizacion = cliente.UltimaCotizacion,
                    Ranking = index + 1
                }).ToList();

                _logger.LogInformation("✅ Top {Top} clientes obtenidos: {Count} registros",
                    top, resultado.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo top clientes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene cotizaciones próximas a vencer en los próximos N días con paginación
        /// </summary>
        public async Task<(List<CotizacionVencimientoDto> items, int total)> GetProximasVencerAsync(int dias, int page, int pageSize)
        {
            try
            {
                var fechaActual = DateTime.Now.Date;
                var fechaLimite = fechaActual.AddDays(dias);

                var query = _readContext.AdmDocumentos
                    .AsNoTracking()
                    .Where(d => d.CSerieDocumento == "CA" && // Solo cotizaciones
                               d.CCancelado == 0 && // Solo activas
                               d.CFechaVencimiento >= fechaActual && // No vencidas
                               d.CFechaVencimiento <= fechaLimite); // Próximas a vencer

                var total = await query.CountAsync();

                var cotizaciones = await query
                    .OrderBy(d => d.CFechaVencimiento) // Más urgentes primero
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var resultado = cotizaciones.Select(d =>
                {
                    var diasRestantes = (d.CFechaVencimiento.Date - fechaActual).Days;
                    var nivelUrgencia = diasRestantes switch
                    {
                        <= 1 => "Crítico",
                        <= 3 => "Alto",
                        <= 7 => "Medio",
                        _ => "Bajo"
                    };

                    return new CotizacionVencimientoDto
                    {
                        IdDocumento = d.CIdDocumento,
                        SerieDocumento = d.CSerieDocumento ?? string.Empty,
                        Folio = d.CFolio,
                        RazonSocial = d.CRazonSocial ?? string.Empty,
                        MontoTotal = (decimal)d.CTotal,
                        FechaVencimiento = d.CFechaVencimiento,
                        DiasRestantes = diasRestantes,
                        NivelUrgencia = nivelUrgencia,
                        FechaCreacion = d.CFecha,
                        Estatus = "Activa"
                    };
                }).ToList();

                _logger.LogInformation("✅ {Count} cotizaciones próximas a vencer en {Dias} días obtenidas (Página {Page})",
                    resultado.Count, dias, page);

                return (resultado, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo cotizaciones próximas a vencer");
                throw;
            }
        }

        /// <summary>
        /// Obtiene rendimiento por agente de ventas
        /// </summary>
        public async Task<List<RendimientoAgenteDto>> GetRendimientoAgentesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = from doc in _readContext.AdmDocumentos.AsNoTracking()
                           join agente in _readContext.AdmAgentes.AsNoTracking() on doc.CIdAgente equals agente.CIdAgente into agenteJoin
                           from agente in agenteJoin.DefaultIfEmpty()
                           where doc.CSerieDocumento == "CA" // Solo cotizaciones
                                 && (!fechaInicio.HasValue || doc.CFecha >= fechaInicio.Value)
                                 && (!fechaFin.HasValue || doc.CFecha <= fechaFin.Value)
                           group doc by new { agente.CIdAgente, agente.CNombreAgente } into g
                           select new RendimientoAgenteDto
                           {
                               IdAgente = (int?)g.Key.CIdAgente ?? 0,
                               NombreAgente = g.Key.CNombreAgente ?? "Sin Asignar",
                               TotalCotizaciones = g.Count(),
                               MontoTotal = (decimal)g.Sum(d => d.CTotal),
                               MontoPromedio = g.Count() > 0 ? (decimal)g.Average(d => d.CTotal) : 0,
                               CotizacionesActivas = g.Count(d => d.CCancelado == 0),
                               CotizacionesCanceladas = g.Count(d => d.CCancelado == 1),
                               TasaConversion = g.Count() > 0 ? (decimal)g.Count(d => d.CCancelado == 0) / g.Count() * 100 : 0
                           };

                var resultado = await query.OrderByDescending(r => r.MontoTotal).ToListAsync();

                _logger.LogInformation("✅ Rendimiento de {Count} agentes obtenido", resultado.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo rendimiento de agentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más cotizados por frecuencia y volumen
        /// </summary>
        public async Task<List<ProductoCotizadoDto>> GetProductosMasCotizadosAsync(int top, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo productos más cotizados: top={Top}, fechas={Inicio}-{Fin}", 
                    top, fechaInicio?.ToString("yyyy-MM-dd") ?? "sin límite", fechaFin?.ToString("yyyy-MM-dd") ?? "sin límite");

                // Usar RAW SQL para evitar OPENJSON con '$' que causa error en SQL Server viejo
                // NOTA: Casteamos a DECIMAL(18,2) porque las columnas originales son float/double
                var sql = @"
                    SELECT TOP (@Top)
                        m.CIDPRODUCTO AS IdProducto,
                        p.CCODIGOPRODUCTO AS CodigoProducto,
                        p.CNOMBREPRODUCTO AS NombreProducto,
                        COUNT(DISTINCT m.CIDDOCUMENTO) AS TotalCotizaciones,
                        CAST(COALESCE(SUM(m.CUNIDADES), 0) AS DECIMAL(18,2)) AS CantidadTotal,
                        CAST(COALESCE(SUM(m.CTOTAL), 0) AS DECIMAL(18,2)) AS MontoTotal,
                        CAST(CASE 
                            WHEN SUM(m.CUNIDADES) > 0 THEN SUM(m.CTOTAL) / SUM(m.CUNIDADES)
                            ELSE 0
                        END AS DECIMAL(18,2)) AS PrecioPromedio,
                        COUNT(DISTINCT d.CIDCLIENTEPROVEEDOR) AS ClientesUnicos
                    FROM dbo.admMovimientos m
                    INNER JOIN dbo.admDocumentos d ON m.CIDDOCUMENTO = d.CIDDOCUMENTO
                    LEFT JOIN dbo.admProductos p ON m.CIDPRODUCTO = p.CIDPRODUCTO
                    WHERE d.CSERIEDOCUMENTO = 'CA' 
                        AND m.CIDPRODUCTO > 0";

                var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
                {
                    new("@Top", top)
                };

                if (fechaInicio.HasValue)
                {
                    sql += " AND d.CFECHA >= @FechaInicio";
                    parameters.Add(new("@FechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    sql += " AND d.CFECHA <= @FechaFin";
                    parameters.Add(new("@FechaFin", fechaFin.Value));
                }

                sql += @"
                    GROUP BY m.CIDPRODUCTO, p.CCODIGOPRODUCTO, p.CNOMBREPRODUCTO
                    ORDER BY COUNT(DISTINCT m.CIDDOCUMENTO) DESC, SUM(m.CTOTAL) DESC";

                // Ejecutar query raw
                var connection = _readContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters.ToArray());

                var resultado = new List<ProductoCotizadoDto>();
                using var reader = await command.ExecuteReaderAsync();
                
                int ranking = 1;
                while (await reader.ReadAsync())
                {
                    resultado.Add(new ProductoCotizadoDto
                    {
                        IdProducto = reader.GetInt32(0),
                        CodigoProducto = reader.IsDBNull(1) ? "SIN CODIGO" : reader.GetString(1),
                        NombreProducto = reader.IsDBNull(2) ? "SIN NOMBRE" : reader.GetString(2),
                        TotalCotizaciones = reader.GetInt32(3),
                        CantidadTotal = reader.GetDecimal(4),
                        MontoTotal = reader.GetDecimal(5),
                        PrecioPromedio = reader.GetDecimal(6),
                        ClientesUnicos = reader.GetInt32(7),
                        Ranking = ranking++
                    });
                }

                _logger.LogInformation("✅ {Count} productos más cotizados obtenidos con RAW SQL", resultado.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo productos más cotizados");
                throw;
            }
        }

        /// <summary>
        /// Obtiene distribución de cotizaciones por rangos de monto
        /// </summary>
        public async Task<List<CotizacionPorRangoDto>> GetCotizacionesPorRangoMontoAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = _readContext.AdmDocumentos
                    .AsNoTracking()
                    .Where(d => d.CSerieDocumento == "CA"); // Solo cotizaciones

                // Aplicar filtros de fecha
                if (fechaInicio.HasValue)
                {
                    query = query.Where(d => d.CFecha >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(d => d.CFecha <= fechaFin.Value);
                }

                var cotizaciones = await query.ToListAsync();

                // Definir rangos de monto predefinidos
                var rangos = new (decimal Min, decimal? Max, string Label)[]
                {
                    (0m, 1000m, "$0 - $1,000"),
                    (1000m, 5000m, "$1,000 - $5,000"),
                    (5000m, 10000m, "$5,000 - $10,000"),
                    (10000m, 25000m, "$10,000 - $25,000"),
                    (25000m, 50000m, "$25,000 - $50,000"),
                    (50000m, null, "$50,000+")
                };

                var resultado = new List<CotizacionPorRangoDto>();
                var totalCotizaciones = cotizaciones.Count;
                var montoTotalGeneral = cotizaciones.Sum(d => d.CTotal);

                foreach (var rango in rangos)
                {
                    var cotizacionesEnRango = cotizaciones.Where(d =>
                        (decimal)d.CTotal >= rango.Min &&
                        (!rango.Max.HasValue || (decimal)d.CTotal < rango.Max.Value));

                    var listaCotizacionesRango = cotizacionesEnRango.ToList();

                    if (listaCotizacionesRango.Any())
                    {
                        var dto = new CotizacionPorRangoDto
                        {
                            RangoMonto = rango.Label,
                            MontoMinimo = rango.Min,
                            MontoMaximo = rango.Max,
                            TotalCotizaciones = listaCotizacionesRango.Count,
                            MontoTotal = (decimal)listaCotizacionesRango.Sum(d => d.CTotal),
                            MontoPromedio = (decimal)listaCotizacionesRango.Average(d => d.CTotal),
                            CotizacionesActivas = listaCotizacionesRango.Count(d => d.CCancelado == 0),
                            CotizacionesCanceladas = listaCotizacionesRango.Count(d => d.CCancelado == 1),
                            PorcentajeDelTotal = totalCotizaciones > 0 ? (decimal)listaCotizacionesRango.Count / totalCotizaciones * 100 : 0
                        };

                        resultado.Add(dto);
                    }
                }

                _logger.LogInformation("✅ Distribución por rangos de monto obtenida: {Count} rangos, {Total} cotizaciones totales",
                    resultado.Count, totalCotizaciones);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo cotizaciones por rango de monto");
                throw;
            }
        }
    }
}
