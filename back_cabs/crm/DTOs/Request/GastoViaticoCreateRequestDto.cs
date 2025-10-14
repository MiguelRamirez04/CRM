using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using back_cabs.CRM.enums;

namespace back_cabs.CRM.DTOs.Request
{

    public class GastoViaticoCreateRequestDto
    {


        [Required]
        public TipoViatico TipoViatico { get; set; } = TipoViatico.GENERAL;

        public int? OrdenId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public bool TieneFactura { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(200)]
        public string? ProveedorNombre { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int? KmRecorridos { get; set; }

        [StringLength(200)]
        public string? LugarDestino { get; set; }


        [StringLength(500)]
        public string? Observaciones { get; set; }



        [Required]
        public List<GastoViaticoDetalleCreateDto> Detalles { get; set; } = new();
    }
            

            public class GastoViaticoDetalleCreateDto
            {
                [Required]
                [StringLength(50)]
                public string TipoGasto { get; set; } = string.Empty; // Ej: 'COMIDA', 'HOSPEDAJE', 'COMBUSTIBLE', etc.

                [Required]
                [Range(0, double.MaxValue, ErrorMessage = "El monto debe de ser un valor positivo ")]
                public decimal Monto { get; set; }

                [StringLength(500)]
                public string? Descripcion { get; set; }
            }


    /// <summary>
    /// DTO para actualizar solo el estado de un viático
    /// </summary>
    public class GastoViaticoUpdateEstadoRequestDto
    {
        [Required]
        public EstadoGasto EstadoGasto { get; set; } // Usando el enum para mayor seguridad
    }
}

