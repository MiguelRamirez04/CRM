# ⚠️ Desafío: Testing de UsuarioAuthService

## 🔴 Problema Actual

Al intentar crear tests unitarios para `UsuarioAuthService`, encontramos un **problema arquitectónico**:

```csharp
public UsuarioAuthService(
    IUsuarioAuthRepository usuarioRepository,  // ✅ Mockeable
    WriteContext writeContext,                 // ❌ Clase concreta
    ReadOnlyContext readContext,               // ❌ Clase concreta
    ServicioJwt servicioJwt,                   // ❌ Clase concreta
    ILogger<UsuarioAuthService> logger,        // ✅ Mockeable
    UsuarioRegistroValidator validator)        // ❌ Clase concreta
{
    // ⚠️ Todas las dependencias tienen validación != null
    _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
    // ... etc
}
```

## 💥 Por Qué Falla

### Problema con Moq
```csharp
// ❌ ESTO FALLA:
var mockJwt = new Mock<ServicioJwt>();
// Error: Can not instantiate proxy of class: ServicioJwt. 
// Could not find a parameterless constructor.
```

**Moq** usa `Castle.DynamicProxy` que requiere **constructores sin parámetros** para crear proxies de clases concretas.

### Problema con null
```csharp
// ❌ ESTO TAMBIÉN FALLA:
var service = new UsuarioAuthService(
    mockRepository.Object,
    null!,  // ArgumentNullException!
    null!,  // ArgumentNullException!
    null!,  // ArgumentNullException!
    mockLogger.Object,
    null!   // ArgumentNullException!
);
```

Las validaciones `?? throw new ArgumentNullException()` impiden pasar `null`.

## ✅ Soluciones Recomendadas

### Opción 1: Refactorizar a Interfaces (RECOMENDADO)

```csharp
// 1. Crear IServicioJwt
public interface IServicioJwt
{
    string GenerarTokenAcceso(List<Claim> claims, TimeSpan duracion);
}

// 2. ServicioJwt implementa la interfaz
public class ServicioJwt : IServicioJwt
{
    // ... implementación existente
}

// 3. UsuarioAuthService recibe la interfaz
public UsuarioAuthService(
    IUsuarioAuthRepository usuarioRepository,
    WriteContext writeContext,
    ReadOnlyContext readContext,
    IServicioJwt servicioJwt,  // ✅ Ahora es mockeable
    ILogger<UsuarioAuthService> logger,
    IValidator<UsuarioRegistroRequestDto> validator  // ✅ FluentValidation ya tiene IValidator<T>
)
```

**Ventajas:**
- ✅ Tests unitarios rápidos y aislados
- ✅ Mejor separación de responsabilidades
- ✅ Más fácil de mantener
- ✅ Sigue SOLID principles

### Opción 2: Tests de Integración

En lugar de tests unitarios, usar `WebApplicationFactory` para tests de integración:

```csharp
public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task RegistrarUsuario_ConDatosValidos_DebeRetornar200YToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new UsuarioRegistroRequestDto { /* ... */ };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/auth/registro", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UsuarioRegistroResponseDto>();
        result.Token.Should().NotBeNullOrEmpty();
    }
}
```

**Ventajas:**
- ✅ Prueba el flujo completo (Controller → Service → Repository → DB)
- ✅ Más realista
- ✅ No requiere refactorización

**Desventajas:**
- ❌ Más lento (levanta servidor, base de datos)
- ❌ Menos aislado (dificulta detectar dónde falla)

### Opción 3: Partial Mocking con NSubstitute

Usar `NSubstitute` en lugar de Moq permite "partial substitutes":

```bash
dotnet add package NSubstitute
```

```csharp
var jwtService = Substitute.ForPartsOf<ServicioJwt>(/* args */);
jwtService.GenerarTokenAcceso(Arg.Any<List<Claim>>(), Arg.Any<TimeSpan>())
    .Returns("fake-token");
```

**Ventajas:**
- ✅ No requiere refactorización

**Desventajas:**
- ❌ Requiere agregar nueva librería
- ❌ Más complejo de configurar

## 📊 Comparación

| Aspecto | Interfaces | Integración | NSubstitute |
|---------|-----------|-------------|-------------|
| **Velocidad** | ⚡⚡⚡ Muy rápido | 🐢 Lento | ⚡⚡⚡ Muy rápido |
| **Aislamiento** | ✅ Total | ❌ Parcial | ✅ Total |
| **Mantenibilidad** | ✅✅ Excelente | ✅ Buena | ❌ Compleja |
| **Refactorización** | ❌ Requiere cambios | ✅ No requiere | ✅ No requiere |
| **Best Practices** | ✅✅ SOLID | ✅ Realista | ⚠️ Workaround |

## 🎯 Recomendación Final

Para **aprendizaje** y **buenas prácticas** a largo plazo:

1. **Corto plazo:** Crear tests de integración para Auth (son necesarios de todos modos)
2. **Mediano plazo:** Refactorizar a interfaces (`IServicioJwt`, usar `IValidator<T>`)
3. **Largo plazo:** Migrar WriteContext/ReadOnlyContext a través del Repository Pattern (ya iniciado)

## 📝 Estado Actual

### Tests Completados ✅
- `CotizacionServiceTests` - 12 tests passing
- `VehiculosServiceTests` - 17 tests passing
- **Total: 29 tests passing**

### Tests Pendientes ⏳
- `UsuarioAuthService` - Bloqueado por arquitectura
- `ClientesCompletosService` - Pendiente
- `OrdenTrabajoService` - Pendiente

## 🚀 Próximos Pasos

1. Continuar con **ClientesCompletosService** (similar a VehiculosService)
2. Agregar tests de **validators** (UsuarioRegistroValidator, etc.)
3. Crear **tests de integración** para Auth endpoints
4. Refactorizar `ServicioJwt` a interfaz (ejercicio educativo SOLID)

---

💡 **Nota Educativa:** Este problema es **muy común** en código legacy. La solución (interfaces) 
es un ejemplo perfecto de por qué seguir **Dependency Inversion Principle** (SOLID) desde el inicio.
