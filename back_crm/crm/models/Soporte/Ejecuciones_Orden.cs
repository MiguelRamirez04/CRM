using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Recepcion;
using back_cabs.CRM.models.Auth;
using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.models.Soporte
{
    /// <summary>
    /// Entidad que representa la ejecución de una orden de trabajo
    /// </summary>
    [Table("ops_ejecuciones_orden")]
    public class EjecucionOrden
    {
        /// <summary>
        /// Identificador único de la ejecución (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo asociada
        /// </summary>
        [Required]
        [Column("orden_id")]
        public int OrdenId { get; set; }

        /// <summary>
        /// Tipo de ejecución (CAMPO, REMOTO)
        /// </summary>
        [Required]
        [Column("tipo_ejecucion", TypeName = "varchar(20)")]
        public TipoEjecucion TipoEjecucion { get; set; }

        /// <summary>
        /// ID del técnico asignado
        /// </summary>
        [Required]
        [Column("tecnico_id")]
        public int TecnicoId { get; set; }

        /// <summary>
        /// Hora de inicio de la ejecución
        /// </summary>
        [Column("hr_inicio", TypeName = "timestamp")]
        public DateTime? HrInicio { get; set; }

        /// <summary>
        /// Hora de fin de la ejecución
        /// </summary>
        [Column("hr_fin", TypeName = "timestamp")]
        public DateTime? HrFin { get; set; }

        /// <summary>
        /// Comentarios sobre la ejecución
        /// </summary>
        [Column("comentarios")]
        public string? Comentarios { get; set; }

        /// <summary>
        /// ID del vehículo utilizado (para ejecuciones de campo)
        /// </summary>
        [Column("vehiculo_id")]
        public int? VehiculoId { get; set; }

        /// <summary>
        /// Kilometraje inicial del vehículo
        /// </summary>
        [Column("km_inicial")]
        public int? KmInicial { get; set; }

        /// <summary>
        /// Kilometraje final del vehículo
        /// </summary>
        [Column("km_final")]
        public int? KmFinal { get; set; }

        /// <summary>
        /// Herramientas utilizadas (para ejecuciones remotas)
        /// </summary>
        [StringLength(100)]
        [Column("herramientas")]
        public string? Herramientas { get; set; }

        /// <summary>
        /// Código de sesión remota
        /// </summary>
        [StringLength(100)]
        [Column("codigo_sesion")]
        public string? CodigoSesion { get; set; }

        /// <summary>
        /// Contraseña de sesión remota
        /// </summary>
        [StringLength(100)]
        [Column("contrasena_sesion")]
        public string? ContrasenaSesion { get; set; }

        // Propiedades de navegación
        [ForeignKey("OrdenId")]
        public virtual OrdenTrabajo? Orden { get; set; }

        [ForeignKey("TecnicoId")]
        public virtual UsuarioAuth? Tecnico { get; set; }

        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }
    }
}