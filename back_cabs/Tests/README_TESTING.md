# 🧪 Guía de Testing - Backend CABS

## 📚 ¿Qué es Testing?

El testing es escribir código que **verifica automáticamente** que tu código de producción funciona correctamente. Es como tener un robot que revisa tu trabajo constantemente.

### ✅ Beneficios del Testing

1. **Confianza**: Sabes que tu código funciona
2. **Refactoring seguro**: Puedes cambiar código sin romper nada
3. **Documentación**: Los tests muestran CÓMO usar tu código
4. **Menos bugs**: Detectas errores antes de producción
5. **Desarrollo más rápido**: Ahorras tiempo debuggeando manualmente

---

## 🏗️ Tipos de Tests

### 1️⃣ **Unit Tests (Tests Unitarios)**
- Prueban **UNA** función/método aislado
- Usan **mocks** para simular dependencias
- Son **rápidos** (milisegundos)
- **Ejemplo**: Probar que `CalcularTotal()` suma correctamente

### 2️⃣ **Integration Tests (Tests de Integración)**
- Prueban **múltiples componentes** juntos
- Usan BD en memoria o de prueba
- Son **más lentos** (segundos)
- **Ejemplo**: Probar todo el flujo de crear una cotización desde el controller hasta la BD

### 3️⃣ **End-to-End Tests (E2E)**
- Prueban **todo el sistema** completo
- Simulan acciones de usuario real
- Son **lentos** (minutos)
- **Ejemplo**: Abrir el navegador, hacer login, crear cotización, verificar resultado

---

## 📦 Librerías Instaladas

### 🧪 **xUnit** - Framework de testing
```bash
# Es el motor que ejecuta los tests
# Similar a NUnit o MSTest, pero más moderno
```

### 🎭 **Moq** - Framework de mocking
```bash
# Permite simular dependencias (repositories, servicios, etc.)
# Para aislar el código que estamos probando
```

### ✨ **FluentAssertions** - Assertions legibles
```bash
# Hace las verificaciones más fáciles de leer
# Compara:
# Assert.Equal(5, resultado)  ❌ Difícil de leer
# resultado.Should().Be(5)    ✅ Mucho más claro
```

### 🗄️ **EntityFrameworkCore.InMemory** - BD en memoria
```bash
# Crea una base de datos temporal en RAM
# Para tests de integración sin tocar la BD real
```

### 🌐 **Microsoft.AspNetCore.Mvc.Testing** - Tests de APIs
```bash
# Permite probar controllers completos
# Levanta un servidor HTTP de prueba
```

---

## 🎯 Patrón AAA (Arrange-Act-Assert)

**TODOS** los tests siguen este patrón:

```csharp
[Fact]
public async Task NombreDelTest()
{
    // 📋 Arrange: Preparar datos y configurar mocks
    var mockRepo = new Mock<IRepository>();
    var service = new Service(mockRepo.Object);
    var datoPrueba = new Dato { Id = 1, Nombre = "Test" };
    
    mockRepo.Setup(r => r.GetAsync(1))
            .ReturnsAsync(datoPrueba);
    
    // 🎬 Act: Ejecutar el método que estamos probando
    var resultado = await service.ObtenerAsync(1);
    
    // ✅ Assert: Verificar que el resultado es correcto
    resultado.Should().NotBeNull();
    resultado.Nombre.Should().Be("Test");
    mockRepo.Verify(r => r.GetAsync(1), Times.Once);
}
```

---

## 🔧 Comandos para Ejecutar Tests

### ▶️ Ejecutar TODOS los tests
```powershell
dotnet test
```

### ▶️ Ejecutar tests con salida detallada
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### ▶️ Ejecutar tests de un archivo específico
```powershell
dotnet test --filter "FullyQualifiedName~CotizacionServiceTests"
```

### ▶️ Ejecutar UN solo test
```powershell
dotnet test --filter "FullyQualifiedName~ObtenerTodasAsync_CuandoHayCotizaciones_DebeRetornarListaNoVacia"
```

### ▶️ Ejecutar tests con cobertura
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### ▶️ Ver resultados en tiempo real (watch mode)
```powershell
dotnet watch test
```

---

## 📖 Anatomía de un Test

### Atributos importantes

```csharp
[Fact]  // ← Test simple, sin parámetros
public async Task MiTest() { }

[Theory]  // ← Test con múltiples datos
[InlineData(100, 16, 116)]  // ← Diferentes combinaciones
[InlineData(200, 32, 232)]
[InlineData(500, 80, 580)]
public async Task MiTestConDatos(decimal sub, decimal imp, decimal total) { }
```

### Nomenclatura de tests

```
NombreDelMetodo_Condicion_ResultadoEsperado

Ejemplos:
✅ ObtenerPorIdAsync_ConIdExistente_DebeRetornarCotizacion
✅ CrearAsync_ConDatosValidos_DebeCrearYRetornarCotizacion
✅ ActualizarAsync_ConIdInexistente_DebeRetornarNull
```

---

## 🎭 Mocking con Moq

### ¿Por qué mockeamos?

Imagina que pruebas `CotizacionService` que depende de `ICotizacionRepository`:

```csharp
public class CotizacionService
{
    private readonly ICotizacionRepository _repo;
    
    public CotizacionService(ICotizacionRepository repo)
    {
        _repo = repo;
    }
    
    public async Task<Cotizacion> ObtenerPorIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);  // ← Llamada al repositorio
    }
}
```

**Problema**: No queremos tocar la BD real en los tests unitarios.

**Solución**: Usamos un **mock** que simula el repositorio:

```csharp
// 1. Crear el mock
var mockRepo = new Mock<ICotizacionRepository>();

// 2. Configurar qué debe retornar
mockRepo.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new Cotizacion { Id = 1, Cliente = "Test" });

// 3. Usar el mock en el servicio
var service = new CotizacionService(mockRepo.Object);

// 4. Llamar al método
var resultado = await service.ObtenerPorIdAsync(1);

// 5. Verificar que llamó al repositorio
mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
```

### Ejemplos de Setup

```csharp
// Retornar un valor
mock.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(cotizacion);

// Retornar null
mock.Setup(r => r.GetByIdAsync(999))
    .ReturnsAsync((Cotizacion)null!);

// Retornar true/false
mock.Setup(r => r.DeleteAsync(1))
    .ReturnsAsync(true);

// Retornar lista
mock.Setup(r => r.GetAllAsync())
    .ReturnsAsync(new List<Cotizacion> { cot1, cot2 });

// Con ANY (cualquier parámetro)
mock.Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
    .ReturnsAsync(cotizacionCreada);

// Con condición
mock.Setup(r => r.CreateAsync(It.Is<Cotizacion>(c => c.Subtotal > 1000)))
    .ReturnsAsync(cotizacionCreada);
```

### Ejemplos de Verify

```csharp
// Verificar que llamó EXACTAMENTE una vez
mock.Verify(r => r.GetByIdAsync(1), Times.Once);

// Verificar que NUNCA llamó
mock.Verify(r => r.DeleteAsync(999), Times.Never);

// Verificar que llamó al menos una vez
mock.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());

// Verificar con condición
mock.Verify(r => r.CreateAsync(
    It.Is<Cotizacion>(c => c.Cliente == "Test")
), Times.Once);
```

---

## ✨ FluentAssertions

### Comparaciones básicas

```csharp
// Valores
resultado.Should().Be(100);
resultado.Should().NotBe(0);
resultado.Should().BeGreaterThan(50);
resultado.Should().BeLessThan(200);

// Nulls
resultado.Should().NotBeNull();
resultado.Should().BeNull();

// Strings
nombre.Should().Be("Test");
nombre.Should().StartWith("Te");
nombre.Should().Contain("es");
nombre.Should().NotBeNullOrEmpty();

// Booleanos
isActive.Should().BeTrue();
isDeleted.Should().BeFalse();
```

### Colecciones

```csharp
// Cantidad
lista.Should().HaveCount(5);
lista.Should().NotBeEmpty();
lista.Should().BeEmpty();

// Contenido
lista.Should().Contain(item);
lista.Should().NotContain(itemNoExistente);
lista.Should().ContainSingle(); // Solo 1 elemento

// Condiciones
lista.Should().OnlyContain(c => c.Estado == "NUEVA");
lista.Should().AllSatisfy(c => c.Subtotal > 0);
```

### Con mensajes personalizados

```csharp
resultado.Should().Be(100, "porque esperábamos ese valor del cálculo");
lista.Should().HaveCount(5, "porque configuramos 5 elementos de prueba");
```

---

## 📁 Estructura de Tests

```
back_cabs/
  ├── Tests/
  │   ├── UnitTests/
  │   │   ├── Services/
  │   │   │   ├── CotizacionServiceTests.cs ✅
  │   │   │   ├── VehiculosServiceTests.cs
  │   │   │   └── ClientesServiceTests.cs
  │   │   └── Validators/
  │   ├── IntegrationTests/
  │   │   ├── Controllers/
  │   │   │   ├── CotizacionesControllerTests.cs
  │   │   │   └── VehiculosControllerTests.cs
  │   │   └── Repositories/
  │   └── Helpers/
  │       ├── TestDataBuilder.cs ✅
  │       └── DatabaseFixture.cs
```

---

## 🚀 Próximos Pasos

### 1️⃣ Ejecuta los tests existentes
```powershell
cd c:\Users\adria\source\repos\fullstack_cabs\back_cabs
dotnet test
```

### 2️⃣ Revisa CotizacionServiceTests.cs
- Lee los comentarios explicativos
- Entiende el patrón AAA
- Observa cómo usamos Moq
- Mira las FluentAssertions

### 3️⃣ Crea tu primer test
- Copia un test existente
- Modifícalo para probar otro método
- Ejecútalo y verifica que pasa

### 4️⃣ Aprende más
- Lee sobre [Theory] y [InlineData]
- Experimenta con diferentes Setups de Moq
- Prueba diferentes FluentAssertions

---

## 🔍 Interpretando Resultados

### ✅ Test exitoso
```
Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15
```

### ❌ Test fallido
```
Failed!  - Failed:     1, Passed:    14, Skipped:     0, Total:    15

  Failed CotizacionServiceTests.ObtenerPorIdAsync_ConIdExistente_DebeRetornarCotizacion [12 ms]
  Error Message:
   Expected resultado to be <not null>, but found null.
```

### ⚠️ Test skipped
```csharp
[Fact(Skip = "Pendiente de implementar")]
public async Task TestPendiente() { }
```

---

## 💡 Tips y Buenas Prácticas

### ✅ DO

- Nombra los tests descriptivamente
- Sigue el patrón AAA
- Cada test prueba UNA cosa
- Usa TestDataBuilder para datos de prueba
- Escribe mensajes claros en assertions

### ❌ DON'T

- No dependas de orden de ejecución
- No uses datos reales de producción
- No hagas tests dependientes entre sí
- No pruebes implementación, prueba comportamiento

---

## 📊 Cobertura de Código

**Meta**: Al menos 80% de cobertura

```powershell
# Instalar herramienta de reporte
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generar cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generar reporte HTML
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport

# Abrir reporte
start coveragereport/index.html
```

---

## 🆘 Ayuda Rápida

### Error común: "Object reference not set to an instance"
```csharp
// ❌ Olvidaste hacer Setup del mock
var resultado = await service.ObtenerAsync(1);  // NullReferenceException

// ✅ Configura el mock primero
mockRepo.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new Cotizacion { Id = 1 });
```

### Error común: "Sequence contains no elements"
```csharp
// ❌ Lista vacía sin verificar
var primero = lista.First();  // InvalidOperationException

// ✅ Verifica que haya elementos
lista.Should().NotBeEmpty();
var primero = lista.First();
```

### Error común: "Expected X calls, but found Y"
```csharp
// ❌ El mock fue llamado diferente cantidad de veces
mockRepo.Verify(r => r.GetAllAsync(), Times.Once);  // Falló

// ✅ Verifica cuántas veces realmente se llamó
mockRepo.Verify(r => r.GetAllAsync(), Times.AtLeastOnce());
```

---

## 🎓 Recursos Adicionales

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Moq Quickstart](https://github.com/devlooped/moq/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**¡Ahora estás listo para escribir tests! 🚀**
