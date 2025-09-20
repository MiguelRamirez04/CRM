# 🔧 Common DTOs

## 🎯 **Propósito**
Esta carpeta contiene DTOs base y comunes que son reutilizados por todos los módulos del CRM. Establece contratos estándar para respuestas, errores y paginación.

## 📋 **Archivos y Responsabilidades**

### **BaseResponseDto.cs**
- **Qué hace**: Define la estructura estándar para todas las respuestas de la API
- **Por qué**: Consistencia en el formato de respuesta
- **Usado por**: Todos los ResponseDTOs de los módulos
- **Conecta con**: Controllers y Endpoints para respuestas uniformes

### **PaginatedDto.cs**
- **Qué hace**: Maneja respuestas paginadas con metadatos
- **Por qué**: Estándar para listas grandes de datos
- **Usado por**: Endpoints que retornan listas
- **Conecta con**: Queries de aplicación que implementan paginación

### **ErrorDto.cs**
- **Qué hace**: Estructura estandarizada para errores de API
- **Por qué**: Manejo consistente de errores para el cliente
- **Usado por**: Middleware de manejo de errores
- **Conecta con**: BuildingBlocks.Domain.Error y Result

### **ValidationErrorDto.cs**
- **Qué hace**: Errores específicos de validación con detalles de campos
- **Por qué**: Feedback detallado al usuario sobre datos inválidos
- **Usado por**: Validation behaviors y controllers
- **Conecta con**: FluentValidation y DataAnnotations

## 🔄 **Flujo de Conexión**

```
Application Result → Mapper → BaseResponseDto → HTTP Response
Domain Error → ErrorDto → Client
Validation Failures → ValidationErrorDto → Client
Paginated Query → PaginatedDto → Client
```

## 🎛️ **Ejemplo de Uso**

```csharp
// En un Controller
public async Task<BaseResponseDto<UsuarioResponseDto>> GetUsuario(int id)
{
    var result = await mediator.Send(new GetUsuarioQuery(id));
    
    return result.IsSuccess 
        ? BaseResponseDto<UsuarioResponseDto>.Success(mapper.Map(result.Value))
        : BaseResponseDto<UsuarioResponseDto>.Error(mapper.Map(result.Error));
}
```

## 🏗️ **Arquitectura**
Los DTOs comunes mantienen la consistencia sin violar los principios hexagonales:
- **No contienen lógica de negocio**
- **Son contratos de datos puros**
- **Facilitan el mapeo entre capas**
- **Mantienen la API predecible**

---
*Esta carpeta es el fundamento para una API consistente y fácil de consumir.*
