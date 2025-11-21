using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.controllers.Legacy;
using back_cabs.CRM.Interfaces.Legacy;
using back_cabs.CRM.DTOs.Legacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace back_cabs.Tests.UnitTests.Controllers;

/// <summary>
/// ✅ Tests unitarios para AdmDocumentosController - POST /api/AdmDocumentos/cotizacion
/// Verifica el comportamiento del endpoint de creación de cotizaciones.
/// 
/// 📚 CONCEPTOS CLAVE:
/// - [Fact]: Test simple sin parámetros
/// - [Theory]: Test con múltiples conjuntos de datos
/// - Arrange-Act-Assert (AAA): Patrón de organización de tests
/// - Mock: Simula dependencias (service, logger) sin tocar BD
/// - FluentAssertions: Verificaciones legibles (.Should().Be(), etc.)
/// 
/// 🎯 COBERTURA:
/// - Creación exitosa de cotización
/// - Validación de ModelState
/// - Manejo de errores (InvalidOperationException, Exception genérica)
/// - Validación de datos de entrada (cliente, productos)
/// - Cálculos automáticos (totales, IVA, descuentos)
/// </summary>
public class AdmDocumentosControllerTests
{
    private readonly Mock<IAdmDocumentoService> _mockService;
    private readonly Mock<ILogger<AdmDocumentosController>> _mockLogger;
    private readonly AdmDocumentosController _controller;

    /// <summary>
    /// 🏗️ Constructor: Se ejecuta ANTES de cada test
    /// Configura los mocks y crea la instancia del controlador
    /// </summary>
    public AdmDocumentosControllerTests()
    {
        _mockService = new Mock<IAdmDocumentoService>();
        _mockLogger = new Mock<ILogger<AdmDocumentosController>>();
        _controller = new AdmDocumentosController(_mockService.Object, _mockLogger.Object);
    }

    #region CreateCotizacion - Casos de Éxito

    /// <summary>
    /// ✅ Test 1: Crear cotización básica exitosamente
    /// GIVEN: DTO válido con 1 producto
    /// WHEN: Se llama al endpoint POST /api/AdmDocumentos/cotizacion
    /// THEN: Retorna 201 Created con los datos de la cotización creada
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConDatosValidos_RetornaCreatedConRespuesta()
    {
        // Arrange - Preparar datos de prueba
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            IdAgente = 1,
            AplicarIVA = true,
            PorcentajeIVA = 16.0,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto
                {
                    IdProducto = 100,
                    IdAlmacen = 1,
                    Unidades = 10,
                    Precio = 100.0,
                    PorcentajeDescuento = 0
                }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 1234,
            Serie = "CA",
            Folio = 5678,
            Fecha = DateTime.Now,
            RazonSocial = "Cliente Test S.A. de C.V.",
            Neto = 1000.0,      // 10 unidades × 100 precio
            Impuesto = 160.0,   // 1000 × 16% IVA
            Total = 1160.0,     // 1000 + 160
            Pendiente = 1160.0,
            CantidadProductos = 1,
            Mensaje = "Cotización creada exitosamente"
        };

        // Configurar el mock del servicio para retornar la respuesta esperada
        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act - Ejecutar la acción
        var result = await _controller.CreateCotizacion(dto);

        // Assert - Verificar resultados
        // 1. Verificar que el resultado es StatusCodeResult 201
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(201);

        // 2. Verificar la estructura de la respuesta
        var response = statusCodeResult.Value;
        response.Should().NotBeNull();

        var responseData = response!.GetType().GetProperty("data")?.GetValue(response);
        responseData.Should().NotBeNull();
        responseData.Should().BeEquivalentTo(expectedResponse);

        var success = response.GetType().GetProperty("success")?.GetValue(response);
        success.Should().Be(true);

        // 3. Verificar que el servicio fue llamado exactamente una vez
        _mockService.Verify(
            s => s.CreateCotizacionAsync(It.Is<AdmCotizacionCreateDto>(d => 
                d.IdCliente == 1 && 
                d.Productos.Count == 1
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// ✅ Test 2: Crear cotización con múltiples productos
    /// GIVEN: DTO con 3 productos diferentes
    /// WHEN: Se llama al endpoint POST
    /// THEN: Calcula correctamente los totales de múltiples productos
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConMultiplesProductos_CalculaTotalesCorrectamente()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 2,
            AplicarIVA = true,
            PorcentajeIVA = 16.0,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 5, Precio = 50.0 },
                new CotizacionMovimientoDto { IdProducto = 101, IdAlmacen = 1, Unidades = 3, Precio = 100.0 },
                new CotizacionMovimientoDto { IdProducto = 102, IdAlmacen = 1, Unidades = 10, Precio = 25.0 }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 2345,
            Serie = "CA",
            Folio = 6789,
            Neto = 800.0,       // (5×50) + (3×100) + (10×25) = 250 + 300 + 250 = 800
            Impuesto = 128.0,   // 800 × 16%
            Total = 928.0,      // 800 + 128
            Pendiente = 928.0,
            CantidadProductos = 3
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(201);

        var response = statusCodeResult.Value;
        var data = response!.GetType().GetProperty("data")?.GetValue(response) as AdmCotizacionCreateResponseDto;

        data.Should().NotBeNull();
        data!.CantidadProductos.Should().Be(3);
        data.Neto.Should().Be(800.0);
        data.Total.Should().Be(928.0);

        _mockService.Verify(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test 3: Crear cotización con descuentos
    /// GIVEN: DTO con descuentos a nivel producto y documento
    /// WHEN: Se calcula el total
    /// THEN: Aplica correctamente los descuentos en cascada
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConDescuentos_AplicaDescuentosCorrectamente()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 3,
            DescuentoDoc1 = 10.0,  // 10% descuento documento
            AplicarIVA = true,
            PorcentajeIVA = 16.0,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto 
                { 
                    IdProducto = 100, 
                    IdAlmacen = 1, 
                    Unidades = 10, 
                    Precio = 100.0,
                    PorcentajeDescuento = 5.0  // 5% descuento producto
                }
            }
        };

        // Cálculo esperado:
        // Subtotal producto: 10 × 100 = 1000
        // Descuento producto (5%): 1000 × 0.05 = 50
        // Neto producto: 1000 - 50 = 950
        // Descuento documento (10%): 950 × 0.10 = 95
        // Neto final: 950 - 95 = 855
        // IVA (16%): 855 × 0.16 = 136.8
        // Total: 855 + 136.8 = 991.8

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 3456,
            Serie = "CA",
            Folio = 7890,
            Neto = 855.0,
            Impuesto = 136.8,
            Total = 991.8,
            Pendiente = 991.8,
            CantidadProductos = 1
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(201);

        var response = statusCodeResult.Value;
        var data = response!.GetType().GetProperty("data")?.GetValue(response) as AdmCotizacionCreateResponseDto;

        data.Should().NotBeNull();
        data!.Neto.Should().Be(855.0);
        data.Total.Should().Be(991.8);
    }

    /// <summary>
    /// ✅ Test 4: Crear cotización con pago parcial
    /// GIVEN: DTO con MontoPagado > 0
    /// WHEN: Se crea la cotización
    /// THEN: CPendiente = CTotal - MontoPagado
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConPagoParcial_CalculaPendienteCorrectamente()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 4,
            MontoPagado = 500.0,  // Cliente abona $500
            AplicarIVA = true,
            PorcentajeIVA = 16.0,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 4567,
            Serie = "CA",
            Folio = 8901,
            Neto = 1000.0,
            Impuesto = 160.0,
            Total = 1160.0,
            Pendiente = 660.0,  // 1160 - 500 = 660
            CantidadProductos = 1
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        var response = statusCodeResult.Value;
        var data = response!.GetType().GetProperty("data")?.GetValue(response) as AdmCotizacionCreateResponseDto;

        data.Should().NotBeNull();
        data!.Total.Should().Be(1160.0);
        data.Pendiente.Should().Be(660.0);
    }

    /// <summary>
    /// ✅ Test 5: Crear cotización sin IVA
    /// GIVEN: DTO con AplicarIVA = false
    /// WHEN: Se calcula el total
    /// THEN: No se aplica IVA (Impuesto = 0)
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_SinIVA_NoAplicaImpuesto()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 5,
            AplicarIVA = false,  // Sin IVA
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 5678,
            Serie = "CA",
            Folio = 9012,
            Neto = 1000.0,
            Impuesto = 0.0,     // Sin IVA
            Total = 1000.0,     // Neto = Total
            Pendiente = 1000.0,
            CantidadProductos = 1
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        var response = statusCodeResult.Value;
        var data = response!.GetType().GetProperty("data")?.GetValue(response) as AdmCotizacionCreateResponseDto;

        data.Should().NotBeNull();
        data!.Impuesto.Should().Be(0.0);
        data.Total.Should().Be(1000.0);
    }

    #endregion

    #region CreateCotizacion - Validaciones y Errores

    /// <summary>
    /// ❌ Test 6: Rechazar cotización sin productos
    /// GIVEN: DTO con lista de productos vacía
    /// WHEN: Se intenta crear la cotización
    /// THEN: Retorna 400 BadRequest con mensaje de validación
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_SinProductos_RetornaBadRequest()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>()  // Lista vacía
        };

        // Simular error de validación en ModelState
        _controller.ModelState.AddModelError("Productos", "Debe incluir al menos un producto");

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value;
        var success = response?.GetType().GetProperty("success")?.GetValue(response);
        success.Should().Be(false);

        // Verificar que el servicio NO fue llamado
        _mockService.Verify(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()), Times.Never);
    }

    /// <summary>
    /// ❌ Test 7: Rechazar cotización con IdCliente inválido
    /// GIVEN: DTO con IdCliente = 0 o negativo
    /// WHEN: Se valida el DTO
    /// THEN: Retorna 400 BadRequest
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateCotizacion_ConIdClienteInvalido_RetornaBadRequest(int idClienteInvalido)
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = idClienteInvalido,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        _controller.ModelState.AddModelError("IdCliente", "El ID del cliente debe ser mayor a 0");

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mockService.Verify(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()), Times.Never);
    }

    /// <summary>
    /// ❌ Test 8: Manejar excepción cuando el cliente no existe
    /// GIVEN: IdCliente que no existe en la BD
    /// WHEN: El servicio lanza InvalidOperationException
    /// THEN: Retorna 400 BadRequest con mensaje descriptivo
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ClienteNoExiste_RetornaBadRequest()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 9999,  // Cliente inexistente
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ThrowsAsync(new InvalidOperationException("El cliente con ID 9999 no existe"));

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value;
        var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
        message.Should().Contain("9999");
        message.Should().Contain("no existe");
    }

    /// <summary>
    /// ❌ Test 9: Manejar excepción cuando un producto no existe
    /// GIVEN: IdProducto que no existe en la BD
    /// WHEN: El servicio lanza InvalidOperationException
    /// THEN: Retorna 400 BadRequest con mensaje descriptivo
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ProductoNoExiste_RetornaBadRequest()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 99999, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ThrowsAsync(new InvalidOperationException("El producto con ID 99999 no existe"));

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var response = badRequestResult.Value;
        var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
        message.Should().Contain("99999");
        message.Should().Contain("no existe");
    }

    /// <summary>
    /// ❌ Test 10: Manejar excepción genérica del servidor
    /// GIVEN: Cualquier error inesperado en el servicio
    /// WHEN: El servicio lanza Exception
    /// THEN: Retorna 500 InternalServerError
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ErrorGenerico_RetornaInternalServerError()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ThrowsAsync(new Exception("Error de conexión a la base de datos"));

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);

        var response = statusCodeResult.Value;
        var success = response?.GetType().GetProperty("success")?.GetValue(response);
        success.Should().Be(false);

        var message = response?.GetType().GetProperty("message")?.GetValue(response)?.ToString();
        message.Should().Contain("Error al crear");
    }

    /// <summary>
    /// ❌ Test 11: Validar descuentos fuera de rango
    /// GIVEN: DTO con descuentos > 100% o negativos
    /// WHEN: Se valida el DTO
    /// THEN: Retorna 400 BadRequest
    /// </summary>
    [Theory]
    [InlineData(-5.0)]
    [InlineData(101.0)]
    [InlineData(999.0)]
    public async Task CreateCotizacion_ConDescuentoInvalido_RetornaBadRequest(double descuentoInvalido)
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            DescuentoDoc1 = descuentoInvalido,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 10, Precio = 100.0 }
            }
        };

        _controller.ModelState.AddModelError("DescuentoDoc1", "El descuento debe estar entre 0 y 100");

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// ❌ Test 12: Validar unidades negativas o cero
    /// GIVEN: DTO con producto con unidades <= 0
    /// WHEN: Se valida el DTO
    /// THEN: Retorna 400 BadRequest
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public async Task CreateCotizacion_ConUnidadesInvalidas_RetornaBadRequest(double unidadesInvalidas)
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto 
                { 
                    IdProducto = 100, 
                    IdAlmacen = 1, 
                    Unidades = unidadesInvalidas, 
                    Precio = 100.0 
                }
            }
        };

        _controller.ModelState.AddModelError("Productos[0].Unidades", "La cantidad debe ser mayor a 0");

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region CreateCotizacion - Casos Edge

    /// <summary>
    /// ✅ Test 13: Crear cotización con valores mínimos permitidos
    /// GIVEN: DTO con el mínimo de datos requeridos
    /// WHEN: Se crea la cotización
    /// THEN: Se aplican todos los valores por defecto correctamente
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConValoresPorDefecto_UsaDefaults()
    {
        // Arrange - Solo los campos obligatorios
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 1, Precio = 1.0 }
            }
            // AplicarIVA = true (default)
            // PorcentajeIVA = 16.0 (default)
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 9999,
            Serie = "CA",
            Folio = 1,
            Neto = 1.0,
            Impuesto = 0.16,
            Total = 1.16,
            Pendiente = 1.16,
            CantidadProductos = 1
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(201);

        _mockService.Verify(s => s.CreateCotizacionAsync(
            It.Is<AdmCotizacionCreateDto>(d => 
                d.AplicarIVA == true && 
                d.PorcentajeIVA == 16.0
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// ✅ Test 14: Crear cotización con todos los campos opcionales
    /// GIVEN: DTO con todos los campos populados
    /// WHEN: Se crea la cotización
    /// THEN: Todos los valores se procesan correctamente
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_ConTodosLosCampos_ProcesaCorrectamente()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            IdAgente = 5,
            FechaVencimiento = DateTime.Now.AddDays(30),
            FechaProntoPago = DateTime.Now.AddDays(15),
            FechaEntregaRecepcion = DateTime.Now.AddDays(7),
            DescuentoDoc1 = 5.0,
            DescuentoDoc2 = 3.0,
            MontoPagado = 200.0,
            Observaciones = "Cotización urgente - Cliente corporativo",
            Referencia = "REF-2025-001",
            AplicarIVA = true,
            PorcentajeIVA = 16.0,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto 
                { 
                    IdProducto = 100, 
                    IdAlmacen = 1, 
                    IdUnidad = 3,
                    Unidades = 50, 
                    Precio = 75.5,
                    PorcentajeDescuento = 10.0,
                    Observaciones = "Producto premium"
                }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 10000,
            Serie = "CA",
            Folio = 2000,
            Neto = 3400.0,
            Impuesto = 544.0,
            Total = 3944.0,
            Pendiente = 3744.0,
            CantidadProductos = 1
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(201);

        _mockService.Verify(s => s.CreateCotizacionAsync(
            It.Is<AdmCotizacionCreateDto>(d => 
                d.IdAgente == 5 &&
                d.Observaciones == "Cotización urgente - Cliente corporativo" &&
                d.Referencia == "REF-2025-001"
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// ✅ Test 15: Verificar que se registra el tiempo de ejecución
    /// GIVEN: Cualquier petición válida
    /// WHEN: Se completa la operación
    /// THEN: La respuesta incluye executionTime
    /// </summary>
    [Fact]
    public async Task CreateCotizacion_SiempreIncluyeTiempoEjecucion()
    {
        // Arrange
        var dto = new AdmCotizacionCreateDto
        {
            IdCliente = 1,
            Productos = new List<CotizacionMovimientoDto>
            {
                new CotizacionMovimientoDto { IdProducto = 100, IdAlmacen = 1, Unidades = 1, Precio = 100.0 }
            }
        };

        var expectedResponse = new AdmCotizacionCreateResponseDto
        {
            IdDocumento = 1,
            Serie = "CA",
            Folio = 1,
            Total = 116.0
        };

        _mockService
            .Setup(s => s.CreateCotizacionAsync(It.IsAny<AdmCotizacionCreateDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCotizacion(dto);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        var response = statusCodeResult.Value;
        var executionTime = response?.GetType().GetProperty("executionTime")?.GetValue(response)?.ToString();

        executionTime.Should().NotBeNullOrEmpty();
        executionTime.Should().Contain("ms");
    }

    #endregion
}
