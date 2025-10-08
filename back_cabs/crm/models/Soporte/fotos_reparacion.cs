// =====================================================================================
// ENTIDAD REPARACIÓN FOTO - ReparacionFoto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad que asocia una foto (documento) a una reparación específica.
// Representa la tabla 'reparacion_fotos' en la base de datos.
//
// CUÁNDO USARLO:
// - Para registrar imágenes del estado de un dispositivo en diferentes etapas de la reparación.
// - Mapeo con Entity Framework.
// - Consultar las fotos asociadas a una reparación.
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Files; // Asumiendo que el modelo Documento está aquí

namespace back_cabs.CRM.models.Soporte
{
    /// <summary>
    /// Entidad que asocia una foto (documento) a una reparación en una etapa específica.
    /// Mapea a la tabla 'reparacion_fotos' en la base de datos.
    /// </summary>
    [Table("reparacion_fotos")]
    public class ReparacionFoto
    {
        /// <summary>
        /// Identificador único del registro (autoincremental).
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la reparación a la que pertenece esta foto.
        /// </summary>
        [Required]
        [Column("reparacion_id")]
        public int ReparacionId { get; set; }

        /// <summary>
        /// Propiedad de navegación para la reparación asociada.
        /// </summary>
        [ForeignKey("ReparacionId")]
        public virtual Reparacion Reparacion { get; set; } = null!;

        /// <summary>
        /// ID del documento (foto) asociado.
        /// </summary>
        [Required]
        [Column("documento_id")]
        public int DocumentoId { get; set; }

        // NOTA: Se asume que existe una entidad 'Documento' que mapea a 'files_documentos'.
        // Si no existe, deberás crearla.
        [ForeignKey("DocumentoId")]
        public virtual FilesDocumento Documento { get; set; } = null!;

        /// <summary>
        /// Etapa de la reparación en la que se tomó la foto (Recibido, Proceso, Entregado, Otro).
        /// </summary>
        [StringLength(20)]
        [Column("etapa")]
        public string? Etapa { get; set; } // Se recomienda usar el enum EtapaReparacionFoto y mapearlo a string.

        /// <summary>
        /// Descripción o nota sobre la foto.
        /// </summary>
        [StringLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro.
        /// </summary>
        [Required]
        [Column("creado_en", TypeName = "DATETIME2(0)")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}