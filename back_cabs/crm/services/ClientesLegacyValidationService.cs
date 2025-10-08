// =====================================================================================
// SERVICIO DE DEPURACIÓN PARA VALIDACIÓN DE CLIENTES LEGACY - ClientesLegacyValidationService.cs
// =====================================================================================
//
// CREADO PARA DEPURACIÓN DE ERRORES DE MAPEO DE COLUMNAS
// Este servicio implementa múltiples estrategias para validar clientes legacy
//
// =====================================================================================

using back_cabs.CRM.contexts;
using back_cabs.CRM.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace back_cabs.CRM.services
{
    public class ClientesLegacyValidationService
    {
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<ClientesLegacyValidationService> _logger;
        private readonly string _connectionString;

        public ClientesLegacyValidationService(
            ReadOnlyContext readContext,
            IConfiguration configuration,
            ILogger<ClientesLegacyValidationService> logger)
        {
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' no encontrada en configuración.");
        }

        /// <summary>
        /// Intenta validar un cliente legacy utilizando varias estrategias diferentes
        /// </summary>
        public async Task<bool> ValidarClienteLegacyUsingMultipleStrategiesAsync(int? clienteId)
        {
            if (!clienteId.HasValue)
            {
                _logger.LogWarning("ClienteId es nulo");
                return false;
            }

            _logger.LogInformation("Iniciando validación de cliente legacy ID: {ClienteId} usando múltiples estrategias", clienteId);

            try
            {
                // Estrategia 1: Usando ADO.NET directamente (más confiable)
                _logger.LogInformation("Estrategia 1: Usando ADO.NET directamente");
                bool existeConADO = await ValidarConADODirectoAsync(clienteId.Value);
                _logger.LogInformation("Resultado Estrategia 1: {Resultado}", existeConADO);
                
                if (existeConADO)
                    return true;

                // Estrategia 2: Usando EF Core directamente
                _logger.LogInformation("Estrategia 2: Usando EF Core directamente");
                var existeConEF = await _readContext.ClientesCompletos
                    .AnyAsync(c => c.ClienteId == clienteId.Value);
                _logger.LogInformation("Resultado Estrategia 2: {Resultado}", existeConEF);
                
                if (existeConEF)
                    return true;

                // Estrategia 3: Usando consulta SQL directa con EF Core
                _logger.LogInformation("Estrategia 3: Usando consulta SQL directa con EF Core");
                var sqlQuery = $"SELECT COUNT(1) FROM VwClientesCompletos WHERE id = {clienteId}";
                var countResult = await _readContext.Database.ExecuteSqlRawAsync(sqlQuery);
                var existeConSQLDirecto = countResult > 0;
                _logger.LogInformation("Resultado Estrategia 3: {Resultado}", existeConSQLDirecto);
                
                if (existeConSQLDirecto)
                    return true;
                    
                // Estrategia 4: Buscar por LegacyClientId
                _logger.LogInformation("Estrategia 4: Buscar por LegacyClientId");
                var existePorLegacyId = await _readContext.ClientesCompletos
                    .AnyAsync(c => c.LegacyClientId == clienteId.Value);
                _logger.LogInformation("Resultado Estrategia 4: {Resultado}", existePorLegacyId);
                
                return existePorLegacyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la validación del cliente legacy: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validación usando ADO.NET directamente para evitar problemas con EF Core
        /// </summary>
        private async Task<bool> ValidarConADODirectoAsync(int clienteId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Intentar obtener la estructura real de la tabla/vista
                    _logger.LogInformation("Obteniendo estructura de VwClientesCompletos");
                    var schemaCommand = new SqlCommand("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'VwClientesCompletos' AND COLUMN_NAME = 'id'", connection);
                    
                    using (var reader = await schemaCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var columnName = reader.GetString(0);
                            var dataType = reader.GetString(1);
                            _logger.LogInformation("Columna ID encontrada: {Columna}, Tipo: {Tipo}", columnName, dataType);
                        }
                    }
                    
                    // Ahora intentamos la consulta real
                    var command = new SqlCommand("SELECT COUNT(1) FROM VwClientesCompletos WHERE id = @clienteId", connection);
                    command.Parameters.AddWithValue("@clienteId", clienteId);
                    
                    var result = await command.ExecuteScalarAsync();
                    var count = result != null ? (int)result : 0;
                    
                    _logger.LogInformation("Consulta ADO.NET: Cliente ID {ClienteId}, Count = {Count}", clienteId, count);
                    
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en validación con ADO.NET: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Obtiene y muestra información detallada sobre la estructura de VwClientesCompletos
        /// </summary>
        public async Task<string> ObtenerInformacionEstructuraAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new SqlCommand("SELECT TOP 1 * FROM VwClientesCompletos", connection);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var schemaTable = reader.GetSchemaTable();
                        
                        if (schemaTable == null)
                            return "No se pudo obtener información del esquema";
                            
                        var columnInfo = new List<string>();
                        
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            var columnName = row["ColumnName"].ToString();
                            var dataType = row["DataType"].ToString();
                            var isNullable = (bool)row["AllowDBNull"];
                            
                            columnInfo.Add($"Columna: {columnName}, Tipo: {dataType}, Nullable: {isNullable}");
                        }
                        
                        return string.Join("\n", columnInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de estructura: {Message}", ex.Message);
                return $"Error: {ex.Message}";
            }
        }
    }
}