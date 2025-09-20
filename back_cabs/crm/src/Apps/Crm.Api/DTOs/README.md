# 📦 DTOs (Data Transfer Objects)

## 🎯 **Propósito**
Los DTOs son objetos simples que transportan datos entre la capa de presentación (API) y las capas de aplicación. Actúan como un contrato de datos que mantiene el desacoplamiento entre capas.

## 🏗️ **Ubicación en la Arquitectura Hexagonal**
```
┌─────────────────┐
│   Controllers   │ ← DTOs entran aquí desde HTTP requests
│   & Endpoints   │ ← DTOs salen aquí hacia HTTP responses
└─────────────────┘
         ↕ 📦 DTOs
┌─────────────────┐
│   Application   │ ← DTOs se mapean a Commands/Queries
│     Layer       │ ← Results se mapean a DTOs
└─────────────────┘
```

## 📁 **Estructura de Carpetas**

### `/Common/`
- **BaseResponseDto.cs**: Estructura base para todas las respuestas
- **PaginatedDto.cs**: DTO para respuestas paginadas
- **ErrorDto.cs**: DTO para manejo de errores
- **ValidationErrorDto.cs**: DTO específico para errores de validación

### `/Administracion/`
- **Request/**: DTOs para requests HTTP (Create, Update, etc.)
- **Response/**: DTOs para responses HTTP (Get, List, etc.)
- **Mapping/**: Mappers entre DTOs y Commands/Queries

### `/Recepcion/`
- **Request/**: DTOs para requests de recepción
- **Response/**: DTOs para responses de recepción
- **Mapping/**: Mappers específicos del módulo

### `/Soporte/`
- **Request/**: DTOs para requests de soporte
- **Response/**: DTOs para responses de soporte
- **Realtime/**: DTOs específicos para SignalR
- **Mapping/**: Mappers específicos del módulo

## 🔄 **Flujo de Datos con DTOs**

### 📥 **Request Flow (Entrada)**
```
HTTP Request → RequestDto → Mapper → Command/Query → Application Layer
```

### 📤 **Response Flow (Salida)**
```
Application Layer → Result → Mapper → ResponseDto → HTTP Response
```

### ⚡ **Realtime Flow (SignalR)**
```
Domain Event → Event Handler → Mapper → RealtimeDto → SignalR Hub → Client
```

## 🎛️ **Tipos de DTOs**

### **Request DTOs**
- Validan datos de entrada
- Contienen solo los campos necesarios para la operación
- Incluyen atributos de validación (DataAnnotations/FluentValidation)

### **Response DTOs**
- Exponen solo los datos que el cliente necesita
- No contienen información sensible
- Optimizados para el formato de salida

### **Realtime DTOs**
- Ligeros y optimizados para SignalR
- Contienen solo datos esenciales para notificaciones
- Serializables para múltiples clientes

## ✅ **Beneficios**

1. **Desacoplamiento**: La API no depende de las entidades de dominio
2. **Seguridad**: Control granular de qué datos exponer
3. **Versionado**: Múltiples versiones de DTOs sin afectar el dominio
4. **Validación**: Validación específica para la capa de presentación
5. **Performance**: DTOs optimizados para serialización

## 🔗 **Conexiones con otras Capas**

### **Con Controllers/Endpoints**
- Controllers reciben RequestDTOs
- Controllers retornan ResponseDTOs

### **Con Application Layer**
- Mappers convierten DTOs a Commands/Queries
- Mappers convierten Results a ResponseDTOs

### **Con SignalR Hubs**
- Hubs envían RealtimeDTOs a clientes conectados
- Event handlers mapean domain events a RealtimeDTOs

## 📝 **Convenciones de Nomenclatura**

- **Request**: `{Operacion}{Entidad}RequestDto` (ej: `CreateUsuarioRequestDto`)
- **Response**: `{Entidad}ResponseDto` (ej: `UsuarioResponseDto`)
- **List**: `{Entidad}ListResponseDto` (ej: `UsuarioListResponseDto`)
- **Realtime**: `{Evento}RealtimeDto` (ej: `TicketCreadoRealtimeDto`)

## 🛡️ **Mejores Prácticas**

1. **Inmutabilidad**: DTOs deben ser inmutables cuando sea posible
2. **Validación**: Usar atributos de validación en RequestDTOs
3. **Mapeo**: Nunca mapear automáticamente sin validación
4. **Documentación**: Documentar cada DTO con XML comments
5. **Versionado**: Versionar DTOs para mantener compatibilidad

---
*Los DTOs son el puente entre el mundo exterior (HTTP/SignalR) y la lógica de negocio, manteniendo la pureza de la arquitectura hexagonal.*
