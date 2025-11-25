using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.enums;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.Interfaces.Soporte; // Usar la interfaz del repositorio
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using back_cabs.CRM.contexts; // <-- Ya no es necesario
// using Microsoft.EntityFrameworkCore; // <-- Ya no es necesario

namespace back_cabs.CRM.services.Soporte
{
    /// <summary>
    /// Servicio de lógica de negocio para Reparaciones y Componentes.
    /// Utiliza IReparacionRepository para el acceso a datos.
    /// </summary>
    public class ReparacionService // Implementa la interfaz de servicio
    {
        // Solo inyectamos el repositorio y el logger
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
<<<<<<< HEAD
=======
                var ordenExistente = await _reparacionRepository.OrdenExisteAsync(request.OrdenId);
                if (!ordenExistente)
                {
                    throw new KeyNotFoundException($"No se encontró la orden de trabajo con ID: {request.OrdenId}");
                }
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
                var tecnicoExistente = await _reparacionRepository.TecnicoExisteAsync(request.TecnicoId);
                if (!tecnicoExistente)
                {
                    throw new KeyNotFoundException($"No se encontró el técnico con ID: {request.TecnicoId}");
                }

                // Mapeo de DTO a Entidad (Lógica del Servicio)
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
                    Resultado = ResultadoReparacion.COTIZAR.ToString().ToUpper(), // Valor inicial definido por el negocio
                    CausaIrreparable = request.CausaIrreparable,
                    RespaldoDatosAutorizado = request.RespaldoDatosAutorizado,
                    CostoManoObra = request.CostoManoObra,
                    CostoRefaccionesCompra = request.CostoRefaccionesCompra,
                    CostoRefaccionesPublico = request.CostoRefaccionesPublico,
                    GarantiaDias = request.GarantiaDias,
                    FechaLlegada = DateTime.UtcNow, // Controlado por el servidor
                    EmpezadoEn = request.EmpezadoEn,
                    EntregadoEn = request.EntregadoEn,
                    TipoEntrega = TipoEntrega.RECOGE_CLIENTE.ToString().ToUpper(), // Valor inicial definido por el negocio
                    UbicacionAlmacenamiento = request.UbicacionAlmacenamiento,
<<<<<<< HEAD
                    Notas = request.Notas,
                    NombreCliente = request.NombreCliente,
                    Telefono = request.Telefono
=======
                    Notas = request.Notas
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
                    // Los campos calculados (CostoTotalCompra, etc.) no se asignan aquí
                };

                // Persistencia: DELEGADO AL REPOSITORIO
                var reparacionCreada = await _reparacionRepository.CrearReparacionAsync(nuevaReparacion); // Pasa la entidad

                _logger.LogInformation("Reparacion creada con ID: {Id}", reparacionCreada.Id);

                // Mapeo de Entidad a DTO de Respuesta (Lógica del Servicio)
                return MapearAResponseDto(reparacionCreada); // Mapea la entidad devuelta por el repo
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

                // LÓGICA DE NEGOCIO: Validación de Enums y Mapeo Parcial (Se mantiene en el Servicio)
                ResultadoReparacion nuevoResultadoEnum = ResultadoReparacion.SIN_REPARAR; // Valor por defecto si no viene
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

                // Aplicar actualizaciones desde el DTO a la entidad existente
                if (request.SolucionAplicada != null) reparacionExistente.SolucionAplicada = request.SolucionAplicada;
                if (request.CausaIrreparable != null) reparacionExistente.CausaIrreparable = request.CausaIrreparable;
                if (request.CostoManoObra.HasValue) reparacionExistente.CostoManoObra = request.CostoManoObra.Value;
                if (request.CostoRefaccionesCompra.HasValue) reparacionExistente.CostoRefaccionesCompra = request.CostoRefaccionesCompra.Value;
                if (request.CostoRefaccionesPublico.HasValue) reparacionExistente.CostoRefaccionesPublico = request.CostoRefaccionesPublico.Value;
                if (request.GarantiaDias.HasValue) reparacionExistente.GarantiaDias = request.GarantiaDias.Value;
                if (request.EmpezadoEn.HasValue) reparacionExistente.EmpezadoEn = request.EmpezadoEn.Value;
                if (request.EntregadoEn.HasValue) reparacionExistente.EntregadoEn = request.EntregadoEn.Value;
                if (request.Notas != null) reparacionExistente.Notas = request.Notas;

                // LÓGICA DE NEGOCIO: Reglas específicas (Se mantiene en el Servicio)
                if (request.Resultado != null && nuevoResultadoEnum == ResultadoReparacion.IRREPARABLE)
                {
                    if (string.IsNullOrWhiteSpace(reparacionExistente.CausaIrreparable))
                    {
                        throw new ArgumentException("Si el resultado es 'IRREPARABLE', la causa es obligatoria.");
                    }
                }
                if (request.Resultado != null && (nuevoResultadoEnum is ResultadoReparacion.REPARADO or ResultadoReparacion.DEVUELTO_SIN_REPARAR) && !reparacionExistente.EntregadoEn.HasValue)
                {
                    reparacionExistente.EntregadoEn = DateTime.UtcNow; // Sellar fecha controlado por el servidor
                }

                // PERSISTENCIA: DELEGADO AL REPOSITORIO
                var (filasAfectadas, reparacionActualizada) = await _reparacionRepository.ActualizarReparacionAsync(reparacionExistente); // Pasa la entidad modificada

                _logger.LogInformation("Reparación ID {Id} actualizada. Filas afectadas: {Filas}", id, filasAfectadas);

                // Mapeo de Entidad a DTO de Respuesta
                var dtoMapeado = reparacionActualizada != null ? MapearAResponseDto(reparacionActualizada) : null;
                return (filasAfectadas, dtoMapeado);
            }
            catch (KeyNotFoundException) { throw; } // Relanzar para que el controller devuelva 404
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

                // LECTURA: DELEGADO AL REPOSITORIO
                var reparaciones = await _reparacionRepository.ObtenerReparacionesAsync(skip, take); // Recibe entidades

                // Mapeo de Entidades a DTOs de Respuesta (Lógica del Servicio)
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

                // LECTURA: DELEGADO AL REPOSITORIO
                var reparacion = await _reparacionRepository.ObtenerReparacionPorIdAsync(id); // Recibe entidad o null

                if (reparacion == null)
                {
                    _logger.LogWarning("Reparacion no encontrada por ID {Id}", id);
                    return null;
                }

                // Mapeo de Entidad a DTO de Respuesta
                return MapearAResponseDto(reparacion);
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

                // LECTURA: DELEGADO AL REPOSITORIO
                var componentes = await _reparacionRepository.ObtenerComponentesReparacionAsync(skip, take); // Recibe entidades

                // Mapeo de Entidades a DTOs de Respuesta
                return componentes.Select(MapearAResponseComponenteDto).ToList();
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

                // LECTURA: DELEGADO AL REPOSITORIO
                var componente = await _reparacionRepository.ObtenerComponenteReparacionPorIdAsync(id); // Recibe entidad o null

                if (componente == null)
                {
<<<<<<< HEAD
                    _logger.LogWarning("Componente de reparación no encontrado por ID {Id}", id);
=======
                     _logger.LogWarning("Componente de reparación no encontrado por ID {Id}", id);
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
                    // Lanzar excepción es más consistente con otros métodos si se espera que exista
                    throw new KeyNotFoundException($"Componente de reparación no encontrado por ID {id}");
                }

                // Mapeo de Entidad a DTO de Respuesta
                return MapearAResponseComponenteDto(componente);
            }
            catch (KeyNotFoundException) { throw; } // Relanzar
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el componente de reparación con ID: {Id}", id);
                throw;
            }
        }
<<<<<<< HEAD
        public async Task<List<ReparacionComponenteResponseDto>?> ObtenerComponentesporIdReparacionAsync(int repId)
        {
            try
            {
                _logger.LogInformation("Buscando componentes para la reparacion: {Id}", repId);

                var componentes = await _reparacionRepository.ObtenerComponentePorIdReparacionAsync(repId);

                if (componentes == null)
                {
                    _logger.LogWarning("Componentes no encontrados por ID de reparacion {Id}", repId);
                    // Lanzar excepción es más consistente con otros métodos si se espera que exista
                    throw new KeyNotFoundException($"Componentes no encontrados por ID de reparacion {repId}");

                }

                return componentes.Select(MapearAResponseComponenteDto).ToList();
            }
            catch (KeyNotFoundException) { throw; } // Relanzar
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los componentes con ID de reparacion: {Id}", repId);
                throw;
            }
        }
=======
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301

        public async Task<ReparacionComponenteResponseDto> CrearComponenteReparacionAsync(ReparacionComponenteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de componente para la reparación ID: {ReparacionId}", request.ReparacionId);

                // Validacion de Referencias: DELEGADO AL REPOSITORIO
                var reparacionExistente = await _reparacionRepository.ReparacionExisteAsync(request.ReparacionId);
                if (!reparacionExistente)
                {
                    throw new KeyNotFoundException($"No se encontró la reparación con ID: {request.ReparacionId}");
                }

                // Lógica de Negocio (Validación de entrada)
                if (string.IsNullOrEmpty(request.Componente))
                {
                    throw new ArgumentException("El nombre del componente no puede ser nulo o vacío.");
                }
                if (request.Cantidad <= 0)
                {
<<<<<<< HEAD
                    throw new ArgumentException("La cantidad debe ser mayor que cero.");
=======
                     throw new ArgumentException("La cantidad debe ser mayor que cero.");
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
                }

                // Mapeo de DTO a Entidad
                var nuevoComponente = new ReparacionComponente
                {
                    ReparacionId = request.ReparacionId,
                    Componente = request.Componente,
                    Cantidad = request.Cantidad,
                    Proveedor = request.Proveedor,
                    GarantiaMeses = request.GarantiaMeses,
                    CostoUnitarioCompra = request.CostoUnitarioCompra,
                    CostoUnitarioPublico = request.CostoUnitarioPublico,
                    Notas = request.Notas
                    // Subtotales son calculados (asumido)
                };

                // Persistencia: DELEGADO AL REPOSITORIO
                var componenteCreado = await _reparacionRepository.CrearComponenteReparacionAsync(nuevoComponente); // Pasa la entidad

                _logger.LogInformation("Componente de reparación creado con ID: {Id}", componenteCreado.Id);

                // Mapeo de Entidad a DTO de Respuesta
                return MapearAResponseComponenteDto(componenteCreado);
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

                // Obtener entidad con tracking: DELEGADO AL REPOSITORIO
                var componenteExistente = await _reparacionRepository.GetComponenteForUpdateAsync(id);
                if (componenteExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró el componente de reparación con ID: {id}");
                }

<<<<<<< HEAD
                // Lógica de Negocio (Validación de entrada)
                if (request.cantidad <= 0) // Asumiendo que 'cantidad' es el nombre en el DTO
                {
                    throw new ArgumentException("La cantidad debe ser mayor que cero.");
=======
                 // Lógica de Negocio (Validación de entrada)
                if (request.cantidad <= 0) // Asumiendo que 'cantidad' es el nombre en el DTO
                {
                     throw new ArgumentException("La cantidad debe ser mayor que cero.");
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
                }

                // Actualizar los campos desde el DTO (Mapeo Parcial)
                componenteExistente.Cantidad = request.cantidad; // Usar el nombre correcto del DTO
                if (request.GarantiaMeses.HasValue) // Usar HasValue para Nullables
                {
                    componenteExistente.GarantiaMeses = request.GarantiaMeses.Value;
                }
                if (request.CostoUnitarioCompra.HasValue)
                {
                    componenteExistente.CostoUnitarioCompra = request.CostoUnitarioCompra.Value;
                }
                if (request.CostoUnitarioPublico.HasValue)
                {
                    componenteExistente.CostoUnitarioPublico = request.CostoUnitarioPublico.Value;
                }
                if (request.Notas != null) // Checkear null para strings
                {
                    componenteExistente.Notas = request.Notas;
                }
                // Actualizar fecha de modificación si existe en la entidad
                // componenteExistente.ActualizadoEn = DateTime.UtcNow; 

                // Persistencia: DELEGADO AL REPOSITORIO
                var (filasAfectadas, componenteActualizado) = await _reparacionRepository.ActualizarComponenteReparacionAsync(componenteExistente); // Pasa la entidad

                _logger.LogInformation("Componente de reparación ID {Id} actualizado. Filas afectadas: {Filas}", id, filasAfectadas);

                // Mapeo de Entidad a DTO de Respuesta
                var dtoMapeado = componenteActualizado != null ? MapearAResponseComponenteDto(componenteActualizado) : null;
                return (filasAfectadas, dtoMapeado);
            }
            catch (KeyNotFoundException) { throw; } // Relanzar
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el componente de reparación con ID: {Id}", id);
                throw;
            }
        }

        // =====================================================================
        // MÉTODOS PRIVADOS DE MAPEO (Se mantienen en el Servicio)
        // =====================================================================
        private ReparacionResponseDto MapearAResponseDto(Reparacion reparacion)
        {
<<<<<<< HEAD
            if (reparacion == null) return null!; // Manejo de nulos
=======
             if (reparacion == null) return null!; // Manejo de nulos
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
            // Lógica para mapear de Entidad a DTO de Respuesta
            return new ReparacionResponseDto
            {
                Id = reparacion.Id,
                OrdenId = reparacion.OrdenId,
                TecnicoId = reparacion.TecnicoId,
                // TecnicoNombre = reparacion.Tecnico?.NombreCompleto, // Ejemplo si el repo incluye Tecnico
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
                CostoTotalCompra = reparacion.CostoTotalCompra, // Asume que la entidad lo tiene calculado
                GarantiaDias = reparacion.GarantiaDias,
                FechaLlegada = reparacion.FechaLlegada,
                EmpezadoEn = reparacion.EmpezadoEn,
                EntregadoEn = reparacion.EntregadoEn,
                TipoEntrega = reparacion.TipoEntrega,
                UbicacionAlmacenamiento = reparacion.UbicacionAlmacenamiento,
                Notas = reparacion.Notas,
<<<<<<< HEAD
                CostoTotalPublico = reparacion.CostoTotalPublico, // Asume que la entidad lo tiene calculado
                NombreCliente = reparacion.NombreCliente,
                Telefono = reparacion.Telefono
=======
                CostoTotalPublico = reparacion.CostoTotalPublico // Asume que la entidad lo tiene calculado
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
            };
        }

        private ReparacionComponenteResponseDto MapearAResponseComponenteDto(ReparacionComponente componente)
        {
<<<<<<< HEAD
            if (componente == null) return null!; // Manejo de nulos
=======
             if (componente == null) return null!; // Manejo de nulos
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
            // Lógica para mapear de Entidad a DTO de Respuesta
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
                SubtotalCompra = componente.SubtotalCompra, // Asume que la entidad lo tiene calculado
                SubtotalPublico = componente.SubtotalPublico, // Asume que la entidad lo tiene calculado
                Notas = componente.Notas
            };
<<<<<<< HEAD
        }
    }
=======
        }
    }
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
}