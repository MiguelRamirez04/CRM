using System;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de una foto de reparación.
    /// </summary>
    public class ReparacionFotoResponseDto
    {
        /// <summary>
        /// ID de la foto.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la reparación asociada.
        /// </summary>
        public int ReparacionId { get; set; }

        /// <summary>
        /// ID del documento en files_documentos.
        /// </summary>
        public int DocumentoId { get; set; }

        /// <summary>
        /// Etapa de la reparación cuando se subió la foto.
        /// </summary>
        public string? Etapa { get; set; }

        /// <summary>
        /// Descripción de la foto.
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Fecha de creación de la foto.
        /// </summary>
        public DateTime CreadoEn { get; set; }

        /// <summary>
        /// Nombre del archivo guardado (formato WebP).
        /// </summary>
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME del archivo (image/webp).
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        public long? TamanoBytes { get; set; }

        /// <summary>
        /// Nombre del usuario que subió la foto.
        /// </summary>
        public string CreadoPorUsuario { get; set; } = string.Empty;

        /// <summary>
        /// URL para descargar la foto.
        /// </summary>
        public string UrlDescarga { get; set; } = string.Empty;

        /// <summary>
        /// Metadatos adicionales (dimensiones, archivo original, etc.).
        /// </summary>
        public string? Metadatos { get; set; }
    }
}
