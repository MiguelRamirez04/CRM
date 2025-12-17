using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Auth;

namespace back_cabs.CRM.models.Shared
{
    /// <summary>
    /// Entidad que representa el historial de uso de un vehículo.
    /// Registra quién usó el vehículo, cuándo y el kilometraje recorrido.
    /// </summary>
    [Table("fleet_uso_vehiculos")]
    public class UsoVehiculo
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("vehiculo_id")]
        public int VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual UsuarioAuth? Usuario { get; set; }

        /// <summary>
        /// Fecha y hora exacta de salida.
        /// Reemplaza/Complementa a los campos legacy fecha/hora_salida.
        /// </summary>
        [Column("fecha_inicio", TypeName = "DATETIME2(0)")]
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha y hora exacta de regreso.
        /// </summary>
        [Column("fecha_fin", TypeName = "DATETIME2(0)")]
        public DateTime? FechaFin { get; set; }

        // Mantenemos campos legacy por compatibilidad si es necesario, pero logicamente usaremos FechaInicio/Fin
        [Required]
        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Required]
        [Column("hora_salida")]
        public TimeSpan HoraSalida { get; set; }

        [Column("hora_regreso")]
        public TimeSpan? HoraRegreso { get; set; }

        [Required]
        [Column("motivo_uso")]
        [StringLength(500)]
        public string MotivoUso { get; set; } = string.Empty;

        [Required]
        [Column("kilometraje_inicial")]
        public int KilometrajeInicial { get; set; }

        [Column("kilometraje_final")]
        public int? KilometrajeFinal { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("firma_base64")]
        public string? FirmaBase64 { get; set; }

        [Required]
        [Column("estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "EN_USO"; // EN_USO, COMPLETADO, CANCELADO

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [NotMapped]
        public double? KilometrosRecorridos => KilometrajeFinal.HasValue ? KilometrajeFinal.Value - KilometrajeInicial : (double?)null;

        [NotMapped]
        public double? DuracionMinutos => FechaFin.HasValue && FechaInicio.HasValue 
            ? (FechaFin.Value - FechaInicio.Value).TotalMinutes 
            : (double?)null;
    }
}
