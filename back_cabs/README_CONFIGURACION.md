# CRM - Clean Architecture

## 📁 Estructura Final del Proyecto

```
back_cabs/
├── CRM/                          # Nueva estructura Clean Architecture
│   ├── config/                   # Configuraciones centralizadas
│   │   ├── AuthenticationConfiguration.cs
│   │   ├── DatabaseConfiguration.cs
│   │   ├── HealthChecksConfiguration.cs
│   │   ├── MediatRConfiguration.cs
│   │   ├── SwaggerConfiguration.cs
│   │   └── ValidationConfiguration.cs
│   └── contexts/                 # Contextos de Entity Framework
│       ├── ReadOnlyContext.cs    # Para consultas (GET)
│       └── WriteContext.cs       # Para modificaciones (POST/PUT/DELETE)
├── Controllers/                  # Controladores (vacío - listo para usar)
├── Program.cs                    # Configuración principal actualizada
├── appsettings.json             # Configuraciones de producción
├── appsettings.Development.json # Configuraciones de desarrollo
└── back_cabs.csproj            # Proyecto con todas las librerías
```

## 🚀 Librerías Instaladas y Configuradas

✅ **Autenticación JWT**
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.8)
- System.IdentityModel.Tokens.Jwt (7.5.1)

✅ **Entity Framework + SQL Server**
- Microsoft.EntityFrameworkCore (8.0.8)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.8)
- Microsoft.EntityFrameworkCore.Design (8.0.8)
- Microsoft.Data.SqlClient (5.2.0)

✅ **Validación y CQRS**
- FluentValidation (11.9.0)
- FluentValidation.DependencyInjectionExtensions (11.9.0)
- MediatR.Extensions.Microsoft.DependencyInjection (11.1.0)

✅ **Logging Estructurado**
- Serilog.AspNetCore (8.0.1)
- Serilog.Sinks.Console (5.0.1)
- Serilog.Sinks.File (5.0.0)

✅ **Swagger/OpenAPI**
- Swashbuckle.AspNetCore (6.6.2)

✅ **Health Checks**
- AspNetCore.HealthChecks.SqlServer (8.0.1)
- AspNetCore.HealthChecks.UI.Client (8.0.1)

## 🔧 Configuraciones Listas

### Bases de Datos
- **Desarrollo**: `CRM_Database_Dev` (LocalDB)
- **Producción**: `CRM_Database` (LocalDB)
- Contextos separados para lectura y escritura

### JWT
- Configurado para desarrollo y producción
- Tokens con expiración configurable
- Esquema Bearer en Swagger

### Logging
- Serilog configurado para consola y archivos
- Logs rotativos por día (30 días de retención)
- Diferentes niveles para desarrollo y producción

### Health Checks
- `/health` - Estado general
- `/health/ready` - Listo para recibir tráfico
- `/health/live` - Estado de vida del servicio

## 📋 Siguiente Pasos

1. **Definir Modelos**: Crear entidades en `CRM/models/`
2. **Agregar DTOs**: Crear DTOs en `CRM/DTOs/`
3. **Implementar Servicios**: Lógica de negocio en `CRM/services/`
4. **Crear Controladores**: APIs en `Controllers/`
5. **Ejecutar Migraciones**: `dotnet ef migrations add Initial`

## 🎯 Estructura Lista Para

- ✅ Desarrollo con arquitectura limpia y familiar
- ✅ Separación de responsabilidades (CQRS básico)
- ✅ Autenticación y autorización
- ✅ Logging estructurado
- ✅ Documentación automática (Swagger)
- ✅ Monitoreo de salud
- ✅ Validaciones automáticas

¡El proyecto está configurado y listo para que los becarios empiecen a desarrollar! 🚀