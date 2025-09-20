# 🛣️ Endpoints (Minimal APIs)

## 🎯 **Propósito**
Los Endpoints representan una alternativa moderna a los Controllers tradicionales. Utilizan las Minimal APIs de .NET para definir rutas de forma más directa y funcional, especialmente útiles para endpoints simples o especializados.

## 🏗️ **Ubicación en la Arquitectura Hexagonal**
```
┌─────────────────┐
│   HTTP Client   │ ← Requests/Responses
└─────────────────┘
         ↕ HTTP
┌─────────────────┐
│   Endpoints     │ ← **ESTAMOS AQUÍ** 
│ (Minimal APIs)  │ ← Definición funcional de rutas
└─────────────────┘
         ↕ DTOs
┌─────────────────┐
│   Application   │ ← Commands/Queries via MediatR
│     Layer       │ ← Lógica de casos de uso
└─────────────────┘
```

## 🔄 **Endpoints vs Controllers**

### **Cuándo usar Endpoints (Minimal APIs)**
- ✅ **Microservicios** pequeños y enfocados
- ✅ **APIs simples** con pocas operaciones
- ✅ **Endpoints especializados** (webhooks, health checks)
- ✅ **Performance crítico** (menos overhead)
- ✅ **Prototipos rápidos**

### **Cuándo usar Controllers**
- ✅ **APIs complejas** con muchas operaciones
- ✅ **Aplicaciones grandes** con múltiples módulos
- ✅ **Funcionalidades avanzadas** (model binding, filters)
- ✅ **Equipos grandes** (mejor organización)

## 📁 **Estructura Esperada**

### **Endpoints por Módulo**
- `AdministracionEndpoints.cs` - Rutas de administración
- `RecepcionEndpoints.cs` - Rutas de recepción
- `SoporteEndpoints.cs` - Rutas de soporte
- `HealthEndpoints.cs` - Health checks y métricas
- `WebhookEndpoints.cs` - Endpoints para integraciones externas

## 🔄 **Flujo de Trabajo**

### **Definición de Endpoint**
```csharp
app.MapPost("/api/tickets", async (CrearTicketRequestDto request, IMediator mediator) =>
{
    var command = new CrearTicketCommand(request);
    var result = await mediator.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});
```

### **Con Validación**
```csharp
app.MapPost("/api/tickets", async (CrearTicketRequestDto request, IValidator<CrearTicketRequestDto> validator, IMediator mediator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);
        
    var command = new CrearTicketCommand(request);
    var result = await mediator.Send(command);
    return result.IsSuccess ? Results.Created($"/api/tickets/{result.Value.Id}", result.Value) : Results.BadRequest(result.Error);
});
```

## 🎛️ **Patrones de Implementación**

### **Agrupación por Módulo**
```csharp
public static class SoporteEndpoints
{
    public static void MapSoporteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/soporte").WithTags("Soporte");
        
        group.MapGet("/tickets", GetTickets);
        group.MapPost("/tickets", CrearTicket);
        group.MapPut("/tickets/{id}", ActualizarTicket);
        group.MapDelete("/tickets/{id}", EliminarTicket);
    }
    
    private static async Task<IResult> GetTickets(IMediator mediator, [AsParameters] GetTicketsRequest request)
    {
        var query = new GetTicketsQuery(request.Filtros);
        var result = await mediator.Send(query);
        return Results.Ok(result.Value);
    }
}
```

### **Registro en Program.cs**
```csharp
// En Program.cs
app.MapSoporteEndpoints();
app.MapRecepcionEndpoints();
app.MapAdministracionEndpoints();
```

## 🔗 **Conexiones Clave**

### **Con DTOs y Validación**
```csharp
app.MapPost("/api/tickets", async (
    CrearTicketRequestDto request,
    IValidator<CrearTicketRequestDto> validator,
    IMediator mediator,
    IMapper mapper) =>
{
    // Validación
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());
    
    // Mapeo y ejecución
    var command = mapper.Map<CrearTicketCommand>(request);
    var result = await mediator.Send(command);
    
    // Respuesta
    return result.IsSuccess 
        ? Results.Created($"/api/tickets/{result.Value.Id}", mapper.Map<TicketResponseDto>(result.Value))
        : Results.BadRequest(result.Error);
});
```

### **Con Autenticación/Autorización**
```csharp
app.MapPost("/api/tickets", CrearTicket)
   .RequireAuthorization("SoportePolicy")
   .WithName("CrearTicket")
   .WithSummary("Crear un nuevo ticket de soporte")
   .WithDescription("Crea un nuevo ticket de soporte con los datos proporcionados");
```

## 🛡️ **Aspectos de Seguridad**

### **Autorización**
```csharp
app.MapGet("/api/admin/users", GetUsers)
   .RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/api/tickets", CrearTicket)
   .RequireAuthorization("SoporteWrite");
```

### **Rate Limiting**
```csharp
app.MapPost("/api/tickets", CrearTicket)
   .RequireRateLimiting("ApiLimiter");
```

### **CORS**
```csharp
app.MapGet("/api/public/status", GetStatus)
   .RequireCors("PublicApi");
```

## 📋 **Ventajas de Minimal APIs**

### **Performance**
- ✅ Menos overhead que Controllers
- ✅ Startup más rápido
- ✅ Menor consumo de memoria

### **Simplicidad**
- ✅ Menos código boilerplate
- ✅ Definición funcional clara
- ✅ Fácil testing

### **Flexibilidad**
- ✅ Inyección de dependencias directa
- ✅ Configuración granular por endpoint
- ✅ Composición funcional

## 🎪 **Ejemplos Prácticos**

### **Health Check Endpoint**
```csharp
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
           .AllowAnonymous()
           .WithTags("Health");
           
        app.MapGet("/health/ready", async (IDbContext context) =>
        {
            try
            {
                await context.Database.CanConnectAsync();
                return Results.Ok(new { Status = "Ready" });
            }
            catch
            {
                return Results.Problem("Database not ready");
            }
        }).AllowAnonymous();
    }
}
```

### **Webhook Endpoint**
```csharp
public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/payment", async (PaymentWebhookDto payload, IMediator mediator) =>
        {
            var command = new ProcessPaymentWebhookCommand(payload);
            await mediator.Send(command);
            return Results.Ok();
        }).AllowAnonymous()
          .WithName("PaymentWebhook");
    }
}
```

## 🔧 **Configuración Global**

### **En Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapSoporteEndpoints();
app.MapRecepcionEndpoints();
app.MapAdministracionEndpoints();
app.MapHealthEndpoints();

app.Run();
```

## 📊 **Documentación Automática**

### **OpenAPI/Swagger**
```csharp
app.MapPost("/api/tickets", CrearTicket)
   .WithName("CrearTicket")
   .WithSummary("Crear ticket de soporte")
   .WithDescription("Crea un nuevo ticket de soporte con validación completa")
   .Produces<TicketResponseDto>(201)
   .ProducesValidationProblem()
   .WithTags("Soporte");
```

---
*Los Endpoints con Minimal APIs ofrecen una alternativa moderna y eficiente para definir rutas HTTP, especialmente útiles para microservicios y APIs simples en el contexto de tu arquitectura hexagonal.*