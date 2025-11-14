// using Xunit;
// using Moq;
// using FluentAssertions;
// using back_cabs.CRM.services.Recepcion;
// using back_cabs.CRM.Interfaces.Recepcion;
// using back_cabs.CRM.models.Sales;
// using CRM.DTOs.Request;
// using CRM.DTOs.Response;
// using Microsoft.Extensions.Logging;

// namespace back_cabs.Tests.UnitTests.Services;

// /// <summary>
// /// ✅ Tests unitarios para CotizacionService
// /// Estos tests verifican la lógica de negocio sin tocar la base de datos real.
// /// 
// /// 📚 CONCEPTOS CLAVE:
// /// - [Fact]: Test simple sin parámetros
// /// - [Theory]: Test con múltiples datos de prueba
// /// - Arrange-Act-Assert (AAA): Patrón de organización de tests
// /// - Mock: Simula dependencias (repository, logger, etc.)
// /// - FluentAssertions: Verificaciones legibles (Should().Be(), etc.)
// /// </summary>
// public class CotizacionServiceTests
// {
//     private readonly Mock<ICotizacionRepository> _mockRepository;
//     private readonly Mock<ILogger<CotizacionService>> _mockLogger;
//     private readonly CotizacionService _service;

//     /// <summary>
//     /// 🏗️ Constructor: Se ejecuta ANTES de cada test
//     /// Aquí configuramos los mocks y creamos la instancia del servicio
//     /// </summary>
//     public CotizacionServiceTests()
//     {
//         // 1. Crear mocks de las dependencias
//         _mockRepository = new Mock<ICotizacionRepository>();
//         _mockLogger = new Mock<ILogger<CotizacionService>>();
        
//         // 2. Inyectar los mocks al servicio
//         _service = new CotizacionService(
//             _mockRepository.Object,  // .Object convierte el mock en la interfaz
//             _mockLogger.Object
//         );
//     }

//     #region ObtenerTodosAsync Tests

//     /// <summary>
//     /// ✅ Test básico: Verifica que el servicio retorna cotizaciones correctamente
//     /// 
//     /// 📋 PATRÓN AAA:
//     /// Arrange: Prepara datos de prueba y configura mocks
//     /// Act: Ejecuta el método que estamos probando
//     /// Assert: Verifica que el resultado es correcto
//     /// </summary>
//     [Fact]
//     public async Task ObtenerTodosAsync_CuandoHayCotizaciones_DebeRetornarListaNoVacia()
//     {
//         // 📋 Arrange: Preparar datos de prueba
//         var cotizacionesSimuladas = new List<Cotizacion>
//         {
//             new Cotizacion 
//             { 
//                 Id = 1,
//                 OrdenId = 1,
//                 Cliente = "Empresa ABC S.A. de C.V.",
//                 Rfc = "ABC123456789",
//                 Subtotal = 5000.00m,
//                 ImpuestosTotal = 800.00m,
//                 Descuento = 200.00m,
//                 Estado = "NUEVA",
//                 CreadoEn = DateTime.Now
//             },
//             new Cotizacion 
//             { 
//                 Id = 2,
//                 OrdenId = 2,
//                 Cliente = "Transportes XYZ",
//                 Rfc = "XYZ987654321",
//                 Subtotal = 10000.00m,
//                 ImpuestosTotal = 1600.00m,
//                 Estado = "APROBADA",
//                 CreadoEn = DateTime.Now
//             }
//         };

//         // Configurar el mock: cuando llamen GetAllAsync(), retorna cotizacionesSimuladas
//         _mockRepository
//             .Setup(r => r.GetAllAsync())
//             .ReturnsAsync(cotizacionesSimuladas);

//         // 🎬 Act: Ejecutar el método a probar
//         var resultado = await _service.ObtenerTodosAsync();

//         // ✅ Assert: Verificar que funciona correctamente
//         resultado.Should().NotBeNull("porque siempre debe retornar una lista");
//         resultado.Should().HaveCount(2, "porque configuramos 2 cotizaciones de prueba");
//         resultado.First().Cliente.Should().Be("Empresa ABC S.A. de C.V.");
        
//         // Verificar que SÍ llamó al repositorio
//         _mockRepository.Verify(r => r.GetAllAsync(), Times.Once, 
//             "debe llamar al repositorio exactamente una vez");
//     }

//     [Fact]
//     public async Task ObtenerTodosAsync_CuandoNoHayCotizaciones_DebeRetornarListaVacia()
//     {
//         // Arrange
//         var listaVacia = new List<Cotizacion>();
        
//         _mockRepository
//             .Setup(r => r.GetAllAsync())
//             .ReturnsAsync(listaVacia);

//         // Act
//         var resultado = await _service.ObtenerTodosAsync();

//         // Assert
//         resultado.Should().NotBeNull();
//         resultado.Should().BeEmpty("porque no hay cotizaciones en la BD");
//         _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
//     }

//     #endregion

//     #region ObtenerPorIdAsync Tests

//     [Fact]
//     public async Task ObtenerPorIdAsync_ConIdExistente_DebeRetornarCotizacion()
//     {
//         // Arrange
//         var cotizacionEsperada = new Cotizacion
//         {
//             Id = 1,
//             OrdenId = 1,
//             Cliente = "Test Cliente",
//             Subtotal = 1000.00m,
//             ImpuestosTotal = 160.00m,
//             Estado = "NUEVA"
//         };

//         _mockRepository
//             .Setup(r => r.GetByIdAsync(1))
//             .ReturnsAsync(cotizacionEsperada);

//         // Act
//         var resultado = await _service.ObtenerPorIdAsync(1);

//         // Assert
//         resultado.Should().NotBeNull();
//         resultado!.Id.Should().Be(1);
//         resultado.Cliente.Should().Be("Test Cliente");
//         resultado.Subtotal.Should().Be(1000.00m);
        
//         _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
//     }

//     [Fact]
//     public async Task ObtenerPorIdAsync_ConIdInexistente_DebeRetornarNull()
//     {
//         // Arrange
//         _mockRepository
//             .Setup(r => r.GetByIdAsync(999))
//             .ReturnsAsync((Cotizacion)null!);

//         // Act
//         var resultado = await _service.ObtenerPorIdAsync(999);

//         // Assert
//         resultado.Should().BeNull("porque la cotización con ID 999 no existe");
//         _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
//     }

//     /// <summary>
//     /// 🎯 [Theory]: Test que se ejecuta múltiples veces con diferentes datos
//     /// [InlineData]: Cada línea es una ejecución con esos valores
//     /// </summary>
//     [Theory]
//     [InlineData(0)]
//     [InlineData(-1)]
//     [InlineData(-999)]
//     public async Task ObtenerPorIdAsync_ConIdInvalido_DebeRetornarNull(int idInvalido)
//     {
//         // Arrange
//         _mockRepository
//             .Setup(r => r.GetByIdAsync(It.IsAny<int>()))  // It.IsAny = cualquier int
//             .ReturnsAsync((Cotizacion)null!);

//         // Act
//         var resultado = await _service.ObtenerPorIdAsync(idInvalido);

//         // Assert
//         resultado.Should().BeNull($"porque el ID {idInvalido} es inválido");
//     }

//     #endregion

//     #region Cálculos de Total Tests

//     /// <summary>
//     /// 🧮 Test de cálculos: Verifica la propiedad calculada Total
//     /// </summary>
//     [Theory]
//     [InlineData(1000.00, 160.00, 0.00, 1160.00)]           // Sin descuento
//     [InlineData(1000.00, 160.00, 100.00, 1060.00)]         // Con descuento
//     [InlineData(5000.00, 800.00, 200.00, 5600.00)]         // Descuento pequeño
//     [InlineData(10000.00, 1600.00, 2000.00, 9600.00)]      // Descuento significativo
//     [InlineData(1000.00, 0.00, 0.00, 1000.00)]             // Sin impuestos ni descuento
//     public void CalcularTotal_ConDiferentesValores_DebeCalcularCorrectamente(
//         double subtotalDouble,
//         double impuestosDouble,
//         double? descuentoDouble,
//         double totalEsperadoDouble)
//     {
//         // Convertir de double a decimal (xUnit no soporta decimal en InlineData)
//         var subtotal = (decimal)subtotalDouble;
//         var impuestos = (decimal)impuestosDouble;
//         var descuento = descuentoDouble.HasValue ? (decimal?)descuentoDouble.Value : null;
//         var totalEsperado = (decimal)totalEsperadoDouble;
        
//         // Arrange
//         var cotizacion = new Cotizacion
//         {
//             Subtotal = subtotal,
//             ImpuestosTotal = impuestos,
//             Descuento = descuento
//         };

//         // Act
//         var total = cotizacion.Total;

//         // Assert
//         total.Should().Be(totalEsperado, 
//             $"porque el cálculo debe ser {subtotal} + {impuestos} - {descuento ?? 0} = {totalEsperado}");
//     }

//     #endregion
// }
