# GitHub Copilot Instructions - Fullstack CABS CRM

## Contexto del Proyecto
Este es un CRM especializado para empresas de servicios técnicos que coordina 3 departamentos:
- **RECEPCION**: Gestión contable, asignación de órdenes, cotizaciones
- **SOPORTE**: Ejecución de servicios (presencial y remoto vía TeamViewer/AnyDesk)
- **ADMINISTRACION**: Reportes, análisis, métricas

## Arquitectura del Backend (.NET 8)
- **Patrón CQRS**: ReadOnlyContext (GET) y WriteContext (POST/PUT/DELETE)
- **DDD**: Separación por módulos (Auth, Recepcion, Soporte, etc.)
- **Esquemas SQL**: auth.*, ops.*, catalog.*
- **JWT Authentication**: Roles string (RECEPCION, SOPORTE, ADMINISTRACION)



## Estructura de Archivos OBLIGATORIA  
## Reglas de Código ESTRICTAS

### Modelos (Entidades)
SQL
- Usar `[Required]`, `[MaxLength]` según definición SQL
- Incluir `Id`, `CreadoEn`, `ActualizadoEn` estándar

### DTOs
- **RequestDto**: Solo campos que cliente PUEDE enviar (sin Id, fechas auto)
- **ResponseDto**: Todos los campos que cliente DEBE recibir
- Usar DataAnnotations para validación
- NO exponer campos sensibles

### Servicios
- Constructor con ReadOnlyContext, WriteContext, ILogger
- Métodos async/await SIEMPRE
- ReadContext para GET (con AsNoTracking())
- WriteContext para POST/PUT/DELETE
- Try-catch con logging
- Mapeo manual DTO ↔ Entidad

### Controladores
- Heredar ControllerBase
- `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`
- Códigos HTTP correctos: 200, 201, 204, 404, 400, 500
- ProducesResponseType para Swagger
- Try-catch con ILogger
- Delegar lógica al servicio

### Contextos EF
- Configurar entidades en OnModelCreating
- Indices para FK y campos de búsqueda frecuente
- Constraints de SQL Server
- DbSet<> para cada entidad

## Patrones de Nombrado
- **Archivos**: PascalCase (`OrdenTrabajo.cs`)
- **Clases**: PascalCase (`OrdenTrabajoService`)
- **Métodos**: PascalCase (`CrearAsync`)
- **Propiedades**: PascalCase (`CreadoEn`)
- **Campos privados**: _camelCase (`_readContext`)
- **Parámetros**: camelCase (`ordenId`)

## Base de Datos SQL Server
- Columnas: snake_case (`creado_en`, `cliente_id`)
- FKs: CONSTRAINT con nombres descriptivos
- Indices: Para FK y campos de búsqueda
- CHECK constraints para enums

## Middleware Pipeline (orden)
1. UseHttpsRedirection()
2. UseCors()
3. UseAuthentication()
4. UseAuthorization()
5. MapControllers()

## Validaciones
- DataAnnotations en DTOs para básicas
- FluentValidation para complejas (opcional)
- Reglas de negocio en servicios
- Try-catch en controladores


## Logging
- ILogger injection siempre
- LogInformation para operaciones exitosas
- LogError para excepciones
- Incluir datos relevantes (IDs, nombres)


## Respuestas HTTP Estándar
- GET: 200 Ok / 404 NotFound
- POST: 201 Created / 400 BadRequest
- PUT: 204 NoContent / 404 NotFound
- DELETE: 204 NoContent / 404 NotFound
- Errores: 500 Internal Server Error

## NO HACER NUNCA
- ❌ Lógica de negocio en controladores
- ❌ Acceso directo a DbContext desde controladores
- ❌ Strings mágicos (usar enums)
- ❌ Exponer entidades directamente en APIs
- ❌ Contraseñas en texto plano
- ❌ Logs con datos sensibles

## SIEMPRE HACER
- ✅ Usar servicios para lógica de negocio
- ✅ DTOs para entrada/salida de APIs
- ✅ Logging en operaciones críticas
- ✅ Try-catch con manejo de errores
- ✅ Async/await para operaciones BD
- ✅ Dependency injection
- ✅ Validaciones en múltiples capas


IMPORTANTE EL FLUJO DE DESARROLLO

MODEL > DTO > WRITECONTEXT > READCONTEXT > INTERFACE > REPOSITORY > SERVICE > CONTROLLER

ADEMAS DE REGISTRARLO EN PORGRAM.CS Y UTILIZAR LA CARPETA MIDDLEWARE


