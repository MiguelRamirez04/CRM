// =====================================================================================
// SERVICIO CLIENTES COMPLETOS - ClientesCompletosService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Contiene la lógica de negocio para consultar la vista VwClientesCompletos.
// Se encarga de acceder a la base de datos a través del procedimiento almacenado
// usp_GetClientesCompletos_Paginado, aplicar filtros y mapear los resultados a DTOs.
// También provee métodos para búsqueda rápida de clientes para el autocompletado.
//
// =====================================================================================

using back_cabs.CRM.DTOs;
using back_cabs.CRM.DTOs.Recepcion;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace back_cabs.CRM.services
{
    /// <summary>
    /// Servicio para consultas de solo lectura sobre los clientes completos utilizando procedimientos almacenados.
    /// </summary>
    public class ClientesCompletosService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<ClientesCompletosService> _logger;

        public ClientesCompletosService(IDbConnection dbConnection, ILogger<ClientesCompletosService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los clientes completos paginados utilizando el procedimiento almacenado.
        /// </summary>
        /// <param name="request">Parámetros de paginación y búsqueda</param>
        /// <returns>Respuesta paginada con los clientes que coinciden con los criterios</returns>
        public async Task<PaginatedResponseDto<ClientesCompletosPaginadoDto>> GetClientesPaginadosAsync(ClientesCompletosPaginadoRequestDto request)
        {
            _logger.LogInformation("Obteniendo clientes completos paginados. Página: {Pagina}, ResultadosPorPagina: {ResultadosPorPagina}, Búsqueda Nombre: {BusquedaNombre}, Búsqueda RFC: {BusquedaRFC}", 
                request.Pagina, request.ResultadosPorPagina, 
                request.NombreBusqueda ?? "No especificado", 
                request.RFCBusqueda ?? "No especificado");

            try
            {
                // Verificar que la conexión existe
                if (_dbConnection == null)
                {
                    throw new InvalidOperationException("La conexión a la base de datos no está disponible.");
                }
                
                // Asegurarse de que la conexión esté abierta
                if (_dbConnection.State != ConnectionState.Open)
                {
                    try
                    {
                        var sqlConnection = _dbConnection as SqlConnection;
                        if (sqlConnection == null)
                        {
                            throw new InvalidOperationException("La conexión no es del tipo SqlConnection esperado.");
                        }
                        
                        await sqlConnection.OpenAsync();
                    }
                    catch (SqlException sqlEx)
                    {
                        _logger.LogError(sqlEx, "Error al abrir la conexión SQL: {Message}", sqlEx.Message);
                        throw new Exception("No se pudo establecer conexión con la base de datos.", sqlEx);
                    }
                }

                // Preparar el comando para el procedimiento almacenado
                using var cmd = (_dbConnection as SqlConnection)!.CreateCommand();
                cmd.CommandText = "dbo.usp_GetClientesCompletos_Paginado";
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    // Agregar parámetros - usar Try/Catch por si algún parámetro no existe en el SP
                    cmd.Parameters.AddWithValue("@NombreBusqueda", (object?)request.NombreBusqueda ?? DBNull.Value);
                    
                    // Verificar si el parámetro RFCBusqueda existe en el SP
                    if (request.RFCBusqueda != null)
                    {
                        try 
                        {
                            cmd.Parameters.AddWithValue("@RFCBusqueda", request.RFCBusqueda);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "El parámetro @RFCBusqueda no existe en el procedimiento almacenado. Se ignorará este filtro.");
                            // Si el parámetro no existe, usamos NombreBusqueda como búsqueda general
                            if (string.IsNullOrEmpty(request.NombreBusqueda) && !string.IsNullOrEmpty(request.RFCBusqueda))
                            {
                                cmd.Parameters["@NombreBusqueda"].Value = request.RFCBusqueda;
                            }
                        }
                    }
                    
                    cmd.Parameters.AddWithValue("@Pagina", request.Pagina);
                    cmd.Parameters.AddWithValue("@ResultadosPorPagina", request.ResultadosPorPagina);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al configurar parámetros del procedimiento almacenado");
                    throw;
                }

                // Ejecutar el procedimiento y leer los resultados
                var clientes = new List<ClientesCompletosPaginadoDto>();
                
                try
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    
                    // Verificar si hay resultados
                    if (!reader.HasRows)
                    {
                        _logger.LogInformation("No se encontraron clientes con los criterios de búsqueda especificados.");
                        return new PaginatedResponseDto<ClientesCompletosPaginadoDto>
                        {
                            Items = new List<ClientesCompletosPaginadoDto>(),
                            Pagina = request.Pagina,
                            ResultadosPorPagina = request.ResultadosPorPagina
                        };
                    }
                    
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            var cliente = new ClientesCompletosPaginadoDto
                            {
                                ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                                NombreComercial = reader.IsDBNull(reader.GetOrdinal("NombreComercial")) ? null : reader.GetString(reader.GetOrdinal("NombreComercial")),
                                RFC = reader.IsDBNull(reader.GetOrdinal("RFC")) ? null : reader.GetString(reader.GetOrdinal("RFC")),
                                Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
                            };
                            
                            // Manejar las propiedades adicionales que podrían faltar en algunos resultados
                            try { cliente.LegacyClientId = reader.IsDBNull(reader.GetOrdinal("LegacyClientId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("LegacyClientId")); } catch { }
                            try { cliente.Calle = reader.IsDBNull(reader.GetOrdinal("Calle")) ? null : reader.GetString(reader.GetOrdinal("Calle")); } catch { }
                            try { cliente.NumeroExterior = reader.IsDBNull(reader.GetOrdinal("NumeroExterior")) ? null : reader.GetString(reader.GetOrdinal("NumeroExterior")); } catch { }
                            try { cliente.Colonia = reader.IsDBNull(reader.GetOrdinal("Colonia")) ? null : reader.GetString(reader.GetOrdinal("Colonia")); } catch { }
                            try { cliente.CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal")) ? null : reader.GetString(reader.GetOrdinal("CodigoPostal")); } catch { }
                            try { cliente.Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString(reader.GetOrdinal("Ciudad")); } catch { }
                            try { cliente.Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetString(reader.GetOrdinal("Estado")); } catch { }
                            try { cliente.Pais = reader.IsDBNull(reader.GetOrdinal("Pais")) ? null : reader.GetString(reader.GetOrdinal("Pais")); } catch { }
                            try { cliente.TelefonoPrincipal = reader.IsDBNull(reader.GetOrdinal("TelefonoPrincipal")) ? null : reader.GetString(reader.GetOrdinal("TelefonoPrincipal")); } catch { }
                            try { cliente.EmailPrincipal = reader.IsDBNull(reader.GetOrdinal("EmailPrincipal")) ? null : reader.GetString(reader.GetOrdinal("EmailPrincipal")); } catch { }
                            
                            clientes.Add(cliente);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al leer un registro de cliente del DataReader: {Message}", ex.Message);
                            // Continuamos con el siguiente registro
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "Error SQL al ejecutar el procedimiento almacenado: {Message}", sqlEx.Message);
                    throw new Exception("Error al consultar clientes en la base de datos.", sqlEx);
                }

                // Para este ejercicio, no estamos obteniendo el total de registros del SP
                // En una implementación completa, el SP debería devolver esto como un segundo conjunto de resultados
                // o se podría hacer una segunda llamada para obtener el conteo total

                // Crear la respuesta paginada
                var response = new PaginatedResponseDto<ClientesCompletosPaginadoDto>
                {
                    Items = clientes,
                    Pagina = request.Pagina,
                    ResultadosPorPagina = request.ResultadosPorPagina
                };

                _logger.LogInformation("Se encontraron {Count} clientes para la consulta", clientes.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes paginados: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Busca clientes por nombre o RFC (versión simple para autocompletado)
        /// </summary>
        /// <param name="termino">Término de búsqueda para nombre o RFC</param>
        /// <param name="limite">Número máximo de resultados</param>
        /// <returns>Lista simplificada de clientes para mostrar en un autocompletado</returns>
        public async Task<List<ClienteResumenDto>> BuscarClientesPorNombreORfcAsync(string? termino, int limite = 10)
        {
            var resultado = new List<ClienteResumenDto>();
            
            if (string.IsNullOrWhiteSpace(termino))
                return resultado;

            _logger.LogInformation("Buscando clientes por término: {Termino}, límite: {Limite}", termino, limite);
                
            try
            {
                // Verificar que la conexión existe y abrirla si es necesario
                if (_dbConnection.State != ConnectionState.Open)
                    _dbConnection.Open();
                
                // Usar una consulta directa optimizada para búsqueda rápida
                using var cmd = _dbConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
                    SELECT TOP (@Limite) 
                        ClienteId, 
                        NombreComercial, 
                        RFC 
                    FROM dbo.VwClientesCompletos 
                    WHERE 
                        (NombreComercial LIKE @Termino + '%' OR RFC LIKE @Termino + '%') 
                        AND Activo = 1
                    ORDER BY 
                        CASE WHEN NombreComercial LIKE @Termino + '%' THEN 0 ELSE 1 END,
                        CASE WHEN RFC LIKE @Termino + '%' THEN 0 ELSE 1 END,
                        NombreComercial";
                
                var paramTermino = cmd.CreateParameter();
                paramTermino.ParameterName = "@Termino";
                paramTermino.Value = termino;
                cmd.Parameters.Add(paramTermino);
                
                var paramLimite = cmd.CreateParameter();
                paramLimite.ParameterName = "@Limite";
                paramLimite.Value = limite;
                cmd.Parameters.Add(paramLimite);

                using var sqlCommand = cmd as SqlCommand;
                if (sqlCommand == null)
                {
                    throw new InvalidOperationException("No se pudo convertir el comando a SqlCommand");
                }
                
                using var reader = await sqlCommand.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    resultado.Add(new ClienteResumenDto
                    {
                        ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                        NombreComercial = reader.IsDBNull(reader.GetOrdinal("NombreComercial")) ? null : reader.GetString(reader.GetOrdinal("NombreComercial")),
                        RFC = reader.IsDBNull(reader.GetOrdinal("RFC")) ? null : reader.GetString(reader.GetOrdinal("RFC"))
                    });
                }

                _logger.LogInformation("Se encontraron {Count} clientes para la búsqueda rápida", resultado.Count);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por término: {Message}", ex.Message);
                throw;
            }
        }
    }
}
