# 🎮 Controllers

## 🎯 **Propósito**
Los Controllers son el punto de entrada HTTP de la API. Orquestan las solicitudes, validan datos, delegan la lógica a la capa Application y retornan respuestas estructuradas.

## 🏗️ **Ubicación en la Arquitectura Hexagonal**
```
┌─────────────────┐
│   HTTP Client   │ ← Requests/Responses
└─────────────────┘
         ↕ HTTP
┌─────────────────┐
│   Controllers   │ ← **ESTAMOS AQUÍ** 
│   (API Layer)   │ ← Orquestación y validación
└─────────────────┘
         ↕ DTOs
┌─────────────────┐
│   Application   │ ← Commands/Queries via MediatR
│     Layer       │ ← Lógica de casos de uso
└─────────────────┘
```

## 📁 **Estructura Esperada**

### **Controllers por Módulo**
- `AdministracionController.cs` - Gestión de usuarios, roles y configuración
- `RecepcionController.cs` - Llamadas, citas y derivaciones
- `SoporteController.cs` - Tickets, asignaciones y seguimiento
- `CommonController.cs` - Endpoints compartidos (salud, métricas)

## 🔄 **Flujo de Trabajo**

### **Request Flow**
```
HTTP Request → Controller Method → Validate DTOs → MediatR Command/Query → Application Layer
```

### **Response Flow**
```
Application Result → Map to ResponseDTO → HTTP Response → Client
```

## 🎛️ **Responsabilidades de un Controller**

### ✅ **Qué SÍ debe hacer**
1. **Recibir requests HTTP** y extraer parámetros
2. **Validar DTOs** usando model binding y validation attributes
3. **Mapear DTOs** a Commands/Queries
4. **Enviar Commands/Queries** via MediatR
5. **Mapear Results** a ResponseDTOs
6. **Retornar respuestas HTTP** con códigos apropiados
7. **Manejar autenticación/autorización** con attributes

### ❌ **Qué NO debe hacer**
1. **Lógica de negocio** - Eso va en Domain/Application
2. **Acceso directo a BD** - Eso va en Infrastructure
3. **Operaciones complejas** - Delegar a Application
4. **Manejo de errores** - Usar middleware global

## 🔗 **Conexiones Clave**

### **Con DTOs**
```csharp
[HttpPost]
public async Task<BaseResponseDto<TicketResponseDto>> CrearTicket(CrearTicketRequestDto request)
{
    var command = _mapper.Map<CrearTicketCommand>(request);
    var result = await _mediator.Send(command);
    return _mapper.Map<BaseResponseDto<TicketResponseDto>>(result);
}
```

### **Con Application Layer (via MediatR)**
```csharp
// Constructor injection
private readonly IMediator _mediator;
private readonly IMapper _mapper;

// En el método
var query = new GetTicketsQuery(filtros);
var result = await _mediator.Send(query);
```

### **Con Autenticación/Autorización**
```csharp
[Authorize(Roles = "Supervisor,Tecnico")]
[HttpPut("{id}")]
public async Task<IActionResult> ActualizarTicket(int id, ActualizarTicketRequestDto request)
```

## 🛡️ **Aspectos de Seguridad**

### **Validación**
- Model binding automático con DTOs
- Validation attributes en RequestDTOs
- Custom validators para reglas complejas

### **Autorización**
- `[Authorize]` attributes por método o controller
- Role-based access control
- Claims-based permissions

### **Rate Limiting**
- Throttling para prevenir abuso
- Límites por usuario/IP
- Protección contra ataques

## 📋 **Convenciones**

### **Naming**
- Controllers: `{Modulo}Controller` (ej: `SoporteController`)
- Actions: Verbos claros (`CrearTicket`, `ObtenerTicket`, `ListarTickets`)

### **HTTP Methods**
- `GET` para consultas (queries)
- `POST` para creación (commands)
- `PUT` para actualización completa
- `PATCH` para actualización parcial
- `DELETE` para eliminación

### **Response Codes**
- `200 OK` - Éxito general
- `201 Created` - Recurso creado
- `400 Bad Request` - Error de validación
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - No autorizado
- `404 Not Found` - Recurso no encontrado
- `500 Internal Server Error` - Error del servidor

## 🎪 **Ejemplo Completo**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SoporteController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SoporteController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost("tickets")]
    [Authorize(Roles = "Recepcionista,Supervisor")]
    public async Task<ActionResult<BaseResponseDto<TicketResponseDto>>> CrearTicket(
        CrearTicketRequestDto request)
    {
        var command = _mapper.Map<CrearTicketCommand>(request);
        var result = await _mediator.Send(command);
        
        if (result.IsFailure)
            return BadRequest(BaseResponseDto<TicketResponseDto>.Error(result.Error));
            
        var response = _mapper.Map<TicketResponseDto>(result.Value);
        return CreatedAtAction(nameof(ObtenerTicket), 
            new { id = response.Id }, 
            BaseResponseDto<TicketResponseDto>.Success(response));
    }

    [HttpGet("tickets/{id}")]
    public async Task<ActionResult<BaseResponseDto<TicketResponseDto>>> ObtenerTicket(int id)
    {
        var query = new GetTicketQuery(id);
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return NotFound(BaseResponseDto<TicketResponseDto>.Error(result.Error));
            
        var response = _mapper.Map<TicketResponseDto>(result.Value);
        return Ok(BaseResponseDto<TicketResponseDto>.Success(response));
    }
}
```

## 🔧 **Configuración Requerida**

### **En Program.cs**
```csharp
// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Controllers
builder.Services.AddControllers();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...
```

---
*Los Controllers son la fachada de tu API, manteniendo la simplicidad mientras orquestan toda la funcionalidad del CRM de manera limpia y eficiente.*