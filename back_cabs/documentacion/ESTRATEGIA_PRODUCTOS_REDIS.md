# ESTRATEGIA DE IMPLEMENTACIÓN - APIs PRODUCTOS CON REDIS CACHE

## 📋 RESUMEN EJECUTIVO

**Objetivo**: Implementar 2 endpoints GET para Productos con cache Redis robusto y funcional.

**Endpoints a Crear**:
1. `GET /api/Producto` - Obtener todos los productos activos
2. `GET /api/Producto/search?q={term}` - Buscar por código o nombre

**Stack Técnico**:
- .NET 8 + EF Core (ReadOnlyContext para queries)
- Redis via IDistributedCache (ya configurado)
- Cache-Aside Pattern
- JSON serialization para DTOs

---

## 🎯 ESTRATEGIA DE CACHÉ REDIS

### 1. Keys Pattern (Convención de Nomenclatura)
```
productos:all                    → Lista completa de productos activos
productos:search:{searchTerm}    → Resultados de búsqueda por término
```

**Razones**:
- Prefijo `productos:` para agrupar lógicamente
- Separador `:` estándar Redis
- Search key parametrizada con el término de búsqueda
- Fácil invalidación por patrón

### 2. TTL (Time To Live)
```
GetAll:  30 minutos  → Datos estables, actualización poco frecuente
Search:  60 minutos  → Búsquedas repetitivas de usuarios
```

**Razones**:
- Productos no cambian constantemente
- Balance entre freshness y performance
- Reducir carga en SQL Server
- GetAll más corto porque es más crítico estar actualizado

### 3. Cache-Aside Pattern (Lazy Loading)

**Flujo de Lectura**:
```
1. Controller recibe request
2. Service → CHECK cache con key
3. CACHE HIT? 
   ✅ YES → Retornar datos desde Redis (fast path)
   ❌ NO  → Query Database → Mapear a DTO → SET cache → Retornar datos
```

**Ventajas**:
- Solo cachea datos realmente usados
- Falla graciosamente si Redis no disponible
- Control total sobre qué y cuándo cachear

### 4. Estrategia de Invalidación

**Actual** (solo GET endpoints):
- Expiración automática por TTL
- No hay invalidación manual (no hay POST/PUT/DELETE aún)

**Futura** (cuando se agreguen POST/PUT/DELETE):
```csharp
// Al crear/actualizar/eliminar producto:
await _cacheService.RemoveAsync("productos:all");
// Opcional: Invalidar búsquedas específicas si es necesario
```

### 5. Manejo de Errores Redis

**Si Redis falla**:
- Aplicación continúa funcionando (graceful degradation)
- Logs de advertencia (no errores críticos)
- Datos se obtienen directamente de BD
- Usuario no nota la diferencia (solo menos performance)

**Implementación**:
```csharp
try 
{
    var cached = await _cacheService.GetAsync<T>(key);
    if (cached != null) return cached;
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Redis unavailable, fallback to DB");
}
// Continuar con query a BD
```

---

## 🏗️ ARQUITECTURA DE COMPONENTES

### Capa 1: DTO (Data Transfer Object)
**Archivo**: `ProductoResponseDto.cs`
```csharp
public class ProductoResponseDto
{
    public int Id { get; set; }
    public string? CodigoProducto { get; set; }      // Filtrable
    public string? NombreProducto { get; set; }      // Filtrable
    public int? TipoProducto { get; set; }
    public decimal? Precio1 { get; set; }
    public string? ClaveSat { get; set; }
    public int? StatusProducto { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaAlta { get; set; }
}
```

### Capa 2: Repository (Acceso a Datos)
**Archivos**: `IProductoRepository.cs` + `ProductoRepository.cs`
```csharp
public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllProductosAsync();
    Task<IEnumerable<Producto>> SearchProductosByFilterAsync(string searchTerm);
}
```

**Optimizaciones EF Core**:
- `.AsNoTracking()` - No tracking para queries read-only
- `.Where(p => p.Activo)` - Solo productos activos
- `EF.Functions.Like()` - Búsqueda insensible a mayúsculas
- Query combinada con OR para código y nombre

### Capa 3: Service (Lógica de Negocio + Cache)
**Archivos**: `IProductoService.cs` + `ProductoService.cs`

**Responsabilidades**:
- Implementar cache-aside pattern
- Mapeo Entidad → DTO
- Logging de operaciones
- Manejo de errores

**Dependencias**:
- `IProductoRepository` - Acceso a datos
- `ICacheService` - Operaciones Redis
- `ILogger` - Trazabilidad

### Capa 4: Controller (API REST)
**Archivo**: `ProductoController.cs`

**Endpoints**:
```http
GET /api/Producto
  → Authorization: Bearer {token}
  → Roles: ADMINISTRACION, RECEPCION
  → Response: 200 OK + ProductoResponseDto[]

GET /api/Producto/search?q={term}
  → Authorization: Bearer {token}
  → Roles: ADMINISTRACION, RECEPCION
  → Query: searchTerm (min 1 char)
  → Response: 200 OK + ProductoResponseDto[] (puede ser vacío)
```

---

## 📊 MÉTRICAS DE PERFORMANCE ESPERADAS

### Sin Cache (Baseline)
- GetAll: ~200-500ms (depende de cantidad de productos)
- Search: ~100-300ms (con índices en BD)

### Con Cache (Target)
- GetAll (cache HIT): ~10-50ms (95% mejora)
- Search (cache HIT): ~10-50ms (95% mejora)
- GetAll (cache MISS): ~250-550ms (query + serialize + set)

### Reducción de Carga BD
- Con 100 requests/minuto
- Cache hit ratio esperado: 80-90%
- Queries a BD: 10-20/min (vs 100/min sin cache)
- **Ahorro: 80-90% de carga en SQL Server**

---

## 🔄 FLUJO DE DESARROLLO (Orden de Implementación)

### Paso 1: DTO
✅ `ProductoResponseDto.cs` → DTOs/Response/

### Paso 2: Repository Interface
✅ `IProductoRepository.cs` → Interfaces/Legacy/

### Paso 3: Repository Implementation
✅ `ProductoRepository.cs` → repositories/Legacy/

### Paso 4: Service Interface
✅ `IProductoService.cs` → Interfaces/Legacy/

### Paso 5: Service Implementation (CON CACHE)
✅ `ProductoService.cs` → services/Legacy/

### Paso 6: Controller
✅ `ProductoController.cs` → controllers/

### Paso 7: DbContext Update
✅ Agregar DbSet en ReadOnlyContext y WriteContext

### Paso 8: Dependency Injection
✅ Registrar en Program.cs

### Paso 9: Testing
✅ Crear productos_pruebas.http
✅ Validar cache HIT/MISS en logs
✅ Performance benchmarks

---

## 🧪 PLAN DE PRUEBAS

### Caso 1: GetAll - Primera Llamada (CACHE MISS)
```http
GET /api/Producto
```
**Esperado**:
- Log: "Cache MISS para productos:all"
- Log: "Consultando BD para productos"
- Log: "Cache SET para productos:all con TTL 30min"
- Response: 200 + array de productos
- Tiempo: ~300-500ms

### Caso 2: GetAll - Segunda Llamada (CACHE HIT)
```http
GET /api/Producto
```
**Esperado**:
- Log: "Cache HIT para productos:all"
- Response: 200 + array de productos (mismos datos)
- Tiempo: ~10-50ms

### Caso 3: Search por Código
```http
GET /api/Producto/search?q=ABC123
```
**Esperado**:
- Cache MISS primera vez
- Búsqueda en BD por CodigoProducto LIKE '%ABC123%'
- Cache HIT segunda vez
- Response: productos que coincidan

### Caso 4: Search por Nombre
```http
GET /api/Producto/search?q=Motor
```
**Esperado**:
- Cache MISS primera vez
- Búsqueda en BD por NombreProducto LIKE '%Motor%'
- Cache SET con key "productos:search:motor"
- Response: productos que contengan "Motor"

### Caso 5: Search Sin Resultados
```http
GET /api/Producto/search?q=NOEXISTE999
```
**Esperado**:
- Response: 200 + array vacío []
- Cache SET con array vacío (válido)

### Caso 6: Validar Expiración TTL
```
1. GET /api/Producto → MISS + SET (TTL 30min)
2. Esperar 31 minutos
3. GET /api/Producto → MISS + SET (cache expiró)
```

---

## 🔒 CONSIDERACIONES DE SEGURIDAD

1. **Autorización**: Solo ADMINISTRACION y RECEPCION
2. **Input Sanitization**: Query parameter `q` validado
3. **Rate Limiting**: Considerar en producción
4. **Cache Poisoning**: Keys validadas y sanitizadas

---

## 📈 MÉTRICAS A MONITOREAR

### Logs Estructurados
```csharp
_logger.LogInformation("Cache {Status} para {CacheKey}. Tiempo: {ElapsedMs}ms", 
    status, key, elapsed);
```

### KPIs
- **Cache Hit Ratio**: (hits / total requests) * 100
- **Avg Response Time**: Para HIT vs MISS
- **Redis Availability**: % uptime
- **DB Query Reduction**: % menos queries

---

## 🚀 PRÓXIMOS PASOS (POST-IMPLEMENTACIÓN)

1. **Monitoreo**: Dashboard con métricas Redis
2. **Alertas**: Si cache hit ratio < 50%
3. **Optimización**: Ajustar TTL según uso real
4. **Write Operations**: Invalidación al POST/PUT/DELETE
5. **Cache Warming**: Pre-cargar datos críticos al startup

---

**Fecha**: 2025-11-12  
**Versión**: 1.0  
**Estado**: ✅ Estrategia Aprobada - Listo para Implementación
