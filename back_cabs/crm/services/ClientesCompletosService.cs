// =====================================================================================
// SERVICIO CLIENTES COMPLETOS - ClientesCompletosService.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Contiene la lógica de negocio para consultar la vista VwClientesCompletos.
// Se encarga de acceder a la base de datos a través del procedimiento almacenado
// usp_GetClientesCompletos_Paginado, aplicar filtros y mapear los resultados a DTOs.
//
// =====================================================================================

using back_cabs.CRM.DTOs;
using Microsoft.Data.SqlClient;
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
            _logger.LogInformation("Obteniendo clientes completos paginados. Página: {Pagina}, ResultadosPorPagina: {ResultadosPorPagina}, Búsqueda: {Busqueda}", 
                request.Pagina, request.ResultadosPorPagina, request.NombreBusqueda ?? "Todos");

            try
            {
                // Asegurarse de que la conexión esté abierta
                if (_dbConnection.State != ConnectionState.Open)
                {
                    await (_dbConnection as SqlConnection)!.OpenAsync();
                }

                // Preparar el comando para el procedimiento almacenado
                using var cmd = (_dbConnection as SqlConnection)!.CreateCommand();
                cmd.CommandText = "dbo.usp_GetClientesCompletos_Paginado";
                cmd.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros
                cmd.Parameters.AddWithValue("@NombreBusqueda", (object?)request.NombreBusqueda ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pagina", request.Pagina);
                cmd.Parameters.AddWithValue("@ResultadosPorPagina", request.ResultadosPorPagina);

                // Ejecutar el procedimiento y leer los resultados
                var clientes = new List<ClientesCompletosPaginadoDto>();
                
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    clientes.Add(new ClientesCompletosPaginadoDto
                    {
                        ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                        NombreComercial = reader.IsDBNull(reader.GetOrdinal("NombreComercial")) ? null : reader.GetString(reader.GetOrdinal("NombreComercial")),
                        RFC = reader.IsDBNull(reader.GetOrdinal("RFC")) ? null : reader.GetString(reader.GetOrdinal("RFC")),
                        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                        LegacyClientId = reader.IsDBNull(reader.GetOrdinal("LegacyClientId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("LegacyClientId")),
                        Calle = reader.IsDBNull(reader.GetOrdinal("Calle")) ? null : reader.GetString(reader.GetOrdinal("Calle")),
                        NumeroExterior = reader.IsDBNull(reader.GetOrdinal("NumeroExterior")) ? null : reader.GetString(reader.GetOrdinal("NumeroExterior")),
                        Colonia = reader.IsDBNull(reader.GetOrdinal("Colonia")) ? null : reader.GetString(reader.GetOrdinal("Colonia")),
                        CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal")) ? null : reader.GetString(reader.GetOrdinal("CodigoPostal")),
                        Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString(reader.GetOrdinal("Ciudad")),
                        Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetString(reader.GetOrdinal("Estado")),
                        Pais = reader.IsDBNull(reader.GetOrdinal("Pais")) ? null : reader.GetString(reader.GetOrdinal("Pais")),
                        TelefonoPrincipal = reader.IsDBNull(reader.GetOrdinal("TelefonoPrincipal")) ? null : reader.GetString(reader.GetOrdinal("TelefonoPrincipal")),
                        EmailPrincipal = reader.IsDBNull(reader.GetOrdinal("EmailPrincipal")) ? null : reader.GetString(reader.GetOrdinal("EmailPrincipal"))
                    });
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
    }
}
