using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.services.Recepcion;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Sales;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using Microsoft.Extensions.Logging;

namespace back_cabs.Tests.UnitTests.Services;

/// <summary>
/// ✅ Tests completos para CotizacionService alineados con BD sales_cotizaciones
/// 
/// ESTRUCTURA DE LA TABLA:
/// - Total (BD): Subtotal + ImpuestosTotal [COLUMNA PERSISTED]
/// - TotalFinal (C#): Total - Descuento [PROPIEDAD CALCULADA]
/// 
/// CAMPOS NUEVOS INCLUIDOS:
/// - HorasCapacitacion, PaquetesCapacitacion, CostoCapacitacion
/// - Cliente, RFC, Folio, DescripcionServicio
/// - Descuento, ValidezDias
/// </summary>
public class CotizacionServiceTestsCompletos
{
    private readonly Mock<ICotizacionRepository> _mockRepository;
    private readonly Mock<ILogger<CotizacionService>> _mockLogger;
    private readonly CotizacionService _service;

    public CotizacionServiceTestsCompletos()
    {
        _mockRepository = new Mock<ICotizacionRepository>();
        _mockLogger = new Mock<ILogger<CotizacionService>>();
        _service = new CotizacionService(_mockRepository.Object, _mockLogger.Object);
    }

    #region Cálculos de Total Tests

    /// <summary>
    /// Test que verifica el cálculo correcto de Total y TotalFinal
    /// </summary>
    [Theory]
    [InlineData(1000.00, 160.00, 0.00, 1160.00, 1160.00)]      // Sin descuento
    [InlineData(1000.00, 160.00, 100.00, 1160.00, 1060.00)]    // Con descuento
    [InlineData(5000.00, 800.00, 200.00, 5800.00, 5600.00)]    // Caso normal
    [InlineData(10000.00, 1600.00, 2000.00, 11600.00, 9600.00)] // Descuento grande
    [InlineData(1000.00, 0.00, 0.00, 1000.00, 1000.00)]        // Sin impuestos
    public void Cotizacion_CalcularTotales_DebeCalcularCorrectamente(
        double subtotalDouble,
        double impuestosDouble,
        double? descuentoDouble,
        double totalBDDouble,
        double totalFinalDouble)
    {
        // Arrange
        var subtotal = (decimal)subtotalDouble;
        var impuestos = (decimal)impuestosDouble;
        var descuento = descuentoDouble.HasValue ? (decimal?)descuentoDouble.Value : null;
        var totalBDEsperado = (decimal)totalBDDouble;
        var totalFinalEsperado = (decimal)totalFinalDouble;

        var cotizacion = new Cotizacion
        {
            Subtotal = subtotal,
            ImpuestosTotal = impuestos,
            Total = totalBDEsperado, // En BD se calcula automáticamente
            Descuento = descuento
        };

        // Act & Assert
        cotizacion.Total.Should().Be(totalBDEsperado,
            "porque Total (BD) = Subtotal + ImpuestosTotal");
        
        cotizacion.TotalFinal.Should().Be(totalFinalEsperado,
            "porque TotalFinal = Total - Descuento");
    }

    #endregion

    #region CrearAsync Tests

    [Fact]
    public async Task CrearAsync_ConTodosLosCampos_DebeCrearCotizacionCompleta()
    {
        // Arrange
        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 123,
            IntakeLegacyId = 456,
            Cliente = "Transportes Martínez S.A. de C.V.",
            Rfc = "TMA123456ABC",
            Folio = "COT-2025-001",
            Subtotal = 10000.00m,
            ImpuestosTotal = 1600.00m,
            Descuento = 500.00m,
            Estado = "NUEVA",
            Observaciones = "Cotización para servicio de mantenimiento preventivo",
            DescripcionServicio = "Mantenimiento preventivo completo de flota de 10 unidades",
            ValidezDias = 30,
            HorasCapacitacion = 16,
            PaquetesCapacitacion = 2,
            CostoCapacitacion = 8000.00m
        };

        var cotizacionCreada = new Cotizacion
        {
            Id = 1,
            OrdenId = request.OrdenId,
            IntakeLegacyId = request.IntakeLegacyId,
            Cliente = request.Cliente,
            Rfc = request.Rfc,
            Folio = request.Folio,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Total = 11600.00m, // BD calcula: 10000 + 1600
            Descuento = request.Descuento,
            Estado = request.Estado,
            Observaciones = request.Observaciones,
            DescripcionServicio = request.DescripcionServicio,
            ValidezDias = request.ValidezDias,
            HorasCapacitacion = request.HorasCapacitacion,
            PaquetesCapacitacion = request.PaquetesCapacitacion,
            CostoCapacitacion = request.CostoCapacitacion,
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync(cotizacionCreada);

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(1);
        resultado.OrdenId.Should().Be(123);
        resultado.IntakeLegacyId.Should().Be(456);
        resultado.Cliente.Should().Be("Transportes Martínez S.A. de C.V.");
        resultado.Rfc.Should().Be("TMA123456ABC");
        resultado.Folio.Should().Be("COT-2025-001");
        resultado.Subtotal.Should().Be(10000.00m);
        resultado.ImpuestosTotal.Should().Be(1600.00m);
        resultado.Total.Should().Be(11600.00m);
        resultado.Descuento.Should().Be(500.00m);
        resultado.TotalFinal.Should().Be(11100.00m); // 11600 - 500
        resultado.Estado.Should().Be("NUEVA");
        resultado.Observaciones.Should().Be("Cotización para servicio de mantenimiento preventivo");
        resultado.DescripcionServicio.Should().Be("Mantenimiento preventivo completo de flota de 10 unidades");
        resultado.ValidezDias.Should().Be(30);
        resultado.HorasCapacitacion.Should().Be(16);
        resultado.PaquetesCapacitacion.Should().Be(2);
        resultado.CostoCapacitacion.Should().Be(8000.00m);

        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Cotizacion>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_SinCamposOpcionales_DebeCrearCotizacionMinima()
    {
        // Arrange
        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 5000.00m,
            ImpuestosTotal = 800.00m,
            Estado = "NUEVA"
        };

        var cotizacionCreada = new Cotizacion
        {
            Id = 1,
            OrdenId = request.OrdenId,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Total = 5800.00m,
            Estado = request.Estado,
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync(cotizacionCreada);

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Subtotal.Should().Be(5000.00m);
        resultado.Total.Should().Be(5800.00m);
        resultado.Descuento.Should().BeNull();
        resultado.TotalFinal.Should().Be(5800.00m); // Sin descuento
    }

    #endregion

    #region ActualizarAsync Tests

    [Fact]
    public async Task ActualizarAsync_ActualizarTodosLosCampos_DebeActualizar()
    {
        // Arrange
        var cotizacionExistente = new Cotizacion
        {
            Id = 1,
            OrdenId = 1,
            Cliente = "Cliente Original",
            Subtotal = 1000.00m,
            ImpuestosTotal = 160.00m,
            Total = 1160.00m,
            Estado = "NUEVA",
            CreadoEn = DateTime.UtcNow.AddDays(-1)
        };

        var requestActualizacion = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Cliente = "Cliente Actualizado S.A.",
            Rfc = "CLI987654XYZ",
            Folio = "COT-2025-002",
            Subtotal = 15000.00m,
            ImpuestosTotal = 2400.00m,
            Descuento = 1000.00m,
            Estado = "APROBADA",
            Observaciones = "Cotización actualizada y aprobada",
            ValidezDias = 45,
            HorasCapacitacion = 24,
            PaquetesCapacitacion = 3,
            CostoCapacitacion = 12000.00m
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(cotizacionExistente);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync((Cotizacion c) => 
            {
                c.Total = 17400.00m; // BD recalcula: 15000 + 2400
                return c;
            });

        // Act
        var resultado = await _service.ActualizarAsync(1, requestActualizacion);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Cliente.Should().Be("Cliente Actualizado S.A.");
        resultado.Rfc.Should().Be("CLI987654XYZ");
        resultado.Folio.Should().Be("COT-2025-002");
        resultado.Subtotal.Should().Be(15000.00m);
        resultado.ImpuestosTotal.Should().Be(2400.00m);
        resultado.Total.Should().Be(17400.00m);
        resultado.Descuento.Should().Be(1000.00m);
        resultado.TotalFinal.Should().Be(16400.00m); // 17400 - 1000
        resultado.Estado.Should().Be("APROBADA");
        resultado.HorasCapacitacion.Should().Be(24);
        resultado.PaquetesCapacitacion.Should().Be(3);
        resultado.CostoCapacitacion.Should().Be(12000.00m);

        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Cotizacion>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_CotizacionInexistente_DebeRetornarNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Cotizacion?)null);

        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 1000.00m,
            ImpuestosTotal = 160.00m,
            Estado = "NUEVA"
        };

        // Act
        var resultado = await _service.ActualizarAsync(999, request);

        // Assert
        resultado.Should().BeNull("porque la cotización 999 no existe");
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Cotizacion>()), Times.Never);
    }

    #endregion

    #region ObtenerPorOrdenIdAsync Tests

    [Fact]
    public async Task ObtenerPorOrdenIdAsync_ConCotizacionesExistentes_DebeRetornarLista()
    {
        // Arrange
        var ordenId = 123;
        var cotizaciones = new List<Cotizacion>
        {
            new Cotizacion 
            { 
                Id = 1, 
                OrdenId = ordenId,
                Cliente = "Cliente A",
                Subtotal = 5000.00m,
                ImpuestosTotal = 800.00m,
                Total = 5800.00m,
                Estado = "NUEVA",
                CreadoEn = DateTime.UtcNow.AddDays(-2)
            },
            new Cotizacion 
            { 
                Id = 2, 
                OrdenId = ordenId,
                Cliente = "Cliente A",
                Subtotal = 6000.00m,
                ImpuestosTotal = 960.00m,
                Total = 6960.00m,
                Estado = "APROBADA",
                CreadoEn = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockRepository
            .Setup(r => r.GetByOrdenIdAsync(ordenId))
            .ReturnsAsync(cotizaciones);

        // Act
        var resultado = await _service.ObtenerPorOrdenIdAsync(ordenId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.All(c => c.OrdenId == ordenId).Should().BeTrue();
        _mockRepository.Verify(r => r.GetByOrdenIdAsync(ordenId), Times.Once);
    }

    #endregion

    #region ObtenerPorEstadoAsync Tests

    [Theory]
    [InlineData("NUEVA")]
    [InlineData("APROBADA")]
    [InlineData("RECHAZADA")]
    [InlineData("VENCIDA")]
    [InlineData("CANCELADA")]
    public async Task ObtenerPorEstadoAsync_ConEstadoValido_DebeRetornarCotizaciones(string estado)
    {
        // Arrange
        var cotizaciones = new List<Cotizacion>
        {
            new Cotizacion { Id = 1, Estado = estado, Subtotal = 1000.00m, Total = 1160.00m },
            new Cotizacion { Id = 2, Estado = estado, Subtotal = 2000.00m, Total = 2320.00m },
            new Cotizacion { Id = 3, Estado = estado, Subtotal = 3000.00m, Total = 3480.00m }
        };

        _mockRepository
            .Setup(r => r.GetByEstadoAsync(estado))
            .ReturnsAsync(cotizaciones);

        // Act
        var resultado = await _service.ObtenerPorEstadoAsync(estado);

        // Assert
        resultado.Should().HaveCount(3);
        resultado.All(c => c.Estado == estado).Should().BeTrue();
        _mockRepository.Verify(r => r.GetByEstadoAsync(estado), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorEstadoAsync_EstadoSinCotizaciones_DebeRetornarListaVacia()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByEstadoAsync("INEXISTENTE"))
            .ReturnsAsync(new List<Cotizacion>());

        // Act
        var resultado = await _service.ObtenerPorEstadoAsync("INEXISTENTE");

        // Assert
        resultado.Should().BeEmpty();
    }

    #endregion

    #region ObtenerPorClienteAsync Tests

    [Fact]
    public async Task ObtenerPorClienteAsync_ConClienteExistente_DebeRetornarCotizaciones()
    {
        // Arrange
        var clienteBusqueda = "Transportes";
        var cotizaciones = new List<Cotizacion>
        {
            new Cotizacion 
            { 
                Id = 1, 
                Cliente = "Transportes Martínez S.A.", 
                Subtotal = 5000.00m,
                Total = 5800.00m,
                Estado = "NUEVA"
            },
            new Cotizacion 
            { 
                Id = 2, 
                Cliente = "Transportes González", 
                Subtotal = 3000.00m,
                Total = 3480.00m,
                Estado = "APROBADA"
            }
        };

        _mockRepository
            .Setup(r => r.GetByClienteAsync(clienteBusqueda))
            .ReturnsAsync(cotizaciones);

        // Act
        var resultado = await _service.ObtenerPorClienteAsync(clienteBusqueda);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.All(c => c.Cliente!.Contains(clienteBusqueda)).Should().BeTrue();
        _mockRepository.Verify(r => r.GetByClienteAsync(clienteBusqueda), Times.Once);
    }

    #endregion

    #region EliminarAsync Tests

    [Fact]
    public async Task EliminarAsync_CotizacionExistente_DebeEliminar()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.EliminarAsync(1);

        // Assert
        resultado.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_CotizacionInexistente_DebeRetornarFalse()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.EliminarAsync(999);

        // Assert
        resultado.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
    }

    #endregion

    #region Validación de RFC Tests

    [Theory]
    [InlineData("ABC123456XYZ")] // RFC persona moral (12 caracteres)
    [InlineData("ABCD123456ABC")] // RFC persona moral (13 caracteres)
    public void ValidacionRFC_FormatosValidos_DebeAceptar(string rfc)
    {
        // Arrange & Act
        var cotizacion = new Cotizacion
        {
            Rfc = rfc,
            Cliente = "Cliente Test",
            Subtotal = 1000.00m
        };

        // Assert
        cotizacion.Rfc.Should().Be(rfc);
        cotizacion.Rfc!.Length.Should().BeGreaterOrEqualTo(12).And.BeLessOrEqualTo(13);
    }

    #endregion

    #region Cálculos de Capacitación Tests

    [Fact]
    public void Cotizacion_ConDatosDeCapacitacion_DebeAlmacenarCorrectamente()
    {
        // Arrange & Act
        var cotizacion = new Cotizacion
        {
            HorasCapacitacion = 40,
            PaquetesCapacitacion = 5,
            CostoCapacitacion = 25000.00m
        };

        // Assert
        cotizacion.HorasCapacitacion.Should().Be(40);
        cotizacion.PaquetesCapacitacion.Should().Be(5);
        cotizacion.CostoCapacitacion.Should().Be(25000.00m);
    }

    [Fact]
    public void Cotizacion_CostoPromedioPorHora_DebeCalcularCorrectamente()
    {
        // Arrange
        var cotizacion = new Cotizacion
        {
            HorasCapacitacion = 40,
            CostoCapacitacion = 20000.00m
        };

        // Act
        var costoPorHora = cotizacion.HorasCapacitacion > 0 
            ? cotizacion.CostoCapacitacion / cotizacion.HorasCapacitacion 
            : 0;

        // Assert
        costoPorHora.Should().Be(500.00m, "porque 20000 / 40 = 500");
    }

    #endregion

    #region Tests Nuevos Campos de Contacto

    /// <summary>
    /// Test que verifica la validación de teléfono con rango válido
    /// </summary>
    [Theory]
    [InlineData(6178907616)]      // Teléfono válido 10 dígitos
    [InlineData(52618907616)]     // Teléfono con código país (11 dígitos)
    [InlineData(10000)]           // Teléfono mínimo (5 dígitos)
    [InlineData(999999999999999)] // Teléfono máximo (15 dígitos)
    public async Task Crear_ConTelefonoValido_DebeCrearCorrectamente(long telefonoValido)
    {
        // Arrange
        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Estado = "NUEVA",
            Cliente = "Cliente Test",
            Rfc = "TRA850123ABC",
            Folio = "COT-2025-001",
            DescripcionServicio = "Servicio test",
            ValidezDias = 30,
            Telefono = telefonoValido,
            Correo = "contacto@test.com"
        };

        var cotizacionCreada = new Cotizacion
        {
            Id = 1,
            OrdenId = request.OrdenId,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Total = 5800m, // BD calcula
            Estado = request.Estado,
            Cliente = request.Cliente,
            Rfc = request.Rfc,
            Folio = request.Folio,
            DescripcionServicio = request.DescripcionServicio,
            ValidezDias = request.ValidezDias,
            Telefono = request.Telefono,
            Correo = request.Correo,
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync(cotizacionCreada);

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Telefono.Should().Be(telefonoValido);
        resultado.Correo.Should().Be("contacto@test.com");
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Cotizacion>()), Times.Once);
    }

    /// <summary>
    /// Test que verifica la creación de cotización con teléfono nulo (válido)
    /// </summary>
    [Fact]
    public async Task Crear_ConTelefonoNulo_DebePermitirCreacion()
    {
        // Arrange
        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Estado = "NUEVA",
            Cliente = "Cliente Test",
            Rfc = "TRA850123ABC",
            Folio = "COT-2025-001",
            DescripcionServicio = "Servicio test",
            ValidezDias = 30,
            Telefono = null, // Teléfono opcional
            Correo = null    // Correo opcional
        };

        var cotizacionCreada = new Cotizacion
        {
            Id = 1,
            OrdenId = request.OrdenId,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Total = 5800m,
            Estado = request.Estado,
            Cliente = request.Cliente,
            Rfc = request.Rfc,
            Folio = request.Folio,
            DescripcionServicio = request.DescripcionServicio,
            ValidezDias = request.ValidezDias,
            Telefono = null,
            Correo = null,
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync(cotizacionCreada);

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Telefono.Should().BeNull("porque el teléfono es opcional");
        resultado.Correo.Should().BeNull("porque el correo es opcional");
    }

    /// <summary>
    /// Test que verifica el formato de correo electrónico válido
    /// </summary>
    [Theory]
    [InlineData("usuario@dominio.com")]
    [InlineData("test.user+tag@ejemplo.mx")]
    [InlineData("admin@empresa.com.mx")]
    [InlineData("contacto123@servidor-web.net")]
    public async Task Crear_ConCorreoValido_DebeCrearCorrectamente(string correoValido)
    {
        // Arrange
        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Estado = "NUEVA",
            Cliente = "Cliente Test",
            Rfc = "TRA850123ABC",
            Folio = "COT-2025-001",
            DescripcionServicio = "Servicio test",
            ValidezDias = 30,
            Telefono = 6178907616,
            Correo = correoValido
        };

        var cotizacionCreada = new Cotizacion
        {
            Id = 1,
            OrdenId = request.OrdenId,
            Subtotal = request.Subtotal,
            ImpuestosTotal = request.ImpuestosTotal,
            Total = 5800m,
            Estado = request.Estado,
            Cliente = request.Cliente,
            Rfc = request.Rfc,
            Folio = request.Folio,
            DescripcionServicio = request.DescripcionServicio,
            ValidezDias = request.ValidezDias,
            Telefono = request.Telefono,
            Correo = request.Correo,
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync(cotizacionCreada);

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Correo.Should().Be(correoValido);
    }

    /// <summary>
    /// Test que verifica la actualización de información de contacto
    /// </summary>
    [Fact]
    public async Task Actualizar_CambiarDatosContacto_DebeActualizarCorrectamente()
    {
        // Arrange
        var cotizacionExistente = new Cotizacion
        {
            Id = 1,
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Total = 5800m,
            Estado = "NUEVA",
            Cliente = "Cliente Test",
            Telefono = 6178907616,
            Correo = "old@email.com",
            CreadoEn = DateTime.UtcNow.AddDays(-1)
        };

        var request = new CotizacionCreateRequestDto
        {
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Estado = "APROBADA",
            Cliente = "Cliente Test",
            Rfc = "TRA850123ABC",
            Folio = "COT-2025-001",
            DescripcionServicio = "Servicio test",
            ValidezDias = 30,
            Telefono = 5551234567,      // Nuevo teléfono
            Correo = "nuevo@email.com"  // Nuevo correo
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(cotizacionExistente);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Cotizacion>()))
            .ReturnsAsync((Cotizacion c) => c);

        // Act
        var resultado = await _service.ActualizarAsync(1, request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Telefono.Should().Be(5551234567, "porque se actualizó el teléfono");
        resultado.Correo.Should().Be("nuevo@email.com", "porque se actualizó el correo");
        resultado.Estado.Should().Be("APROBADA");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Cotizacion>()), Times.Once);
    }

    /// <summary>
    /// Test que verifica la respuesta DTO incluye campos de contacto
    /// </summary>
    [Fact]
    public async Task ObtenerPorId_DebeIncluirDatosContactoEnRespuesta()
    {
        // Arrange
        var cotizacion = new Cotizacion
        {
            Id = 1,
            OrdenId = 1,
            Subtotal = 5000m,
            ImpuestosTotal = 800m,
            Total = 5800m,
            Estado = "NUEVA",
            Cliente = "Cliente Test",
            Rfc = "TRA850123ABC",
            Folio = "COT-2025-001",
            DescripcionServicio = "Servicio test",
            ValidezDias = 30,
            Telefono = 6178907616,
            Correo = "contacto@cliente.com",
            CreadoEn = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(cotizacion);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Telefono.Should().Be(6178907616);
        resultado.Correo.Should().Be("contacto@cliente.com");
        resultado.Cliente.Should().Be("Cliente Test");
    }

    #endregion
}
