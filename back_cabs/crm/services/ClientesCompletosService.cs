// =====================================================================================
// SERVICIO CLIENTES COMPLETOS - ClientesCompletosService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Contiene la lógica de negocio para consultar la vista VwClientesCompletos.
// Se encarga de acceder a la base de datos a través del ReadOnlyContext,
// aplicar filtros y mapear los resultados a DTOs.
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services
{
    /// <summary>
    /// Servicio para consultas de solo lectura sobre la vista de clientes completos.
    /// </summary>
    public class ClientesCompletosService
    {
        private readonly ReadOnlyContext _context;
        private readonly ILogger<ClientesCompletosService> _logger;

        public ClientesCompletosService(ReadOnlyContext context, ILogger<ClientesCompletosService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los clientes de la vista.
        /// </summary>
        public async Task<IEnumerable<VwClientesCompletosDto>> GetAllAsync()
        {
            _logger.LogInformation("Obteniendo todos los clientes completos");
            var clientes = await _context.ClientesCompletos
                .Select(c => new VwClientesCompletosDto
                {
                    ClienteId = c.ClienteId,
                    NombreComercial = c.NombreComercial,
                    RFC = c.RFC,
                    Activo = c.Activo,
                    LegacyClientId = c.LegacyClientId,
                    Calle = c.Calle,
                    NumeroExterior = c.NumeroExterior,
                    Colonia = c.Colonia,
                    CodigoPostal = c.CodigoPostal,
                    Ciudad = c.Ciudad,
                    Estado = c.Estado,
                    Pais = c.Pais,
                    TelefonoPrincipal = c.TelefonoPrincipal,
                    EmailPrincipal = c.EmailPrincipal
                })
                .ToListAsync();
            
            return clientes;
        }

        /// <summary>
        /// Busca clientes por un término de búsqueda en varios campos.
        /// </summary>
        public async Task<IEnumerable<VwClientesCompletosDto>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            _logger.LogInformation("Buscando clientes completos con el término: {SearchTerm}", searchTerm);
            var lowerCaseSearchTerm = searchTerm.ToLower();

            var query = _context.ClientesCompletos.AsQueryable();

            query = query.Where(c =>
                (c.NombreComercial != null && c.NombreComercial.ToLower().Contains(lowerCaseSearchTerm)) ||
                (c.RFC != null && c.RFC.ToLower().Contains(lowerCaseSearchTerm)) ||
                (c.EmailPrincipal != null && c.EmailPrincipal.ToLower().Contains(lowerCaseSearchTerm)) ||
                (c.TelefonoPrincipal != null && c.TelefonoPrincipal.Contains(searchTerm)) // Phone numbers might not be case-sensitive
            );

            return await query.Select(c => new VwClientesCompletosDto
                {
                    ClienteId = c.ClienteId,
                    NombreComercial = c.NombreComercial,
                    RFC = c.RFC,
                    Activo = c.Activo,
                    LegacyClientId = c.LegacyClientId,
                    Calle = c.Calle,
                    NumeroExterior = c.NumeroExterior,
                    Colonia = c.Colonia,
                    CodigoPostal = c.CodigoPostal,
                    Ciudad = c.Ciudad,
                    Estado = c.Estado,
                    Pais = c.Pais,
                    TelefonoPrincipal = c.TelefonoPrincipal,
                    EmailPrincipal = c.EmailPrincipal
                })
                .ToListAsync();
        }
    }
}
