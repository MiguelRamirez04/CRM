# 🚀 Guía Completa de Implementación de Redis en el Proyecto

## 📋 Índice
1. [¿Qué es Redis?](#qué-es-redis)
2. [Instalación](#instalación)
3. [Configuración en el Proyecto](#configuración-en-el-proyecto)
4. [¿Dónde Usar Redis?](#dónde-usar-redis)
5. [Patrones de Implementación](#patrones-de-implementación)
6. [Ejemplos Prácticos](#ejemplos-prácticos)
7. [Mejores Prácticas](#mejores-prácticas)
8. [Monitoreo y Debugging](#monitoreo-y-debugging)

---

## ¿Qué es Redis?

**Redis** (REmote DIctionary Server) es una **base de datos en memoria** ultra-rápida que funciona como **caché distribuida** para mejorar el rendimiento de aplicaciones.

### ✅ Beneficios:
- ⚡ **Velocidad**: 100,000+ operaciones por segundo
- 📊 **Reduce carga en BD**: Menos consultas a SQL Server
- 🌐 **Escalabilidad**: Funciona en ambientes distribuidos
- 💾 **Persistencia opcional**: Puede guardar datos en disco

### ❌ Cuándo NO usar Redis:
- ❌ Datos que **cambian constantemente** (transacciones en tiempo real)
- ❌ Datos **sensibles** sin encriptar
- ❌ **Datos que deben estar siempre consistentes** (mejor usar BD directamente)

---

## Instalación

### Windows (3 opciones):

#### Opción 1: Chocolatey (Recomendado para desarrollo)
```powershell
choco install redis-64
redis-server
```

#### Opción 2: Winget
```powershell
winget install Redis.Redis
```

#### Opción 3: Docker (Recomendado para producción)
```powershell
docker run -d -p 6379:6379 --name redis redis:latest
```

### Verificar instalación:
```powershell
redis-cli ping
# Respuesta esperada: PONG
```

---

## Configuración en el Proyecto

### 1️⃣ **appsettings.json** ✅ (YA ESTÁ CONFIGURADO)

```json
{
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379,abortConnect=false,connectTimeout=10000,syncTimeout=10000"
  }
}
```

**Explicación:**
- `localhost:6379` → Dirección de Redis
- `abortConnect=false` → No aborta si falla la conexión inicial
- `connectTimeout=10000` → Timeout de 10 segundos para conectar
- `syncTimeout=10000` → Timeout de 10 segundos para operaciones síncronas

### 2️⃣ **Program.cs** ✅ (YA ESTÁ CONFIGURADO)

```csharp
// ✅ Configuración de caché distribuido (para IDistributedCache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "CABS_"; // Prefijo para todas las claves
});

// ✅ ConnectionMultiplexer (para operaciones avanzadas)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configurationOptions = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("RedisConnection")!, true);
    return ConnectionMultiplexer.Connect(configurationOptions);
});

// ✅ Servicio de caché personalizado
builder.Services.AddScoped<ICacheService, CacheService>();
```

### 3️⃣ **CacheService.cs** ✅ (YA ESTÁ IMPLEMENTADO)

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveAsync(string key);
}
```

---

## ¿Dónde Usar Redis?

### ✅ **SÍ usar Redis:**

#### 1. **Catálogos y Datos de Lookup**
```csharp
// ✅ Lista de vehículos (cambia poco, se consulta mucho)
// ✅ Tipos de vehículos, estados, modalidades
// ✅ Usuarios activos
```

#### 2. **Resultados de Consultas Costosas**
```csharp
// ✅ Dashboards con agregaciones complejas
// ✅ Reportes estadísticos
// ✅ Búsquedas con múltiples JOINs
```

#### 3. **Datos de Sesión/Usuario**
```csharp
// ✅ Permisos del usuario logueado
// ✅ Configuración de usuario
// ✅ Tokens de sesión
```

### ❌ **NO usar Redis:**

```csharp
// ❌ Órdenes de trabajo (cambian constantemente)
// ❌ Cotizaciones recién creadas
// ❌ Ejecuciones en tiempo real
// ❌ Datos transaccionales críticos
```

---

## Patrones de Implementación

### 🔹 **Cache-Aside Pattern** (Más común)

```csharp
public async Task<IEnumerable<VehiculoResponseDto>> ObtenerTodosAsync()
{
    // 1️⃣ Intentar obtener del caché
    var cached = await _cacheService.GetAsync<IEnumerable<VehiculoResponseDto>>("vehiculos:all");
    if (cached != null)
    {
        return cached; // ✅ Cache HIT
    }

    // 2️⃣ Si no está en caché, consultar BD
    var vehiculos = await _vehiculoRepository.GetAllAsync();
    var dtos = vehiculos.Select(MapToResponseDto).ToList();

    // 3️⃣ Guardar en caché para futuras consultas
    await _cacheService.SetAsync("vehiculos:all", dtos, TimeSpan.FromMinutes(30));

    return dtos; // ❌ Cache MISS
}
```

### 🔹 **Write-Through Pattern** (Invalidación de caché)

```csharp
public async Task<VehiculoResponseDto> CrearAsync(VehiculoRequestDto request)
{
    var vehiculo = new Vehiculo { /* ... */ };
    var vehiculoCreado = await _vehiculoRepository.CreateAsync(vehiculo);

    // ✅ Invalidar caché al escribir
    await _cacheService.RemoveAsync("vehiculos:all");

    return MapToResponseDto(vehiculoCreado);
}
```

---

## Ejemplos Prácticos

### Ejemplo 1: **VehiculosService** (Implementado)

```csharp
public class VehiculosService
{
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_ALL = "vehiculos:all";
    private const string CACHE_KEY_PREFIX = "vehiculos:id:";
    private const int CACHE_EXPIRATION_MINUTES = 30;

    // ✅ GET All con caché
    public async Task<IEnumerable<VehiculoResponseDto>> ObtenerTodosAsync()
    {
        var cached = await _cacheService.GetAsync<IEnumerable<VehiculoResponseDto>>(CACHE_KEY_ALL);
        if (cached != null) return cached;

        var vehiculos = await _vehiculoRepository.GetAllAsync();
        var dtos = vehiculos.Select(MapToResponseDto).ToList();

        await _cacheService.SetAsync(CACHE_KEY_ALL, dtos, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
        return dtos;
    }

    // ✅ GET ById con caché
    public async Task<VehiculoResponseDto?> ObtenerPorIdAsync(int id)
    {
        string cacheKey = $"{CACHE_KEY_PREFIX}{id}";
        var cached = await _cacheService.GetAsync<VehiculoResponseDto>(cacheKey);
        if (cached != null) return cached;

        var vehiculo = await _vehiculoRepository.GetByIdAsync(id);
        if (vehiculo == null) return null;

        var dto = MapToResponseDto(vehiculo);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
        return dto;
    }

    // ✅ CREATE con invalidación
    public async Task<VehiculoResponseDto> CrearAsync(VehiculoRequestDto request)
    {
        var vehiculoCreado = await _vehiculoRepository.CreateAsync(/* ... */);
        await _cacheService.RemoveAsync(CACHE_KEY_ALL); // ✅ Invalidar listado
        return MapToResponseDto(vehiculoCreado);
    }

    // ✅ UPDATE con invalidación
    public async Task<VehiculoResponseDto?> ActualizarAsync(int id, VehiculoRequestDto request)
    {
        var vehiculoActualizado = await _vehiculoRepository.UpdateAsync(/* ... */);
        await InvalidarCacheVehiculo(id); // ✅ Invalidar ambos
        return MapToResponseDto(vehiculoActualizado);
    }

    // ✅ DELETE con invalidación
    public async Task<bool> EliminarAsync(int id)
    {
        var eliminado = await _vehiculoRepository.DeleteAsync(id);
        if (eliminado) await InvalidarCacheVehiculo(id);
        return eliminado;
    }

    private async Task InvalidarCacheVehiculo(int id)
    {
        await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");
        await _cacheService.RemoveAsync(CACHE_KEY_ALL);
    }
}
```

### Ejemplo 2: **Dashboard con Estadísticas**

```csharp
public class DashRecepcionService
{
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_STATS = "dashboard:stats";
    private const int CACHE_EXPIRATION_MINUTES = 10; // Stats cambian más rápido

    public async Task<EstadisticasDto> ObtenerEstadisticasAsync()
    {
        // ✅ Intentar obtener del caché
        var cached = await _cacheService.GetAsync<EstadisticasDto>(CACHE_KEY_STATS);
        if (cached != null)
        {
            _logger.LogDebug("Estadísticas obtenidas desde caché");
            return cached;
        }

        // ❌ Cache miss: Calcular estadísticas (consulta costosa)
        var stats = new EstadisticasDto
        {
            TotalOrdenes = await _context.OrdenesTrabajoAsync.CountAsync(),
            OrdenesPendientes = await _context.OrdenesTrabajoAsync
                .CountAsync(o => o.Estado == "CAPTURADA"),
            // ... más cálculos complejos
        };

        // ✅ Guardar en caché
        await _cacheService.SetAsync(CACHE_KEY_STATS, stats, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
        _logger.LogDebug("Estadísticas guardadas en caché por {Minutes} minutos", CACHE_EXPIRATION_MINUTES);

        return stats;
    }
}
```

### Ejemplo 3: **Permisos de Usuario**

```csharp
public class UsuarioAuthService
{
    private const string CACHE_KEY_USER_PREFIX = "user:permissions:";
    private const int CACHE_EXPIRATION_MINUTES = 60; // Los roles cambian poco

    public async Task<IEnumerable<string>> ObtenerPermisosAsync(int usuarioId)
    {
        string cacheKey = $"{CACHE_KEY_USER_PREFIX}{usuarioId}";

        // ✅ Intentar obtener del caché
        var cached = await _cacheService.GetAsync<IEnumerable<string>>(cacheKey);
        if (cached != null) return cached;

        // ❌ Cache miss: Consultar permisos
        var usuario = await _repository.GetByIdAsync(usuarioId);
        var permisos = ObtenerPermisosPorRol(usuario.Rol);

        // ✅ Guardar en caché
        await _cacheService.SetAsync(cacheKey, permisos, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));

        return permisos;
    }
}
```

---

## Mejores Prácticas

### 1️⃣ **Nomenclatura de Claves**

```csharp
// ✅ BUENO: Claves descriptivas y consistentes
"vehiculos:all"
"vehiculos:id:123"
"user:permissions:45"
"dashboard:stats"

// ❌ MALO: Claves inconsistentes
"AllVehicles"
"veh_123"
"userPerms45"
```

### 2️⃣ **Tiempos de Expiración**

```csharp
// ✅ Catálogos estables (rara vez cambian)
private const int CACHE_EXPIRATION_MINUTES = 30;

// ✅ Datos dinámicos (cambian frecuentemente)
private const int CACHE_EXPIRATION_MINUTES = 5;

// ✅ Datos casi estáticos (tipos, estados)
private const int CACHE_EXPIRATION_MINUTES = 120; // 2 horas
```

### 3️⃣ **Invalidación Consistente**

```csharp
// ✅ BUENO: Invalidar al escribir
public async Task CrearAsync(...)
{
    await _repository.CreateAsync(...);
    await _cacheService.RemoveAsync("vehiculos:all"); // ✅ Invalidar
}

// ❌ MALO: No invalidar (datos obsoletos en caché)
public async Task CrearAsync(...)
{
    await _repository.CreateAsync(...);
    // ❌ Caché queda desactualizado
}
```

### 4️⃣ **Logging**

```csharp
// ✅ BUENO: Loguear para debugging
var cached = await _cacheService.GetAsync<T>(key);
if (cached != null)
{
    _logger.LogDebug("Datos obtenidos desde caché: {Key}", key);
    return cached;
}
_logger.LogDebug("Cache miss: Consultando BD para {Key}", key);
```

---

## Monitoreo y Debugging

### 🔍 **Verificar Estado de Redis**

```powershell
# Conectar a Redis CLI
redis-cli

# Ver todas las claves
KEYS *

# Ver valor de una clave
GET "CABS_vehiculos:all"

# Ver tiempo restante de expiración (en segundos)
TTL "CABS_vehiculos:all"

# Ver estadísticas de Redis
INFO stats

# Limpiar toda la caché (¡CUIDADO!)
FLUSHALL
```

### 📊 **Métricas de Rendimiento**

```csharp
public async Task<T> GetWithMetricsAsync<T>(string key)
{
    var stopwatch = Stopwatch.StartNew();
    var result = await _cacheService.GetAsync<T>(key);
    stopwatch.Stop();

    if (result != null)
    {
        _logger.LogInformation("Cache HIT: {Key} en {Ms}ms", key, stopwatch.ElapsedMilliseconds);
    }
    else
    {
        _logger.LogInformation("Cache MISS: {Key} en {Ms}ms", key, stopwatch.ElapsedMilliseconds);
    }

    return result;
}
```

### 🐛 **Debugging Tips**

```csharp
// ✅ Deshabilitar caché temporalmente para pruebas
public async Task<IEnumerable<T>> ObtenerTodosAsync()
{
    #if DEBUG
    return await _repository.GetAllAsync(); // Sin caché en desarrollo
    #endif

    var cached = await _cacheService.GetAsync<IEnumerable<T>>(key);
    // ...
}
```

---

## 🎯 Resumen

### ✅ **Tu configuración actual está CORRECTA**
- `appsettings.json` ✅
- `Program.cs` ✅
- `CacheService.cs` ✅
- `FilesController.cs` ya usa Redis ✅
- `VehiculosService.cs` ahora usa Redis ✅

### 📝 **¿Dónde implementar Redis ahora?**

1. ✅ **VehiculosService** → Implementado
2. ⏳ **CotizacionService** → Pendiente (solo cachear listados por estado)
3. ⏳ **DashRecepcionService** → Pendiente (estadísticas)
4. ⏳ **UsuarioAuthService** → Pendiente (permisos de usuario)
5. ❌ **OrdenTrabajoService** → NO (datos transaccionales)
6. ❌ **EjecucionOrdenService** → NO (datos en tiempo real)

### 🚫 **NO uses Redis en:**
- Órdenes de trabajo (cambian constantemente)
- Ejecuciones en curso
- Transacciones críticas
- Datos que deben estar siempre 100% consistentes

---

## 🔗 Referencias
- [Redis Documentation](https://redis.io/docs/)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [Microsoft Caching Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching)
