# 📞 DTOs de Recepción

## 🎯 **Propósito**
Contiene los DTOs específicos para el módulo de Recepción del CRM. Maneja la primera línea de contacto con clientes: llamadas, mensajes, citas y derivaciones a otros módulos.

## 📁 **Estructura de Subcarpetas**

### `/Request/`
**Qué hace**: DTOs para solicitudes HTTP de recepción
**Archivos típicos**:
- `RegistrarLlamadaRequestDto.cs` - Registrar llamada entrante/saliente
- `CrearCitaRequestDto.cs` - Programar cita con cliente
- `DerivarConsultaRequestDto.cs` - Derivar consulta a Soporte
- `ActualizarContactoRequestDto.cs` - Actualizar datos de contacto
- `BuscarClienteRequestDto.cs` - Búsqueda de cliente

**Por qué**: Validación específica para operaciones de recepción
**Conecta con**: Controllers de Recepción → Commands del módulo

### `/Response/`
**Qué hace**: DTOs para respuestas HTTP de recepción
**Archivos típicos**:
- `LlamadaResponseDto.cs` - Detalles de llamada registrada
- `CitaResponseDto.cs` - Información de cita programada
- `ContactoResponseDto.cs` - Datos de contacto del cliente
- `HistorialContactoResponseDto.cs` - Historial de interacciones
- `EstadisticasRecepcionResponseDto.cs` - Métricas del módulo

**Por qué**: Información optimizada para el front-end de recepción
**Conecta con**: Queries del módulo → Controllers de Recepción

### `/Mapping/`
**Qué hace**: Mappers entre DTOs y Commands/Queries de Recepción
**Archivos típicos**:
- `RecepcionDtoMapper.cs` - Mapeos del módulo
- `LlamadaDtoProfile.cs` - AutoMapper profile para llamadas
- `CitaDtoProfile.cs` - AutoMapper profile para citas

**Por qué**: Transformación controlada entre capas
**Conecta con**: DTOs ↔ Recepcion.Application

## 🔄 **Flujo de Datos Específico**

### **Registro de Llamadas**
```
RegistrarLlamadaRequestDto → RegistrarLlamadaCommand → Domain → LlamadaResponseDto
```

### **Gestión de Citas**
```
CrearCitaRequestDto → CrearCitaCommand → Domain → CitaResponseDto → Notification
```

### **Derivación a Soporte**
```
DerivarConsultaRequestDto → DerivarConsultaCommand → Domain → SoporteTicketDto
```

## 🎛️ **Casos de Uso Cubiertos**

- **Llamadas**: Registro de llamadas entrantes y salientes
- **Citas**: Programación y gestión de reuniones
- **Contactos**: Gestión de información de contacto
- **Derivaciones**: Envío de consultas a Soporte o Administración
- **Búsquedas**: Localización rápida de clientes
- **Métricas**: Estadísticas de actividad de recepción

## 🔗 **Conexiones Clave**

### **Con Recepcion.Application**
- RequestDTOs → Commands (Registrar, Crear, Derivar)
- Queries → ResponseDTOs (Buscar, Listar, Obtener)

### **Con otros Módulos**
- **Soporte**: DTOs para derivar tickets
- **Administración**: DTOs para consultas de usuarios

### **Con SignalR (Realtime)**
- Notificaciones de nuevas llamadas
- Actualizaciones de estado de citas
- Alertas de derivaciones urgentes

## 🎪 **Características Especiales**

### **Integración Telefónica**
- DTOs preparados para integración con sistemas telefónicos
- Campos para caller ID, duración, grabaciones

### **Priorización**
- Niveles de urgencia en las derivaciones
- Clasificación de tipo de consulta

### **Trazabilidad**
- Seguimiento completo del flujo de la consulta
- Historial de todas las interacciones

## 🎪 **Ejemplos de DTOs**

### **Request Example**
```csharp
public record RegistrarLlamadaRequestDto
{
    public string NumeroTelefono { get; init; }
    public TipoLlamada Tipo { get; init; } // Entrante/Saliente
    public string Motivo { get; init; }
    public int? ClienteId { get; init; }
    public bool RequiereDerivacion { get; init; }
    public Prioridad Prioridad { get; init; }
}
```

### **Response Example**
```csharp
public record LlamadaResponseDto
{
    public int Id { get; init; }
    public string NumeroTelefono { get; init; }
    public string ClienteNombre { get; init; }
    public string Motivo { get; init; }
    public DateTime FechaHora { get; init; }
    public TimeSpan Duracion { get; init; }
    public EstadoLlamada Estado { get; init; }
    public string? TicketDerivadoId { get; init; }
}
```

## 🔄 **Integración con Workflow**

1. **Llamada Entrante** → RegistrarLlamadaRequestDto
2. **Identificación Cliente** → BuscarClienteRequestDto
3. **Evaluación** → ¿Requiere derivación?
4. **Derivación** → DerivarConsultaRequestDto → SoporteTicketDto
5. **Seguimiento** → HistorialContactoResponseDto

---
*Los DTOs de Recepción son el punto de entrada del CRM, diseñados para capturar eficientemente todas las interacciones con clientes y facilitar su procesamiento posterior.*
