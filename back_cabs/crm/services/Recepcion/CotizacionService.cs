using back_cabs.CRM.Interfaces.Recepcion;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.models.Sales;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.Core.Exceptions;

namespace back_cabs.CRM.services.Recepcion;

/// <summary>
/// Servicio que maneja la lógica de negocio para cotizaciones.
/// Usa Repository Pattern para acceso a datos con separación de responsabilidades.
/// </summary>
public class CotizacionService
{
    // ✅ Inyección de dependencias: Repository Pattern para acceso a datos
    // desacoplado y testeable
    private readonly ICotizacionRepository _cotizacionRepository;
    private readonly ILogger<CotizacionService> _logger;

    public CotizacionService(
        ICotizacionRepository cotizacionRepository,
        ILogger<CotizacionService> logger)
    {
        _cotizacionRepository = cotizacionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cotizaciones ordenadas por fecha de creación descendente.
    /// ✅ Usa Repository Pattern para acceso optimizado a datos
    /// </summary>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerTodosAsync()
    {
        // ✅ Repository Pattern: abstracción completa del acceso a datos
        var cotizaciones = await _cotizacionRepository.GetAllAsync();

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene una cotización por ID.
    /// ✅ Usa Repository Pattern para consulta optimizada
    /// </summary>
    public async Task<CotizacionResponseDto?> ObtenerPorIdAsync(int id)
    {
        // ✅ Repository Pattern: consulta optimizada sin tracking
        var cotizacion = await _cotizacionRepository.GetByIdAsync(id);

        return cotizacion != null ? MapToResponseDto(cotizacion) : null;
    }

    // /// <summary>
    // /// Obtiene cotizaciones por OrdenId.
    // /// ✅ Usa Repository Pattern para consultas filtradas
    // /// </summary>
    // public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorOrdenIdAsync(int ordenId)
    // {
    //     var cotizaciones = await _cotizacionRepository.GetByOrdenIdAsync(ordenId);

    //     return cotizaciones.Select(MapToResponseDto);
    // }

    /// <summary>
    /// Obtiene cotizaciones por estado (campo y valor específicos).
    /// </summary>
    /// <param name="campo">Campo a filtrar: "cancelado", "afectado", "impreso", "usaCliente"</param>
    /// <param name="valor">Valor del campo: 0 o 1</param>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorEstadoAsync(string campo, int valor)
    {
        var cotizaciones = await _cotizacionRepository.GetByEstadoAsync(campo, valor);

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene cotizaciones por ID de cliente.
    /// </summary>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorClienteIdAsync(int clienteId)
    {
        var cotizaciones = await _cotizacionRepository.GetByClienteIdAsync(clienteId);

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva cotización.
    /// ✅ Valida llaves foráneas antes de crear
    /// </summary>
    public async Task<CotizacionResponseDto> CrearAsync(CotizacionRequestDto request)
    {
        try
        {
            // 1. Validar llaves foráneas
            var validacionFKs = await _cotizacionRepository.ValidarLlavesForaneasAsync(
                request.DocumentoDeId,
                request.ConceptoDocumentoId,
                request.ClienteProveedorId,
                request.AgenteId
            );

            // 2. Lanzar excepciones específicas si alguna FK no existe
            foreach (var (campo, existe) in validacionFKs)
            {
                if (!existe)
                {
                    var valorId = campo switch
                    {
                        "DocumentoDeId" => request.DocumentoDeId,
                        "ConceptoDocumentoId" => request.ConceptoDocumentoId,
                        "ClienteProveedorId" => request.ClienteProveedorId,
                        "AgenteId" => request.AgenteId,
                        _ => 0
                    };
                    throw new ForeignKeyNotFoundException(campo, valorId);
                }
            }

            // 3. Validar duplicados si es necesario (por ejemplo, mismo Folio)
            // TODO: Agregar validación de duplicados si se requiere

            // 4. Mapear y crear la entidad
            var cotizacion = MapFromRequestDto(request);
            cotizacion.Fecha = DateTime.UtcNow; // El servidor establece la fecha de creación

            var creada = await _cotizacionRepository.CreateAsync(cotizacion);

            _logger.LogInformation("Cotización creada con ID {Id}", creada.Id);

            return MapToResponseDto(creada);
        }
        catch (ForeignKeyNotFoundException)
        {
            // Re-lanzar las excepciones personalizadas sin envolverlas
            throw;
        }
        catch (DuplicateRecordException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cotización");
            throw;
        }
    }

    /// <summary>
    /// Actualiza una cotización existente.
    /// ✅ Usa Repository Pattern para escritura transaccional
    /// </summary>
    public async Task<CotizacionResponseDto?> ActualizarAsync(int id, CotizacionRequestDto request)
    {
        try
        {
            var existente = await _cotizacionRepository.GetByIdAsync(id);
            if (existente == null)
            {
                _logger.LogWarning("Intento de actualizar cotización {Id} que no existe", id);
                return null;
            }

            // Mapear cambios
            //Datos Principales
            existente.Folio = request.Folio ?? existente.Folio;
            existente.FechaVencimiento = request.FechaVencimiento;
            existente.FechaEntregaRecepcion = request.FechaEntregaRecepcion;
            
            //Datos Descriptivos
            existente.Referencia = request.Referencia;
            existente.Observaciones = request.Observaciones;
            
            //Totales en cantidad y dinero
            existente.Neto = request.Neto;
            existente.Total = request.Total;
            existente.TotalUnidades = request.TotalUnidades;

            var actualizada = await _cotizacionRepository.UpdateAsync(existente);

            _logger.LogInformation("Cotización {Id} actualizada", actualizada.Id);

            return MapToResponseDto(actualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Elimina una cotización por ID.
    /// ✅ Usa Repository Pattern para eliminación transaccional
    /// </summary>
    public async Task<bool> EliminarAsync(int id)
    {
        try
        {
            var eliminado = await _cotizacionRepository.DeleteAsync(id);

            if (eliminado)
            {
                _logger.LogInformation("Cotización {Id} eliminada", id);
            }
            else
            {
                _logger.LogWarning("Intento de eliminar cotización {Id} que no existe", id);
            }

            return eliminado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
            throw;
        }
    }

    // ✅ MÉTODOS DE MAPEO: Separación clara entre entidades y DTOs

    private static CotizacionResponseDto MapToResponseDto(Cotizacion cotizacion)
    {
        return new CotizacionResponseDto
        {
            Id = cotizacion.Id,

            // DocumentoDeId = cotizacion.DocumentoDeId,
            // ConceptoDocumentoId = cotizacion.ConceptoDocumentoId,
            // ClienteProveedorId = cotizacion.ClienteProveedorId,
            // AgenteId = cotizacion.AgenteId,
            // DocumentoOrigenId = cotizacion.DocumentoOrigenId,

            SerieDocumento = cotizacion.SerieDocumento,
            Folio = cotizacion.Folio,
            Fecha = cotizacion.Fecha,
            FechaVencimiento = cotizacion.FechaVencimiento,
            FechaEntregaRecepcion = cotizacion.FechaEntregaRecepcion,
            RazonSocial = cotizacion.RazonSocial,
            Rfc = cotizacion.Rfc,
            Referencia = cotizacion.Referencia,
            Observaciones = cotizacion.Observaciones,
            Naturaleza = cotizacion.Naturaleza,
            UsaCliente = cotizacion.UsaCliente,
            Afectado = cotizacion.Afectado,
            Impreso = cotizacion.Impreso,
            Cancelado = cotizacion.Cancelado,
            Neto = cotizacion.Neto,
            Impuesto1 = cotizacion.Impuesto1,
            DescuentoMovimiento = cotizacion.DescuentoMovimiento,
            Total = cotizacion.Total,
            Pendiente = cotizacion.Pendiente,
            TotalUnidades = cotizacion.TotalUnidades
        };
    }

    private static Cotizacion MapFromRequestDto(CotizacionRequestDto request)
    {
        return new Cotizacion
        {
            DocumentoDeId = request.DocumentoDeId,
            ConceptoDocumentoId = request.ConceptoDocumentoId,
            ClienteProveedorId = request.ClienteProveedorId,
            AgenteId = request.AgenteId,
            DocumentoOrigenId = request.DocumentoOrigenId,
            SerieDocumento = request.SerieDocumento,
            FechaVencimiento = request.FechaVencimiento,
            FechaEntregaRecepcion = request.FechaEntregaRecepcion,
            RazonSocial = request.RazonSocial,
            Rfc = request.Rfc,
            Referencia = request.Referencia,
            Observaciones = request.Observaciones,
            TotalUnidades = request.TotalUnidades
        };
    }
}