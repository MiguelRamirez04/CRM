# 🏛️ DTOs de Administración

## 🎯 **Propósito**
Contiene los DTOs específicos para el módulo de Administración del CRM. Maneja la gestión de usuarios, roles, permisos y configuraciones del sistema.

## 📁 **Estructura de Subcarpetas**

### `/Request/`
**Qué hace**: DTOs para solicitudes HTTP de administración
**Archivos típicos**:
- `CreateUsuarioRequestDto.cs` - Crear nuevo usuario
- `UpdateUsuarioRequestDto.cs` - Actualizar usuario existente
- `CreateRolRequestDto.cs` - Crear rol
- `AsignarPermisosRequestDto.cs` - Asignar permisos a rol
- `ConfiguracionSistemaRequestDto.cs` - Configurar parámetros del sistema

**Por qué**: Validación específica para operaciones administrativas
**Conecta con**: Controllers de Administración → Commands del módulo

### `/Response/`
**Qué hace**: DTOs para respuestas HTTP de administración
**Archivos típicos**:
- `UsuarioResponseDto.cs` - Datos de usuario para respuesta
- `UsuarioListResponseDto.cs` - Lista de usuarios
- `RolResponseDto.cs` - Información de rol
- `PermisoResponseDto.cs` - Datos de permiso
- `ConfiguracionResponseDto.cs` - Configuración del sistema

**Por qué**: Control granular de qué datos administrativos exponer
**Conecta con**: Queries del módulo → Controllers de Administración

### `/Mapping/`
**Qué hace**: Mappers entre DTOs y Commands/Queries de Administración
**Archivos típicos**:
- `AdministracionDtoMapper.cs` - Mapeos del módulo
- `UsuarioDtoProfile.cs` - AutoMapper profile para usuarios
- `RolDtoProfile.cs` - AutoMapper profile para roles

**Por qué**: Separación clara entre capas, mapeo controlado
**Conecta con**: DTOs ↔ Administracion.Application

## 🔄 **Flujo de Datos Específico**

### **Gestión de Usuarios**
```
CreateUsuarioRequestDto → CreateUsuarioCommand → Domain → UsuarioResponseDto
```

### **Configuración del Sistema**
```
ConfiguracionRequestDto → UpdateConfigCommand → Domain → ConfiguracionResponseDto
```

### **Roles y Permisos**
```
AsignarPermisosRequestDto → AsignarPermisosCommand → Domain → RolResponseDto
```

## 🎛️ **Casos de Uso Cubiertos**

- **Gestión de Usuarios**: CRUD completo de usuarios del sistema
- **Roles y Permisos**: Asignación y gestión de autorización
- **Configuración**: Parámetros generales del CRM
- **Auditoría**: DTOs para logs y seguimiento de cambios
- **Reportes**: DTOs para estadísticas administrativas

## 🔗 **Conexiones Clave**

### **Con Administracion.Application**
- RequestDTOs → Commands (Create, Update, Delete)
- Queries → ResponseDTOs (Get, List, Search)

### **Con Controllers**
- AdministracionController consume estos DTOs
- Endpoints de configuración usan estos contratos

### **Con Seguridad**
- DTOs incluyen validación de permisos
- Campos sensibles protegidos en ResponseDTOs

## 🛡️ **Consideraciones de Seguridad**

1. **Datos Sensibles**: Passwords nunca en ResponseDTOs
2. **Autorización**: RequestDTOs validan permisos del usuario
3. **Auditoría**: Tracking de cambios administrativos
4. **Validación**: Reglas estrictas para datos críticos

## 🎪 **Ejemplos de DTOs**

### **Request Example**
```csharp
public record CreateUsuarioRequestDto
{
    public string Nombre { get; init; }
    public string Email { get; init; }
    public List<int> RolesIds { get; init; }
    public bool Activo { get; init; } = true;
}
```

### **Response Example**
```csharp
public record UsuarioResponseDto
{
    public int Id { get; init; }
    public string Nombre { get; init; }
    public string Email { get; init; }
    public List<RolResponseDto> Roles { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}
```

---
*Los DTOs de Administración son críticos para la seguridad y gestión del CRM, manteniendo un control estricto sobre quién puede hacer qué en el sistema.*
