/*
Aqui van los controller que se van a usar para la base de datos y el dashboarad
*/
using back_cabs.CRM.DTOs.Recepcion;
using back_cabs.CRM.enums.Recepcion;
using back_cabs.CRM.models.Recepcion;
using Microsoft.AspNetCore.Mvc; //este es importante para que la api funcione
/*using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
*/
using System.Text;
using System.Net;

[Apicontroller]
[Route]("api/[controller]") //la ruta que usare para lo que es la peticion
public class RecepcionController : ControllerBase{
    private static List<OrdenTrabajo> _ordenes = new List<OrdenTrabajo>{
        new OrdenTrabajo{
            Id = 1,
            Notas = "",
            CitaProgramadaInicio = DateTime.UtcNow,
            CitaProgramadaFin = DateTime.UtcNow,
            Modalidad = "presencial",
            TipoOrden = "servicio",
            Prioridad = 3,
            Estado = true,
            CostoEstimado = 500,
            id_usuario = 1,
            CreadoEn = DateTime.UtcNow
            ActualizadoEn =  DateTime.UtcNow
        }
    };
    private static int _nextId = 2;
    // ----------------------------------------------------------------------
    // [GET] /api/recepcion
    // ----------------------------------------------------------------------

    /// <summary>
    /// Obtiene la lista completa de Órdenes de Trabajo ademas de que me identifica que fue de que
    /// </summary>
    /// <returns>Lista de OrdenTrabajoResponseDto. osea que me va a regresar egun lo que quiero</returns>
    /// 
    [HttpGet]
    [ProduccesResponseType((int)HttpStatusCode.OK, Type = typeof(iEnumerable<OrdenTrabajoResponseDto>))]
    public ActionResult<iEnumerable<OrdenTrabajoCreacionRequestDto>> GetAll();{
        //mapeo (modelo domiino -> DTO de respuesta)
        var responseDtos = _ordenes.Select(o => new OrdenTrabajoResponseDto{
            Id = o.id,
            Notas =o.Notas,
            CitaProgramadaInicio = o.citaProgramadaInicio,
            CitaProgramadaFin = o.citaProgramadaFin,
            Modalidad = o.Modalidad,
            TipoOrden = o.tipoOrden,
            legacyClientId = o.legacyClientId,
            Prioridad = o.Prioridad,
            Estado = o.Estado,
            UbicacionText =o.UbicacionText,
            EstadoFacturacion = o.EstadoFacturado,
            RequiereFactura = o.RequiereFactura,
            CostoReal = o.costoReal,
            CostoEstimado = o.CostoEstimado,
            CreadoEn = o.CreadoEn,
            ActualizadoEn = o.ActualizadoEn,
            id_usuario = o.id_usuario
        }).ToList();
        return
        Ok(responseDtos);
    }
    // ----------------------------------------------------------------------
    // [POST] /api/recepcion
    // ----------------------------------------------------------------------

    /// <summary>
    /// Crea una nueva Orden de Trabajo.
    /// </summary>
    /// <param name="requestDto">DTO con los datos de creación.</param>
    /// <returns>La Orden de Trabajo recién creada.</returns>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof (OrdenTrabajoCreacionRequestDto))]
    [ProducesResponseType((int)StatusCode.BadRequest)] // por si falla la validacion del DTO
    public ActionResult<OrdenTrabajoResponseDto> CrearOrden([FromBody] OrdenTrabajoCreacionRequestDto){
        {
        // 1. **Validación Automática del Modelo:**
        // Si el DTO de entrada no cumple con los atributos [Required], [StringLength], [Range],
        // el framework ASP.NET Core automáticamente retorna un 400 Bad Request
        // antes de que la ejecución llegue aquí.

        // 2. Mapeo (DTO de Creación -> Modelo de Dominio)
        var nuevaOrden = new OrdenTrabajo
        {
            Id = _nextId++, // Simulación de ID generado por la BD
            Notas = requestDto.Notas,
            CitaProgramadaInicio = requestDto.CitaProgramadaInicio,
            CitaProgramadaFin = requestDto.CitaProgramadaFin,
            Modalidad = requestDto.Modalidad,
            TipoOrden = requestDto.TipoOrden,
            LegacyClientId = requestDto.LegacyClientId.GetValueOrDefault(), // Usamos GetValueOrDefault ya que es [Required]
            Prioridad = requestDto.Prioridad,
            Estado = requestDto.Estado,
            UbicacionText = requestDto.UbicacionText,
            EstadoFacturado = requestDto.EstadoFacturado,
            RequiereFactura = requestDto.RequiereFactura,
            CostoReal = requestDto.CostoReal,
            CostoEstimado = requestDto.CostoEstimado.GetValueOrDefault(),
            id_usuario = requestDto.id_usuario,
            CreadoEn = DateTime.UtcNow, // El sistema establece la fecha de creación
            ActualizadoEn = DateTime.UtcNow
        };

        // 3. Lógica de negocio: Guardar en la "base de datos"
        _ordenes.Add(nuevaOrden);

        // 4. Mapeo final (Modelo de Dominio -> DTO de Respuesta)
        var responseDto = new OrdenTrabajoResponseDto
        {
            Id = nuevaOrden.Id,
            Notas = nuevaOrden.Notas,
            CitaProgramadaInicio = nuevaOrden.CitaProgramadaInicio,
            CitaProgramadaFin = nuevaOrden.CitaProgramadaFin,
            Modalidad = nuevaOrden.Modalidad,
            TipoOrden = nuevaOrden.TipoOrden,
            LegacyClientId = nuevaOrden.LegacyClientId,
            Prioridad = nuevaOrden.Prioridad,
            Estado = nuevaOrden.Estado,
            UbicacionText = nuevaOrden.UbicacionText,
            EstadoFacturado = nuevaOrden.EstadoFacturado,
            RequiereFactura = nuevaOrden.RequiereFactura,
            CostoReal = nuevaOrden.CostoReal,
            CostoEstimado = nuevaOrden.CostoEstimado,
            CreadoEn = nuevaOrden.CreadoEn,
            ActualizadoEn = nuevaOrden.ActualizadoEn,
            id_usuario = nuevaOrden.id_usuario
        };

        // 5. Retorna 201 Created y la ruta para obtener el nuevo recurso.
        return CreatedAtAction(nameof(GetOrdenesPorId), new { id = responseDto.Id }, responseDto);
    }
    
    // ----------------------------------------------------------------------
    // [GET] /api/recepcion/{id}
    // ----------------------------------------------------------------------

    /// <summary>
    /// Obtiene una Orden de Trabajo por su ID.
    /// </summary>
    /// <param name="id">El ID único de la orden.</param>
    /// <returns>OrdenTrabajoResponseDto</returns>
    [HttpGet("{id}")] // Agrega el parámetro {id} a la ruta: /api/recepcion/1
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(OrdenTrabajoResponseDto))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<OrdenTrabajoResponseDto> GetOrdenesPorId(int id)
    {
        // 1. Lógica de Negocio: Buscar el modelo
        var orden = _ordenes.FirstOrDefault(o => o.Id == id);

        if (orden == null)
        {
            // Retorna HTTP 404 Not Found si el recurso no existe.
            return NotFound($"Orden de Trabajo con ID {id} no encontrada.");
        }

        // 2. Mapeo (Modelo de Dominio -> DTO de Respuesta)
        var responseDto = new OrdenTrabajoResponseDto
        {
            Id = orden.Id,
            Notas = orden.Notas,
            CitaProgramadaInicio = orden.CitaProgramadaInicio,
            CitaProgramadaFin = orden.CitaProgramadaFin,
            Modalidad = orden.Modalidad,
            TipoOrden = orden.TipoOrden,
            LegacyClientId = orden.LegacyClientId,
            Prioridad = orden.Prioridad,
            Estado = orden.Estado,
            UbicacionText = orden.UbicacionText,
            EstadoFacturado = orden.EstadoFacturado,
            RequiereFactura = orden.RequiereFactura,
            CostoReal = orden.CostoReal,
            CostoEstimado = orden.CostoEstimado,
            CreadoEn = orden.CreadoEn,
            ActualizadoEn = orden.ActualizadoEn,
            id_usuario = orden.id_usuario
        };

        // 3. Retorna HTTP 200 OK.
        return Ok(responseDto);
    }

    // ----------------------------------------------------------------------
    // [PUT] /api/recepcion/{id}
    // ----------------------------------------------------------------------

    /// <summary>
    /// Actualiza completamente una Orden de Trabajo existente.
    /// </summary>
    /// <param name="id">El ID único de la orden a actualizar.</param>
    /// <param name="requestDto">DTO con los campos a modificar (anulables).</param>
    /// <returns>No Content si la actualización es exitosa.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IActionResult ActualizarOrden(int id, [FromBody] OrdenTrabajoActualizacionRequestDto requestDto)
    {
        // 1. Lógica de Negocio: Buscar el modelo
        var ordenExistente = _ordenes.FirstOrDefault(o => o.Id == id);

        if (ordenExistente == null)
        {
            // Retorna HTTP 404 Not Found.
            return NotFound($"Orden de Trabajo con ID {id} no encontrada para actualizar.");
        }
        
        // 2. Aplicar Actualizaciones solo si el campo viene en el DTO (es la clave de un DTO de actualización)
        // Como todos los campos en OrdenTrabajoActualizacionRequestDto son anulables (string? o int?),
        // solo actualizamos si el valor es diferente de null.
        
        if (requestDto.Notas is not null) ordenExistente.Notas = requestDto.Notas;
        if (requestDto.CitaProgramadaInicio is not null) ordenExistente.CitaProgramadaInicio = requestDto.CitaProgramadaInicio.Value;
        if (requestDto.CitaProgramadaFin is not null) ordenExistente.CitaProgramadaFin = requestDto.CitaProgramadaFin;
        if (requestDto.Prioridad is not null) ordenExistente.Prioridad = requestDto.Prioridad.Value;
        if (requestDto.Estado is not null) ordenExistente.Estado = requestDto.Estado.Value;
        if (requestDto.EstadoFacturado is not null) ordenExistente.EstadoFacturado = requestDto.EstadoFacturado;
        if (requestDto.RequiereFactura is not null) ordenExistente.RequiereFactura = requestDto.RequiereFactura.Value;
        if (requestDto.FacturaFolio is not null) ordenExistente.FacturaFolio = requestDto.FacturaFolio;
        if (requestDto.CostoReal is not null) ordenExistente.CostoReal = requestDto.CostoReal;
        if (requestDto.CostoEstimado is not null) ordenExistente.CostoEstimado = requestDto.CostoEstimado;
        if (requestDto.id_usuario != 0) ordenExistente.id_usuario = requestDto.id_usuario; // Asumo que id_usuario no puede ser 0
        
        // 3. El sistema actualiza el timestamp
        ordenExistente.ActualizadoEn = DateTime.UtcNow;

        // 4. Lógica de Negocio: Guardar cambios (si usaras EF Core, harías un _context.SaveChanges() aquí)

        // 5. Retorna 204 No Content (la actualización fue exitosa y no se envía cuerpo de respuesta).
        return NoContent(); 
    }

}