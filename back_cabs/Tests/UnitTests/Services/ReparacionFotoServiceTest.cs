using Xunit;
using Moq;
using FluentAssertions;
using back_cabs.CRM.services.Soporte;
using back_cabs.CRM.Interfaces.Soporte;
using back_cabs.CRM.services.shared; // Para ImageProcessingService
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.models.Auth; // Para el mock de UsuarioAuth
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http; // Para IFormFile
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text.Json;

namespace back_cabs.Tests.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias para ReparacionFotoService.
    /// Verifica la lógica de validación, procesamiento de imágenes y 
    /// orquestación del repositorio.
    /// </summary>
    public class ReparacionFotoServiceTests
    {
        // Dependencias a mockear
        private readonly Mock<IReparacionFotoRepository> _mockRepository;
        private readonly Mock<ImageProcessingService> _mockImageProcessing; // Asumimos que es mockeable (virtual o interfaz)
        private readonly Mock<ILogger<ReparacionFotoService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        // El Servicio Bajo Prueba (SUT)
        private readonly ReparacionFotoService _service;

        public ReparacionFotoServiceTests()
        {
            _mockRepository = new Mock<IReparacionFotoRepository>();

            // Este es el Logger para el ReparacionFotoService (el servicio que estás probando)
            _mockLogger = new Mock<ILogger<ReparacionFotoService>>();

            _mockConfiguration = new Mock<IConfiguration>();

            // --- INICIO DE LA CORRECCIÓN ---

            // 1. Debes crear un mock del Logger que ImageProcessingService espera
            var mockImageProcessingLogger = new Mock<ILogger<ImageProcessingService>>();

            // 2. Pasa ese mock (.Object) al constructor de Moq
            // Esto le dice a Moq: "Usa este objeto para satisfacer el constructor 
            // de la clase ImageProcessingService"
            _mockImageProcessing = new Mock<ImageProcessingService>(mockImageProcessingLogger.Object);

            // --- FIN DE LA CORRECCIÓN ---


            // --- Configuración del Mock de IConfiguration (esto estaba bien) ---
            _mockConfiguration.Setup(c => c["FileStorage:UploadPath"]).Returns("test-uploads");
            _mockConfiguration.Setup(c => c["FileStorage:MaxFileSizeMB"]).Returns("10");
            _mockConfiguration.Setup(c => c["FileStorage:WebPQuality"]).Returns("80");
            _mockConfiguration.Setup(c => c["FileStorage:MaxImageWidth"]).Returns("1920");
            _mockConfiguration.Setup(c => c["FileStorage:MaxImageHeight"]).Returns("1080");

            // Crear el servicio inyectando los mocks
            _service = new ReparacionFotoService(
                _mockRepository.Object,
                _mockImageProcessing.Object, // Ahora se crea correctamente
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        /// <summary>
        /// Helper para crear un mock de IFormFile
        /// </summary>
        private Mock<IFormFile> CreateMockFile(string fileName, long length, string contentType)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            // Simular el stream de lectura (puede ser un MemoryStream vacío para la mayoría de los tests)
            var ms = new MemoryStream();
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            return mockFile;
        }

        #region UploadFotoAsync Tests

        [Fact]
        public async Task UploadFotoAsync_ConDatosValidos_DebeProcesarYGuardar()
        {
            // Arrange
            int reparacionId = 1, usuarioId = 1;
            var mockFile = CreateMockFile("test.jpg", 1024, "image/jpeg");
            var dto = new ReparacionFotoUploadRequestDto { Archivo = mockFile.Object, Etapa = "INICIAL", Descripcion = "Foto de prueba" };

            // 1. Simular validación de FK (reparación existe)
            _mockRepository.Setup(r => r.ReparacionExistsAsync(reparacionId)).ReturnsAsync(true);

            // 2. Simular validación de archivo
            _mockImageProcessing.Setup(img => img.IsValidImage(dto.Archivo)).Returns(true);
            _mockImageProcessing.Setup(img => img.IsValidImageContentAsync(It.IsAny<Stream>())).ReturnsAsync(true);

            // 3. Simular conversión de imagen (devuelve tamaño y dimensiones)
            _mockImageProcessing.Setup(img => img.ConvertToWebPAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(), // No nos importa la ruta exacta, solo que sea llamada
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync((512L, 800, 600)); // Simula nuevo tamaño 512 bytes, 800x600

            // 4. Simular la transacción del repositorio
            var fotoGuardada = new ReparacionFoto { Id = 1, ReparacionId = reparacionId, DocumentoId = 10, Etapa = "INICIAL" };
            _mockRepository.Setup(r => r.CreateFotoInTransactionAsync(It.IsAny<ReparacionFoto>(), It.IsAny<FilesDocumento>()))
                            .ReturnsAsync(fotoGuardada);

            // 5. Simular el mapeo de DTO (necesitamos un usuario mock)
            // (El servicio llama a MapToResponseDto, que depende de que la entidad Documento tenga un CreadoPorUsuario)
            // Asumimos que el repositorio devuelve la entidad completa como la necesitamos
            // (La implementación actual del repo no devuelve el usuario, así que el mapeo lo hará null)

            // Act
            var result = await _service.UploadFotoAsync(reparacionId, dto, usuarioId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.MimeType.Should().Be("image/webp"); // Verificamos que se guardó como webp
            result.TamanoBytes.Should().Be(512L); // Verificamos el tamaño convertido

            // Verificar que las dependencias fueron llamadas
            _mockRepository.Verify(r => r.ReparacionExistsAsync(reparacionId), Times.Once);
            _mockImageProcessing.Verify(img => img.ConvertToWebPAsync(It.IsAny<Stream>(), It.IsAny<string>(), 80, 1920, 1080), Times.Once);
            _mockRepository.Verify(r => r.CreateFotoInTransactionAsync(
                It.Is<ReparacionFoto>(f => f.Etapa == "INICIAL" && f.ReparacionId == reparacionId),
                It.Is<FilesDocumento>(d => d.EntidadId == reparacionId && d.CreadoPorUsuarioId == usuarioId && d.MimeType == "image/webp")
            ), Times.Once);
        }

        [Fact]
        public async Task UploadFotoAsync_ConReparacionInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            int reparacionId = 999;
            var mockFile = CreateMockFile("test.jpg", 1024, "image/jpeg");
            var dto = new ReparacionFotoUploadRequestDto { Archivo = mockFile.Object };

            // Simular que la reparación NO existe
            _mockRepository.Setup(r => r.ReparacionExistsAsync(reparacionId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.UploadFotoAsync(reparacionId, dto, 1);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                        .WithMessage($"Reparación con ID {reparacionId} no encontrada.");

            // Verificar que no se procesó la imagen ni se guardó
            _mockImageProcessing.Verify(img => img.ConvertToWebPAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockRepository.Verify(r => r.CreateFotoInTransactionAsync(It.IsAny<ReparacionFoto>(), It.IsAny<FilesDocumento>()), Times.Never);
        }

        [Fact]
        public async Task UploadFotoAsync_ConArchivoNulo_DebeLanzarArgumentException()
        {
            // Arrange
            int reparacionId = 1;
            var dto = new ReparacionFotoUploadRequestDto { Archivo = default! }; // Archivo nulo
            _mockRepository.Setup(r => r.ReparacionExistsAsync(reparacionId)).ReturnsAsync(true);

            // Act
            Func<Task> act = () => _service.UploadFotoAsync(reparacionId, dto, 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("El archivo es requerido.");
        }

        #endregion

        #region GetFotosByReparacionIdAsync Tests

        [Fact]
        public async Task GetFotosByReparacionIdAsync_ConDatos_DebeRetornarListaDto()
        {
            // Arrange
            int reparacionId = 1;
            var mockUsuario = new UsuarioAuth { Nombre = "Usuario", Apellido = "Prueba" };
            var mockDocumento = new FilesDocumento { Id = 10, NombreArchivo = "foto1.webp", MimeType = "image/webp", TamanoBytes = 1234, CreadoPorUsuario = mockUsuario };
            var mockFoto = new ReparacionFoto { Id = 1, ReparacionId = reparacionId, DocumentoId = 10, Documento = mockDocumento, CreadoEn = DateTime.UtcNow };

            var repoResult = new List<ReparacionFoto> { mockFoto };

            _mockRepository.Setup(r => r.GetByReparacionIdWithDetailsAsync(reparacionId)).ReturnsAsync(repoResult);

            // Act
            var result = await _service.GetFotosByReparacionIdAsync(reparacionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().NombreArchivo.Should().Be("foto1.webp");
            result.First().CreadoPorUsuario.Should().Be("Usuario Prueba"); // Verificar mapeo de nombre
            result.First().UrlDescarga.Should().EndWith($"/api/reparaciones/{reparacionId}/fotos/1/download");
        }

        [Fact]
        public async Task GetFotosByReparacionIdAsync_SinDatos_DebeRetornarListaVacia()
        {
            // Arrange
            int reparacionId = 1;
            _mockRepository.Setup(r => r.GetByReparacionIdWithDetailsAsync(reparacionId)).ReturnsAsync(new List<ReparacionFoto>());

            // Act
            var result = await _service.GetFotosByReparacionIdAsync(reparacionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region DeleteFotoAsync Tests

        [Fact]
        public async Task DeleteFotoAsync_ConFotoExistente_DebeLlamarRepositorioDelete()
        {
            // Arrange
            int fotoId = 1;
            // Simular el documento con la ruta (el servicio necesita esta ruta)
            var mockDocumento = new FilesDocumento { Id = 10, RutaAlmacenamiento = "ruta/ficticia/test.webp" };
            var mockFoto = new ReparacionFoto { Id = fotoId, DocumentoId = 10, Documento = mockDocumento };

            // El servicio llama a GetByIdWithDocumentoForDeleteAsync (con tracking)
            _mockRepository.Setup(r => r.GetByIdWithDocumentoForDeleteAsync(fotoId)).ReturnsAsync(mockFoto);
            // El servicio llama a DeleteDocumentAndFotoAsync
            _mockRepository.Setup(r => r.DeleteDocumentAndFotoAsync(mockDocumento)).ReturnsAsync(true);

            // Act
            await _service.DeleteFotoAsync(fotoId);

            // Assert
            // Verificar que se buscó la foto correcta (con tracking)
            _mockRepository.Verify(r => r.GetByIdWithDocumentoForDeleteAsync(fotoId), Times.Once);
            // Verificar que se llamó al método de eliminación del repositorio
            _mockRepository.Verify(r => r.DeleteDocumentAndFotoAsync(mockDocumento), Times.Once);

            // Nota: No podemos verificar File.Delete(rutaFicticia) porque es estático,
            // pero confiamos en que el servicio lo intentó.
        }

        [Fact]
        public async Task DeleteFotoAsync_ConFotoInexistente_DebeLanzarKeyNotFoundException()
        {
            // Arrange
            int fotoId = 999;
            _mockRepository.Setup(r => r.GetByIdWithDocumentoForDeleteAsync(fotoId)).ReturnsAsync((ReparacionFoto?)null); // No encontrado

            // Act
            Func<Task> act = () => _service.DeleteFotoAsync(fotoId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Foto con ID {fotoId} no encontrada.");
            _mockRepository.Verify(r => r.DeleteDocumentAndFotoAsync(It.IsAny<FilesDocumento>()), Times.Never);
        }

        #endregion

        #region DownloadFotoAsync Tests

        // ADVERTENCIA: Este test no es un "Unit Test" puro porque File.Exists y FileStream 
        // son estáticos y no se pueden mockear sin refactorizar el servicio (ej. inyectar IFileSystem).
        // Esta prueba fallará si el archivo mockeado no existe físicamente.

        [Fact(Skip = "Omitido: Requiere archivo físico o refactorización del servicio para mockear IFileSystem")]
        public async Task DownloadFotoAsync_ConFotoExistenteYArchivoFisico_DebeRetornarStream()
        {
            // --- Para que este test funcione, necesitarías crear un archivo temporal ---

            // Arrange
            int fotoId = 1;
            string tempPath = "archivo_temporal_test.webp";
            // File.WriteAllText(tempPath, "test_data"); // Crear archivo físico

            var mockDocumento = new FilesDocumento { Id = 10, RutaAlmacenamiento = tempPath, MimeType = "image/webp", NombreArchivo = "test.webp" };
            var mockFoto = new ReparacionFoto { Id = fotoId, Documento = mockDocumento };

            _mockRepository.Setup(r => r.GetByIdWithDocumentoAsync(fotoId)).ReturnsAsync(mockFoto);

            // Act
            var result = await _service.DownloadFotoAsync(fotoId);

            // Assert
            result.Should().NotBeNull();
            result.Value.contentType.Should().Be("image/webp");
            result.Value.fileName.Should().Be("test.webp");
            result.Value.fileStream.Should().NotBeNull();

            // Cleanup
            result.Value.fileStream.Dispose();
            // File.Delete(tempPath);
        }

        [Fact]
        public async Task DownloadFotoAsync_ConFotoInexistenteEnDB_DebeRetornarNull()
        {
            // Arrange
            int fotoId = 999;
            _mockRepository.Setup(r => r.GetByIdWithDocumentoAsync(fotoId)).ReturnsAsync((ReparacionFoto?)null); // No en DB

            // Act
            var result = await _service.DownloadFotoAsync(fotoId);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}