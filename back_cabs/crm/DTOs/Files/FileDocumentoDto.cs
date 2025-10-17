using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Files;

/// <summary>
/// DTO para subir un archivo al sistema
/// </summary>
public class FileUploadRequestDto
{
    /// <summary>
    /// Archivo a subir
    /// </summary>
    [Required(ErrorMessage = "El archivo es requerido")]
    public IFormFile Archivo { get; set; } = null!;

    /// <summary>
    /// Tipo de entidad (Evaluacion, Reparacion, GastoViatico, etc.)
    /// </summary>
    [Required(ErrorMessage = "El tipo de entidad es requerido")]
    [StringLength(50)]
    public string EntidadTipo { get; set; } = string.Empty;

    /// <summary>
    /// ID de la entidad a la que pertenece el archivo
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la entidad debe ser mayor a 0")]
    public int EntidadId { get; set; }

    /// <summary>
    /// Descripción opcional del archivo
    /// </summary>
    [StringLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoría del archivo (opcional)
    /// </summary>
    [StringLength(50)]
    public string? Categoria { get; set; }
}

/// <summary>
/// DTO de respuesta al subir un archivo
/// </summary>
public class FileResponseDto
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string TamanoFormateado { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; }
    public string CreadoPorNombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UrlDescarga { get; set; } = string.Empty;
    public bool EsImagen { get; set; }
    public bool ConvertidoAWebP { get; set; }
    public string? Categoria { get; set; }
}

/// <summary>
/// DTO para listar archivos por entidad
/// </summary>
public class FileListByEntidadRequestDto
{
    [Required]
    [StringLength(50)]
    public string EntidadTipo { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int EntidadId { get; set; }

    /// <summary>
    /// Filtrar solo por categoría específica
    /// </summary>
    public string? Categoria { get; set; }

    /// <summary>
    /// Solo imágenes
    /// </summary>
    public bool? SoloImagenes { get; set; }
}

/// <summary>
/// Metadatos almacenados en el campo JSON
/// </summary>
public class FileMetadatosDto
{
    public string? ArchivoOriginal { get; set; }
    public long? TamanoOriginal { get; set; }
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public bool ConvertidoAWebP { get; set; }
    public DateTime FechaSubida { get; set; }
    public int? AnchoOriginal { get; set; }
    public int? AltoOriginal { get; set; }
    public int? AnchoFinal { get; set; }
    public int? AltoFinal { get; set; }
}
