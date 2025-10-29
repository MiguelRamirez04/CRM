using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.services.Fleet;
using back_cabs.CRM.Interfaces.Shared;
using back_cabs.CRM.models.Shared;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.services;
using Microsoft.Extensions.Logging;

namespace back_cabs.Tests.UnitTests.Services;

/// <summary>
/// ✅ Tests unitarios para VehiculosService
/// 
/// 📚 QUÉ APRENDERÁS AQUÍ:
/// - Mockear múltiples dependencias (Repository + Logger + Cache)
/// - Verificar que el servicio usa el caché correctamente
/// - Probar validaciones de negocio (placas únicas)
/// - Testear excepciones esperadas (InvalidOperationException)
/// - Usar [Theory] con múltiples escenarios
/// </summary>
public class VehiculosServiceTests
{
    private readonly Mock<IVehiculoRepository> _mockRepository;
    private readonly Mock<ILogger<VehiculosService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCache;
    private readonly VehiculosService _service;

    /// <summary>
    /// 🏗️ Constructor: Setup común para todos los tests
    /// </summary>
    public VehiculosServiceTests()
    {
        _mockRepository = new Mock<IVehiculoRepository>();
        _mockLogger = new Mock<ILogger<VehiculosService>>();
        _mockCache = new Mock<ICacheService>();
        
        _service = new VehiculosService(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    #region ObtenerTodosAsync Tests

    [Fact]
    public async Task ObtenerTodosAsync_ConCacheHit_DebeRetornarDesdeCacheYNoLlamarRepositorio()
    {
        // Arrange
        var vehiculosEnCache = new List<VehiculoResponseDto>
        {
            new VehiculoResponseDto { Id = 1, Placas = "ABC123", TipoVehiculo = "Sedan", Activo = true },
            new VehiculoResponseDto { Id = 2, Placas = "XYZ789", TipoVehiculo = "SUV", Activo = true }
        };

        // Configurar que el cache TIENE datos (cache hit)
        _mockCache
            .Setup(c => c.GetAsync<IEnumerable<VehiculoResponseDto>>("vehiculos:active"))
            .ReturnsAsync(vehiculosEnCache);

        // Act
        var resultado = await _service.ObtenerTodosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.Should().BeEquivalentTo(vehiculosEnCache);
        
        // ✅ IMPORTANTE: Verificar que NO llamó al repositorio (cache hit)
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Never, 
            "porque debería obtener los datos del caché, no de la BD");
        
        // ✅ Verificar que SÍ llamó al cache
        _mockCache.Verify(c => c.GetAsync<IEnumerable<VehiculoResponseDto>>("vehiculos:active"), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosAsync_ConCacheMiss_DebeConsultarBDYGuardarEnCache()
    {
        // Arrange
        var vehiculosBD = new List<Vehiculo>
        {
            new Vehiculo { Id = 1, Placas = "ABC123", TipoVehiculo = "Sedan", Activo = true, EsDeEmpresa = true },
            new Vehiculo { Id = 2, Placas = "XYZ789", TipoVehiculo = "SUV", Activo = true, EsDeEmpresa = true }
        };

        // Cache vacío (cache miss)
        _mockCache
            .Setup(c => c.GetAsync<IEnumerable<VehiculoResponseDto>>("vehiculos:active"))
            .ReturnsAsync((IEnumerable<VehiculoResponseDto>)null!);

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(vehiculosBD);

        // Act
        var resultado = await _service.ObtenerTodosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Placas.Should().Be("ABC123");
        
        // ✅ Verificar que SÍ llamó al repositorio (cache miss)
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once, 
            "porque no había datos en caché y debe consultar la BD");
        
        // ✅ Verificar que guardó en caché los resultados
        _mockCache.Verify(c => c.SetAsync(
            "vehiculos:active",
            It.IsAny<IEnumerable<VehiculoResponseDto>>(),
            It.IsAny<TimeSpan>()
        ), Times.Once, "porque debe guardar los resultados en caché para futuras consultas");
    }

    [Fact]
    public async Task ObtenerTodosAsync_ConBDVacia_DebeRetornarListaVacia()
    {
        // Arrange
        _mockCache
            .Setup(c => c.GetAsync<IEnumerable<VehiculoResponseDto>>("vehiculos:active"))
            .ReturnsAsync((IEnumerable<VehiculoResponseDto>)null!);

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Vehiculo>());

        // Act
        var resultado = await _service.ObtenerTodosAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty("porque no hay vehículos en la BD");
    }

    #endregion

    #region ObtenerPorIdAsync Tests

    [Fact]
    public async Task ObtenerPorIdAsync_ConCacheHit_DebeRetornarDesdeCacheYNoLlamarRepositorio()
    {
        // Arrange
        var vehiculoEnCache = new VehiculoResponseDto 
        { 
            Id = 1, 
            Placas = "ABC123", 
            TipoVehiculo = "Sedan",
            Activo = true 
        };

        _mockCache
            .Setup(c => c.GetAsync<VehiculoResponseDto>("vehiculos:id:1"))
            .ReturnsAsync(vehiculoEnCache);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Placas.Should().Be("ABC123");
        
        // No debe llamar al repositorio
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConCacheMissYVehiculoExiste_DebeConsultarBDYGuardarEnCache()
    {
        // Arrange
        var vehiculoBD = new Vehiculo 
        { 
            Id = 5, 
            Placas = "TEST123", 
            TipoVehiculo = "Pickup",
            Transmision = "Manual",
            Activo = true,
            EsDeEmpresa = true
        };

        _mockCache
            .Setup(c => c.GetAsync<VehiculoResponseDto>("vehiculos:id:5"))
            .ReturnsAsync((VehiculoResponseDto)null!);

        _mockRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(vehiculoBD);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(5);
        resultado.Placas.Should().Be("TEST123");
        resultado.TipoVehiculo.Should().Be("Pickup");
        
        // Verificar que llamó al repositorio
        _mockRepository.Verify(r => r.GetByIdAsync(5), Times.Once);
        
        // Verificar que guardó en caché
        _mockCache.Verify(c => c.SetAsync(
            "vehiculos:id:5",
            It.IsAny<VehiculoResponseDto>(),
            It.IsAny<TimeSpan>()
        ), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        _mockCache
            .Setup(c => c.GetAsync<VehiculoResponseDto>("vehiculos:id:999"))
            .ReturnsAsync((VehiculoResponseDto)null!);

        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Vehiculo)null!);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(999);

        // Assert
        resultado.Should().BeNull("porque el vehículo con ID 999 no existe");
        
        // No debe guardar null en caché
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<VehiculoResponseDto>(),
            It.IsAny<TimeSpan>()
        ), Times.Never, "porque no debe cachear resultados null");
    }

    #endregion

    #region CrearAsync Tests

    [Fact]
    public async Task CrearAsync_ConDatosValidos_DebeCrearVehiculoEInvalidarCache()
    {
        // Arrange
        var dto = new VehiculoRequestDto
        {
            Placas = "NEW123",
            TipoVehiculo = "Sedan",
            Transmision = "Automática",
            EsDeEmpresa = true,
            Activo = true,
            Observaciones = "Vehículo nuevo"
        };

        var vehiculoCreado = new Vehiculo
        {
            Id = 10,
            Placas = dto.Placas,
            TipoVehiculo = dto.TipoVehiculo,
            Transmision = dto.Transmision,
            EsDeEmpresa = dto.EsDeEmpresa,
            Activo = dto.Activo,
            Observaciones = dto.Observaciones
        };

        // Las placas NO existen (validación pasa)
        _mockRepository
            .Setup(r => r.PlacasExistAsync("NEW123"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Vehiculo>()))
            .ReturnsAsync(vehiculoCreado);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(10);
        resultado.Placas.Should().Be("NEW123");
        resultado.TipoVehiculo.Should().Be("Sedan");
        
        // ✅ Verificar que validó las placas
        _mockRepository.Verify(r => r.PlacasExistAsync("NEW123"), Times.Once);
        
        // ✅ Verificar que creó el vehículo
        _mockRepository.Verify(r => r.CreateAsync(It.Is<Vehiculo>(v => 
            v.Placas == "NEW123" &&
            v.TipoVehiculo == "Sedan"
        )), Times.Once);
        
        // ✅ IMPORTANTE: Verificar que invalidó el caché del listado
        _mockCache.Verify(c => c.RemoveAsync("vehiculos:active"), Times.Once,
            "porque al crear un vehículo nuevo, el caché del listado queda desactualizado");
    }

    [Fact]
    public async Task CrearAsync_ConPlacasDuplicadas_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var dto = new VehiculoRequestDto
        {
            Placas = "DUPLICADO",
            TipoVehiculo = "Sedan"
        };

        // Las placas YA EXISTEN
        _mockRepository
            .Setup(r => r.PlacasExistAsync("DUPLICADO"))
            .ReturnsAsync(true);

        // Act & Assert
        var accion = async () => await _service.CrearAsync(dto);
        
        // ✅ Verificar que lanza la excepción esperada
        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Las placas 'DUPLICADO' ya están registradas.");
        
        // ✅ Verificar que NO intentó crear (validación lo detuvo)
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Vehiculo>()), Times.Never,
            "porque la validación debe detener la creación");
    }

    [Fact]
    public async Task CrearAsync_ConPlacasNullOVacias_NoDebeValidarUnicidad()
    {
        // Arrange
        var dto = new VehiculoRequestDto
        {
            Placas = null, // Sin placas
            TipoVehiculo = "Sedan"
        };

        var vehiculoCreado = new Vehiculo
        {
            Id = 20,
            Placas = null,
            TipoVehiculo = "Sedan",
            Activo = true,
            EsDeEmpresa = true
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Vehiculo>()))
            .ReturnsAsync(vehiculoCreado);

        // Act
        var resultado = await _service.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        
        // ✅ NO debe validar unicidad si las placas están vacías
        _mockRepository.Verify(r => r.PlacasExistAsync(It.IsAny<string>()), Times.Never,
            "porque no hay placas que validar");
    }

    #endregion

    #region ActualizarAsync Tests

    [Fact]
    public async Task ActualizarAsync_ConDatosValidos_DebeActualizarEInvalidarCache()
    {
        // Arrange
        var vehiculoExistente = new Vehiculo
        {
            Id = 1,
            Placas = "OLD123",
            TipoVehiculo = "Sedan",
            Activo = true,
            EsDeEmpresa = true
        };

        var dto = new VehiculoRequestDto
        {
            Placas = "OLD123", // Mismas placas (no cambia)
            TipoVehiculo = "SUV", // Cambio de tipo
            Transmision = "Automática",
            Activo = true,
            EsDeEmpresa = true
        };

        var vehiculoActualizado = new Vehiculo
        {
            Id = 1,
            Placas = dto.Placas,
            TipoVehiculo = dto.TipoVehiculo,
            Transmision = dto.Transmision,
            Activo = dto.Activo,
            EsDeEmpresa = dto.EsDeEmpresa
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(vehiculoExistente);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Vehiculo>()))
            .ReturnsAsync(vehiculoActualizado);

        // Act
        var resultado = await _service.ActualizarAsync(1, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.TipoVehiculo.Should().Be("SUV", "porque se actualizó el tipo");
        
        // ✅ NO debe validar unicidad porque las placas no cambiaron
        _mockRepository.Verify(r => r.PlacasExistAsync(It.IsAny<string>()), Times.Never);
        
        // ✅ Debe invalidar AMBOS cachés (listado + detalle)
        _mockCache.Verify(c => c.RemoveAsync("vehiculos:id:1"), Times.Once);
        _mockCache.Verify(c => c.RemoveAsync("vehiculos:active"), Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_CambiandoPlacas_DebeValidarUnicidad()
    {
        // Arrange
        var vehiculoExistente = new Vehiculo
        {
            Id = 1,
            Placas = "OLD123",
            TipoVehiculo = "Sedan",
            Activo = true,
            EsDeEmpresa = true
        };

        var dto = new VehiculoRequestDto
        {
            Placas = "NEW456", // ⚠️ PLACAS NUEVAS
            TipoVehiculo = "SUV",
            Activo = true,
            EsDeEmpresa = true
        };

        var vehiculoActualizado = new Vehiculo
        {
            Id = 1,
            Placas = "NEW456",
            TipoVehiculo = "SUV",
            Activo = true,
            EsDeEmpresa = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(vehiculoExistente);

        // Las nuevas placas NO existen (validación pasa)
        _mockRepository
            .Setup(r => r.PlacasExistAsync("NEW456"))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Vehiculo>()))
            .ReturnsAsync(vehiculoActualizado);

        // Act
        var resultado = await _service.ActualizarAsync(1, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Placas.Should().Be("NEW456");
        
        // ✅ SÍ debe validar unicidad porque las placas cambiaron
        _mockRepository.Verify(r => r.PlacasExistAsync("NEW456"), Times.Once,
            "porque las placas cambiaron y debe validar que no existan");
    }

    [Fact]
    public async Task ActualizarAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var dto = new VehiculoRequestDto
        {
            Placas = "TEST",
            TipoVehiculo = "Sedan"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Vehiculo)null!);

        // Act
        var resultado = await _service.ActualizarAsync(999, dto);

        // Assert
        resultado.Should().BeNull("porque el vehículo no existe");
        
        // No debe intentar actualizar
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Vehiculo>()), Times.Never);
        
        // No debe invalidar caché
        _mockCache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region EliminarAsync Tests

    [Fact]
    public async Task EliminarAsync_ConIdExistente_DebeEliminarEInvalidarCache()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.EliminarAsync(1);

        // Assert
        resultado.Should().BeTrue("porque la eliminación fue exitosa");
        
        // ✅ Verificar que eliminó
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        
        // ✅ Verificar que invalidó AMBOS cachés
        _mockCache.Verify(c => c.RemoveAsync("vehiculos:id:1"), Times.Once);
        _mockCache.Verify(c => c.RemoveAsync("vehiculos:active"), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_ConIdInexistente_DebeRetornarFalseYNoInvalidarCache()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.EliminarAsync(999);

        // Assert
        resultado.Should().BeFalse("porque el vehículo no existe");
        
        // ✅ NO debe invalidar caché si no eliminó nada
        _mockCache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never,
            "porque no hay cambios que invalidar");
    }

    #endregion

    #region Tests de Validación de Negocio

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CrearAsync_ConPlacasVaciasONulas_NoDebeValidarUnicidad(string? placasInvalidas)
    {
        // Arrange
        var dto = new VehiculoRequestDto
        {
            Placas = placasInvalidas,
            TipoVehiculo = "Sedan"
        };

        var vehiculoCreado = new Vehiculo
        {
            Id = 100,
            Placas = placasInvalidas,
            TipoVehiculo = "Sedan",
            Activo = true,
            EsDeEmpresa = true
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Vehiculo>()))
            .ReturnsAsync(vehiculoCreado);

        // Act
        await _service.CrearAsync(dto);

        // Assert
        // ✅ NO debe llamar a PlacasExistAsync porque las placas están vacías
        _mockRepository.Verify(r => r.PlacasExistAsync(It.IsAny<string>()), Times.Never,
            $"porque las placas '{placasInvalidas}' no requieren validación de unicidad");
    }

    #endregion
}
