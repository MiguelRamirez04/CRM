using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.services.Soporte;
using back_cabs.CRM.Interfaces.Soporte; // Asegúrate que IReparacionRepository está aquí
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.DTOs.Soporte;
using back_cabs.CRM.enums;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq; // Necesario para .Select

namespace back_cabs.Tests.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias para ReparacionService utilizando Mocks.
    /// Verifica la lógica de negocio para Reparaciones y sus Componentes.
    /// </summary>
    public class ReparacionComponenteServiceTests
    {
        private readonly Mock<IReparacionRepository> _mockRepository;
        private readonly Mock<ILogger<ReparacionService>> _mockLogger;
        private readonly ReparacionService _service;

        public ReparacionComponenteServiceTests()
        {
            _mockRepository = new Mock<IReparacionRepository>();
            _mockLogger = new Mock<ILogger<ReparacionService>>();

            // Inyectar el mock del repositorio al servicio
            _service = new ReparacionService(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        
        // ==================== TESTS DE COMPONENTES ====================

        #region Componente - Crear Tests

        [Fact]
        public async Task CrearComponenteReparacionAsync_ConDatosValidos_DebeCrearYRetornarDto()
        {
            // Arrange
            var request = new ReparacionComponenteRequestDto { ReparacionId = 1, Componente = "RAM", Cantidad = 1 };
            var componenteCreado = new ReparacionComponente { Id = 50, ReparacionId = 1, Componente = "RAM", Cantidad = 1 };

            // Simular que la reparación existe
            _mockRepository.Setup(r => r.ReparacionExisteAsync(request.ReparacionId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.CrearComponenteReparacionAsync(It.IsAny<ReparacionComponente>())).ReturnsAsync(componenteCreado); // Changed to return ReparacionComponente

            // Act
            var result = await _service.CrearComponenteReparacionAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(50);
            result.Componente.Should().Be("RAM");

            _mockRepository.Verify(r => r.ReparacionExisteAsync(1), Times.Once);
            _mockRepository.Verify(r => r.CrearComponenteReparacionAsync(It.Is<ReparacionComponente>(c => c.Componente == "RAM")), Times.Once);
        }

        [Fact]
        public async Task CrearComponenteReparacionAsync_ConReparacionInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            var request = new ReparacionComponenteRequestDto { ReparacionId = 999, Componente = "RAM" };
            _mockRepository.Setup(r => r.ReparacionExisteAsync(999)).ReturnsAsync(false); // Reparación NO existe

            // Act
            Func<Task> act = async () => await _service.CrearComponenteReparacionAsync(request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*reparación con ID: {request.ReparacionId}*");
            _mockRepository.Verify(r => r.CrearComponenteReparacionAsync(It.IsAny<ReparacionComponente>()), Times.Never);
        }

        [Fact]
        public async Task CrearComponenteReparacionAsync_ConComponenteVacio_DebeLanzarArgumentException()
        {
            // Arrange
            var request = new ReparacionComponenteRequestDto { ReparacionId = 1, Componente = "" }; // Componente vacío
            _mockRepository.Setup(r => r.ReparacionExisteAsync(1)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.CrearComponenteReparacionAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*componente no puede ser nulo o vacío*");
            _mockRepository.Verify(r => r.CrearComponenteReparacionAsync(It.IsAny<ReparacionComponente>()), Times.Never);
        }

        #endregion

        #region Componente - Actualizar Tests

        [Fact]
        public async Task ActualizarComponenteReparacionAsync_ConDatosValidos_DebeActualizarYRetornarTupla()
        {
            // Arrange
            var id = 1;
            var request = new ReparacionComponenteActualizacionDto { cantidad = 2, Notas = "Actualizado" };
            var componenteExistente = new ReparacionComponente { Id = id, Cantidad = 1, Notas = "Original" };

            // Simular obtener CON TRACKING
            _mockRepository.Setup(r => r.GetComponenteForUpdateAsync(id))
                            .ReturnsAsync(componenteExistente);
            // Simular actualización exitosa
            _mockRepository.Setup(r => r.ActualizarComponenteReparacionAsync(It.IsAny<ReparacionComponente>()))
                            .ReturnsAsync((ReparacionComponente comp) => (1, comp));

            // Act
            var (filas, resultDto) = await _service.ActualizarComponenteReparacionAsync(id, request);

            // Assert
            filas.Should().Be(1);
            resultDto.Should().NotBeNull();
            resultDto!.Id.Should().Be(id);
            resultDto.Cantidad.Should().Be(2);
            resultDto.Notas.Should().Be("Actualizado");

            _mockRepository.Verify(r => r.GetComponenteForUpdateAsync(id), Times.Once);
            _mockRepository.Verify(r => r.ActualizarComponenteReparacionAsync(It.Is<ReparacionComponente>(c => c.Id == id && c.Cantidad == 2)), Times.Once);
        }

        [Fact]
        public async Task ActualizarComponenteReparacionAsync_ConIdInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            var id = 999;
            var request = new ReparacionComponenteActualizacionDto { cantidad = 2 };
            _mockRepository.Setup(r => r.ObtenerComponenteReparacionPorIdAsync(id)).ReturnsAsync((ReparacionComponente?)null); // No encontrado

            // Act
            Func<Task> act = async () => await _service.ActualizarComponenteReparacionAsync(id, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*componente de reparación con ID: {id}*");
            _mockRepository.Verify(r => r.ActualizarComponenteReparacionAsync(It.IsAny<ReparacionComponente>()), Times.Never);
        }


        #endregion

        #region Componente - Consultar Tests

        [Fact]
        public async Task ObtenerComponentesReparacionAsync_ConDatos_DebeRetornarListaDto()
        {
            // Arrange
            var componentesDb = new List<ReparacionComponente> { new ReparacionComponente { Id = 1 }, new ReparacionComponente { Id = 2 } };
            _mockRepository.Setup(r => r.ObtenerComponentesReparacionAsync(null, null)).ReturnsAsync(componentesDb);

            // Act
            var result = await _service.ObtenerComponentesReparacionAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockRepository.Verify(r => r.ObtenerComponentesReparacionAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task ObtenerComponenteReparacionPorIdAsync_ConIdExistente_DebeRetornarDto()
        {
            // Arrange
            var id = 5;
            var componenteDb = new ReparacionComponente { Id = id, Componente = "Disco Duro" };
            _mockRepository.Setup(r => r.ObtenerComponenteReparacionPorIdAsync(id)).ReturnsAsync(componenteDb);

            // Act
            var result = await _service.ObtenerComponenteReparacionPorIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Componente.Should().Be("Disco Duro");
            _mockRepository.Verify(r => r.ObtenerComponenteReparacionPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObtenerComponenteReparacionPorIdAsync_ConIdInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(r => r.ObtenerComponenteReparacionPorIdAsync(id)).ReturnsAsync((ReparacionComponente?)null);

            // Act
            Func<Task> act = async () => await _service.ObtenerComponenteReparacionPorIdAsync(id);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>(); // Verifica que el servicio lance la excepción si el repo devuelve null
            _mockRepository.Verify(r => r.ObtenerComponenteReparacionPorIdAsync(id), Times.Once);
        }

        #endregion
    }
}