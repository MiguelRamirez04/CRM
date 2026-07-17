using System.Security.Cryptography;
using System.Text.Json;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Files;
using back_cabs.CRM.enums.Files;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.services.shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Files;

/// <summary>
/// Servicio genérico para almacenar y gestionar archivos en el sistema.
/// Maneja imágenes (con conversión a WebP) y documentos (PDF, Excel, Word).
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ReadOnlyContext _readContext;
    private readonly WriteContext _writeContext;
    private readonly ImageProcessingService _imageProcessing;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadBasePath;
    private readonly long _maxFileSizeBytes;

    // Tipos MIME permitidos
    private static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
    {
        { "image", new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" } },
        { "pdf", new[] { "application/pdf" } },
        { "excel", new[] {
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        }},
        { "word", new[] {
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        }}
    };

    // Entidades que SOLO aceptan imágenes (se convierten a WebP)
    private static readonly HashSet<TipoEntidadDocumento> ImageOnlyEntities = new()
    {
        TipoEntidadDocumento.Evaluacion,
        TipoEntidadDocumento.Reparacion
    };

    public FileStorageService(
        ReadOnlyContext readContext,
        WriteContext writeContext,
        ImageProcessingService imageProcessing,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _readContext = readContext;
        _writeContext = writeContext;
        _imageProcessing = imageProcessing;
        _configuration = configuration;
        _logger = logger;
        _uploadBasePath = configuration["FileStorage:UploadPath"] ?? "uploads";
        _maxFileSizeBytes = configuration.GetValue<long>("FileStorage:MaxFileSizeMB", 10) * 1024 * 1024;

        // Crear directorio base si no existe
        if (!Directory.Exists(_uploadBasePath))
        {
            Directory.CreateDirectory(_uploadBasePath);
            _logger.LogInformation("Directorio de uploads creado: {Path}", _uploadBasePath);
        }
    }

    public async Task<FilesDocumento> UploadFileAsync(
        IFormFile file,
        TipoEntidadDocumento entidadTipo,
        int entidadId,
        int usuarioId,
        string? descripcion = null,
        string? categoria = null)
    {
        _logger.LogInformation(
            "Iniciando subida de archivo: {FileName}, Entidad: {TipoEntidad}/{EntidadId}, Usuario: {UsuarioId}",
            file.FileName, entidadTipo, entidadId, usuarioId);

        // 1. Validaciones
        ValidateFile(file, entidadTipo);

        // 2. Calcular checksum del archivo original
        string checksum;
        using (var stream = file.OpenReadStream())
        {
            checksum = await CalculateSHA256Async(stream);
        }

        _logger.LogDebug("Checksum calculado: {Checksum}", checksum);

        // 3. Verificar duplicados (mismo archivo, misma entidad)
        var existingFile = await _readContext.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.ChecksumSHA256 == checksum &&
                d.EntidadTipo == entidadTipo.ToString() &&
                d.EntidadId == entidadId &&
                d.Activo);

        if (existingFile != null)
        {
            _logger.LogInformation(
                "Archivo duplicado detectado - Reutilizando documento existente. Checksum: {Checksum}, DocumentoId: {Id}",
                checksum, existingFile.Id);
            
            // Retornar el documento existente en lugar de lanzar excepción
            // Esto permite reutilizar archivos idénticos (ej: misma foto en diferentes relaciones)
            return existingFile;
        }

        // 4. Determinar si es imagen
        var isImage = IsImageFile(file);

        // 5. Validar que entidades de solo imágenes reciban imágenes
        if (ImageOnlyEntities.Contains(entidadTipo) && !isImage)
        {
            throw new InvalidOperationException(
                $"La entidad {entidadTipo} solo acepta archivos de imagen.");
        }

        // 6. Procesar archivo
        string finalPath;
        string mimeType;
        long tamanoBytes;
        FileMetadatosDto metadatos;

        if (isImage && ImageOnlyEntities.Contains(entidadTipo))
        {
            // Convertir a WebP (solo para Evaluacion y Reparacion)
            (finalPath, mimeType, tamanoBytes, metadatos) = await ProcessImageToWebPAsync(
                file, entidadTipo, descripcion, categoria);
        }
        else
        {
            // Guardar archivo original (PDFs, Excel, Word, otras imágenes)
            (finalPath, mimeType, tamanoBytes, metadatos) = await SaveOriginalFileAsync(
                file, entidadTipo, descripcion, categoria);
        }

        // 7. Crear registro en base de datos
        var documento = new FilesDocumento
        {
            CreadoPorUsuarioId = usuarioId,
            CreadoEn = DateTime.UtcNow,
            EntidadTipo = entidadTipo.ToString(),
            EntidadId = entidadId,
            NombreArchivo = Path.GetFileName(finalPath),
            NombreOriginal = file.FileName,
            RutaAlmacenamiento = finalPath,
            MimeType = mimeType,
            TamanoBytes = tamanoBytes,
            ChecksumSHA256 = checksum,
            Activo = true,
            MetadatosJson = JsonSerializer.Serialize(metadatos)
        };

        _writeContext.Documentos.Add(documento);
        await _writeContext.SaveChangesAsync();

        _logger.LogInformation(
            "Archivo subido exitosamente. DocumentoId: {Id}, Ruta: {Ruta}",
            documento.Id, finalPath);

        return documento;
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(int documentoId)
    {
        var documento = await _readContext.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentoId && d.Activo);

        if (documento == null)
        {
            _logger.LogWarning("Documento no encontrado o inactivo: {Id}", documentoId);
            return null;
        }

        if (!File.Exists(documento.RutaAlmacenamiento))
        {
            _logger.LogError("Archivo físico no encontrado: {Ruta}", documento.RutaAlmacenamiento);
            throw new FileNotFoundException($"El archivo físico no existe: {documento.RutaAlmacenamiento}");
        }

        var stream = new FileStream(documento.RutaAlmacenamiento, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = documento.MimeType ?? "application/octet-stream";
        var fileName = documento.NombreOriginal ?? documento.NombreArchivo;

        _logger.LogInformation("Archivo descargado: {DocumentoId}, {FileName}", documentoId, fileName);

        return (stream, contentType, fileName);
    }

    public async Task<bool> DeleteFileAsync(int documentoId, int usuarioId)
    {
        var documento = await _writeContext.Documentos
            .FirstOrDefaultAsync(d => d.Id == documentoId && d.Activo);

        if (documento == null)
        {
            _logger.LogWarning("Documento no encontrado para eliminar: {Id}", documentoId);
            return false;
        }

        // Soft delete
        documento.Activo = false;
        documento.ActualizadoEn = DateTime.UtcNow;

        await _writeContext.SaveChangesAsync();

        _logger.LogInformation(
            "Archivo eliminado (soft delete). DocumentoId: {Id}, Usuario: {UsuarioId}",
            documentoId, usuarioId);

        return true;
    }

    public async Task<List<FilesDocumento>> GetFilesByEntidadAsync(
        TipoEntidadDocumento entidadTipo,
        int entidadId,
        bool soloActivos = true)
    {
        var query = _readContext.Documentos
            .AsNoTracking()
            .Where(d => d.EntidadTipo == entidadTipo.ToString() && d.EntidadId == entidadId);

        if (soloActivos)
        {
            query = query.Where(d => d.Activo);
        }

        return await query
            .OrderByDescending(d => d.CreadoEn)
            .ToListAsync();
    }

    public async Task<bool> ValidateFileIntegrityAsync(int documentoId)
    {
        var documento = await _readContext.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentoId && d.Activo);

        if (documento == null || !File.Exists(documento.RutaAlmacenamiento))
        {
            return false;
        }

        using var stream = File.OpenRead(documento.RutaAlmacenamiento);
        var currentChecksum = await CalculateSHA256Async(stream);

        var isValid = currentChecksum == documento.ChecksumSHA256;

        if (!isValid)
        {
            _logger.LogError(
                "Validación de integridad falló. DocumentoId: {Id}, Esperado: {Esperado}, Actual: {Actual}",
                documentoId, documento.ChecksumSHA256, currentChecksum);
        }

        return isValid;
    }

    public async Task<FilesDocumento?> GetFileByIdAsync(int documentoId)
    {
        return await _readContext.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentoId);
    }

    #region Métodos Privados

    private void ValidateFile(IFormFile file, TipoEntidadDocumento entidadTipo)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("El archivo es requerido y no puede estar vacío.");
        }

        if (file.Length > _maxFileSizeBytes)
        {
            var maxSizeMB = _maxFileSizeBytes / (1024 * 1024);
            throw new ArgumentException($"El archivo no puede exceder {maxSizeMB}MB. Tamaño actual: {file.Length / (1024 * 1024)}MB");
        }

        var isAllowed = AllowedMimeTypes.Values
            .SelectMany(x => x)
            .Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);

        if (!isAllowed)
        {
            throw new ArgumentException(
                $"Tipo de archivo no permitido: {file.ContentType}. " +
                $"Permitidos: imágenes (JPEG, PNG, WebP, GIF), PDF, Excel, Word.");
        }

        // Validar extensión también (doble verificación)
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".xls", ".xlsx", ".doc", ".docx" };

        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Extensión de archivo no permitida: {extension}");
        }
    }

    private bool IsImageFile(IFormFile file)
    {
        return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<(string path, string mimeType, long size, FileMetadatosDto metadatos)> ProcessImageToWebPAsync(
        IFormFile file,
        TipoEntidadDocumento entidadTipo,
        string? descripcion,
        string? categoria)
    {
        // Crear directorio específico para la entidad
        var entityFolder = Path.Combine(_uploadBasePath, entidadTipo.ToString().ToLowerInvariant());
        if (!Directory.Exists(entityFolder))
        {
            Directory.CreateDirectory(entityFolder);
        }

        // Generar nombre único
        var uniqueFileName = $"{Guid.NewGuid()}.webp";
        var finalPath = Path.Combine(entityFolder, uniqueFileName);

        // Convertir a WebP usando el servicio existente
        long tamanoBytes;
        int ancho, alto;
        
        using (var inputStream = file.OpenReadStream())
        {
            // Obtener configuración desde appsettings
            var quality = _configuration.GetValue<int>("FileStorage:WebPQuality", 80);
            var maxWidth = _configuration.GetValue<int?>("FileStorage:MaxImageWidth");
            var maxHeight = _configuration.GetValue<int?>("FileStorage:MaxImageHeight");

            (tamanoBytes, ancho, alto) = await _imageProcessing.ConvertToWebPAsync(
                inputStream,
                finalPath,
                quality,
                maxWidth,
                maxHeight);
        }

        var metadatos = new FileMetadatosDto
        {
            ArchivoOriginal = file.FileName,
            TamanoOriginal = file.Length,
            Descripcion = descripcion,
            Categoria = categoria,
            ConvertidoAWebP = true,
            FechaSubida = DateTime.UtcNow,
            AnchoFinal = ancho,
            AltoFinal = alto
        };

        _logger.LogInformation(
            "Imagen convertida a WebP: {Original} -> {WebP}, Tamaño original: {SizeOrig}bytes, Tamaño WebP: {SizeWebP}bytes, Dimensiones: {Width}x{Height}",
            file.FileName, uniqueFileName, file.Length, tamanoBytes, ancho, alto);

        return (finalPath, "image/webp", tamanoBytes, metadatos);
    }

    private async Task<(string path, string mimeType, long size, FileMetadatosDto metadatos)> SaveOriginalFileAsync(
        IFormFile file,
        TipoEntidadDocumento entidadTipo,
        string? descripcion,
        string? categoria)
    {
        // Crear directorio específico para la entidad
        var entityFolder = Path.Combine(_uploadBasePath, entidadTipo.ToString().ToLowerInvariant());
        if (!Directory.Exists(entityFolder))
        {
            Directory.CreateDirectory(entityFolder);
        }

        // Generar nombre único manteniendo la extensión original
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var finalPath = Path.Combine(entityFolder, uniqueFileName);

        // Guardar archivo original
        using (var stream = new FileStream(finalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var metadatos = new FileMetadatosDto
        {
            ArchivoOriginal = file.FileName,
            TamanoOriginal = file.Length,
            Descripcion = descripcion,
            Categoria = categoria,
            ConvertidoAWebP = false,
            FechaSubida = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Archivo guardado sin conversión: {FileName}, Tamaño: {Size}bytes",
            uniqueFileName, file.Length);

        return (finalPath, file.ContentType, file.Length, metadatos);
    }

    private async Task<string> CalculateSHA256Async(Stream stream)
    {
        using var sha256 = SHA256.Create();
        stream.Position = 0; // Asegurar que estamos al inicio
        var hashBytes = await sha256.ComputeHashAsync(stream);
        stream.Position = 0; // Resetear para uso posterior
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}
