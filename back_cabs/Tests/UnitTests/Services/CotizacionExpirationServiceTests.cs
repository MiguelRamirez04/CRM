using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.models.Sales;
using back_cabs.CRM.services.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace back_cabs.Tests.UnitTests.Services;

public class CotizacionExpirationServiceTests
{
    private readonly Mock<ILogger<CotizacionExpirationService>> _mockLogger;
    private readonly Mock<ICotizacionRepository> _mockRepository;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly CotizacionExpirationService _service;

    public CotizacionExpirationServiceTests()
    {
        _mockLogger = new Mock<ILogger<CotizacionExpirationService>>();
        _mockRepository = new Mock<ICotizacionRepository>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();

        // Setup service provider
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);

        _mockScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(_mockScope.Object);

        _mockScope
            .Setup(x => x.ServiceProvider.GetService(typeof(ICotizacionRepository)))
            .Returns(_mockRepository.Object);

        _service = new CotizacionExpirationService(
            _mockLogger.Object,
            _mockServiceProvider.Object
        );
    }

    [Fact]
    public async Task VerificarCotizacionesVencidas_DebeMarcarComoVencida_CuandoExpiraEnFebrero()
    {
        // Arrange
        var fechaCreacion = new DateTime(2024, 2, 1); // 1 de febrero 2024
        var diasValidez = 30; // Debería expirar el 2 de marzo
        var fechaActual = new DateTime(2024, 3, 2); // 2 de marzo 2024

        var cotizacion = new Cotizacion
        {
            Id = 1,
            CreadoEn = fechaCreacion,
            ValidezDias = diasValidez,
            Estado = "NUEVA"
        };

        _mockRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Cotizacion> { cotizacion });

        // Act
        await _service.TestVerificarCotizacionesVencidas(fechaActual);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<Cotizacion>(c => 
                c.Id == 1 && 
                c.Estado == "VENCIDO")), 
            Times.Once);
    }

    [Fact]
    public async Task VerificarCotizacionesVencidas_NoDebeMarcarComoVencida_AntesDeVencimiento()
    {
        // Arrange
        var fechaCreacion = new DateTime(2024, 2, 1); // 1 de febrero 2024
        var diasValidez = 30; // Debería expirar el 2 de marzo
        var fechaActual = new DateTime(2024, 3, 1); // 1 de marzo 2024 (un día antes)

        var cotizacion = new Cotizacion
        {
            Id = 1,
            CreadoEn = fechaCreacion,
            ValidezDias = diasValidez,
            Estado = "NUEVA"
        };

        _mockRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Cotizacion> { cotizacion });

        // Act
        await _service.TestVerificarCotizacionesVencidas(fechaActual);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Cotizacion>()), Times.Never);
    }

    [Fact]
    public async Task VerificarCotizacionesVencidas_DebeMarcarComoVencida_ExactamenteEnFechaVencimiento()
    {
        // Arrange
        var fechaCreacion = new DateTime(2024, 2, 1); // 1 de febrero 2024
        var diasValidez = 28; // Debería expirar el 29 de febrero (2024 es bisiesto)
        var fechaActual = new DateTime(2024, 2, 29); // Exactamente en la fecha de vencimiento

        var cotizacion = new Cotizacion
        {
            Id = 1,
            CreadoEn = fechaCreacion,
            ValidezDias = diasValidez,
            Estado = "NUEVA"
        };

        _mockRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Cotizacion> { cotizacion });

        // Act
        await _service.TestVerificarCotizacionesVencidas(fechaActual);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<Cotizacion>(c => 
                c.Id == 1 && 
                c.Estado == "VENCIDO")), 
            Times.Once);
    }
}

// Extensión del servicio para testing
public static class CotizacionExpirationServiceExtensions
{
    public static Task TestVerificarCotizacionesVencidas(
        this CotizacionExpirationService service,
        DateTime fechaActual)
    {
        var method = service.GetType()
            .GetMethod("VerificarCotizacionesVencidas", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)!;

        return method.Invoke(service, new object[] { 
            CancellationToken.None,
            fechaActual
        }) as Task ?? Task.CompletedTask;
    }
}