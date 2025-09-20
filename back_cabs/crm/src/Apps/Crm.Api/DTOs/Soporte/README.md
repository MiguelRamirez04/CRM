# 🛠️ DTOs de Soporte

## 🎯 **Propósito**
Contiene los DTOs específicos para el módulo de Soporte del CRM. Maneja tickets, seguimiento de problemas, comunicación en tiempo real y resolución de casos derivados desde Recepción.

## 📁 **Estructura de Subcarpetas**

### `/Request/`
**Qué hace**: DTOs para solicitudes HTTP de soporte
**Archivos típicos**:
- `CrearTicketRequestDto.cs` - Crear nuevo ticket de soporte
- `ActualizarTicketRequestDto.cs` - Actualizar ticket existente
- `AsignarTicketRequestDto.cs` - Asignar ticket a técnico
- `CerrarTicketRequestDto.cs` - Cerrar/resolver ticket
- `AgregarComentarioRequestDto.cs` - Añadir comentario al ticket
- `EscalarTicketRequestDto.cs` - Escalar ticket a nivel superior

**Por qué**: Validación específica para operaciones de soporte técnico
**Conecta con**: Controllers de Soporte → Commands del módulo

### `/Response/`
**Qué hace**: DTOs para respuestas HTTP de soporte
**Archivos típicos**:
- `TicketResponseDto.cs` - Información completa del ticket
- `TicketListResponseDto.cs` - Lista de tickets (vista resumida)
- `ComentarioResponseDto.cs` - Comentarios del ticket
- `TecnicoResponseDto.cs` - Información del técnico asignado
- `EstadisticasSoporteResponseDto.cs` - Métricas y KPIs
- `HistorialTicketResponseDto.cs` - Historial de cambios

**Por qué**: Información optimizada para dashboards y vistas de soporte
**Conecta con**: Queries del módulo → Controllers de Soporte

### `/Realtime/`
**Qué hace**: DTOs específicos para comunicación en tiempo real via SignalR
**Archivos típicos**:
- `TicketCreadoRealtimeDto.cs` - Notificación de nuevo ticket
- `TicketActualizadoRealtimeDto.cs` - Cambios en tiempo real
- `ComentarioRealtimeDto.cs` - Nuevos comentarios
- `AsignacionRealtimeDto.cs` - Notificación de asignación
- `EscalacionRealtimeDto.cs` - Alertas de escalación

**Por qué**: Comunicación inmediata de eventos críticos
**Conecta con**: SignalR Hubs → Clientes conectados

### `/Mapping/`
**Qué hace**: Mappers entre DTOs y Commands/Queries/Events de Soporte
**Archivos típicos**:
- `SoporteDtoMapper.cs` - Mapeos principales del módulo
- `TicketDtoProfile.cs` - AutoMapper profile para tickets
- `RealtimeDtoMapper.cs` - Mapeos específicos para SignalR

**Por qué**: Transformación controlada entre todas las capas
**Conecta con**: DTOs ↔ Soporte.Application ↔ SignalR

## 🔄 **Flujo de Datos Específico**

### **Gestión de Tickets**
```
CrearTicketRequestDto → CrearTicketCommand → Domain → TicketResponseDto → TicketCreadoRealtimeDto
```

### **Comunicación en Tiempo Real**
```
Domain Event → RealtimeMapper → RealtimeDto → SignalR Hub → All Connected Clients
```

### **Escalación de Casos**
```
EscalarTicketRequestDto → EscalarCommand → Domain → EscalacionRealtimeDto → Management Dashboard
```

## 🎛️ **Casos de Uso Cubiertos**

- **Tickets**: CRUD completo de tickets de soporte
- **Asignaciones**: Gestión de técnicos y cargas de trabajo
- **Comentarios**: Comunicación interna y con clientes
- **Escalaciones**: Manejo de casos complejos
- **Métricas**: SLA, tiempos de respuesta, satisfacción
- **Notificaciones**: Alertas en tiempo real

## ⚡ **Características de Tiempo Real**

### **SignalR Integration**
Los DTOs de Realtime están optimizados para SignalR:
- **Ligeros**: Solo datos esenciales
- **Serializables**: Compatible con múltiples clientes
- **Tipados**: Contratos específicos para cada evento

### **Event-Driven Updates**
```
Ticket Created → TicketCreadoRealtimeDto → All Supervisors
Ticket Assigned → AsignacionRealtimeDto → Assigned Technician
Comment Added → ComentarioRealtimeDto → Relevant Users
Ticket Escalated → EscalacionRealtimeDto → Management
```

## 🔗 **Conexiones Clave**

### **Con Soporte.Application**
- RequestDTOs → Commands (Crear, Asignar, Actualizar)
- Queries → ResponseDTOs (Obtener, Listar, Buscar)
- Domain Events → RealtimeDTOs

### **Con Soporte.Infrastructure.Realtime**
- RealtimeDTOs → SignalR Hubs
- Event Handlers → Realtime Notifications

### **Con Recepcion (Cross-Module)**
- TicketDerivadoDto recibido desde Recepción
- Estado de tickets comunicado de vuelta

## 🎪 **Patrones Especiales**

### **Estado del Ticket**
Los DTOs manejan transiciones de estado:
- Nuevo → En Progreso → Resuelto → Cerrado
- Validaciones específicas por estado
- Notificaciones automáticas en cambios

### **Priorización Dinámica**
- Algoritmos de prioridad en RequestDTOs
- SLA tracking en ResponseDTOs
- Alertas automáticas por tiempo

## 🎪 **Ejemplos de DTOs**

### **Request Example**
```csharp
public record CrearTicketRequestDto
{
    public string Titulo { get; init; }
    public string Descripcion { get; init; }
    public PrioridadTicket Prioridad { get; init; }
    public CategoriaTicket Categoria { get; init; }
    public int? ClienteId { get; init; }
    public string? LlamadaOrigenId { get; init; }
    public List<string>? Adjuntos { get; init; }
}
```

### **Response Example**
```csharp
public record TicketResponseDto
{
    public int Id { get; init; }
    public string Titulo { get; init; }
    public string Descripcion { get; init; }
    public EstadoTicket Estado { get; init; }
    public PrioridadTicket Prioridad { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaResolucion { get; init; }
    public TecnicoResponseDto? TecnicoAsignado { get; init; }
    public List<ComentarioResponseDto> Comentarios { get; init; }
    public TimeSpan TiempoTranscurrido { get; init; }
}
```

### **Realtime Example**
```csharp
public record TicketCreadoRealtimeDto
{
    public int TicketId { get; init; }
    public string Titulo { get; init; }
    public PrioridadTicket Prioridad { get; init; }
    public string ClienteNombre { get; init; }
    public DateTime FechaCreacion { get; init; }
    public bool RequiereAtencionInmediata { get; init; }
}
```

## 📊 **Integración con Métricas**

Los DTOs incluyen campos para tracking:
- **SLA Compliance**: Tiempos de respuesta y resolución
- **Customer Satisfaction**: Ratings y feedback
- **Performance**: Métricas por técnico y equipo
- **Trends**: Análisis de patrones y tipos de problemas

---
*Los DTOs de Soporte son el corazón de la comunicación en tiempo real del CRM, diseñados para mantener a todos los stakeholders informados y facilitar la resolución eficiente de problemas.*
