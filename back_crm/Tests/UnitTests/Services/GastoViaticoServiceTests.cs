using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.Services.Shared;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;
using Microsoft.Extensions.Logging;

namespace back_cabs.Tests.UnitTests.Services;

/// <summary>
/// ✅ Tests unitarios para GastoViaticoService
/// Estos tests verifican la lógica de negocio sin tocar la base de datos real.
/// 
/// 📚 CONCEPTOS CLAVE:
/// - [Fact]: Test simple sin parámetros
/// - [Theory]: Test con múltiples datos de prueba
/// - Arrange-Act-Assert (AAA): Patrón de organización de tests
/// - Mock: Simula dependencias (repository, logger, etc.)
/// - FluentAssertions: Verificaciones legibles (Should().Be(), etc.)
/// </summary>
public class GastoViaticoServiceTests
{
    private readonly Mock<IGastoViaticoRepository> _mockRepository;
    private readonly Mock<ILogger<GastoViaticoService>> _mockLogger;
    private readonly GastoViaticoService _service;

    /// <summary>
    /// 🏗️ Constructor: Se ejecuta ANTES de cada test
    /// Aquí configuramos los mocks y creamos la instancia del servicio
    /// </summary>
    public GastoViaticoServiceTests()
    {
        // 1. Crear mocks de las dependencias
        _mockRepository = new Mock<IGastoViaticoRepository>();
        _mockLogger = new Mock<ILogger<GastoViaticoService>>();
        
        // 2. Inyectar los mocks al servicio
        _service = new GastoViaticoService(
            _mockRepository.Object,  // .Object convierte el mock en la interfaz
            _mockLogger.Object
        );
    }

    #region CreateViaticoAsync Tests

    /// <summary>
    /// ✅ Test básico: Verifica que el servicio crea un viático correctamente
    /// 
    /// 📋 PATRÓN AAA:
    /// Arrange: Prepara datos de prueba y configura mocks
    /// Act: Ejecuta el método que estamos probando
    /// Assert: Verifica que el resultado es correcto
    /// </summary>
    [Fact]
    public async Task CreateViaticoAsync_ConDatosValidos_DebeCrearViaticoCorrectamente()
    {
        // 📋 Arrange: Preparar datos de prueba
        var createDto = new GastoViaticoCreateRequestDto
        {
            OrdenId = 1,
            TieneFactura = true,
            Descripcion = "Gasolina para viaje a Monterrey",
            ProveedorNombre = "Gasolinera Shell",
            Fecha = DateTime.Now,
            KmRecorridos = 450,
            Gastos = "1500.00",
            MontoTotal = 1500.00m,
            LugarDestino = "Monterrey, NL"
        };

        var viaticoGuardado = new GastoViatico
        {
            Id = 1,
            OrdenId = createDto.OrdenId,
            TieneFactura = createDto.TieneFactura,
            Descripcion = createDto.Descripcion,
            ProveedorNombre = createDto.ProveedorNombre,
            Fecha = createDto.Fecha,
            KmRecorridos = createDto.KmRecorridos,
            Gastos = createDto.Gastos,
            MontoTotal = createDto.MontoTotal,
            LugarDestino = createDto.LugarDestino
        };

        // Configurar el mock: cuando llamen CreateViaticoAsync, simular guardado exitoso
        _mockRepository
            .Setup(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()))
            .ReturnsAsync((GastoViatico v) => { v.Id = 1; return v; });

        // 🎬 Act: Ejecutar el método a probar
        var resultado = await _service.CreateViaticoAsync(createDto);

        // ✅ Assert: Verificar que funciona correctamente
        resultado.Should().NotBeNull("porque debe retornar el viático creado");
        resultado.Id.Should().BeGreaterThan(0, "porque debe tener un ID asignado");
        resultado.OrdenId.Should().Be(createDto.OrdenId);
        resultado.Descripcion.Should().Be(createDto.Descripcion);
        resultado.ProveedorNombre.Should().Be(createDto.ProveedorNombre);
        resultado.MontoTotal.Should().Be(createDto.MontoTotal);
        resultado.LugarDestino.Should().Be(createDto.LugarDestino);
        
        // Verificar que SÍ llamó al repositorio
        _mockRepository.Verify(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()), Times.Once, 
            "debe crear el viático en el repositorio");
    }

    [Fact]
    public async Task CreateViaticoAsync_SinFactura_DebeCrearConTieneFacturaFalse()
    {
        // Arrange
        var createDto = new GastoViaticoCreateRequestDto
        {
            OrdenId = 2,
            TieneFactura = false,
            Descripcion = "Peajes y casetas",
            Fecha = DateTime.Now,
            Gastos = "350.00",
            MontoTotal = 350.00m,
            LugarDestino = "Carretera México-Querétaro"
        };

        _mockRepository
            .Setup(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()))
            .ReturnsAsync((GastoViatico v) => { v.Id = 2; return v; });

        // Act
        var resultado = await _service.CreateViaticoAsync(createDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.TieneFactura.Should().BeFalse("porque se especificó sin factura");
        resultado.ProveedorNombre.Should().BeNullOrEmpty("porque puede no tener proveedor sin factura");
        
        _mockRepository.Verify(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()), Times.Once);
    }

    /// <summary>
    /// 🎯 [Theory]: Test que verifica diferentes montos
    /// </summary>
    [Theory]
    [InlineData("100.00", 0, "Estacionamiento")]
    [InlineData("1500.50", 250, "Gasolina viaje largo")]
    [InlineData("5000.00", 800, "Hospedaje y alimentos")]
    public async Task CreateViaticoAsync_ConDiferentesMontos_DebeCrearCorrectamente(
        string gastos, 
        int kmRecorridos,
        string descripcion)
    {
        // Arrange
        var createDto = new GastoViaticoCreateRequestDto
        {
            OrdenId = 1,
            TieneFactura = true,
            Descripcion = descripcion,
            Fecha = DateTime.Now,
            KmRecorridos = kmRecorridos,
            Gastos = gastos,
            MontoTotal = decimal.Parse(gastos),
            LugarDestino = "Ciudad de México"
        };

        _mockRepository
            .Setup(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()))
            .ReturnsAsync((GastoViatico v) => { v.Id = 1; return v; });

        // Act
        var resultado = await _service.CreateViaticoAsync(createDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Gastos.Should().Be(gastos);
        resultado.MontoTotal.Should().Be(decimal.Parse(gastos));
        resultado.KmRecorridos.Should().Be(kmRecorridos);
        resultado.Descripcion.Should().Be(descripcion);
    }

    #endregion

    #region GetViaticosAsync Tests (Paginación)

    [Fact]
    public async Task GetViaticosAsync_SinFiltros_DebeRetornarTodosLosViaticos()
    {
        // Arrange
        var viaticosSimulados = new List<GastoViatico>
        {
            new GastoViatico 
            { 
                Id = 1,
                OrdenId = 1,
                Descripcion = "Gasolina",
                Gastos = "1500.00",
                MontoTotal = 1500.00m,
                Fecha = DateTime.Now.AddDays(-5)
            },
            new GastoViatico 
            { 
                Id = 2,
                OrdenId = 1,
                Descripcion = "Peajes",
                Gastos = "350.00",
                MontoTotal = 350.00m,
                Fecha = DateTime.Now.AddDays(-3)
            },
            new GastoViatico 
            { 
                Id = 3,
                OrdenId = 2,
                Descripcion = "Hospedaje",
                Gastos = "2000.00",
                MontoTotal = 2000.00m,
                Fecha = DateTime.Now.AddDays(-1)
            }
        };

        _mockRepository
            .Setup(r => r.GetViaticosFilteredAsync(
                It.IsAny<int?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((viaticosSimulados, 3)); // (items, totalCount)

        // Act
        var resultado = await _service.GetViaticosAsync(
            ordenId: null,
            fechaDesde: null,
            fechaHasta: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(3, "porque hay 3 viáticos en total");
        resultado.TotalItems.Should().Be(3);
        resultado.Pagina.Should().Be(1);
        resultado.ResultadosPorPagina.Should().Be(10);
        resultado.TotalPaginas.Should().Be(1, "porque todos caben en una página");
        
        _mockRepository.Verify(r => r.GetViaticosFilteredAsync(
            null, null, null, 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetViaticosAsync_ConFiltroOrdenId_DebeRetornarSoloEsaOrden()
    {
        // Arrange
        var viaticosOrden1 = new List<GastoViatico>
        {
            new GastoViatico { Id = 1, OrdenId = 1, Descripcion = "Gasolina", Gastos = "1500.00", MontoTotal = 1500.00m },
            new GastoViatico { Id = 2, OrdenId = 1, Descripcion = "Peajes", Gastos = "350.00", MontoTotal = 350.00m }
        };

        _mockRepository
            .Setup(r => r.GetViaticosFilteredAsync(1, null, null, 1, 10))
            .ReturnsAsync((viaticosOrden1, 2));

        // Act
        var resultado = await _service.GetViaticosAsync(
            ordenId: 1,
            fechaDesde: null,
            fechaHasta: null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        resultado.Items.Should().HaveCount(2);
        resultado.Items.Should().OnlyContain(v => v.OrdenId == 1, 
            "porque filtramos solo por OrdenId = 1");
        resultado.TotalItems.Should().Be(2);
        
        _mockRepository.Verify(r => r.GetViaticosFilteredAsync(1, null, null, 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetViaticosAsync_ConRangoFechas_DebeRetornarSoloEsePeriodo()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 11, 1);
        var fechaHasta = new DateTime(2024, 11, 30);

        var viaticosNoviembre = new List<GastoViatico>
        {
            new GastoViatico 
            { 
                Id = 1, 
                OrdenId = 1, 
                Descripcion = "Gasto noviembre", 
                Fecha = new DateTime(2024, 11, 15),
                Gastos = "1000.00",
                MontoTotal = 1000.00m 
            }
        };

        _mockRepository
            .Setup(r => r.GetViaticosFilteredAsync(null, fechaDesde, fechaHasta, 1, 10))
            .ReturnsAsync((viaticosNoviembre, 1));

        // Act
        var resultado = await _service.GetViaticosAsync(
            ordenId: null,
            fechaDesde: fechaDesde,
            fechaHasta: fechaHasta,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        resultado.Items.Should().HaveCount(1);
        resultado.Items.First().Fecha.Should().BeOnOrAfter(fechaDesde);
        resultado.Items.First().Fecha.Should().BeOnOrBefore(fechaHasta);
        
        _mockRepository.Verify(r => r.GetViaticosFilteredAsync(null, fechaDesde, fechaHasta, 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetViaticosAsync_SinResultados_DebeRetornarListaVacia()
    {
        // Arrange
        var listaVacia = new List<GastoViatico>();
        
        _mockRepository
            .Setup(r => r.GetViaticosFilteredAsync(
                It.IsAny<int?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((listaVacia, 0));

        // Act
        var resultado = await _service.GetViaticosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty("porque no hay viáticos en la BD");
        resultado.TotalItems.Should().Be(0);
        resultado.TotalPaginas.Should().Be(0);
    }

    #endregion

    #region GetViaticoByIdAsync Tests

    [Fact]
    public async Task GetViaticoByIdAsync_ConIdExistente_DebeRetornarViatico()
    {
        // Arrange
        var viaticoEsperado = new GastoViatico
        {
            Id = 1,
            OrdenId = 1,
            TieneFactura = true,
            Descripcion = "Gasolina para servicio",
            ProveedorNombre = "Shell",
            Fecha = DateTime.Now,
            KmRecorridos = 300,
            Gastos = "1200.00",
            MontoTotal = 1200.00m,
            LugarDestino = "Guadalajara"
        };

        _mockRepository
            .Setup(r => r.GetViaticoByIdReadOnlyAsync(1))
            .ReturnsAsync(viaticoEsperado);

        // Act
        var resultado = await _service.GetViaticoByIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Descripcion.Should().Be("Gasolina para servicio");
        resultado.ProveedorNombre.Should().Be("Shell");
        resultado.MontoTotal.Should().Be(1200.00m);
        resultado.LugarDestino.Should().Be("Guadalajara");
        resultado.KmRecorridos.Should().Be(300);
        
        _mockRepository.Verify(r => r.GetViaticoByIdReadOnlyAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetViaticoByIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetViaticoByIdReadOnlyAsync(999))
            .ReturnsAsync((GastoViatico)null!);

        // Act
        var resultado = await _service.GetViaticoByIdAsync(999);

        // Assert
        resultado.Should().BeNull("porque el viático con ID 999 no existe");
        _mockRepository.Verify(r => r.GetViaticoByIdReadOnlyAsync(999), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetViaticoByIdAsync_ConIdInvalido_DebeRetornarNull(int idInvalido)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetViaticoByIdReadOnlyAsync(It.IsAny<int>()))
            .ReturnsAsync((GastoViatico)null!);

        // Act
        var resultado = await _service.GetViaticoByIdAsync(idInvalido);

        // Assert
        resultado.Should().BeNull($"porque el ID {idInvalido} es inválido");
    }

    #endregion

    #region UpdateViaticoAsync Tests

    [Fact]
    public async Task UpdateViaticoAsync_ConDatosValidos_DebeActualizarCorrectamente()
    {
        // Arrange
        var viaticoExistente = new GastoViatico
        {
            Id = 1,
            OrdenId = 1,
            TieneFactura = false,
            Descripcion = "Descripción original",
            Gastos = "1000.00",
            MontoTotal = 1000.00m,
            LugarDestino = "Original"
        };

        var updateDto = new GastoViaticoUpdateRequestDto
        {
            TieneFactura = true,
            Descripcion = "Descripción actualizada",
            ProveedorNombre = "Nuevo Proveedor",
            Fecha = DateTime.Now,
            KmRecorridos = 500,
            Gastos = "1500.00",
            MontoTotal = 1500.00m,
            LugarDestino = "Nuevo destino"
        };

        _mockRepository
            .Setup(r => r.GetViaticoByIdForUpdateAsync(1))
            .ReturnsAsync(viaticoExistente);

        _mockRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.UpdateViaticoAsync(1, updateDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.TieneFactura.Should().BeTrue("porque se actualizó a true");
        resultado.Descripcion.Should().Be("Descripción actualizada");
        resultado.ProveedorNombre.Should().Be("Nuevo Proveedor");
        resultado.Gastos.Should().Be("1500.00");
        resultado.MontoTotal.Should().Be(1500.00m);
        resultado.LugarDestino.Should().Be("Nuevo destino");
        resultado.KmRecorridos.Should().Be(500);
        
        _mockRepository.Verify(r => r.GetViaticoByIdForUpdateAsync(1), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateViaticoAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var updateDto = new GastoViaticoUpdateRequestDto
        {
            Descripcion = "Test",
            Gastos = "100.00",
            MontoTotal = 100m
        };

        _mockRepository
            .Setup(r => r.GetViaticoByIdForUpdateAsync(999))
            .ReturnsAsync((GastoViatico)null!);

        // Act
        var resultado = await _service.UpdateViaticoAsync(999, updateDto);

        // Assert
        resultado.Should().BeNull("porque el viático con ID 999 no existe");
        _mockRepository.Verify(r => r.GetViaticoByIdForUpdateAsync(999), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never, 
            "no debe intentar guardar si no existe el viático");
    }

    #endregion

    #region Validación de Mapeo (DTO a Entity y viceversa)

    [Fact]
    public async Task CreateViaticoAsync_DebeMapearCorrectamenteDtoAEntity()
    {
        // Arrange
        var dto = new GastoViaticoCreateRequestDto
        {
            OrdenId = 5,
            TieneFactura = true,
            Descripcion = "Test Mapeo",
            ProveedorNombre = "Proveedor Test",
            Fecha = new DateTime(2024, 11, 15),
            KmRecorridos = 250,
            Gastos = "800.50",
            MontoTotal = 800.50m,
            LugarDestino = "Querétaro"
        };

        GastoViatico? viaticoCreado = null;

        _mockRepository
            .Setup(r => r.CreateViaticoAsync(It.IsAny<GastoViatico>()))
            .ReturnsAsync((GastoViatico v) => 
            {
                viaticoCreado = v;
                v.Id = 10;
                return v;
            });

        // Act
        await _service.CreateViaticoAsync(dto);

        // Assert - Verificar que el mapeo fue correcto
        viaticoCreado.Should().NotBeNull();
        viaticoCreado!.OrdenId.Should().Be(dto.OrdenId);
        viaticoCreado.TieneFactura.Should().Be(dto.TieneFactura);
        viaticoCreado.Descripcion.Should().Be(dto.Descripcion);
        viaticoCreado.ProveedorNombre.Should().Be(dto.ProveedorNombre);
        viaticoCreado.Fecha.Should().Be(dto.Fecha);
        viaticoCreado.KmRecorridos.Should().Be(dto.KmRecorridos);
        viaticoCreado.Gastos.Should().Be(dto.Gastos);
        viaticoCreado.MontoTotal.Should().Be(dto.MontoTotal);
        viaticoCreado.LugarDestino.Should().Be(dto.LugarDestino);
    }

    #endregion
}
