// =====================================================================================
// SERVICIO RECEPCIÓN - DashRecepcionService.cs (SIMPLIFICADO)
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Implementa lógica de negocio básica para el módulo de Recepción.
// Usa los DTOs existentes y mapea a la nueva tabla ops.ordenes_trabajo.
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Recepcion;
using back_cabs.CRM.models.Recepcion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Recepcion
{
    /// <summary>
    /// Servicio para gestión de órdenes de trabajo del módulo de Recepción
    /// </summary>
    public class DashRecepcionService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<DashRecepcionService> _logger;

        public DashRecepcionService(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<DashRecepcionService> logger)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // =====================================================================================
        // MÉTODOS DE CONSULTA (READ)
        // =====================================================================================

        /// <summary>
        /// Obtiene todas las órdenes de trabajo
        /// </summary>
        public async Task<List<OrdenTrabajoResponseDto>> ObtenerTodasLasOrdenesAsync(int? skip = null, int? take = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo órdenes de trabajo");

                var query = _readContext.OrdenesTrabajo.AsQueryable();

                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                var ordenes = await query
                    .OrderByDescending(o => o.CreadoEn)
                    .ToListAsync();

                return ordenes.Select(MapearAResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de trabajo");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        public async Task<OrdenTrabajoResponseDto?> ObtenerOrdenPorIdAsync(int id)
        {
            try
            {
                var orden = await _readContext.OrdenesTrabajo
                    .FirstOrDefaultAsync(o => o.Id == id);

                return orden != null ? MapearAResponseDto(orden) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden por ID: {Id}", id);
                throw;
            }
        }

        // =====================================================================================
        // MÉTODOS DE ESCRITURA (CREATE/UPDATE)
        // =====================================================================================

        /// <summary>
        /// Crea una nueva orden de trabajo
        /// </summary>
        public async Task<OrdenTrabajoResponseDto> CrearOrdenTrabajoAsync(OrdenTrabajoCreacionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creando nueva orden de trabajo");

                // Validar que el usuario existe
                var usuarioExiste = await _readContext.UsuariosAuth.AnyAsync(u => u.Id == request.IdUsuario);
                if (!usuarioExiste)
                    throw new ArgumentException($"Usuario con ID {request.IdUsuario} no existe");

                // Validar cliente legacy si se proporciona
                if (request.LegacyClientId.HasValue)
                {
                    var clienteLegacyExiste = await ValidarClienteLegacyAsync(request.LegacyClientId);
                    if (!clienteLegacyExiste)
                        throw new ArgumentException($"Cliente legacy con ID {request.LegacyClientId} no encontrado en el sistema legacy");
                }

                var nuevaOrden = new OrdenTrabajo
                {
                    ClienteId = request.LegacyClientId, // Para clientes legacy, usar el LegacyClientId
                    NuevoCliente = !request.LegacyClientId.HasValue, // TRUE si es cliente nuevo (sin LegacyClientId)
                    NombreCliente = !request.LegacyClientId.HasValue ? "Cliente Nuevo - Pendiente de Captura" : null, // Solo para clientes nuevos
                    CreadoPorUserId = request.IdUsuario,
                    AsignadaAUserId = request.IdUsuario, // Por ahora mismo usuario
                    Notas = request.Notas,
                    CitaProgramadaInicio = request.CitaProgramadaInicio,
                    CitaProgramadaFin = request.CitaProgramadaFin,
                    Modalidad = request.Modalidad,
                    TipoOrden = request.TipoOrden,
                    Prioridad = request.Prioridad,
                    Estado = request.Estado ? "ASIGNADA" : "CAPTURADA", // Mapear bool a string
                    UbicacionText = request.UbicacionText,
                    RequiereFactura = request.RequiereFactura,
                    EstadoFacturado = request.EstadoFacturado.HasValue 
                        ? (request.EstadoFacturado.Value ? "FACTURADO" : "PENDIENTE") 
                        : null,
                    FacturaFolio = request.FacturaFolio?.ToString(), // int? a string?
                    CostoEstimado = request.CostoEstimado,
                    CostoReal = request.CostoReal,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                _writeContext.OrdenesTrabajo.Add(nuevaOrden);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Orden creada con ID: {Id}", nuevaOrden.Id);

                return MapearAResponseDto(nuevaOrden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de trabajo");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una orden de trabajo
        /// </summary>
        public async Task<bool> ActualizarOrdenTrabajoAsync(int id, OrdenTrabajoActualizacionRequestDto request)
        {
            try
            {
                var orden = await _writeContext.OrdenesTrabajo.FindAsync(id);
                if (orden == null) return false;

                // Actualizar campos si vienen en el request
                if (request.Notas != null) orden.Notas = request.Notas;
                if (request.CitaProgramadaInicio.HasValue) orden.CitaProgramadaInicio = request.CitaProgramadaInicio;
                if (request.CitaProgramadaFin.HasValue) orden.CitaProgramadaFin = request.CitaProgramadaFin;
                if (request.Prioridad.HasValue) orden.Prioridad = request.Prioridad;
                if (request.Estado.HasValue) orden.Estado = request.Estado.Value ? "ASIGNADA" : "CAPTURADA";
                if (request.EstadoFacturado.HasValue) 
                    orden.EstadoFacturado = request.EstadoFacturado.Value ? "FACTURADO" : "PENDIENTE";
                if (request.RequiereFactura.HasValue) orden.RequiereFactura = request.RequiereFactura.Value;
                if (request.FacturaFolio.HasValue) orden.FacturaFolio = request.FacturaFolio.ToString();
                if (request.CostoReal.HasValue) orden.CostoReal = request.CostoReal.Value;
                if (request.CostoEstimado.HasValue) orden.CostoEstimado = request.CostoEstimado.Value;

                if (request.IdUsuario.HasValue)
                    orden.AsignadaAUserId = request.IdUsuario.Value;
                orden.ActualizadoEn = DateTime.UtcNow;

                await _writeContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar orden ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas básicas
        /// </summary>
        public async Task<Dictionary<string, object>> ObtenerEstadisticasAsync()
        {
            try
            {
                var totalOrdenes = await _readContext.OrdenesTrabajo.CountAsync();
                var ordenesActivas = await _readContext.OrdenesTrabajo.CountAsync(o => o.Estado != "CERRADA");
                
                return new Dictionary<string, object>
                {
                    ["totalOrdenes"] = totalOrdenes,
                    ["ordenesActivas"] = ordenesActivas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                throw;
            }
        }

        // =====================================================================================
        // MÉTODOS PRIVADOS
        // =====================================================================================

        /// <summary>
        /// Valida si existe un cliente legacy por LegacyClientId en la vista ViewClientesCompletos
        /// </summary>
        private async Task<bool> ValidarClienteLegacyAsync(int? legacyClientId)
        {
            if (!legacyClientId.HasValue)
                return false;

            // Buscar en la vista de clientes completos (solo lectura)
            var clienteLegacy = await _readContext.ClientesCompletos
                .FirstOrDefaultAsync(c => c.LegacyClientId == legacyClientId.Value);
                
            return clienteLegacy != null;
        }

        /// <summary>
        /// Mapea OrdenTrabajo a OrdenTrabajoResponseDto (usando estructura vieja)
        /// </summary>
        private OrdenTrabajoResponseDto MapearAResponseDto(OrdenTrabajo orden)
        {
            return new OrdenTrabajoResponseDto
            {
                Id = orden.Id,
                Notas = orden.Notas,
                CitaProgramadaInicio = orden.CitaProgramadaInicio,
                CitaProgramadaFin = orden.CitaProgramadaFin,
                Modalidad = orden.Modalidad,
                TipoOrden = orden.TipoOrden,
                LegacyClientId = orden.ClienteId ?? 0, // Mapear ClienteId (nullable) a LegacyClientId, usar 0 si es null
                Prioridad = orden.Prioridad,
                Estado = orden.Estado == "ASIGNADA" || orden.Estado == "EN_CURSO", // string a bool
                UbicacionText = orden.UbicacionText,
                EstadoFacturado = orden.EstadoFacturado == "FACTURADO", // string a bool
                RequiereFactura = orden.RequiereFactura,
                CostoReal = orden.CostoReal, // mantener como decimal
                CostoEstimado = orden.CostoEstimado,
                CreadoEn = orden.CreadoEn,
                ActualizadoEn = orden.ActualizadoEn,
                IdUsuario = orden.AsignadaAUserId ?? orden.CreadoPorUserId // Mapear a campo corregido
            };
        }
    }
}