using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.shared;
using System;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace back_cabs.Tests.UnitTests.Services
{
    public class DetallesEvaluacionServiceTest
    {
        private readonly Mock<IDetalleEvaluacionRepository> _mockRepo;
        private readonly EvaluacionDetallesService _service;

        public DetallesEvaluacionServiceTest()
        {
            _mockRepo = new Mock<IDetalleEvaluacionRepository>();
            _service = new EvaluacionDetallesService(_mockRepo.Object);
        }

        [Fact(DisplayName = "GetDEtallleByEvaluacionIdAsync - Retorna detalles por evaluacion")]
        public async Task GetDetalleByEvaluacionIdAsync_RetunrDetales()
        {
            //Arrange
            var detalles = new List<EvaluacionDetalle>
            {
                new EvaluacionDetalle { Id = 1, EvaluacionId = 10, Fase = "F1" },
                new EvaluacionDetalle { Id = 2, EvaluacionId = 10, Fase = "F2"}
            };
            _mockRepo.Setup(r => r.GetByEvaluacionIdAsync(10)).ReturnsAsync(detalles);

            var result = await _service.GetDetallesByEvaluacionIdAsync(10);

            Assert.Equal(2, result.Count);
            Assert.All(result, d => Assert.Equal(10, d.EvaluacionId));

        }

        [Fact]
        public async Task GetDetallesByIdAsnyc_ReturnDetalle_WhenExist()
        {
            var detalle = new EvaluacionDetalle { Id = 1, EvaluacionId = 10, Fase = "F1" };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(detalle);

            var result = await _service.GetDetalleByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact(DisplayName = "GetDetalleByIdAsync - Retorna null cuando no existe")]
        public async Task GetDetalleByIdAsync_ReturnNull_WhemMoExist()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((EvaluacionDetalle?)null);

            var result = await _service.GetDetalleByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateDetallesAsync_CreateNewDetalle()
        {
            var request = new DtoEvaDetallesRequest
            {
                EvaluacionId = 10,
                Fase = "F1",
                Descripcion = "desc",
                Sugerencias = "sug",
                ScoreFase = 5,
                EvidenciaNota = "nota",
                Lugar = "lugar"
            };
            var detalleCreado = new EvaluacionDetalle
            {
                Id = 1,
                EvaluacionId = 10,
                Fase = "F1",
                Descripcion = "desc",
                Sugerencias = "sug",
                ScoreFase = 5,
                EvidenciaNota = "nota",
                Lugar = "lugar",
                CreadoEn = DateTime.UtcNow
            };
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<EvaluacionDetalle>())).ReturnsAsync(detalleCreado);

            var result = await _service.CreateDetalleAsync(request);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("F1", result.Fase);
        }

        [Fact(DisplayName = "UptadeDetalleAsync - Actualiza detalle existente")]
        public async Task UpdateDetallesAsync_UppdatesExistingDetalle()
        {
            var detalleExistente = new EvaluacionDetalle { Id = 1, EvaluacionId = 102, Fase = "F1" };
            var request = new DtoEvaDetallesRequest
            {
                EvaluacionId = 10,
                Fase = "F2",
                Descripcion = "desc2",
                Sugerencias = "sug2",
                ScoreFase = 4,
                EvidenciaNota = "nota2",
                Lugar = "lugar2"
            };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(detalleExistente);
            _mockRepo.Setup(r => r.UpdateAsync(detalleExistente)).Returns(Task.CompletedTask);

            var result = await _service.UpdateDetalleAsync(1, request);

            Assert.NotNull(result);
            Assert.Equal("F2", result.Fase);
        }

        [Fact(DisplayName = "UpdateDetalleAsync - Retorna null cuando detalle no existe")]
        public async Task UpdateDetalleAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((EvaluacionDetalle?)null);

            var request = new DtoEvaDetallesRequest();

            // Act
            var result = await _service.UpdateDetalleAsync(1, request);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "DeleteDetalleAsync - Elimina detalle existente")]
        public async Task DeleteDetalleAsync_DeletesExistingDetalle()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteDetalleAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteDetalleAsync - Retorna false cuando detalle no existe")]
        public async Task DeleteDetalleAsync_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteDetalleAsync(1);

            // Assert
            Assert.False(result);
        }

    }
}