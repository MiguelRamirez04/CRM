using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models;
using back_cabs.CRM.models.Shared; // Asumo que VwClientesCompletos está aquí
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection; // Necesario para reflexionar sobre DbContext

namespace back_cabs.CRM.repositories
{
    /// <summary>
    /// Implementación del repositorio para la validación de clientes legacy.
    /// Utiliza ReadOnlyContext para acceder a la vista VwClientesCompletos.
    /// </summary>
    public class ClientesLegacyValidationRepository : IClientesLegacyValidationRepository
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<ClientesLegacyValidationRepository> _logger;

        public ClientesLegacyValidationRepository(
            ReadOnlyContext readContext,
            ILogger<ClientesLegacyValidationRepository> logger)
        {
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ---------------------------------------------------------------------
        // IMPLEMENTACIÓN: ValidarClienteLegacyUsingMultipleStrategiesAsync
        // ---------------------------------------------------------------------

        public async Task<bool> ValidarClienteLegacyUsingMultipleStrategiesAsync(int? clienteId)
        {
            if (!clienteId.HasValue)
            {
                return false;
            }

            try
            {
                _logger.LogInformation("Iniciando validación de cliente legacy para ID: {Id}", clienteId.Value);

                // ESTRATEGIA 1: Buscar por ClienteId (la clave primaria del CRM o vista)
                var existsById = await _readContext.Set<VwClientesCompletos>()
                    .AsNoTracking()
                    .AnyAsync(c => c.ClienteId == clienteId.Value);

                if (existsById)
                {
                    _logger.LogDebug("Cliente {Id} validado por Estrategia 1 (ClienteId)", clienteId.Value);
                    return true;
                }

                // ESTRATEGIA 2: Buscar por LegacyClientId (la clave externa del sistema antiguo)
                // Esta estrategia es necesaria si el 'clienteId' pasado podría ser el ID antiguo.
                var existsByLegacyId = await _readContext.Set<VwClientesCompletos>()
                    .AsNoTracking()
                    .AnyAsync(c => c.LegacyClientId == clienteId.Value);

                if (existsByLegacyId)
                {
                    _logger.LogDebug("Cliente {Id} validado por Estrategia 2 (LegacyClientId)", clienteId.Value);
                    return true;
                }

                _logger.LogWarning("Cliente {Id} no encontrado por ninguna estrategia de validación legacy.", clienteId.Value);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la validación multi-estrategia para cliente {Id}", clienteId.Value);
                throw;
            }
        }

        // ---------------------------------------------------------------------
        // IMPLEMENTACIÓN: ObtenerInformacionEstructuraAsync (Depuración)
        // ---------------------------------------------------------------------

        public Task<string> ObtenerInformacionEstructuraAsync()
        {
            try
            {
                // Usamos la reflexión de .NET para inspeccionar la metadata de EF Core.

                var entityType = _readContext.Model.FindEntityType(typeof(VwClientesCompletos));
                if (entityType == null)
                {
                    return Task.FromResult("Error: La entidad VwClientesCompletos no está registrada en el DbContext.");
                }

                var builder = new System.Text.StringBuilder();

                builder.AppendLine($"--- Estructura de Mapeo EF Core para {nameof(VwClientesCompletos)} ---");
                builder.AppendLine($"Mapeado a: {entityType.GetSchema()}.{entityType.GetTableName()} (Vista)");

                builder.AppendLine("\nPropiedades del Modelo (C#):");
                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    var columnType = property.GetColumnType() ?? "N/A";
                    builder.AppendLine($"- {property.Name} ({property.ClrType.Name}) -> DB Columna: {columnName} | DB Tipo: {columnType}");
                }

                // Información del modelo de C# (Clase)
                builder.AppendLine("\nPropiedades de la Clase (Reflexión C#):");
                var classProperties = typeof(VwClientesCompletos).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in classProperties)
                {
                    builder.AppendLine($"- {prop.Name} ({prop.PropertyType.Name})");
                }


                return Task.FromResult(builder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de la estructura de la vista.");
                return Task.FromResult($"Error al generar metadata de EF: {ex.Message}");
            }
        }
    }
}