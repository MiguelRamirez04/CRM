# ⚡ Realtime (SignalR)

## 🎯 **Propósito**
Implementa comunicación en tiempo real usando SignalR para notificaciones instantáneas, actualizaciones de estado y colaboración en vivo entre usuarios del CRM.

## 🏗️ **Ubicación en la Arquitectura Hexagonal**
```
┌─────────────────┐
│   Web Clients   │ ← WebSocket/SSE connections
│   (JavaScript)  │ ← Real-time updates
└─────────────────┘
         ↕ SignalR
┌─────────────────┐
│   SignalR Hubs  │ ← **ESTAMOS AQUÍ**
│   (Realtime)    │ ← Connection management
└─────────────────┘
         ↕ RealtimeDTOs
┌─────────────────┐
│   Application   │ ← Domain Events
│     Layer       │ ← Event Handlers
└─────────────────┘
```

## 📁 **Estructura de Subcarpetas**

### `/Hubs/`
**Qué hace**: Contiene los SignalR Hubs que manejan conexiones y grupos
**Archivos típicos**:
- `SoporteHub.cs` - Hub para notificaciones de soporte
- `RecepcionHub.cs` - Hub para actualizaciones de recepción
- `AdminHub.cs` - Hub para notificaciones administrativas
- `BaseHub.cs` - Funcionalidad común de todos los hubs

**Por qué**: Centraliza la lógica de conexión y broadcasting
**Conecta con**: Clientes web ↔ Event Handlers ↔ Application Layer

### `/Contracts/`
**Qué hace**: Interfaces que definen métodos disponibles para clientes
**Archivos típicos**:
- `ISoporteClient.cs` - Métodos que clientes pueden recibir para soporte
- `IRecepcionClient.cs` - Métodos para clientes de recepción
- `IAdminClient.cs` - Métodos para clientes administrativos

**Por qué**: Contrato tipado para comunicación cliente-servidor
**Conecta con**: Hubs → Clientes (TypeScript/JavaScript)

### `/Mapping/`
**Qué hace**: Mappers entre Domain Events y RealtimeDTOs
**Archivos típicos**:
- `RealtimeEventMapper.cs` - Mapeos generales
- `SoporteRealtimeMapper.cs` - Mapeos específicos de soporte
- `RecepcionRealtimeMapper.cs` - Mapeos específicos de recepción

**Por qué**: Transforma eventos de dominio en DTOs optimizados para SignalR
**Conecta con**: Domain Events → RealtimeDTOs → SignalR Hubs

## 🔄 **Flujo de Comunicación Tiempo Real**

### **Event-Driven Notifications**
```
Domain Event → Event Handler → Realtime Mapper → RealtimeDto → SignalR Hub → Connected Clients
```

### **User-Initiated Actions**
```
Client Action → Hub Method → Application Command → Domain Event → Broadcast Update
```

### **Cross-Module Communication**
```
Recepcion: Llamada → Deriva → Soporte: Ticket Created → Realtime Update → All Interested Clients
```

## 🎛️ **Casos de Uso Cubiertos**

### **Soporte en Tiempo Real**
- ✅ **Nuevo ticket creado** → Notificar supervisores
- ✅ **Ticket asignado** → Notificar técnico
- ✅ **Comentario añadido** → Notificar participantes
- ✅ **Escalación** → Notificar management
- ✅ **Ticket resuelto** → Notificar cliente y equipo

### **Recepción en Tiempo Real**
- ✅ **Llamada entrante** → Notificar recepcionistas disponibles
- ✅ **Cita programada** → Notificar agenda
- ✅ **Derivación creada** → Notificar soporte
- ✅ **Cliente identificado** → Actualizar información

### **Administración en Tiempo Real**
- ✅ **Usuario conectado/desconectado** → Dashboard de presencia
- ✅ **Configuración actualizada** → Notificar todos los usuarios
- ✅ **Alertas del sistema** → Notificar administradores
- ✅ **Métricas en tiempo real** → Dashboards ejecutivos

## 🔗 **Conexiones Clave**

### **Con Application Layer (Event Handlers)**
```csharp
public class TicketCreadoEventHandler : INotificationHandler<TicketCreadoDomainEvent>
{
    private readonly IHubContext<SoporteHub, ISoporteClient> _hubContext;
    private readonly IRealtimeMapper _mapper;

    public async Task Handle(TicketCreadoDomainEvent notification, CancellationToken cancellationToken)
    {
        var realtimeDto = _mapper.Map<TicketCreadoRealtimeDto>(notification);
        await _hubContext.Clients.Group("Supervisores").TicketCreado(realtimeDto);
    }
}
```

### **Con DTOs de Realtime**
```csharp
public class SoporteHub : Hub<ISoporteClient>
{
    public async Task JoinSoporteGroup(string userId, string rol)
    {
        if (rol == "Supervisor" || rol == "Tecnico")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Soporte");
        }
    }
}
```

### **Con Infrastructure Layer**
```csharp
// En Soporte.Infrastructure.Realtime
public class SoporteNotificationService : ISoporteNotificationService
{
    private readonly IHubContext<SoporteHub, ISoporteClient> _hubContext;
    
    public async Task NotificarTicketCreado(TicketCreadoRealtimeDto dto)
    {
        await _hubContext.Clients.Group("Supervisores").TicketCreado(dto);
    }
}
```

## 🎪 **Implementación de Hubs**

### **Base Hub**
```csharp
public abstract class BaseHub<T> : Hub<T> where T : class
{
    private readonly ILogger<BaseHub<T>> _logger;
    
    protected BaseHub(ILogger<BaseHub<T>> logger)
    {
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
```

### **Soporte Hub Específico**
```csharp
public class SoporteHub : BaseHub<ISoporteClient>
{
    public SoporteHub(ILogger<SoporteHub> logger) : base(logger) { }
    
    [Authorize(Roles = "Tecnico,Supervisor")]
    public async Task JoinTicketGroup(int ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
    }
    
    [Authorize(Roles = "Supervisor")]
    public async Task JoinSupervisorGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Supervisores");
    }
}
```

## 🛡️ **Seguridad y Autorización**

### **Autenticación**
```csharp
// En Program.cs
builder.Services.AddSignalR();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...

app.MapHub<SoporteHub>("/hubs/soporte");
```

### **Autorización por Método**
```csharp
[Authorize(Roles = "Admin")]
public async Task BroadcastSystemMessage(string message)
{
    await Clients.All.SystemMessage(message);
}
```

### **Filtrado por Usuario**
```csharp
public async Task NotificarTicketAsignado(int ticketId, string tecnicoId)
{
    await Clients.User(tecnicoId).TicketAsignado(ticketId);
}
```

## 📋 **Contratos de Cliente**

### **ISoporteClient**
```csharp
public interface ISoporteClient
{
    Task TicketCreado(TicketCreadoRealtimeDto ticket);
    Task TicketAsignado(TicketAsignadoRealtimeDto asignacion);
    Task ComentarioAñadido(ComentarioRealtimeDto comentario);
    Task TicketEscalado(TicketEscaladoRealtimeDto escalacion);
    Task TicketResuelto(TicketRealtimeDto ticket);
}
```

### **IRecepcionClient**
```csharp
public interface IRecepcionClient
{
    Task LlamadaEntrante(LlamadaRealtimeDto llamada);
    Task CitaProgramada(CitaRealtimeDto cita);
    Task DerivacionCreada(DerivacionRealtimeDto derivacion);
    Task ClienteIdentificado(ClienteRealtimeDto cliente);
}
```

## 🔧 **Configuración**

### **En Program.cs**
```csharp
// Servicios
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Hubs
app.MapHub<SoporteHub>("/hubs/soporte");
app.MapHub<RecepcionHub>("/hubs/recepcion");
app.MapHub<AdminHub>("/hubs/admin");
```

### **Escalabilidad con Redis**
```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis(connectionString, options =>
    {
        options.Configuration.ChannelPrefix = "CRM";
    });
```

## 🎮 **Ejemplo de Cliente JavaScript**

```javascript
// Conexión al hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/soporte", {
        accessTokenFactory: () => localStorage.getItem("jwt")
    })
    .build();

// Eventos del servidor
connection.on("TicketCreado", (ticket) => {
    console.log("Nuevo ticket:", ticket);
    actualizarListaTickets(ticket);
    mostrarNotificacion(`Nuevo ticket: ${ticket.titulo}`);
});

// Iniciar conexión
connection.start().then(() => {
    console.log("Conectado al hub de soporte");
    connection.invoke("JoinSupervisorGroup");
});
```

## 📊 **Métricas y Monitoring**

### **Tracking de Conexiones**
- Usuarios conectados por módulo
- Tiempo de conexión promedio
- Mensajes enviados/recibidos
- Latencia de notificaciones

### **Health Checks**
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<SignalRHealthCheck>("signalr");
```

---
*El módulo Realtime es el sistema nervioso del CRM, manteniendo a todos los usuarios sincronizados y informados en tiempo real sobre eventos críticos del negocio.*