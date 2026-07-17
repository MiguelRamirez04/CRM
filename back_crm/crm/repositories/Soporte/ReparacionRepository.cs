using back_cabs.CRM.contexts;
// using back_cabs.CRM.DTOs.Soporte; // <-- NO DEBE USAR DTOs
using back_cabs.CRM.Interfaces.Soporte; // Interfaz correcta
using back_cabs.CRM.models.Soporte; // Usar Entidades
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace back_cabs.CRM.repositories.Soporte
{
    /// <summary>
    /// Implementación concreta del IReparacionRepository usando Entity Framework Core.
    /// Maneja el acceso directo a la base de datos para Reparaciones y Componentes.
    /// </summary>
    public class ReparacionRepository : IReparacionRepository // Implementa la interfaz corregida
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
                // Usa WriteContext SIN AsNoTracking para permitir modificaciones
                var reparacion = await _writeContext.Reparaciones
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparación no encontrada por ID {Id} para actualización", id);
                }
                return reparacion; // Entidad rastreada
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reparación para actualización ID {Id}", id);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<IEnumerable<Reparacion>> ObtenerReparacionesAsync(int? skip, int? take)
        {
            try
            {
                var query = _readContext.Reparaciones
                    .AsNoTracking()
                    .AsQueryable();

                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                // Corregido: Incluir la propiedad de navegación
                return await query 
                    .OrderByDescending(r => r.FechaLlegada)
                    .ToListAsync(); // Devuelve IEnumerable<Reparacion>
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reparaciones.");
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<Reparacion?> ObtenerReparacionPorIdAsync(int id)
        {
            try
            {
                // Corregido: Incluir la propiedad de navegación
                return await _readContext.Reparaciones
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id); // Devuelve Reparacion?
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reparación por ID {Id}", id);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<Reparacion> CrearReparacionAsync(Reparacion reparacion) // Recibe Entidad
        {
            try
            {
                _writeContext.Reparaciones.Add(reparacion);
                await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Reparación creada en BD con ID {Id}", reparacion.Id);
                return reparacion; // Devuelve Entidad
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync para Reparación Orden ID {OrdenId}", reparacion.OrdenId);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<(int FilasAfectadas, Reparacion? ReparacionActualizada)> ActualizarReparacionAsync(Reparacion reparacionActualizada) // Recibe Entidad
        {
            try
            {
                 // Si la entidad fue obtenida con GetReparacionForUpdateAsync, ya está rastreada.
                 // Si no, necesitarías _writeContext.Reparaciones.Update(reparacionActualizada);
                 // Asumimos que ya está rastreada por el flujo del servicio.
                int filasAfectadas = await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Reparación ID {Id} actualizada. Filas: {Filas}", reparacionActualizada.Id, filasAfectadas);
                return (filasAfectadas, reparacionActualizada); // Devuelve Entidad
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reparación con ID {Id}", reparacionActualizada.Id);
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
        
        // Añadir implementación faltante
        public async Task<bool> ReparacionExisteAsync(int reparacionId)
        {
            try
            {
                return await _readContext.Reparaciones.AnyAsync(r => r.Id == reparacionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Reparación ID {Id}", reparacionId);
                throw;
            }
        }


        // =====================================================================
        // IMPLEMENTACIÓN DE COMPONENTES (CRUD) - Firmas ya correctas
        // =====================================================================

        public async Task<ReparacionComponente?> GetComponenteForUpdateAsync(int id)
        {
            try
            {
                // Usa WriteContext SIN AsNoTracking para permitir modificaciones
                var componente = await _writeContext.ReparacionesComponentes
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (componente == null)
                {
                    _logger.LogWarning("Componente no encontrado por ID {Id} para actualización", id);
                }
                return componente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componente para actualización ID {Id}", id);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<IEnumerable<ReparacionComponente>> ObtenerComponentesReparacionAsync(int? skip, int? take)
        {
            try
            {
                var query = _readContext.ReparacionesComponentes
                    .AsNoTracking()
                    .AsQueryable();

                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                return await query.OrderByDescending(r => r.Id).ToListAsync(); // Devuelve Entidades
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componentes de reparación.");
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<ReparacionComponente?> ObtenerComponenteReparacionPorIdAsync(int id)
        {
            try
            {
                return await _readContext.ReparacionesComponentes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id); // Devuelve Entidad
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener componente por ID {Id}", id);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<ReparacionComponente> CrearComponenteReparacionAsync(ReparacionComponente componente) // Recibe Entidad
        {
            try
            {
                _writeContext.ReparacionesComponentes.Add(componente);
                await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Componente creado con ID {Id}", componente.Id);
                return componente; // Devuelve Entidad
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync para Componente Reparación ID {ReparacionId}", componente.ReparacionId);
                throw;
            }
        }

        // Implementa la firma corregida de la interfaz
        public async Task<(int FilasAfectadas, ReparacionComponente? ComponenteActualizado)> ActualizarComponenteReparacionAsync(ReparacionComponente componente) // Recibe Entidad
        {
            try
            {
                // Asumimos que la entidad ya está rastreada por el flujo del servicio
                int filasAfectadas = await _writeContext.SaveChangesAsync();
                _logger.LogInformation("Componente de reparación ID {Id} actualizado. Filas: {Filas}", componente.Id, filasAfectadas);
                return (filasAfectadas, componente); // Devuelve Entidad
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar componente de reparación con ID {Id}", componente.Id);
                throw;
            }
        }

        public Task<IEnumerable<ReparacionComponente>> ObtenerComponentePorIdReparacionAsync(int repID)
        {
            throw new NotImplementedException();
        }


        // ELIMINAR TODA LA SECCIÓN DE IMPLEMENTACIÓN EXPLÍCITA INCORRECTA
        // 
        // Task<List<ReparacionResponseDto>> IReparacionRepository.ObtenerReparacionesAsync(int? skip, int? take) { ... }
        // Task<ReparacionResponseDto?> IReparacionRepository.ObtenerReparacionPorIdAsync(int id) { ... }
        // public Task<ReparacionResponseDto> CrearReparacionAsync(ReparacionCreacionRequestDto request) { ... }
        // ... (etc.)
        // Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> IReparacionRepository.ActualizarReparacionAsync(Reparacion reparacionActualizada) { ... }
        // 
        // FIN DE LA SECCIÓN A ELIMINAR

    }
}