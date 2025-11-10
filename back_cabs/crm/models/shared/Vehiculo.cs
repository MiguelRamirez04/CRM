using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Entidad que representa un vehículo de la flota
    /// </summary>
    [Table("fleet_vehiculos")]
    public class Vehiculo
    {
        /// <summary>
        /// Identificador único del vehículo (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Tipo de vehículo
        /// </summary>
        [Column("tipo_vehiculo")]
        [StringLength(50)]
        public string? TipoVehiculo { get; set; }

        /// <summary>
        /// Tipo de transmisión del vehículo
        /// </summary>
        [Column("transmision")]
        [StringLength(20)]
        public string? Transmision { get; set; }

        /// <summary>
        /// Indica si el vehículo pertenece a la empresa
        /// </summary>
        [Required]
        [Column("es_de_empresa")]
        public bool EsDeEmpresa { get; set; } = true;

        /// <summary>
        /// Placas del vehículo
        /// </summary>
        [Column("placas")]
        [StringLength(20)]
        public string? Placas { get; set; }

        /// <summary>
        /// Indica si el vehículo está activo
        /// </summary>
        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Observaciones adicionales del vehículo
        /// </summary>
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Nombre del vehículo
        /// </summary>
        [Required]
        [Column("nombre_vehiculo")]
        [StringLength(100)]
        public string NombreVehiculo { get; set; } = string.Empty;

        /// <summary>
        /// Kilometraje actual del vehículo
        /// </summary>
        [Required]
        [Column("kilometraje")]
        public int Kilometraje { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del usuario que creó el registro
        /// </summary>
        [Column("creado_por_usuario_id")]
        public int? CreadoPorUsuarioId { get; set; }

        /// <summary>
        /// Fecha de última actualización del registro
        /// </summary>
        [Column("actualizado_en")]
        public DateTime? ActualizadoEn { get; set; }

        /// <summary>
        /// ID del usuario que realizó la última actualización
        /// </summary>
        [Column("actualizado_por_usuario_id")]
        public int? ActualizadoPorUsuarioId { get; set; }

        /// <summary>
        /// Historial de cambios en formato JSON (array de objetos con fecha, usuario_id y cambios)
        /// </summary>
        [Column("historial_cambios")]
        public string? HistorialCambios { get; set; }
    }
}
