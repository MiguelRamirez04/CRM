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
    public class ReparacionServiceTests
    {
        private readonly Mock<IReparacionRepository> _mockRepository;
        private readonly Mock<ILogger<ReparacionService>> _mockLogger;
        private readonly ReparacionService _service;

        public ReparacionServiceTests()
        {
            _mockRepository = new Mock<IReparacionRepository>();
            _mockLogger = new Mock<ILogger<ReparacionService>>();

            // Inyectar el mock del repositorio al servicio
            _service = new ReparacionService(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        // ==================== TESTS DE REPARACIÓN ====================

        #region Reparacion - Crear Tests

        [Fact]
        public async Task CrearReparacionAsync_ConDatosValidos_DebeLlamarRepositorioYRetornarDto()
        {
            // Arrange
            var request = new ReparacionCreacionRequestDto { OrdenId = 1, TecnicoId = 2, DispositivoTipo = "Laptop", DescripcionFalla = "Test" };
            var reparacionCreada = new Reparacion { Id = 10, OrdenId = 1, TecnicoId = 2, Resultado = "COTIZAR", FechaLlegada = DateTime.UtcNow };

            _mockRepository.Setup(r => r.OrdenExisteAsync(request.OrdenId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.TecnicoExisteAsync(request.TecnicoId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.CrearReparacionAsync(It.IsAny<Reparacion>())).ReturnsAsync(reparacionCreada);

            // Act
            var result = await _service.CrearReparacionAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(10);
            result.OrdenId.Should().Be(1);
            result.Resultado.Should().Be("COTIZAR"); // Verificar estado inicial

            _mockRepository.Verify(r => r.OrdenExisteAsync(1), Times.Once);
            _mockRepository.Verify(r => r.TecnicoExisteAsync(2), Times.Once);
            _mockRepository.Verify(r => r.CrearReparacionAsync(It.Is<Reparacion>(r => r.OrdenId == 1 && r.TecnicoId == 2)), Times.Once);
        }

        [Fact]
        public async Task CrearReparacionAsync_ConOrdenInvalida_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            var request = new ReparacionCreacionRequestDto { OrdenId = 999, TecnicoId = 1 };
            _mockRepository.Setup(r => r.OrdenExisteAsync(999)).ReturnsAsync(false); // Orden NO existe
            _mockRepository.Setup(r => r.TecnicoExisteAsync(1)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.CrearReparacionAsync(request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*orden de trabajo*");
            _mockRepository.Verify(r => r.CrearReparacionAsync(It.IsAny<Reparacion>()), Times.Never);
        }

        // ... (Test similar para TecnicoInexistente) ...

        #endregion

        #region Reparacion - Actualizar Tests

        [Fact]
        public async Task ActualizarReparacionAsync_ConDatosValidos_DebeActualizarYRetornarTupla()
        {
            // Arrange
            var id = 7;
            var request = new ReparacionActualizacionRequestDto { Resultado = "REPARADO", SolucionAplicada = "Fix", TipoEntrega = "DOMICILIO" };
            var reparacionExistente = new Reparacion {
                Id = id,
                Resultado = "COTIZAR"
            };

            // Simular obtener la entidad CON TRACKING (devolver la misma instancia)
            _mockRepository.Setup(r => r.GetReparacionForUpdateAsync(id))
                            .ReturnsAsync(reparacionExistente);
            // Simular la actualización exitosa
            _mockRepository.Setup(r => r.ActualizarReparacionAsync(It.IsAny<Reparacion>()))
                           .ReturnsAsync((Reparacion rep) => (1, rep)); // Devolver 1 fila y la entidad modificada

            // Act
            var (filas, resultDto) = await _service.ActualizarReparacionAsync(id, request);

            // Assert
            filas.Should().Be(1);
            resultDto.Should().NotBeNull();
            resultDto!.Id.Should().Be(id);
            resultDto.Resultado.Should().Be("REPARADO");
            resultDto.SolucionAplicada.Should().Be("Fix");

            _mockRepository.Verify(r => r.GetReparacionForUpdateAsync(id), Times.Once); // Verificar que se obtuvo la entidad
            _mockRepository.Verify(r => r.ActualizarReparacionAsync(It.Is<Reparacion>(r => r.Id == id && r.Resultado == "REPARADO")), Times.Once);
        }

        [Fact]
        public async Task ActualizarReparacionAsync_ConIdInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            var id = 999;
            var request = new ReparacionActualizacionRequestDto { Resultado = "REPARADO" };
            _mockRepository.Setup(r => r.GetReparacionForUpdateAsync(id))
                            .ReturnsAsync((Reparacion?)null); // No encontrado

            // Act
            Func<Task> act = async () => await _service.ActualizarReparacionAsync(id, request);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*reparación con ID: {id}*");
            _mockRepository.Verify(r => r.ActualizarReparacionAsync(It.IsAny<Reparacion>()), Times.Never);
        }

        [Fact]
        public async Task ActualizarReparacionAsync_ConResultadoIrreparableSinCausa_DebeLanzarArgumentException()
        {
            // Arrange
            var id = 8;
            var request = new ReparacionActualizacionRequestDto { Resultado = "IRREPARABLE", CausaIrreparable = "", TipoEntrega = "DOMICILIO"}; // Causa vacía
            var reparacionExistente = new Reparacion { Id = id, Resultado = "COTIZAR" };
            _mockRepository.Setup(r => r.GetReparacionForUpdateAsync(id)).ReturnsAsync(reparacionExistente);

            // Act
            Func<Task> act = async () => await _service.ActualizarReparacionAsync(id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*causa es obligatoria*");
            _mockRepository.Verify(r => r.ActualizarReparacionAsync(It.IsAny<Reparacion>()), Times.Never);
        }


        // ... (Tests para Resultado inválido, TipoEntrega inválido) ...

        #endregion

        #region Reparacion - Consultar Tests

        [Fact]
        public async Task ObtenerReparacionesAsync_ConDatos_DebeRetornarListaDto()
        {
            // Arrange
            var reparacionesDb = new List<Reparacion> { new Reparacion { Id = 1 }, new Reparacion { Id = 2 } };
            _mockRepository.Setup(r => r.ObtenerReparacionesAsync(null, null)).ReturnsAsync(reparacionesDb);

            // Act
            var result = await _service.ObtenerReparacionesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockRepository.Verify(r => r.ObtenerReparacionesAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task ObtenerReparacionPorIdAsync_ConIdExistente_DebeRetornarDto()
        {
            // Arrange
            var id = 7;
            var reparacionDb = new Reparacion { Id = id };
            _mockRepository.Setup(r => r.ObtenerReparacionPorIdAsync(id)).ReturnsAsync(reparacionDb);

            // Act
            var result = await _service.ObtenerReparacionPorIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            _mockRepository.Verify(r => r.ObtenerReparacionPorIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task ObtenerReparacionPorIdAsync_ConIdInexistente_DebeRetornarNull()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(r => r.ObtenerReparacionPorIdAsync(id)).ReturnsAsync((Reparacion?)null);

            // Act
            var result = await _service.ObtenerReparacionPorIdAsync(id);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.ObtenerReparacionPorIdAsync(id), Times.Once);
        }

        #endregion
    }
}