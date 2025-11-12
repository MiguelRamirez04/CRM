using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.repositories.Recepcion
{
    /// <summary>
    /// Implementación de ICotizacionRepository usando Entity Framework Core.
    /// Encapsula todas las operaciones de BD para Cotizaciones.
    /// </summary>
    public class CotizacionRepository : ICotizacionRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<CotizacionRepository> _logger;

        public CotizacionRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<CotizacionRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // 📖 IMPLEMENTACIÓN DE LECTURAS

        public async Task<IEnumerable<Cotizacion>> GetAllAsync()
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .OrderByDescending(c => c.Fecha) // Ordenado por la nueva fecha del documento
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones", cotizaciones.Count);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las cotizaciones");
                throw;
            }
        }

        public async Task<Cotizacion?> GetByIdAsync(int id)
        {
            try
            {
                var cotizacion = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cotizacion != null)
                {
                    _logger.LogDebug("Cotización {Id} encontrada", id);
                }
                else
                {
                    _logger.LogDebug("Cotización {Id} no encontrada", id);
                }

                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotización por ID {Id}", id);
                throw;
            }
        }

        // /// <summary>
        // /// Obtiene cotizaciones por el ID del documento de origen (ej. Orden de Trabajo).
        // /// </summary>
        // public async Task<IEnumerable<Cotizacion>> GetOrdenServicio(int IDConcepto)
        // {
        //     try
        //     {
        //         var resultado = await _readContext.Cotizaciones
        //             .AsNoTracking()
        //             .Where(c => c.ConceptoDocumentoId == IDConcepto) // Adaptado para usar DocumentoOrigenId
        //             .OrderByDescending(c => c.Fecha)
        //             .ToListAsync();

        //         _logger.LogDebug("Se obtuvo la Orden de Servicio con ID {resultado.id}", resultado.Count, IDConcepto);
        //         return resultado;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error al obtener cotizaciones por DocumentoOrigenId {OrdenId}", ordenId);
        //         throw;
        //     }
        // }

        /// <summary>
        /// Obtiene cotizaciones filtrando por el campo especificado.
        /// Campo: "cancelado", "afectado", "impreso", "usaCliente"
        /// Valor: 0 o 1
        /// </summary>
        public async Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string campo, int valor)
        {
            try
            {
                IQueryable<Cotizacion> query = _readContext.Cotizaciones.AsNoTracking();

                // Filtrar según el campo y valor especificados
                switch (campo.ToLower())
                {
                    case "cancelado":
                        query = query.Where(c => c.Cancelado == valor);
                        break;
                    case "afectado":
                        query = query.Where(c => c.Afectado == valor);
                        break;
                    case "impreso":
                        query = query.Where(c => c.Impreso == valor);
                        break;
                    case "usacliente":
                        query = query.Where(c => c.UsaCliente == valor);
                        break;
                    default:
                        _logger.LogWarning("Campo de estado no reconocido: {Campo}", campo);
                        break;
                }

                var cotizaciones = await query.OrderByDescending(c => c.Fecha).ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones con {Campo} = {Valor}", cotizaciones.Count, campo, valor);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por estado {Campo} = {Valor}", campo, valor);
                throw;
            }
        }

        /// <summary>
        /// Obtiene cotizaciones por ID de cliente (ClienteProveedorId).
        /// </summary>
        public async Task<IEnumerable<Cotizacion>> GetByClienteIdAsync(int clienteId)
        {
            try
            {
                var cotizaciones = await _readContext.Cotizaciones
                    .AsNoTracking()
                    .Where(c => c.ClienteProveedorId == clienteId)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                _logger.LogDebug("Obtenidas {Count} cotizaciones para ClienteProveedorId {ClienteId}", cotizaciones.Count, clienteId);
                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones por ClienteProveedorId {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var exists = await _readContext.Cotizaciones
                    .AnyAsync(c => c.Id == id);

                _logger.LogDebug("Cotización {Id} existe: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de cotización {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Valida que todas las llaves foráneas existan en sus respectivas tablas.
        /// Retorna un diccionario con el resultado de cada validación.
        /// NOTA: Los IDs de DocumentoDe, ConceptoDocumento, ClienteProveedor y Agente 
        /// provienen de tablas del sistema Contpaqi que no están en este contexto.
        /// Esta validación debe hacerse llamando a un SP o API externa.
        /// Por ahora, retornamos true para permitir la operación.
        /// </summary>
        public async Task<Dictionary<string, bool>> ValidarLlavesForaneasAsync(
            int documentoDeId, 
            int conceptoDocumentoId, 
            int clienteProveedorId, 
            int agenteId)
        {
            var resultados = new Dictionary<string, bool>();

            try
            {
                // TODO: Validar contra tablas de Contpaqi o API externa
                // Por ahora, asumimos que si el ID > 0, es válido
                resultados["DocumentoDeId"] = documentoDeId > 0;
                resultados["ConceptoDocumentoId"] = conceptoDocumentoId > 0;
                
                // Validar ClienteProveedorId contra la tabla de clientes si existe en el contexto
                if (clienteProveedorId > 0)
                {
                    // Buscar por Id o LegacyClientId
                    var clienteExiste = await _readContext.CatalogClientes
                        .AnyAsync(c => c.Id == clienteProveedorId || c.LegacyClientId == clienteProveedorId);
                    resultados["ClienteProveedorId"] = clienteExiste;
                }
                else
                {
                    resultados["ClienteProveedorId"] = false;
                }
                
                // Validar AgenteId (usuario)
                if (agenteId > 0)
                {
                    var agenteExiste = await _readContext.UsuariosAuth
                        .AnyAsync(u => u.Id == agenteId);
                    resultados["AgenteId"] = agenteExiste;
                }
                else
                {
                    resultados["AgenteId"] = false;
                }

                _logger.LogDebug("Validación de FKs completada: {@Resultados}", resultados);
                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar llaves foráneas");
                throw;
            }
        }

        // ✏️ IMPLEMENTACIÓN DE ESCRITURAS

        public async Task<Cotizacion> CreateAsync(Cotizacion cotizacion)
        {
            try
            {
                _writeContext.Cotizaciones.Add(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización creada con ID {Id}", cotizacion.Id);
                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cotización");
                throw;
            }
        }

        public async Task<Cotizacion> UpdateAsync(Cotizacion cotizacion)
        {
            try
            {
                _writeContext.Cotizaciones.Update(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización {Id} actualizada", cotizacion.Id);
                return cotizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cotización {Id}", cotizacion.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cotizacion = await _writeContext.Cotizaciones.FindAsync(id);
                if (cotizacion == null)
                {
                    _logger.LogWarning("Intento de eliminar cotización {Id} que no existe", id);
                    return false;
                }

                _writeContext.Cotizaciones.Remove(cotizacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Cotización {Id} eliminada", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
                throw;
            }
        }
    }
}