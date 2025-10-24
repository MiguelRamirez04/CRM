using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.Interfaces.Soporte; // Asumo que IReparacionRepository está aquí
using back_cabs.CRM.models.Soporte;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.repositories.Soporte
{
    /// <summary>
    /// Repositorio que implementa la lógica de acceso a datos para Reparaciones y sus Componentes,
    /// utilizando Entity Framework Core.
    /// </summary>
    public class ReparacionRepository : IReparacionRepository // Asumo que esta es la interfaz que se implementa
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<ReparacionRepository> _logger;

        public ReparacionRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<ReparacionRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // =====================================================================
        // IMPLEMENTACIÓN DE REPARACIÓN (CRUD)
        // =====================================================================

        public async Task<Reparacion?> GetReparacionForUpdateAsync(int id)
        {
            try
            {
                // Se usa WriteContext y NO AsNoTracking()
                var reparacion = await _writeContext.Reparaciones
                    .FirstOrDefaultAsync(r => r.Id == id);

                // La entidad 'reparacion' ahora está siendo rastreada por _writeContext.

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparación no encontrada por ID {Id} para actualización", id);
                }
                return reparacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reparación para actualización ID {Id}", id);
                throw;
            }
        }
        public async Task<IEnumerable<Reparacion>> ObtenerReparacionesAsync(int? skip, int? take)
        {
            try
            {
                var query = _readContext.Reparaciones
                    .AsNoTracking()
                    .AsQueryable();

                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                // Incluir relaciones clave para el DTO de respuesta (ej. Técnico)
                return await query
                    .Include(r => r.TecnicoId)
                    .OrderByDescending(r => r.FechaLlegada)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reparaciones.");
                throw;
            }
        }

        public async Task<Reparacion?> ObtenerReparacionPorIdAsync(int id)
        {
            try
            {
                // Usamos FindAsync con WriteContext para UPDATE posterior, si es necesario,
                // o ReadContext.FirstOrDefaultAsync si solo se usa para lectura.
                // Aquí usamos ReadContext (AsNoTracking) para la función GET.
                return await _readContext.Reparaciones
                    .AsNoTracking()
                    .Include(r => r.TecnicoId)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reparación por ID {Id}", id);
                throw;
            }
        }

        public async Task<Reparacion> CrearReparacionAsync(Reparacion reparacion)
        {
            try
            {
                _writeContext.Reparaciones.Add(reparacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Reparación creada en BD con ID {Id}", reparacion.Id);
                return reparacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync para Reparación ID {OrdenId}", reparacion.OrdenId);
                throw;
            }
        }

        public async Task<(int FilasAfectadas, Reparacion? ReparacionActualizada)> ActualizarReparacionAsync(Reparacion reparacion)
        {
            try
            {
                // El servicio ya debió cargar la entidad con FindAsync(id) en el WriteContext 
                // y modificar sus propiedades. Solo necesitamos guardar.
                _writeContext.Reparaciones.Update(reparacion); // Usar Update si la entidad fue modificada en el servicio
                int filasAfectadas = await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Reparación ID {Id} actualizada. Filas: {Filas}", reparacion.Id, filasAfectadas);

                // Devolver el objeto actualizado (que ya está modificado en memoria)
                return (filasAfectadas, reparacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reparación con ID {Id}", reparacion.Id);
                throw;
            }
        }

        public async Task<bool> OrdenExisteAsync(int ordenId)
        {
            try
            {
                return await _readContext.OrdenesTrabajo.AnyAsync(o => o.Id == ordenId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Orden ID {Id}", ordenId);
                throw;
            }
        }

        public async Task<bool> TecnicoExisteAsync(int tecnicoId)
        {
            try
            {
                return await _readContext.UsuariosAuth.AnyAsync(u => u.Id == tecnicoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Técnico ID {Id}", tecnicoId);
                throw;
            }
        }


        // =====================================================================
        // IMPLEMENTACIÓN DE COMPONENTES (CRUD)
        // =====================================================================

        public async Task<ReparacionComponente?> GetComponenteForUpdateAsync(int id)
        {
            try
            {
                // Se usa WriteContext y NO AsNoTracking()
                var componente = await _writeContext.ReparacionesComponentes
                    .FirstOrDefaultAsync(r => r.Id == id);

                // La entidad 'componente' ahora está siendo rastreada por _writeContext.

                if (componente == null)
                {
                    _logger.LogWarning("Componente no encontrada por ID {Id} para actualización", id);
                }
                return componente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componente para actualización ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ReparacionComponente>> ObtenerComponentesReparacionAsync(int? skip, int? take)
        {
            try
            {
                var query = _readContext.ReparacionesComponentes
                    .AsNoTracking()
                    .AsQueryable();

                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                return await query.OrderByDescending(r => r.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componentes de reparación.");
                throw;
            }
        }

        public async Task<ReparacionComponente?> ObtenerComponenteReparacionPorIdAsync(int id)
        {
            try
            {
                return await _readContext.ReparacionesComponentes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componente por ID {Id}", id);
                throw;
            }
        }

        public async Task<ReparacionComponente> CrearComponenteReparacionAsync(ReparacionComponente componente)
        {
            try
            {
                _writeContext.ReparacionesComponentes.Add(componente);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Componente creado con ID {Id}", componente.Id);
                return componente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync para Componente ID {ReparacionId}", componente.ReparacionId);
                throw;
            }
        }

        public async Task<(int FilasAfectadas, ReparacionComponente? ComponenteActualizado)> ActualizarComponenteReparacionAsync(ReparacionComponente componente)
        {
            try
            {
                // El servicio ya debió cargar la entidad y modificar sus propiedades.
                _writeContext.ReparacionesComponentes.Update(componente);
                int filasAfectadas = await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Componente de reparación ID {Id} actualizado. Filas: {Filas}", componente.Id, filasAfectadas);

                return (filasAfectadas, componente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar componente de reparación con ID {Id}", componente.Id);
                throw;
            }
        }

        Task<List<ReparacionResponseDto>> IReparacionRepository.ObtenerReparacionesAsync(int? skip, int? take)
        {
            throw new NotImplementedException();
        }

        Task<ReparacionResponseDto?> IReparacionRepository.ObtenerReparacionPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ReparacionResponseDto> CrearReparacionAsync(ReparacionCreacionRequestDto request)
        {
            throw new NotImplementedException();
        }

        public Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> ActualizarReparacionAsync(int id, ReparacionActualizacionRequestDto request)
        {
            throw new NotImplementedException();
        }

        Task<List<ReparacionComponenteResponseDto>> IReparacionRepository.ObtenerComponentesReparacionAsync(int? skip, int? take)
        {
            throw new NotImplementedException();
        }

        Task<ReparacionComponenteResponseDto?> IReparacionRepository.ObtenerComponenteReparacionPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ReparacionComponenteResponseDto> CrearComponenteReparacionAsync(ReparacionComponenteRequestDto request)
        {
            throw new NotImplementedException();
        }

        Task<(int FilasAfectadas, ReparacionComponenteResponseDto? ReparacionComponenteActualizada)> IReparacionRepository.ActualizarComponenteReparacionAsync(ReparacionComponente Componente)
        {
            throw new NotImplementedException();
        }

        Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> IReparacionRepository.ActualizarReparacionAsync(Reparacion reparacionActualizada)
        {
            throw new NotImplementedException();
        }
    }
}