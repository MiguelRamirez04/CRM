using System;
using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Request
{
    /// <summary>
    /// DTO para crear un nuevo gasto de viático
    /// </summary>
    public class GastoViaticoCreateRequestDto
    {
        public int? OrdenId { get; set; }

        [Required(ErrorMessage = "El campo TieneFactura es requerido")]
        public bool TieneFactura { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(200, ErrorMessage = "El nombre del proveedor no puede exceder 200 caracteres")]
        public string? ProveedorNombre { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los kilómetros recorridos deben ser un valor positivo")]
        public int? KmRecorridos { get; set; }

        [Required(ErrorMessage = "El campo Gastos es requerido")]
        [StringLength(200, ErrorMessage = "El campo Gastos no puede exceder 200 caracteres")]
        public string Gastos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total debe ser un valor positivo")]
        public decimal MontoTotal { get; set; }

        [StringLength(200, ErrorMessage = "El lugar destino no puede exceder 200 caracteres")]
        public string? LugarDestino { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un viático existente
    /// </summary>
    public class GastoViaticoUpdateRequestDto
    {
        public int? OrdenId { get; set; }

        [Required(ErrorMessage = "El campo TieneFactura es requerido")]
        public bool TieneFactura { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(200)]
        public string? ProveedorNombre { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Range(0, int.MaxValue)]
        public int? KmRecorridos { get; set; }

        [Required(ErrorMessage = "El campo Gastos es requerido")]
        [StringLength(200)]
        public string Gastos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0, double.MaxValue)]
        public decimal MontoTotal { get; set; }

        [StringLength(200)]
        public string? LugarDestino { get; set; }
    }
}

