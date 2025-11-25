using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Recepcion;
using Microsoft.EntityFrameworkCore;


namespace back_cabs.CRM.repositories.Recepcion
{
    /// <summary>
    /// Implementación concreta del IOrdenTrabajoRepository usando Entity Framework Core.
    /// Maneja el acceso directo a la base de datos para la entidad OrdenTrabajo.
    /// </summary>
    public class OrdenTrabajoRepository : IOrdenTrabajoRepository
    {
        // Se inyectan ambos contextos para la separación de lectura/escritura (CQRS simplificado)
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<OrdenTrabajoRepository> _logger;

        public OrdenTrabajoRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<OrdenTrabajoRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // 📖 QUERIES (Lectura)
        // ---------------------------------------------------------------------

        public async Task<OrdenTrabajo?> GetByIdAsync(int id)
        {
            try
            {
                // Usamos ReadContext para no tracking
                return await _readContext.OrdenesTrabajo
                    .AsNoTracking()
                    .Include(o => o.CreadoPor) // Eager loading de relaciones clave
                    .Include(o => o.AsignadaA)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByIdAsync para Orden ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetAllFilteredAsync(int? skip, int? take, string? estado)
        {
            try
            {
                var query = _readContext.OrdenesTrabajo
                    .AsNoTracking()
                    .AsQueryable();

                // Aplicar filtro de estado
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(o => o.Estado == estado);
                }

                // Aplicar paginación
                if (skip.HasValue) query = query.Skip(skip.Value);
                if (take.HasValue) query = query.Take(take.Value);

                return await query
                    .Include(o => o.CreadoPor)
                    .Include(o => o.AsignadaA)
                    .OrderByDescending(o => o.CreadoEn) // Asumimos CreadoEn es DateTime?
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllFilteredAsync");
                throw;
            }
        }

        public async Task<bool> UsuarioExistsAsync(int userId)
        {
            try
            {
                // Consulta eficiente en la tabla de usuarios
                return await _readContext.UsuariosAuth.AnyAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UsuarioExistsAsync para ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> OrdenExistsAsync(int id)
        {
            try
            {
                // Consulta eficiente
                return await _readContext.OrdenesTrabajo.AnyAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en OrdenExistsAsync para ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAllEstadosAsync()
        {
            try
            {
                // Obtiene la lista de todos los estados existentes para el cálculo de estadísticas
                var estados = await _readContext.OrdenesTrabajo
                    .Where(o => o.Estado != null)
                    .Select(o => o.Estado!)
                    .ToListAsync();
                return estados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllEstadosAsync");
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // ✍️ COMMANDS (Escritura)
        // ---------------------------------------------------------------------

        public async Task<OrdenTrabajo> CreateAsync(OrdenTrabajo orden)
        {
            try
            {
                // Usamos WriteContext para la persistencia
                _writeContext.OrdenesTrabajo.Add(orden);
                await _writeContext.SaveChangesAsync();
                
                _logger.LogInformation("Orden creada en BD con ID {Id}", orden.Id);
                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync para Orden");
                // Es responsabilidad del servicio manejar DbUpdateException
                throw;
            }
        }

        public async Task<bool> UpdateAsync(OrdenTrabajo orden)
        {
            try
            {
                // Attach y marcar como modificada si no está siendo rastreada,
                // o simplemente SaveChanges si fue obtenida con tracking.
                // Asumimos que la entidad 'orden' ya está lista para ser actualizada.
                _writeContext.OrdenesTrabajo.Update(orden);
                int filasAfectadas = await _writeContext.SaveChangesAsync();
                
                _logger.LogInformation("Orden ID {Id} actualizada. Filas: {Filas}", orden.Id, filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateAsync para Orden ID {Id}", orden.Id);
                // Es responsabilidad del servicio manejar DbUpdateException
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // MÉTODOS AUXILIARES
        // ---------------------------------------------------------------------

        public async Task<IEnumerable<dynamic>> FindClientesLegacyAsync(string termino, int limite)
        {
            try
            {
                // Consulta sobre la vista ClientesCompletos (que se asume está mapeada)
                var clientes = await _readContext.ClientesCompletos
                    .AsNoTracking()
                    .Where(c => (c.NombreComercial != null && c.NombreComercial.Contains(termino)) ||
                            (c.RFC != null && c.RFC.Contains(termino)))
                    .Select(c => new 
                    {
                        c.ClienteId,
                        c.NombreComercial,
                        c.RFC,
                        c.LegacyClientId // Devolvemos un objeto anónimo (dynamic)
                    })
                    .Take(limite)
                    .ToListAsync();

                // El servicio se encarga de convertir este dynamic/anónimo a ClienteResumenDto
                return clientes.Select(c => (dynamic)c); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FindClientesLegacyAsync");
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetClientesNuevosAgrupadosAsync()
        {
            try
            {
                // Lógica de agrupación de SQL, movida al repositorio
                var clientesNuevos = await _readContext.OrdenesTrabajo
                    .AsNoTracking()
                    .Where(o => o.NuevoCliente == true && o.NombreCliente != null)
                    .GroupBy(o => new { o.NombreCliente, o.ClienteTelefono })
                    .Select(g => new 
                    {
                        NombreCliente = g.Key.NombreCliente,
                        Telefono = g.Key.ClienteTelefono,
                        NumeroOrdenes = g.Count()
                    })
                    .OrderBy(c => c.NombreCliente)
                    .ToListAsync();

                return clientesNuevos.Select(c => (dynamic)c);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetClientesNuevosAgrupadosAsync");
                throw;
            }
        }
    }
}