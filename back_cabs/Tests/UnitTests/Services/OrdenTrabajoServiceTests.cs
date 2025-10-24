using back_cabs.CRM.contexts;
using back_cabs.CRM.enums;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Recepcion;
using back_cabs.CRM.services.Recepcion;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace back_cabs.Tests.UnitTests.Services;

/// <summary>
/// ✅ Tests unitarios para OrdenTrabajoService
/// 
/// 📚 QUÉ APRENDERÁS AQUÍ:
/// - Testear servicios con lógica de estados (EstadoOrden enum)
/// - Validar filtros dinámicos y paginación
/// - Probar operaciones CRUD complejas
/// - Testear validación de estados con Theory
/// - Manejo de excepciones y validaciones
/// 
/// 🎯 CASOS DE PRUEBA:
/// - Consulta con filtros de estado
/// - Paginación (skip/take)
/// - Validación de estados permitidos
/// - CRUD (crear, obtener, actualizar)
/// - Estadísticas
/// - Manejo de errores
/// </summary>
public class OrdenTrabajoServiceTests
{
    private readonly Mock<ReadOnlyContext> _mockReadContext;
    private readonly Mock<IOrdenTrabajoRepository> _mockRepository;
    private readonly Mock<ILogger<OrdenTrabajoService>> _mockLogger;
    private readonly OrdenTrabajoService _service;

    public OrdenTrabajoServiceTests()
    {
        _mockReadContext = new Mock<ReadOnlyContext>();
        _mockRepository = new Mock<IOrdenTrabajoRepository>();
        _mockLogger = new Mock<ILogger<OrdenTrabajoService>>();

        // ✅ ClientesLegacyValidationService es nullable para unit tests
        _service = new OrdenTrabajoService(
            _mockReadContext.Object,
            _mockRepository.Object,
            null,
            _mockLogger.Object
        );
    }

    // ==================== TESTS DE CONSULTA ====================

    [Fact]
    public async Task ObtenerTodasLasOrdenesAsync_SinFiltros_DebeRetornarTodasLasOrdenes()
    {
        // Arrange
        var ordenes = new List<OrdenTrabajo>
        {
            new OrdenTrabajo
            {
                Id = 1,
                Estado = EstadoOrden.CAPTURADA.ToDbValue(),
                ClienteId = 100,
                CreadoEn = DateTime.Now
            },
            new OrdenTrabajo
            {
                Id = 2,
                Estado = EstadoOrden.EN_CURSO.ToDbValue(),
                ClienteId = 101,
                CreadoEn = DateTime.Now
            }
        };

        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(null, null, null))
            .ReturnsAsync(ordenes);

        // Act
        var result = await _service.ObtenerTodasLasOrdenesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetAllFilteredAsync(null, null, null), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodasLasOrdenesAsync_ConPaginacion_DebeUsarSkipYTake()
    {
        // Arrange
        var skip = 10;
        var take = 5;
        var ordenes = new List<OrdenTrabajo>();

        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(skip, take, null))
            .ReturnsAsync(ordenes);

        // Act
        var result = await _service.ObtenerTodasLasOrdenesAsync(skip, take);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetAllFilteredAsync(skip, take, null), Times.Once);
    }

    [Theory]
    [InlineData("CAPTURADA")]
    [InlineData("ASIGNADA")]
    [InlineData("EN_CURSO")]
    [InlineData("COMPLETADA")]
    [InlineData("POR_FACTURAR")]
    [InlineData("FACTURADA")]
    [InlineData("CERRADA")]
    public async Task ObtenerTodasLasOrdenesAsync_ConEstadoValido_DebeFiltrarPorEstado(string estado)
    {
        // Arrange
        var ordenes = new List<OrdenTrabajo>
        {
            new OrdenTrabajo
            {
                Id = 1,
                Estado = estado,
                ClienteId = 100,
                CreadoEn = DateTime.Now
            }
        };

    

        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(null, null, estado))
            .ReturnsAsync(ordenes);

        // Act
        var result = await _service.ObtenerTodasLasOrdenesAsync(estado: estado);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Estado.Should().Be(estado);
        _mockRepository.Verify(r => r.GetAllFilteredAsync(null, null, estado), Times.Once);
    }

    [Theory]
    [InlineData("ESTADO_INVALIDO")]
    [InlineData("Capturada")] // Case sensitive
    [InlineData("")]
    [InlineData(" ")]
    public async Task ObtenerTodasLasOrdenesAsync_ConEstadoInvalido_DebeLanzarArgumentException(string estadoInvalido)
    {
        // Act
        Func<Task> act = async () => await _service.ObtenerTodasLasOrdenesAsync(estado: estadoInvalido);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*no es válido*");
    }

    [Fact]
    public async Task ObtenerOrdenPorIdAsync_ConIdExistente_DebeRetornarOrden()
    {
        // Arrange
        var ordenId = 1;
        var orden = new OrdenTrabajo
        {
            Id = ordenId,
            Estado = EstadoOrden.CAPTURADA.ToDbValue(),
            ClienteId = 100,
            Notas = "Orden de prueba",
            CreadoEn = DateTime.Now
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(ordenId))
            .ReturnsAsync(orden);

        // Act
        var result = await _service.ObtenerOrdenPorIdAsync(ordenId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ordenId);
        result.Estado.Should().Be(EstadoOrden.CAPTURADA.ToDbValue());
        _mockRepository.Verify(r => r.GetByIdAsync(ordenId), Times.Once);
    }

    [Fact]
    public async Task ObtenerOrdenPorIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var ordenId = 999;

        _mockRepository
            .Setup(r => r.GetByIdAsync(ordenId))
            .ReturnsAsync((OrdenTrabajo?)null);

        // Act
        var result = await _service.ObtenerOrdenPorIdAsync(ordenId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(ordenId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task ObtenerOrdenPorIdAsync_ConIdInvalido_DebeRetornarNull(int invalidId)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(invalidId))
            .ReturnsAsync((OrdenTrabajo?)null);

        // Act
        var result = await _service.ObtenerOrdenPorIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    // ==================== TESTS DE CREACIÓN ====================

    [Fact]
    public async Task CrearOrdenTrabajoAsync_ConClienteNuevo_DebeCrearOrden()
    {
        // Arrange
        var request = new OrdenTrabajoRequestDto
        {
            NuevoCliente = true,
            NombreCliente = "Juan Pérez",
            ClienteTelefono = 5551234567,
            Notas = "Primera visita",
            CitaProgramadaInicio = DateTime.Now.AddDays(1),
            Modalidad = "presencial",
            TipoOrden = "servicio"
        };

        var ordenCreada = new OrdenTrabajo
        {
            Id = 1,
            NuevoCliente = true,
            NombreCliente = request.NombreCliente,
            ClienteTelefono = request.ClienteTelefono,
            Notas = request.Notas,
            Estado = EstadoOrden.CAPTURADA.ToDbValue(),
            CreadoEn = DateTime.Now
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(ordenCreada);

        // Act
        var result = await _service.CrearOrdenTrabajoAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.NombreCliente.Should().Be(request.NombreCliente);
        
        _mockRepository.Verify(r => r.CreateAsync(It.Is<OrdenTrabajo>(o =>
            o.NuevoCliente == true &&
            o.NombreCliente == request.NombreCliente &&
            o.Estado == EstadoOrden.CAPTURADA.ToDbValue()
        )), Times.Once);
    }

    [Fact]
    public async Task CrearOrdenTrabajoAsync_ConClienteLegacy_DebeCrearOrden()
    {
        // Arrange
        var request = new OrdenTrabajoRequestDto
        {
            NuevoCliente = false,
            ClienteId = 100,
            Notas = "Cliente existente",
            CitaProgramadaInicio = DateTime.Now.AddDays(1),
            Modalidad = "presencial",
            TipoOrden = "servicio"
        };

        var ordenCreada = new OrdenTrabajo
        {
            Id = 2,
            NuevoCliente = false,
            ClienteId = request.ClienteId,
            Notas = request.Notas,
            Estado = EstadoOrden.CAPTURADA.ToDbValue(),
            CreadoEn = DateTime.Now
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(ordenCreada);

        // Act
        var result = await _service.CrearOrdenTrabajoAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.ClienteId.Should().Be(request.ClienteId);
        
        _mockRepository.Verify(r => r.CreateAsync(It.Is<OrdenTrabajo>(o =>
            o.NuevoCliente == false &&
            o.ClienteId == request.ClienteId &&
            o.Estado == EstadoOrden.CAPTURADA.ToDbValue()
        )), Times.Once);
    }

    [Fact]
    public async Task CrearOrdenTrabajoAsync_DebeEstablecerEstadoInicialCAPTURADA()
    {
        // Arrange
        var request = new OrdenTrabajoRequestDto
        {
            NuevoCliente = false,
            ClienteId = 100,
            CitaProgramadaInicio = DateTime.Now.AddDays(1),
            Modalidad = "presencial",
            TipoOrden = "servicio"
        };

        OrdenTrabajo? ordenCapturada = null;
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<OrdenTrabajo>()))
            .Callback<OrdenTrabajo>(o => ordenCapturada = o)
            .ReturnsAsync((OrdenTrabajo o) => o);

        // Act
        await _service.CrearOrdenTrabajoAsync(request);

        // Assert
        ordenCapturada.Should().NotBeNull();
        ordenCapturada!.Estado.Should().Be(EstadoOrden.CAPTURADA.ToDbValue());
    }

    // ==================== TESTS DE ACTUALIZACIÓN ====================

    [Fact]
    public async Task ActualizarOrdenTrabajoAsync_ConOrdenExistente_DebeActualizar()
    {
        // Arrange
        var ordenId = 1;
        var updateRequest = new OrdenTrabajoUpdateRequestDto
        {
            Estado = EstadoOrden.EN_CURSO.ToDbValue(),
            Notas = "Actualización de notas"
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ActualizarOrdenTrabajoAsync(ordenId, updateRequest);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarOrdenTrabajoAsync_ConOrdenInexistente_DebeRetornarFalse()
    {
        // Arrange
        var ordenId = 999;
        var updateRequest = new OrdenTrabajoUpdateRequestDto
        {
            Estado = EstadoOrden.EN_CURSO.ToDbValue()
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ActualizarOrdenTrabajoAsync(ordenId, updateRequest);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("CAPTURADA", "ASIGNADA")] // Transición válida
    [InlineData("ASIGNADA", "EN_CURSO")]
    [InlineData("EN_CURSO", "COMPLETADA")]
    public async Task ActualizarOrdenTrabajoAsync_ConTransicionDeEstado_DebePermitir(string estadoInicial, string estadoFinal)
    {
        // Arrange
        var ordenId = 1;
        var updateRequest = new OrdenTrabajoUpdateRequestDto
        {
            Estado = estadoFinal
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ActualizarOrdenTrabajoAsync(ordenId, updateRequest);

        // Assert
        result.Should().BeTrue();
    }

    // ==================== TESTS DE ESTADÍSTICAS ====================

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConOrdenes_DebeRetornarEstadisticas()
    {
        // Arrange
        var ordenes = new List<OrdenTrabajo>
        {
            new OrdenTrabajo { Id = 1, Estado = EstadoOrden.CAPTURADA.ToDbValue() },
            new OrdenTrabajo { Id = 2, Estado = EstadoOrden.EN_CURSO.ToDbValue() },
            new OrdenTrabajo { Id = 3, Estado = EstadoOrden.COMPLETADA.ToDbValue() },
            new OrdenTrabajo { Id = 4, Estado = EstadoOrden.CAPTURADA.ToDbValue() }
        };

        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(null, null, null))
            .ReturnsAsync(ordenes);

        // Act
        var result = await _service.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetAllFilteredAsync(null, null, null), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_SinOrdenes_DebeRetornarEstadisticasVacias()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(null, null, null))
            .ReturnsAsync(new List<OrdenTrabajo>());

        // Act
        var result = await _service.ObtenerEstadisticasAsync();

        // Assert
        result.Should().NotBeNull();
    }

    // ==================== TESTS DE VALIDACIÓN ====================

    [Fact]
    public async Task CrearOrdenTrabajoAsync_ConRequestNull_DebeLanzarExcepcion()
    {
        // Arrange
        OrdenTrabajoRequestDto? request = null;

        // Act
        Func<Task> act = async () => await _service.CrearOrdenTrabajoAsync(request!);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ActualizarOrdenTrabajoAsync_ConIdInvalido_DebeRetornarFalse()
    {
        // Arrange
        var invalidId = -1;
        var updateRequest = new OrdenTrabajoUpdateRequestDto();

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ActualizarOrdenTrabajoAsync(invalidId, updateRequest);

        // Assert
        result.Should().BeFalse();
    }

    // ==================== TESTS DE MANEJO DE ERRORES ====================

    [Fact]
    public async Task ObtenerTodasLasOrdenesAsync_CuandoRepositoryFalla_DebeLanzarExcepcion()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllFilteredAsync(null, null, null))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        Func<Task> act = async () => await _service.ObtenerTodasLasOrdenesAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Error de base de datos*");
    }

    [Fact]
    public async Task ObtenerOrdenPorIdAsync_CuandoRepositoryFalla_DebeLanzarExcepcion()
    {
        // Arrange
        var ordenId = 1;
        _mockRepository
            .Setup(r => r.GetByIdAsync(ordenId))
            .ThrowsAsync(new Exception("Error al consultar"));

        // Act
        Func<Task> act = async () => await _service.ObtenerOrdenPorIdAsync(ordenId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CrearOrdenTrabajoAsync_CuandoRepositoryFalla_DebeLanzarExcepcion()
    {
        // Arrange
        var request = new OrdenTrabajoRequestDto
        {
            NuevoCliente = false,
            ClienteId = 100,
            CitaProgramadaInicio = DateTime.Now.AddDays(1),
            Modalidad = "presencial",
            TipoOrden = "servicio"
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<OrdenTrabajo>()))
            .ThrowsAsync(new Exception("Error al crear"));

        // Act
        Func<Task> act = async () => await _service.CrearOrdenTrabajoAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ActualizarOrdenTrabajoAsync_CuandoRepositoryFalla_DebeLanzarExcepcion()
    {
        // Arrange
        var ordenId = 1;
        var updateRequest = new OrdenTrabajoUpdateRequestDto
        {
            Estado = EstadoOrden.EN_CURSO.ToDbValue()
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<OrdenTrabajo>()))
            .ThrowsAsync(new Exception("Error al actualizar"));

        // Act
        Func<Task> act = async () => await _service.ActualizarOrdenTrabajoAsync(ordenId, updateRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
