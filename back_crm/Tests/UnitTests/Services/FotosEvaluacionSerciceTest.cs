using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.contexts;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.DTOs.shared;
using back_cabs.CRM.services.Files;
using back_cabs.CRM.enums.Files;
using back_cabs.CRM.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using back_cabs.CRM.models.Files;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace back_cabs.Tests.UnitTests.Services
{
    public class FotosEvaluacionServiceTests
    {
        private readonly Mock<ReadOnlyContext> _mockReadContext;
        private readonly Mock<WriteContext> _mockWriteContext;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly Mock<ILogger<FotosEvaluacionService>> _mockLogger;
        private readonly FotosEvaluacionService _service;

        public FotosEvaluacionServiceTests()
        {
            _mockReadContext = new Mock<ReadOnlyContext>();
            _mockWriteContext = new Mock<WriteContext>();
            _mockFileStorage = new Mock<IFileStorageService>();
            _mockLogger = new Mock<ILogger<FotosEvaluacionService>>();

            _service = new FotosEvaluacionService(
                _mockReadContext.Object,
                _mockWriteContext.Object,
                _mockFileStorage.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateFotoAsync_ConDetalleExistente_CreaFotoYRetornaDto()
        {
            // Arrange
            var dto = new EvaluacionFotoRequestDto
            {
                DetalleId = 1,
                Archivo = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object,
                Descripcion = "desc",
                Tipo = "tipo"
            };
            int usuarioId = 10;

            _mockReadContext.Setup(c => c.EvaluacionesDetalles.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionDetalle, bool>>>(), default))
                .ReturnsAsync(true);

            var documento = new FilesDocumento
            {
                Id = 100,
                NombreArchivo = "foto.jpg",
                MimeType = "image/jpeg",
                TamanoBytes = 1234
            };
            _mockFileStorage.Setup(f => f.UploadFileAsync(dto.Archivo, TipoEntidadDocumento.Evaluacion, dto.DetalleId, usuarioId, dto.Descripcion, dto.Tipo))
                .ReturnsAsync(documento);

            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            _mockReadContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);
            _mockReadContext.Setup(c => c.EvaluacionesFotos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionFoto, bool>>>(), default))
                .ReturnsAsync(default(EvaluacionFoto)!);

            var fotosDbSetWrite = new Mock<DbSet<EvaluacionFoto>>();
            _mockWriteContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSetWrite.Object);
            _mockWriteContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.CreateFotoAsync(dto, usuarioId);

            // Assert
            result.Should().NotBeNull();
            result.DocumentoId.Should().Be(100);
            result.NombreArchivo.Should().Be("foto.jpg");
            result.Tipo.Should().Be("tipo");
        }

        [Fact]
        public async Task CreateFotoAsync_ConDetalleInexistente_LanzaKeyNotFoundException()
        {
            // Arrange
            var dto = new EvaluacionFotoRequestDto { DetalleId = 99, Archivo = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object };
            _mockReadContext.Setup(c => c.EvaluacionesDetalles.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionDetalle, bool>>>(), default))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.CreateFotoAsync(dto, 1);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetAllFotosAsync_RetornaListaDeFotos()
        {
            // Arrange
            var fotos = new List<EvaluacionFoto>
            {
                new EvaluacionFoto
                {
                    Id = 1,
                    DetalleId = 2,
                    DocumentoId = 3,
                    Tipo = "tipo",
                    Descripcion = "desc",
                    CreadoEn = DateTime.UtcNow,
                    Documento = new FilesDocumento {
                        Id = 3,
                        NombreArchivo = "foto1.jpg",
                        MimeType = "image/jpeg",
                        TamanoBytes = 100,
                        Activo = true,
                        EntidadTipo = "Evaluacion",
                        EntidadId = 2,
                        CreadoPorUsuarioId = 1,
                        RutaAlmacenamiento = "dummy/path/foto1.jpg"
                    }
                }
            }.AsQueryable();

            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.Provider).Returns(fotos.Provider);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.Expression).Returns(fotos.Expression);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.ElementType).Returns(fotos.ElementType);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.GetEnumerator()).Returns(fotos.GetEnumerator());

            _mockReadContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);

            // Act
            var result = await _service.GetAllFotosAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().NombreArchivo.Should().Be("foto1.jpg");
        }

        [Fact]
        public async Task GetFotosByDetalleAsync_RetornaFotosPorDetalle()
        {
            // Arrange
            int detalleId = 5;
            var fotos = new List<EvaluacionFoto>
            {
                new EvaluacionFoto
                {
                    Id = 1,
                    DetalleId = detalleId,
                    DocumentoId = 3,
                    Tipo = "tipo",
                    Descripcion = "desc",
                    CreadoEn = DateTime.UtcNow,
                    
                }
            }.AsQueryable();

            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.Provider).Returns(fotos.Provider);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.Expression).Returns(fotos.Expression);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.ElementType).Returns(fotos.ElementType);
            fotosDbSet.As<IQueryable<EvaluacionFoto>>().Setup(m => m.GetEnumerator()).Returns(fotos.GetEnumerator());

            _mockReadContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);

            // Act
            var result = await _service.GetFotosByDetalleAsync(detalleId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().DetalleId.Should().Be(detalleId);
        }

        [Fact]
        public async Task GetFotoByIdAsync_ConFotoExistente_RetornaDto()
        {
            // Arrange
            int fotoId = 1;
            var foto = new EvaluacionFoto
            {
                Id = fotoId,
                DetalleId = 2,
                DocumentoId = 3,
                Tipo = "tipo",
                Descripcion = "desc",
                CreadoEn = DateTime.UtcNow,
                Documento = new FilesDocumento {
                    Id = 3,
                    NombreArchivo = "foto3.jpg",
                    MimeType = "image/jpeg",
                    TamanoBytes = 300,
                    Activo = true,
                    EntidadTipo = "Evaluacion",
                    EntidadId = 2,
                    CreadoPorUsuarioId = 1,
                    RutaAlmacenamiento = "dummy/path/foto3.jpg"
                }
            };
            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            _mockReadContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);
            _mockReadContext.Setup(c => c.EvaluacionesFotos.Include(It.IsAny<string>())).Returns(fotosDbSet.Object);
            _mockReadContext.Setup(c => c.EvaluacionesFotos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionFoto, bool>>>(), default))
                .ReturnsAsync(foto);

            // Act
            var result = await _service.GetFotoByIdAsync(fotoId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(fotoId);
            result.NombreArchivo.Should().Be("foto3.jpg");
        }

        [Fact]
        public async Task DeleteFotoAsync_ConFotoExistente_EliminaYRetornaTrue()
        {
            // Arrange
            int fotoId = 1, usuarioId = 2;
            var foto = new EvaluacionFoto { Id = fotoId, DocumentoId = 10 };
            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            _mockWriteContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);
            _mockWriteContext.Setup(c => c.EvaluacionesFotos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionFoto, bool>>>(), default))
                .ReturnsAsync(foto);
            _mockFileStorage.Setup(f => f.DeleteFileAsync(foto.DocumentoId, usuarioId)).ReturnsAsync(true);
            _mockWriteContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteFotoAsync(fotoId, usuarioId);

            // Assert
            result.Should().BeTrue();
            fotosDbSet.Verify(m => m.Remove(foto), Times.Once);
        }

        [Fact]
        public async Task DeleteFotoAsync_ConFotoInexistente_RetornaFalse()
        {
            // Arrange
            int fotoId = 1, usuarioId = 2;
            var fotosDbSet = new Mock<DbSet<EvaluacionFoto>>();
            _mockWriteContext.Setup(c => c.EvaluacionesFotos).Returns(fotosDbSet.Object);
            _mockWriteContext.Setup(c => c.EvaluacionesFotos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EvaluacionFoto, bool>>>(), default))
                .ReturnsAsync(default(EvaluacionFoto)!);

            // Act
            var result = await _service.DeleteFotoAsync(fotoId, usuarioId);

            // Assert
            result.Should().BeFalse();
        }
    }
}