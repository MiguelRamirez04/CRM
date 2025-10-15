using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Auth;

namespace back_cabs.CRM.models.Files
{
    /// <summary>
    /// Entidad que representa un documento o archivo en el sistema.
    /// Mapea a la tabla 'files_documentos' y utiliza un enlace polimórfico.
    /// </summary>
    [Table("files_documentos")]
    public class FilesDocumento
    {
        /// <summary>
        /// Identificador único del documento (autoincremental).
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que subió el archivo.
        /// </summary>
        [Required]
        [Column("creado_por_usuario_id")]
        public int CreadoPorUsuarioId { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro.
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tipo de la entidad a la que se asocia el documento (ej: "Reparacion", "Evaluacion").
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("entidad_tipo", TypeName = "varchar(50)")] // Corregido: agregar TypeName
        public string EntidadTipo { get; set; } = string.Empty;

        /// <summary>
        /// ID de la entidad a la que se asocia el documento.
        /// </summary>
        [Required]
        [Column("entidad_id")]
        public int EntidadId { get; set; }

        /// <summary>
        /// Metadatos adicionales en formato JSON.
        /// </summary>
        [Column("metadatos_json", TypeName = "NVARCHAR(MAX)")]
        public string? MetadatosJson { get; set; }

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        [Column("tamano_bytes")]
        public long? TamanoBytes { get; set; }

        /// <summary>
        /// Tipo MIME del archivo (ej: "image/jpeg", "application/pdf").
        /// </summary>
        [StringLength(100)]
        [Column("mimetype", TypeName = "varchar(100)")] // Corregido: agregar TypeName
        public string? MimeType { get; set; }

        /// <summary>
        /// Ruta física o virtual donde se almacena el archivo.
        /// </summary>
        [Required]
        [StringLength(500)]
        [Column("ruta_almacenamiento")]
        public string RutaAlmacenamiento { get; set; } = string.Empty;

        /// <summary>
        /// Nombre original del archivo.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("nombre_archivo")]
        public string NombreArchivo { get; set; } = string.Empty;

        // --- Propiedades de Navegación ---
        [ForeignKey("CreadoPorUsuarioId")]
        public virtual UsuarioAuth? CreadoPorUsuario { get; set; }
    }
}