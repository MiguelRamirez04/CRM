// =====================================================================================
// MODELO NOTIFICACIÓN - Notificacion.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad Notificacion para el sistema de notificaciones en tiempo real.
// Almacena notificaciones que se envían a los usuarios sobre eventos del sistema.
//
// CUÁNDO USARLO:
// - Notificaciones de delegación de tareas
// - Alertas de tareas asignadas
// - Recordatorios del sistema
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Auth;

namespace back_cabs.CRM.models
{
    /// <summary>
    /// Entidad que representa una notificación del sistema
    /// </summary>
    [Table("notificaciones")]
    public class Notificacion
    {
        /// <summary>
        /// ID único de la notificación
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario destinatario
        /// </summary>
        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Tipo de notificación
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Título de la notificación
        /// </summary>
        [Required]
        [StringLength(200)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje detallado
        /// </summary>
        [Required]
        [StringLength(1000)]
        [Column("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Datos adicionales en JSON
        /// </summary>
        [Column("datos", TypeName = "NVARCHAR(MAX)")]
        public string? Datos { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        [Required]
        [Column("leida")]
        public bool Leida { get; set; } = false;

        /// <summary>
        /// Fecha de creación
        /// </summary>
        [Required]
        [Column("fecha_creacion", TypeName = "DATETIME2(0)")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de lectura
        /// </summary>
        [Column("fecha_lectura", TypeName = "DATETIME2(0)")]
        public DateTime? FechaLectura { get; set; }

        /// <summary>
        /// Nivel de prioridad
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("prioridad")]
        public string Prioridad { get; set; } = "MEDIA";

        /// <summary>
        /// Acción sugerida (URL o identificador)
        /// </summary>
        [StringLength(500)]
        [Column("accion")]
        public string? Accion { get; set; }

        // Navegación
        /// <summary>
        /// Usuario destinatario
        /// </summary>
        [ForeignKey("UsuarioId")]
        public virtual UsuarioAuth? Usuario { get; set; }

        /// <summary>
        /// Marca la notificación como leída
        /// </summary>
        public void MarcarComoLeida()
        {
            Leida = true;
            FechaLectura = DateTime.UtcNow;
        }
    }
}