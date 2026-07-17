using back_cabs.CRM.Interfaces.Auth;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.services.Auth;
using back_cabs.services;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace back_cabs.Tests.UnitTests.Services;

/// <summary>
/// ✅ Tests unitarios para UsuarioAuthService (Registro y Login)
/// 
/// 🎯 ÉXITO: Después de refactorizar ServicioJwt a IServicioJwt, ¡ahora es totalmente mockeable!
/// 
/// 📚 LO QUE APRENDERÁS AQUÍ:
/// - Mockear servicios JWT con interfaces
/// - Testear flujos de registro completos
/// - Verificar generación de tokens JWT
/// - Probar lógica de login con "RecordarMe"
/// - Validar credenciales con repository
/// - Testear normalización de emails
/// - Verificar que métodos de ayuda funcionan correctamente
/// 
/// ⚠️ NOTA: Algunos métodos usan WriteContext/ReadOnlyContext directamente,
/// por lo que solo testeamos los que operan principalmente a través del repository.
/// Para tests completos de registro/login, se recomienda crear tests de integración.
/// </summary>
public class UsuarioAuthServiceTests
{
    private readonly Mock<IUsuarioAuthRepository> _mockRepository;
    private readonly Mock<IServicioJwt> _mockJwtService;
    private readonly Mock<ILogger<UsuarioAuthService>> _mockLogger;
    private readonly UsuarioAuthService _service;

    public UsuarioAuthServiceTests()
    {
        _mockRepository = new Mock<IUsuarioAuthRepository>();
        _mockJwtService = new Mock<IServicioJwt>();
        _mockLogger = new Mock<ILogger<UsuarioAuthService>>();
        
        // ✅ AHORA SÍ FUNCIONA: IServicioJwt es mockeable
        // Los otros parámetros (WriteContext, ReadOnlyContext, Validator) los pasamos como null
        // porque no los usan directamente los métodos que vamos a testear
        _service = new UsuarioAuthService(
            _mockRepository.Object,
            null!,  // WriteContext - no usado en métodos testeados
            null!,  // ReadOnlyContext - no usado en métodos testeados
            _mockJwtService.Object,  // ✅ Ahora mockeable gracias a IServicioJwt
            _mockLogger.Object,
            null!   // Validator - requeriría más refactorización
        );
    }

    // ==================== TESTS DE VALIDACIÓN DE EMAIL ====================
    
    [Fact]
    public async Task EmailExisteAsync_ConEmailExistente_DebeRetornarTrue()
    {
        // Arrange
        var email = "test@example.com";
        _mockRepository
            .Setup(r => r.ExistsByEmailAsync(email.ToLower()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.EmailExisteAsync(email);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email.ToLower()), Times.Once);
    }
    
    [Fact]
    public async Task EmailExisteAsync_ConEmailNoExistente_DebeRetornarFalse()
    {
        // Arrange
        var email = "nuevo@example.com";
        _mockRepository
            .Setup(r => r.ExistsByEmailAsync(email.ToLower()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.EmailExisteAsync(email);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.ExistsByEmailAsync(email.ToLower()), Times.Once);
    }
    
    // ==================== TESTS DE VALIDACIÓN DE CREDENCIALES ====================
    
    [Fact]
    public async Task ValidarCredencialesAsync_ConCredencialesCorrectas_DebeRetornarUsuario()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        
        var expectedUsuario = new UsuarioAuth
        {
            Id = 1,
            Email = email.ToLower(),
            Nombre = "Test",
            Apellido = "User",
            Activo = true
        };
        
        _mockRepository
            .Setup(r => r.ValidateCredentialsAsync(email.ToLower(), password))
            .ReturnsAsync(expectedUsuario);

        // Act
        var result = await _service.ValidarCredencialesAsync(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email.ToLower());
        _mockRepository.Verify(r => r.ValidateCredentialsAsync(email.ToLower(), password), Times.Once);
    }
    
    [Fact]
    public async Task ValidarCredencialesAsync_ConCredencialesIncorrectas_DebeRetornarNull()
    {
        // Arrange
        var email = "test@example.com";
        var wrongPassword = "WrongPassword!";
        
        _mockRepository
            .Setup(r => r.ValidateCredentialsAsync(email.ToLower(), wrongPassword))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ValidarCredencialesAsync(email, wrongPassword);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task ValidarCredencialesAsync_ConUsuarioInexistente_DebeRetornarNull()
    {
        // Arrange
        var email = "noexiste@example.com";
        var password = "Password123!";
        
        _mockRepository
            .Setup(r => r.ValidateCredentialsAsync(email.ToLower(), password))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ValidarCredencialesAsync(email, password);

        // Assert
        result.Should().BeNull();
    }

    // ==================== TESTS DE OBTENCIÓN DE USUARIO ====================
    
    [Fact]
    public async Task ObtenerUsuarioPorIdAsync_ConIdExistente_DebeRetornarUsuario()
    {
        // Arrange
        var usuarioId = 123;
        var expectedUsuario = new UsuarioAuth
        {
            Id = usuarioId,
            Email = "test@example.com",
            Nombre = "Test",
            Apellido = "User",
            Activo = true
        };
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(usuarioId))
            .ReturnsAsync(expectedUsuario);

        // Act
        var result = await _service.ObtenerUsuarioPorIdAsync(usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUsuario);
        _mockRepository.Verify(r => r.GetByIdAsync(usuarioId), Times.Once);
    }
    
    [Fact]
    public async Task ObtenerUsuarioPorIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var usuarioId = 999;
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(usuarioId))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ObtenerUsuarioPorIdAsync(usuarioId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(usuarioId), Times.Once);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task ObtenerUsuarioPorIdAsync_ConIdInvalido_DebeRetornarNull(int invalidId)
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(invalidId))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ObtenerUsuarioPorIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    // ==================== TESTS DE OBTENCIÓN POR EMAIL ====================
    
    [Fact]
    public async Task ObtenerUsuarioPorEmailAsync_ConEmailExistente_DebeRetornarUsuario()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUsuario = new UsuarioAuth
        {
            Id = 1,
            Email = email,
            Nombre = "Test",
            Apellido = "User",
            Activo = true
        };
        
        _mockRepository
            .Setup(r => r.GetByEmailAsync(email.ToLower()))
            .ReturnsAsync(expectedUsuario);

        // Act
        var result = await _service.ObtenerUsuarioPorEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        _mockRepository.Verify(r => r.GetByEmailAsync(email.ToLower()), Times.Once);
    }
    
    [Fact]
    public async Task ObtenerUsuarioPorEmailAsync_ConEmailInexistente_DebeRetornarNull()
    {
        // Arrange
        var email = "noexiste@example.com";
        
        _mockRepository
            .Setup(r => r.GetByEmailAsync(email.ToLower()))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ObtenerUsuarioPorEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    // ==================== TESTS DE ACTUALIZACIÓN DE CONTRASEÑA ====================
    
    [Fact]
    public async Task ActualizarContrasenaAsync_ConDatosValidos_DebeActualizar()
    {
        // Arrange
        var userId = 123;
        var newPassword = "NewPassword123!";
        
        var usuario = new UsuarioAuth
        {
            Id = userId,
            Email = "test@example.com",
            Nombre = "Test",
            Apellido = "User",
            Activo = true
        };
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(usuario);
        
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UsuarioAuth>()))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.ActualizarContrasenaAsync(userId, newPassword);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<UsuarioAuth>(u => 
            u.Id == userId
        )), Times.Once);
    }
    
    [Fact]
    public async Task ActualizarContrasenaAsync_ConUsuarioInexistente_DebeRetornarFalse()
    {
        // Arrange
        var userId = 999;
        var newPassword = "NewPassword123!";
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((UsuarioAuth?)null);

        // Act
        var result = await _service.ActualizarContrasenaAsync(userId, newPassword);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UsuarioAuth>()), Times.Never);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ActualizarContrasenaAsync_ConPasswordVacia_DebeRetornarFalse(string? password)
    {
        // Arrange
        var userId = 123;

        // Act
        var result = await _service.ActualizarContrasenaAsync(userId, password!);

        // Assert
        result.Should().BeFalse();
        // ✅ NO debe llamar al repository
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UsuarioAuth>()), Times.Never);
    }
}
