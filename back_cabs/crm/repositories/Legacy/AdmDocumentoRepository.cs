using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.models.legacy;
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
    }
}
