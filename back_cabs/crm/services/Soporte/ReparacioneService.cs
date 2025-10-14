using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
=======
using Microsoft.IdentityModel.Tokens;
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6

namespace back_cabs.CRM.services.Soporte
{
    public class ReparacionService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readOnlyContext;
        private readonly ILogger<ReparacionService> _logger;

        public ReparacionService(
            WriteContext writeContext,
            ReadOnlyContext readOnlyContext,
            ILogger<ReparacionService> logger
        )
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readOnlyContext = readOnlyContext ?? throw new ArgumentNullException(nameof(readOnlyContext));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        public async Task<ReparacionResponseDto> CrearReparacionAsync(ReparacionCreacionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de reparación para la orden ID: {OrdenId}", request.OrdenId);

                //Validacion de Referencias
                var ordenExistente = await _readOnlyContext.OrdenesTrabajo.AnyAsync(o => o.Id == request.OrdenId);
                if (!ordenExistente)
                {
                    _logger.LogWarning("No se encontró la orden de trabajo con ID: {OrdenId}", request.OrdenId);
                    throw new KeyNotFoundException($"No se encontró la orden de trabajo con ID: {request.OrdenId}");
                }
                var tecnicoExistente = await _readOnlyContext.UsuariosAuth.AnyAsync(u => u.Id == request.TecnicoId);
                if (!tecnicoExistente)
                {
                    _logger.LogWarning("No se encontró el técnico con ID: {TecnicoId}", request.TecnicoId);
                    throw new KeyNotFoundException($"No se encontró el técnico con ID: {request.TecnicoId}");
                }

                //Mapeo de Valores sobre el modelo

                var nuevaReparacion = new Reparacion
                {
                    OrdenId = request.OrdenId,
                    TecnicoId = request.TecnicoId,
                    DispositivoTipo = request.DispositivoTipo,
                    Marca = request.Marca,
                    Modelo = request.Modelo,
                    AccesoriosRecibidos = request.AccesoriosRecibidos,
                    DescripcionFalla = request.DescripcionFalla,
                    Diagnostico = request.Diagnostico,
<<<<<<< HEAD
                    Resultado = ResultadoReparacion.SIN_REPARAR.ToString(), // Valor por defecto al crear
=======
                    Resultado = ResultadoReparacion.COTIZAR.ToString().ToUpper(), // Valor por defecto al crear
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6
                    CausaIrreparable = request.CausaIrreparable,
                    RespaldoDatosAutorizado = request.RespaldoDatosAutorizado,
                    CostoManoObra = request.CostoManoObra,
                    CostoRefaccionesCompra = request.CostoRefaccionesCompra,
                    CostoRefaccionesPublico = request.CostoRefaccionesPublico,
                    GarantiaDias = request.GarantiaDias,
                    FechaLlegada = DateTime.UtcNow,
                    EmpezadoEn = request.EmpezadoEn,
                    EntregadoEn = request.EntregadoEn,
<<<<<<< HEAD
                    TipoEntrega = TipoEntrega.RECOGE_CLIENTE.ToString(), // Valor por defecto al crear
=======
                    TipoEntrega = TipoEntrega.RECOGE_CLIENTE.ToString().ToUpper(), // Valor por defecto al crear
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6
                    UbicacionAlmacenamiento = request.UbicacionAlmacenamiento,
                    Notas = request.Notas
                };

                //Resistencia del modelo
                _writeContext.Reparaciones.Add(nuevaReparacion);
                await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Reparacion creada con ID: {Id}", nuevaReparacion.Id);

                return MapearAResponseDto(nuevaReparacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reparación para la orden ID: {OrdenId}", request.OrdenId);
                throw; // Re-lanzar la excepción para que pueda ser manejada por el controlador
            }
        }

        public async Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> ActualizarReparacionAsync(int id, ReparacionActualizacionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de reparación con ID: {Id}", id);

                var reparacionExistente = await _writeContext.Reparaciones.FindAsync(id);
                if (reparacionExistente == null)
                {
                    _logger.LogWarning("No se encontró la reparación con ID: {Id}", id);
                    throw new KeyNotFoundException($"No se encontró la reparación con ID: {id}");
                }

                // ----------------------------------------------------------------------
                // 1. MANEJO Y VALIDACIÓN DEL ENUM RESULTADO
                // ----------------------------------------------------------------------
                ResultadoReparacion nuevoResultadoEnum = ResultadoReparacion.SIN_REPARAR; // Valor por defecto

                // Intentar convertir el string del DTO a Enum.
                // Se usa 'true' para ignorar mayúsculas/minúsculas.
                if (request.Resultado != null && !Enum.TryParse(request.Resultado, true, out nuevoResultadoEnum))
                {
                    // Si la conversión falla, se lanza una excepción con los valores válidos.
                    string validos = string.Join(", ", Enum.GetNames(typeof(ResultadoReparacion)));
                    throw new ArgumentException($"El resultado '{request.Resultado}' no es válido. Valores permitidos: {validos}");
                }
                else if (request.Resultado != null)
                {
                    // La conversión fue exitosa. Asignamos el nombre del Enum al campo string de la entidad.
                    // Esto es crucial si el campo de la DB es un string (NVARCHAR).
                    reparacionExistente.Resultado = nuevoResultadoEnum.ToString().ToUpper();
                }

<<<<<<< HEAD
                // ----------------------------------------------------------------------
                // 2. ACTUALIZACIÓN DE CAMPOS Y VALIDACIONES DE NEGOCIO
                // ----------------------------------------------------------------------

                // Actualizar solo los campos que vengan en el Request.
                // Usamos la comprobación de nulidad para no sobrescribir con null o valores vacíos.

                // Campos de solo lectura (FKs), se mantienen
                // reparacionExistente.OrdenId = reparacionExistente.OrdenId; 
                // reparacionExistente.TecnicoId = reparacionExistente.TecnicoId;

                if (request.SolucionAplicada != null)
                    reparacionExistente.SolucionAplicada = request.SolucionAplicada;

                if (request.CausaIrreparable != null)
                    reparacionExistente.CausaIrreparable = request.CausaIrreparable;

                // Se actualizan solo si tienen valor. Los tipos 'decimal?' se comprueban con HasValue.
                if (request.CostoManoObra.HasValue)
                    reparacionExistente.CostoManoObra = request.CostoManoObra.Value;
                if (request.CostoRefaccionesCompra.HasValue)
                    reparacionExistente.CostoRefaccionesCompra = request.CostoRefaccionesCompra.Value;
                if (request.CostoRefaccionesPublico.HasValue)
                    reparacionExistente.CostoRefaccionesPublico = request.CostoRefaccionesPublico.Value;
                if (request.GarantiaDias.HasValue)
                    reparacionExistente.GarantiaDias = request.GarantiaDias.Value;
                if (request.EmpezadoEn.HasValue)
                    reparacionExistente.EmpezadoEn = request.EmpezadoEn.Value;
                if (request.EntregadoEn.HasValue)
                    reparacionExistente.EntregadoEn = request.EntregadoEn.Value;

                if (request.TipoEntrega != null)
                    reparacionExistente.TipoEntrega = request.TipoEntrega;
                if (request.Notas != null)
                    reparacionExistente.Notas = request.Notas;

                // 2.1 Aplicar Regla de Negocio de Irreparable
                if (request.Resultado != null && nuevoResultadoEnum == ResultadoReparacion.IRREPARABLE)
                {
                    if (string.IsNullOrWhiteSpace(request.CausaIrreparable))
                    {
                        throw new ArgumentException("Si el resultado es 'IRREPARABLE', la causa es obligatoria.");
                    }
                }

                // 2.2 Sellar fecha de entrega al finalizar la reparación
                if (request.Resultado != null && nuevoResultadoEnum is ResultadoReparacion.REPARADO or ResultadoReparacion.DEVUELTO_SIN_REPARAR)
                {
                    // Solo actualizamos si no se proporcionó una fecha de entrega en el request
                    if (!reparacionExistente.EntregadoEn.HasValue)
                    {
                        reparacionExistente.EntregadoEn = DateTime.UtcNow;
                    }
                }

                // ----------------------------------------------------------------------
                // 3. PERSISTENCIA
                // ----------------------------------------------------------------------

                int filasAfectadas = await _writeContext.SaveChangesAsync();

                _logger.LogInformation("Reparación ID {Id} actualizada. Filas afectadas: {Filas}", id, filasAfectadas);

                var DTOMapeado = MapearAResponseDto(reparacionExistente);


                return (filasAfectadas, DTOMapeado);
            }
=======

                if (request.TipoEntrega != null)
                {
                    TipoEntrega nuevoTipoEntregaEnum;

                    // A. Validación del Enum
                    if (!Enum.TryParse(request.TipoEntrega, true, out nuevoTipoEntregaEnum))
                    {
                        string validos = string.Join(", ", Enum.GetNames(typeof(TipoEntrega)));
                        throw new ArgumentException($"El tipo de entrega '{request.TipoEntrega}' no es válido. Valores permitidos: {validos}");
                    }

                    // B. Normalización del Valor para SQL
                    // Si la DB espera 'RECOGE_CLIENTE' y 'DOMICILIO' (con underscore):
                    string valorParaDB = nuevoTipoEntregaEnum.ToString().ToUpper();
                    reparacionExistente.TipoEntrega = nuevoTipoEntregaEnum.ToString().ToUpper();
                }
                    // ----------------------------------------------------------------------
                    // 2. ACTUALIZACIÓN DE CAMPOS Y VALIDACIONES DE NEGOCIO
                    // ----------------------------------------------------------------------

                    // Actualizar solo los campos que vengan en el Request.
                    // Usamos la comprobación de nulidad para no sobrescribir con null o valores vacíos.

                    // Campos de solo lectura (FKs), se mantienen
                    // reparacionExistente.OrdenId = reparacionExistente.OrdenId; 
                    // reparacionExistente.TecnicoId = reparacionExistente.TecnicoId;

                    if (request.SolucionAplicada != null)
                        reparacionExistente.SolucionAplicada = request.SolucionAplicada;

                    if (request.CausaIrreparable != null)
                        reparacionExistente.CausaIrreparable = request.CausaIrreparable;

                    // Se actualizan solo si tienen valor. Los tipos 'decimal?' se comprueban con HasValue.
                    if (request.CostoManoObra.HasValue)
                        reparacionExistente.CostoManoObra = request.CostoManoObra.Value;
                    if (request.CostoRefaccionesCompra.HasValue)
                        reparacionExistente.CostoRefaccionesCompra = request.CostoRefaccionesCompra.Value;
                    if (request.CostoRefaccionesPublico.HasValue)
                        reparacionExistente.CostoRefaccionesPublico = request.CostoRefaccionesPublico.Value;
                    if (request.GarantiaDias.HasValue)
                        reparacionExistente.GarantiaDias = request.GarantiaDias.Value;
                    if (request.EmpezadoEn.HasValue)
                        reparacionExistente.EmpezadoEn = request.EmpezadoEn.Value;
                    if (request.EntregadoEn.HasValue)
                        reparacionExistente.EntregadoEn = request.EntregadoEn.Value;

                    if (request.Notas != null)
                        reparacionExistente.Notas = request.Notas;

                    // 2.1 Aplicar Regla de Negocio de Irreparable
                    if (request.Resultado != null && nuevoResultadoEnum == ResultadoReparacion.IRREPARABLE)
                    {
                        if (string.IsNullOrWhiteSpace(request.CausaIrreparable))
                        {
                            throw new ArgumentException("Si el resultado es 'IRREPARABLE', la causa es obligatoria.");
                        }
                    }

                    // 2.2 Sellar fecha de entrega al finalizar la reparación
                    if (request.Resultado != null && nuevoResultadoEnum is ResultadoReparacion.REPARADO or ResultadoReparacion.DEVUELTO_SIN_REPARAR)
                    {
                        // Solo actualizamos si no se proporcionó una fecha de entrega en el request
                        if (!reparacionExistente.EntregadoEn.HasValue)
                        {
                            reparacionExistente.EntregadoEn = DateTime.UtcNow;
                        }
                    }

                    // ----------------------------------------------------------------------
                    // 3. PERSISTENCIA
                    // ----------------------------------------------------------------------

                    int filasAfectadas = await _writeContext.SaveChangesAsync();

                    _logger.LogInformation("Reparación ID {Id} actualizada. Filas afectadas: {Filas}", id, filasAfectadas);

                    var DTOMapeado = MapearAResponseDto(reparacionExistente);


                    return (filasAfectadas, DTOMapeado);
                }
>>>>>>> 26ed7eef6405f23b5f35e858f5e4a208e4eb26c6
            catch (KeyNotFoundException)
            {
                throw; // Re-lanzar la excepción específica para que el controlador pueda devolver 404
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reparación con ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<ReparacionResponseDto>> ObtenerReparacionesAsync(int? skip = null, int? take = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de reparaciones. Skip: {Skip}, Take: {Take}", skip, take);

                var query = _readOnlyContext.Reparaciones.AsQueryable();

                if (skip.HasValue)
                {
                    query = query.Skip(skip.Value);
                }
                if (take.HasValue)
                {
                    query = query.Take(take.Value);
                }

                //Query lanzado a la BD para obtencion de registros
                var reparaciones = await query
                .OrderByDescending(r => r.FechaLlegada)
                .ToListAsync();

                //Mapeo de response dto
                return reparaciones.Select(MapearAResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reparaciones");
                throw;
            }
        }


        public async Task<ReparacionResponseDto?> ObtenerReparacionPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando Reparacion por ID: {Id}", id);

                var reparacion = await _readOnlyContext.Reparaciones.FirstOrDefaultAsync(r => r.Id == id);

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparacion no encontrada por ID  {Id}", id);
                    return null;
                }
                //Mapeo de la entidad localizada
                return MapearAResponseDto(reparacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la reparación con ID: {Id}", id);
                throw;
            }
        }


        // Método auxiliar para mantener el código DRY (Don't Repeat Yourself)
        private ReparacionResponseDto MapearAResponseDto(Reparacion reparacion)
        {
            // Lógica para mapear de Entidad a DTO de Respuesta
            return new ReparacionResponseDto
            {
                Id = reparacion.Id,
                OrdenId = reparacion.OrdenId,
                TecnicoId = reparacion.TecnicoId,
                DispositivoTipo = reparacion.DispositivoTipo,
                Marca = reparacion.Marca,
                Modelo = reparacion.Modelo,
                AccesoriosRecibidos = reparacion.AccesoriosRecibidos,
                DescripcionFalla = reparacion.DescripcionFalla,
                Diagnostico = reparacion.Diagnostico,
                SolucionAplicada = reparacion.SolucionAplicada,
                Resultado = reparacion.Resultado,
                CausaIrreparable = reparacion.CausaIrreparable,
                RespaldoDatosAutorizado = reparacion.RespaldoDatosAutorizado,
                CostoManoObra = reparacion.CostoManoObra,
                CostoRefaccionesCompra = reparacion.CostoRefaccionesCompra,
                CostoRefaccionesPublico = reparacion.CostoRefaccionesPublico,
                CostoTotalCompra = reparacion.CostoTotalCompra,
                GarantiaDias = reparacion.GarantiaDias,
                FechaLlegada = reparacion.FechaLlegada,
                EmpezadoEn = reparacion.EmpezadoEn,
                EntregadoEn = reparacion.EntregadoEn,
                TipoEntrega = reparacion.TipoEntrega,
                UbicacionAlmacenamiento = reparacion.UbicacionAlmacenamiento,
                Notas = reparacion.Notas,
                CostoTotalPublico = reparacion.CostoTotalPublico
            };
        }
    }
}
