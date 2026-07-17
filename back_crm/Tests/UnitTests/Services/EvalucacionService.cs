using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using back_cabs.CRM.Interfaces;

namespace back_cabs.Tests.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias para EvaluacionService.
    /// Valida toda la lógica de negocio mediante mocks.
    /// </summary>
    public class EvaluacionServiceTests
    {
        private readonly Mock<ReadOnlyContext> _mockReadContext;
        private readonly Mock<WriteContext> _mockWriteContext;
        private readonly EvaluacionService _service;

        public EvaluacionServiceTests()
        {
            _mockReadContext = new Mock<ReadOnlyContext>();
            _mockWriteContext = new Mock<WriteContext>();
            _service = new EvaluacionService(_mockReadContext.Object, _mockWriteContext.Object);
        }

        #region Tests de Lectura (Queries)

        [Fact(DisplayName = "GetAllEvaluacionesAsync - Retorna todas las evaluaciones")]
        public async Task GetAllEvaluacionesAsync_ReturnsAllEvaluaciones()
        {
            // Arrange
            var evaluaciones = new List<Evaluacion>
            {
                new Evaluacion { Id = 1, OrdenId = 1 },
                new Evaluacion { Id = 2, OrdenId = 2 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.Provider).Returns(evaluaciones.Provider);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.Expression).Returns(evaluaciones.Expression);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.ElementType).Returns(evaluaciones.ElementType);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.GetEnumerator()).Returns(evaluaciones.GetEnumerator());

            _mockReadContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllEvaluacionesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact(DisplayName = "GetEvaluacionByIdAsync - Retorna evaluación cuando existe")]
        public async Task GetEvaluacionByIdAsync_ReturnsEvaluacion_WhenExists()
        {
            // Arrange
            var evaluacion = new Evaluacion { Id = 1, OrdenId = 1 };
            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(evaluacion);

            _mockReadContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            // Act
            var result = await _service.GetEvaluacionByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact(DisplayName = "GetEvaluacionByIdAsync - Retorna null cuando no existe")]
        public async Task GetEvaluacionByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync((Evaluacion?)null);

            _mockReadContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            // Act
            var result = await _service.GetEvaluacionByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GetEvaluacionByOrdenIdAsync - Retorna evaluaciones por orden")]
        public async Task GetEvaluacionByOrdenIdAsync_ReturnsEvaluacionesByOrden()
        {
            // Arrange
            var evaluaciones = new List<Evaluacion>
            {
                new Evaluacion { Id = 1, OrdenId = 1 },
                new Evaluacion { Id = 2, OrdenId = 1 },
                new Evaluacion { Id = 3, OrdenId = 2 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.Provider).Returns(evaluaciones.Provider);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.Expression).Returns(evaluaciones.Expression);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.ElementType).Returns(evaluaciones.ElementType);
            mockSet.As<IQueryable<Evaluacion>>().Setup(m => m.GetEnumerator()).Returns(evaluaciones.GetEnumerator());

            _mockReadContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            // Act
            var result = await _service.GetEvaluacionByOrdenIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(1, e.OrdenId));
        }

        #endregion

        #region Tests de Escritura (Commands)

        [Fact(DisplayName = "CreateEvaluacionAsync - Crea nueva evaluación correctamente")]
        public async Task CreateEvaluacionAsync_CreatesNewEvaluacion()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Evaluacion>>();
            _mockWriteContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);
            _mockWriteContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var dto = new EvaluacionRequestDto
            {
                OrdenId = 1,
                EjecucionId = 2,
                CLienteId = 3,
                EvaluadorId = 4,
                Objetivo = "Test Objetivo",
                ComentariosGenerales = "Comentarios de prueba",
                ScoreCalidadTotal = 5,
                RequiereSeguimiento = false,
                SeguimientoNotas = ""
            };

            // Act
            var result = await _service.CreateEvaluacionAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrdenId);
            Assert.Equal(2, result.EjecucionId);
            Assert.Equal(3, result.ClienteId);
            Assert.Equal(4, result.EvaluadorId);
            Assert.Equal("Test Objetivo", result.Objetivo);
            Assert.Equal("Comentarios de prueba", result.ComentariosGenerales);
            Assert.Equal(5, result.ScoreCalidadTotal);
            Assert.False(result.RequiereSeguimiento);
            Assert.Equal("", result.SeguimientoNotas);
            Assert.True(result.CreadoEn <= DateTime.UtcNow);

            mockSet.Verify(m => m.Add(It.IsAny<Evaluacion>()), Times.Once);
            _mockWriteContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact(DisplayName = "UpdateEvaluacionAsync - Actualiza evaluación existente")]
        public async Task UpdateEvaluacionAsync_UpdatesExistingEvaluacion()
        {
            // Arrange
            var existingEvaluacion = new Evaluacion
            {
                Id = 1,
                OrdenId = 1,
                EjecucionId = 2,
                ClienteId = 3,
                EvaluadorId = 4,    
                Objetivo = "Objetivo original",
                ComentariosGenerales = "Comentarios originales",
                ScoreCalidadTotal = 4,
                RequiereSeguimiento = true,
                SeguimientoNotas = "Notas originales"
            };

            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingEvaluacion);
            _mockWriteContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);
            _mockWriteContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var dto = new EvaluacionRequestDto
            {
                EjecucionId = 3,
                EvaluadorId = 5,
                Objetivo = "Objetivo actualizado",
                ComentariosGenerales = "Comentarios actualizados",
                ScoreCalidadTotal = 5,
                RequiereSeguimiento = false,
                SeguimientoNotas = "Notas actualizadas"
            };

            // Act
            var result = await _service.UpdateEvaluacionAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(3, result.EjecucionId);
            Assert.Equal(5, result.EvaluadorId);
            Assert.Equal("Objetivo actualizado", result.Objetivo);
            Assert.Equal("Comentarios actualizados", result.ComentariosGenerales);
            Assert.Equal(5, result.ScoreCalidadTotal);
            Assert.False(result.RequiereSeguimiento);
            Assert.Equal("Notas actualizadas", result.SeguimientoNotas);

            _mockWriteContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact(DisplayName = "UpdateEvaluacionAsync - Retorna null cuando evaluación no existe")]
        public async Task UpdateEvaluacionAsync_ReturnsNull_WhenEvaluacionNotExists()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync((Evaluacion?)null);
            _mockWriteContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            var dto = new EvaluacionRequestDto
            {
                EjecucionId = 3,
                EvaluadorId = 5,
                Objetivo = "Objetivo",
                ComentariosGenerales = "Comentarios",
                ScoreCalidadTotal = 5,
                RequiereSeguimiento = false,
                SeguimientoNotas = ""
            };

            // Act
            var result = await _service.UpdateEvaluacionAsync(1, dto);

            // Assert
            Assert.Null(result);
            _mockWriteContext.Verify(m => m.SaveChangesAsync(default), Times.Never);
        }

        [Fact(DisplayName = "DeleteEvaluacionAsync - Elimina evaluación existente")]
        public async Task DeleteEvaluacionAsync_DeletesExistingEvaluacion()
        {
            // Arrange
            var evaluacion = new Evaluacion { Id = 1, OrdenId = 1 };
            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(evaluacion);
            _mockWriteContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);
            _mockWriteContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteEvaluacionAsync(1);

            // Assert
            Assert.True(result);
            mockSet.Verify(m => m.Remove(evaluacion), Times.Once);
            _mockWriteContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact(DisplayName = "DeleteEvaluacionAsync - Retorna false cuando evaluación no existe")]
        public async Task DeleteEvaluacionAsync_ReturnsFalse_WhenEvaluacionNotExists()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Evaluacion>>();
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync((Evaluacion?)null);
            _mockWriteContext.Setup(c => c.Evaluaciones).Returns(mockSet.Object);

            // Act
            var result = await _service.DeleteEvaluacionAsync(1);

            // Assert
            Assert.False(result);
            mockSet.Verify(m => m.Remove(It.IsAny<Evaluacion>()), Times.Never);
            _mockWriteContext.Verify(m => m.SaveChangesAsync(default), Times.Never);
        }

        #endregion
    }
}