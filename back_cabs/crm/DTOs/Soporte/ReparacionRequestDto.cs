using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Soporte
{
    /// <summary>
    /// DTO para la creación de una nueva Reparación.
    /// Contiene solo los datos que el cliente debe enviar.
    /// </summary>
    public record ReparacionCreacionRequestDto
    {
        // LLAVES FORÁNEAS (Obligatorias)
        [Required(ErrorMessage = "El ID de la orden es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden debe ser válido.")]
        public int OrdenId { get; init; }

        [Required(ErrorMessage = "El ID del técnico es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del técnico debe ser válido.")]
        public int TecnicoId { get; init; }

        // INFORMACIÓN DEL DISPOSITIVO (Obligatoria)
        [Required(ErrorMessage = "El tipo de dispositivo es obligatorio.")]
        [StringLength(80, ErrorMessage = "El tipo de dispositivo no puede exceder 80 caracteres.")]
        public string DispositivoTipo { get; init; } = string.Empty;


        [StringLength(80, ErrorMessage = "La marca no puede exceder 80 caracteres.")]
        public string? Marca { get; init; }

        [StringLength(120, ErrorMessage = "El modelo no puede exceder 120 caracteres.")]
        public string? Modelo { get; init; }

        [StringLength(500, ErrorMessage = "La lista de accesorios no puede exceder 500 caracteres.")]
        public string? AccesoriosRecibidos { get; init; }

        [Required(ErrorMessage = "La descripción de la falla es obligatoria.")]
        public string DescripcionFalla { get; init; } = string.Empty;

        public string? Diagnostico { get; init; }

        public string? SolucionAplicada { get; init; }

        [Required(ErrorMessage = "El resultado de la reparación es obligatorio.")]
        [StringLength(20, ErrorMessage = "El resultado no puede exceder 20 caracteres.")]
        public string? Resultado { get; init; }


        public string? CausaIrreparable { get; init; }


        // ESTADO INICIAL Y LOGÍSTICA
        [Required]
        public bool RespaldoDatosAutorizado { get; init; } = false;


        public decimal CostoManoObra { get; init; } = 0; // Puede ser 0 si no se cobra mano de obra inicialmente


        public decimal CostoRefaccionesCompra { get; init; } = 0;

        public decimal CostoRefaccionesPublico { get; init; } = 0;

        // public decimal CostoTotalCompra { get; init; }

        // public decimal CostoTotalPublico { get; init; }

        // public decimal MargenEstimado { get; init; }

        public int? GarantiaDias { get; init; }

        public DateTime FechaLlegada { get; init; } = DateTime.UtcNow;

        public DateTime? EmpezadoEn { get; init; }

        public DateTime? EntregadoEn { get; init; }

        [Required(ErrorMessage = "El tipo de entrega es obligatorio.")]
        [StringLength(20, ErrorMessage = "El tipo de entrega no puede exceder 20 caracteres.")]
        public string TipoEntrega { get; init; } = string.Empty; // Considerar un enum para valores fijos


        [StringLength(200, ErrorMessage = "La ubicación de almacenamiento no puede exceder 200 caracteres.")]
        public string? UbicacionAlmacenamiento { get; init; }

        public string? Notas { get; init; }


    }

    public record ReparacionActualizacionRequestDto
    {
        // LLAVES FORÁNEAS (Obligatorias)
        [Required(ErrorMessage = "El ID de la orden es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden debe ser válido.")]
        public int OrdenId { get; init; }

        [Required(ErrorMessage = "El ID del técnico es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del técnico debe ser válido.")]
        public int TecnicoId { get; init; }

        [Required(ErrorMessage = "La solucion realizada es obligatoria.")]
        public string SolucionAplicada { get; init; } = string.Empty;

        [Required(ErrorMessage = "El resultado de la reparación es obligatorio.")]
        public string Resultado { get; init; } = string.Empty;

        public string? CausaIrreparable { get; init; }

        [Required(ErrorMessage = "El costo de mano de obra es necesario")]
        public decimal? CostoManoObra { get; init; }

        [Required(ErrorMessage = "El costo de la compra de refacciones es necesario")]
        public decimal? CostoRefaccionesCompra { get; init; }

        [Required(ErrorMessage = "El costo de las refacciones es necesario")]
        public decimal? CostoRefaccionesPublico { get; init; }

        public int? GarantiaDias { get; init; }

        [Required(ErrorMessage ="La fecha de inicio de reparacion es obligatoria")]
        public DateTime? EmpezadoEn { get; init; }

        [Required(ErrorMessage ="La fecha de entrega de la reparacion es obligatoria")]
        public DateTime? EntregadoEn { get; init; }

        [Required(ErrorMessage = "El tipo de entrega es obligatorio.")]
        public string TipoEntrega { get; init; } = string.Empty;

        
        public string? Notas { get; init; }
        
    }
}