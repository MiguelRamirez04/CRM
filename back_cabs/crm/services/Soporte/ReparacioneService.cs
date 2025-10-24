using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.enums;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.Interfaces.Soporte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.CRM.services.Soporte
{
    /// <summary>
    /// Servicio de lógica de negocio para Reparaciones y Componentes.
    /// Utiliza IReparacionRepository para el acceso a datos.
    /// </summary>
    public class ReparacionService 
    {
        // ELIMINAMOS _writeContext y _readOnlyContext. Mantenemos el repositorio.
        private readonly IReparacionRepository _reparacionRepository;
        private readonly ILogger<ReparacionService> _logger;

        public ReparacionService(
            IReparacionRepository reparacionRepository,
            ILogger<ReparacionService> logger
        )
        {
            _reparacionRepository = reparacionRepository ?? throw new ArgumentNullException(nameof(reparacionRepository));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        // =====================================================================
        // OPERACIONES DE REPARACIÓN (CRUD)
        // =====================================================================

        public async Task<ReparacionResponseDto> CrearReparacionAsync(ReparacionCreacionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de reparación para la orden ID: {OrdenId}", request.OrdenId);

                // Validacion de Referencias: DELEGADO AL REPOSITORIO
                var ordenExistente = await _reparacionRepository.OrdenExisteAsync(request.OrdenId);
                if (!ordenExistente)
                {
                    throw new KeyNotFoundException($"No se encontró la orden de trabajo con ID: {request.OrdenId}");
                }
                var tecnicoExistente = await _reparacionRepository.TecnicoExisteAsync(request.TecnicoId);
                if (!tecnicoExistente)
                {
                    throw new KeyNotFoundException($"No se encontró el técnico con ID: {request.TecnicoId}");
                }

                // Persistencia: DELEGADO AL REPOSITORIO
                var reparacionCreada = await _reparacionRepository.CrearReparacionAsync(request);

                _logger.LogInformation("Reparacion creada con ID: {Id}", reparacionCreada.Id);

                return reparacionCreada;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reparación para la orden ID: {OrdenId}", request.OrdenId);
                throw; 
            }
        }

        public async Task<(int FilasAfectadas, ReparacionResponseDto? ReparacionActualizada)> ActualizarReparacionAsync(int id, ReparacionActualizacionRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de reparación con ID: {Id}", id);

                // Obtener entidad con tracking: DELEGADO AL REPOSITORIO
                var reparacionExistente = await _reparacionRepository.GetReparacionForUpdateAsync(id); 
                if (reparacionExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró la reparación con ID: {id}");
                }

                // ----------------------------------------------------------------------
                // 1. MANEJO Y VALIDACIÓN DEL ENUM RESULTADO (Lógica de Negocio)
                // ----------------------------------------------------------------------
                ResultadoReparacion nuevoResultadoEnum = ResultadoReparacion.SIN_REPARAR; 

                if (request.Resultado != null)
                {
                    if (!Enum.TryParse(request.Resultado, true, out nuevoResultadoEnum))
                    {
                        string validos = string.Join(", ", Enum.GetNames(typeof(ResultadoReparacion)));
                        throw new ArgumentException($"El resultado '{request.Resultado}' no es válido. Valores permitidos: {validos}");
                    }
                    reparacionExistente.Resultado = nuevoResultadoEnum.ToString().ToUpper();
                }

                if (request.TipoEntrega != null)
                {
                    TipoEntrega nuevoTipoEntregaEnum;
                    if (!Enum.TryParse(request.TipoEntrega, true, out nuevoTipoEntregaEnum))
                    {
                        string validos = string.Join(", ", Enum.GetNames(typeof(TipoEntrega)));
                        throw new ArgumentException($"El tipo de entrega '{request.TipoEntrega}' no es válido. Valores permitidos: {validos}");
                    }
                    reparacionExistente.TipoEntrega = nuevoTipoEntregaEnum.ToString().ToUpper();
                }
                
                // ----------------------------------------------------------------------
                // 2. ACTUALIZACIÓN DE CAMPOS Y VALIDACIONES DE NEGOCIO (Mapeo Parcial)
                // ----------------------------------------------------------------------

                if (request.SolucionAplicada != null)
                    reparacionExistente.SolucionAplicada = request.SolucionAplicada;
                if (request.CausaIrreparable != null)
                    reparacionExistente.CausaIrreparable = request.CausaIrreparable;

                // Actualizar solo si tienen valor (Se asume que los FKs OrdenId y TecnicoId NO se actualizan aquí)
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
                    if (string.IsNullOrWhiteSpace(reparacionExistente.CausaIrreparable))
                    {
                        throw new ArgumentException("Si el resultado es 'IRREPARABLE', la causa es obligatoria.");
                    }
                }

                // 2.2 Sellar fecha de entrega al finalizar la reparación
                if (request.Resultado != null && (nuevoResultadoEnum is ResultadoReparacion.REPARADO or ResultadoReparacion.DEVUELTO_SIN_REPARAR) && !reparacionExistente.EntregadoEn.HasValue)
                {
                    reparacionExistente.EntregadoEn = DateTime.UtcNow;
                }

                // ----------------------------------------------------------------------
                // 3. PERSISTENCIA: DELEGADO AL REPOSITORIO
                // ----------------------------------------------------------------------

                var (filasAfectadas, reparacionActualizada) = await _reparacionRepository.ActualizarReparacionAsync(reparacionExistente);
                
                _logger.LogInformation("Reparación ID {Id} actualizada. Filas afectadas: {Filas}", id, filasAfectadas);

                // Devolver el objeto actualizado mapeado
                return (filasAfectadas, reparacionActualizada);
            }
            catch (KeyNotFoundException)
            {
                throw; 
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

                // DELEGADO AL REPOSITORIO
                var reparaciones = await _reparacionRepository.ObtenerReparacionesAsync(skip, take);

                // Mapeo de response dto
                return reparaciones;
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

                // DELEGADO AL REPOSITORIO
                var reparacion = await _reparacionRepository.ObtenerReparacionPorIdAsync(id);

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparacion no encontrada por ID  {Id}", id);
                    return null;
                }
                
                return reparacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la reparación con ID: {Id}", id);
                throw;
            }
        }

        // =====================================================================================
        // LOGICA DE NEGOCIO DE REPARACION COMPONENTES
        // =====================================================================================
        public async Task<List<ReparacionComponenteResponseDto>> ObtenerComponentesReparacionAsync(int? skip = null, int? take = null)
        {
            try
            {
                _logger.LogInformation("Buscando componentes de reparación");
                
                // DELEGADO AL REPOSITORIO
                var componenteReparacion = await _reparacionRepository.ObtenerComponentesReparacionAsync(skip, take);

                return componenteReparacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los componentes de reparacion");
                throw;
            }
        }

        public async Task<ReparacionComponenteResponseDto?> ObtenerComponenteReparacionPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando componente de reparación por ID: {Id}", id);

                // DELEGADO AL REPOSITORIO
                var componenteReparacion = await _reparacionRepository.ObtenerComponenteReparacionPorIdAsync(id);

                if (componenteReparacion == null)
                {
                    throw new KeyNotFoundException($"Componente de reparación no encontrada por ID  {id}");
                }
                return componenteReparacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el componente de reparación con ID: {Id}", id);
                throw;
            }
        }

        public async Task<ReparacionComponenteResponseDto> CrearComponenteReparacionAsync(ReparacionComponenteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de componente de reparación para la reparación ID: {ReparacionId}", request.ReparacionId);

                // Validacion de Referencias: DELEGADO AL REPOSITORIO
                var reparacionExistente = await _reparacionRepository.ObtenerReparacionPorIdAsync(request.ReparacionId);
                if (reparacionExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró la reparación con ID: {request.ReparacionId}");
                }

                // Lógica de Negocio
                if (string.IsNullOrEmpty(request.Componente))
                {
                    throw new ArgumentException("El componente no puede ser nulo o vacío.");
                }


                // Persistencia: DELEGADO AL REPOSITORIO
                var componenteCreado = await _reparacionRepository.CrearComponenteReparacionAsync(request);

                _logger.LogInformation("Componente de reparación creado con ID: {Id}", componenteCreado.Id);

                return componenteCreado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el componente de reparación para la reparación ID: {ReparacionId}", request.ReparacionId);
                throw; 
            }
        }

        public async Task<(int FilasAfectadas, ReparacionComponenteResponseDto? reparacionComponenteActualizada)> ActualizarComponenteReparacionAsync(int id, ReparacionComponenteActualizacionDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de componente de reparación con ID: {Id}", id);

                // Obtener entidad con tracking: DELEGADO AL REPOSITORIO (usamos un método con tracking)
                var componenteReparacionExistente = await _reparacionRepository.GetComponenteForUpdateAsync(id);

                if (componenteReparacionExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró el componente de reparación con ID: {id}");
                }
                
                // Actualizar los campos del DTO (Mapeo Parcial)
                componenteReparacionExistente.Cantidad = request.cantidad;
                if (request.GarantiaMeses != null)
                {
                    componenteReparacionExistente.GarantiaMeses = request.GarantiaMeses;
                }
                if (request.CostoUnitarioCompra != null)
                {
                    componenteReparacionExistente.CostoUnitarioCompra = request.CostoUnitarioCompra;
                }
                if (request.CostoUnitarioPublico != null)
                {
                    componenteReparacionExistente.CostoUnitarioPublico = request.CostoUnitarioPublico;
                }
                if (request.Notas != null)
                {
                    componenteReparacionExistente.Notas = request.Notas;
                }

                // Persistencia: DELEGADO AL REPOSITORIO
                var (filasAfectadas, componenteActualizado) = await _reparacionRepository.ActualizarComponenteReparacionAsync(componenteReparacionExistente);
                
                _logger.LogInformation("Componente de reparación ID {Id} actualizado. Filas afectadas: {Filas}", id, filasAfectadas);
                
                return (filasAfectadas, componenteActualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el componente de reparación con ID: {Id}", id);
                throw;
            }
        }








        // Método auxiliar para mantener el código DRY (Don't Repeat Yourself)
        private ReparacionResponseDto? MapearAResponseDto(Reparacion reparacion)
        {
            // ... (Mapeo de Reparacion a ReparacionResponseDto)
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
        private ReparacionComponenteResponseDto? MapearAResponseComponenteDto(ReparacionComponente componente)
        {
            // ... (Mapeo de Componente a ComponenteResponseDto)
            return new ReparacionComponenteResponseDto
            {
                Id = componente.Id,
                ReparacionId = componente.ReparacionId,
                Componente = componente.Componente,
                Cantidad = componente.Cantidad,
                Proveedor = componente.Proveedor,
                GarantiaMeses = componente.GarantiaMeses,
                CostoUnitarioCompra = componente.CostoUnitarioCompra,
                CostoUnitarioPublico = componente.CostoUnitarioPublico,
                SubtotalCompra = componente.SubtotalCompra,
                SubtotalPublico = componente.SubtotalPublico,
                Notas = componente.Notas
            };
        }
    }
}