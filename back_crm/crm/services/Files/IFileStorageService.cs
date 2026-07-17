using back_cabs.CRM.enums.Files;
using back_cabs.CRM.models.Files;
using Microsoft.AspNetCore.Http;

namespace back_cabs.CRM.services.Files;

/// <summary>
/// Interfaz para el servicio de almacenamiento genérico de archivos
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Sube un archivo al sistema
    /// </summary>
    /// <param name="file">Archivo a subir</param>
    /// <param name="entidadTipo">Tipo de entidad (Evaluacion, Reparacion, etc.)</param>
    /// <param name="entidadId">ID de la entidad</param>
    /// <param name="usuarioId">ID del usuario que sube el archivo</param>
    /// <param name="descripcion">Descripción opcional</param>
    /// <param name="categoria">Categoría opcional</param>
    /// <returns>Documento creado</returns>
    Task<FilesDocumento> UploadFileAsync(
        IFormFile file,
        TipoEntidadDocumento entidadTipo,
        int entidadId,
        int usuarioId,
        string? descripcion = null,
        string? categoria = null);

    /// <summary>
    /// Descarga un archivo por su ID
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>Stream del archivo, content type y nombre</returns>
    Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(int documentoId);

    /// <summary>
    /// Elimina un archivo (soft delete)
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <param name="usuarioId">ID del usuario que elimina</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<bool> DeleteFileAsync(int documentoId, int usuarioId);

    /// <summary>
    /// Obtiene todos los archivos de una entidad
    /// </summary>
    /// <param name="entidadTipo">Tipo de entidad</param>
    /// <param name="entidadId">ID de la entidad</param>
    /// <param name="soloActivos">Solo archivos activos</param>
    /// <returns>Lista de documentos</returns>
    Task<List<FilesDocumento>> GetFilesByEntidadAsync(
        TipoEntidadDocumento entidadTipo,
        int entidadId,
        bool soloActivos = true);

    /// <summary>
    /// Valida la integridad de un archivo usando su checksum
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>True si el archivo es íntegro</returns>
    Task<bool> ValidateFileIntegrityAsync(int documentoId);

    /// <summary>
    /// Obtiene un documento por ID
    /// </summary>
    Task<FilesDocumento?> GetFileByIdAsync(int documentoId);
}
