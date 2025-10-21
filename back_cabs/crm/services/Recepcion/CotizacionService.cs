using back_cabs.CRM.Interfaces.Recepcion;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.models.Sales;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Obtiene cotizaciones por OrdenId.
    /// ✅ Usa Repository Pattern para consultas filtradas
    /// </summary>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorOrdenIdAsync(int ordenId)
    {
        var cotizaciones = await _cotizacionRepository.GetByOrdenIdAsync(ordenId);

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene cotizaciones por estado.
    /// ✅ Usa Repository Pattern para consultas filtradas
    /// </summary>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorEstadoAsync(string estado)
    {
        var cotizaciones = await _cotizacionRepository.GetByEstadoAsync(estado);

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene cotizaciones por cliente.
    /// ✅ Usa Repository Pattern para consultas filtradas
    /// </summary>
    public async Task<IEnumerable<CotizacionResponseDto>> ObtenerPorClienteAsync(string cliente)
    {
        var cotizaciones = await _cotizacionRepository.GetByClienteAsync(cliente);

        return cotizaciones.Select(MapToResponseDto);
    }

    /// <summary>
    /// Crea una nueva cotización.
    /// ✅ Usa Repository Pattern para escritura transaccional
    /// </summary>
    public async Task<CotizacionResponseDto> CrearAsync(CotizacionCreateRequestDto request)
    {
        try
        {
            var cotizacion = MapFromCreateRequestDto(request);
            cotizacion.CreadoEn = DateTime.UtcNow;

            var creada = await _cotizacionRepository.CreateAsync(cotizacion);

            _logger.LogInformation("Cotización creada con ID {Id}", creada.Id);

            return MapToResponseDto(creada);
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
    public async Task<CotizacionResponseDto?> ActualizarAsync(int id, CotizacionCreateRequestDto request)
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
            existente.OrdenId = request.OrdenId;
            existente.IntakeLegacyId = request.IntakeLegacyId;
            existente.Subtotal = request.Subtotal;
            existente.ImpuestosTotal = request.ImpuestosTotal;
            existente.Descuento = request.Descuento;
            existente.Estado = request.Estado;
            existente.Observaciones = request.Observaciones;
            existente.Cliente = request.Cliente;
            existente.Rfc = request.Rfc;
            existente.Folio = request.Folio;
            existente.DescripcionServicio = request.DescripcionServicio;
            existente.ValidezDias = request.ValidezDias;
            existente.ActualizadoEn = DateTime.UtcNow;

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
            OrdenId = cotizacion.OrdenId,
            IntakeLegacyId = cotizacion.IntakeLegacyId,
            Subtotal = cotizacion.Subtotal,
            ImpuestosTotal = cotizacion.ImpuestosTotal,
            Descuento = cotizacion.Descuento,
            Total = cotizacion.Total, // Calculado automáticamente en el modelo
            Estado = cotizacion.Estado,
            Observaciones = cotizacion.Observaciones,
            Cliente = cotizacion.Cliente,
            Rfc = cotizacion.Rfc,
            Folio = cotizacion.Folio,
            DescripcionServicio = cotizacion.DescripcionServicio,
            ActualizadoEn = cotizacion.ActualizadoEn,
            CreadoEn = cotizacion.CreadoEn,
            ValidezDias = cotizacion.ValidezDias
        };
    }

    private static Cotizacion MapFromCreateRequestDto(CotizacionCreateRequestDto request)
    {
        return new Cotizacion
        {
            OrdenId = request.OrdenId,
            IntakeLegacyId = request.IntakeLegacyId,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Descuento = request.Descuento,
            Estado = request.Estado,
            Observaciones = request.Observaciones,
            Cliente = request.Cliente,
            Rfc = request.Rfc,
            Folio = request.Folio,
            DescripcionServicio = request.DescripcionServicio,
            ValidezDias = request.ValidezDias
        };
    }
}