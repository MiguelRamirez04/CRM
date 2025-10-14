using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using back_cabs.CRM.enums;
namespace back_cabs.CRM.DTOs.Response
{
    public class GastoViaticoResponseDto
    {
        public int Id { get; set; }
        public TipoViatico TipoViatico { get; set; } = TipoViatico.ORDEN;

        public int? OrdenId { get; set; }
        public int UsuarioId { get; set; }
        public bool TieneFactura { get; set; }
        public string? Descripcion { get; set; }
        public string? ProveedorNombre { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? KmRecorridos { get; set; }
        public string? LugarDestino { get; set; }
        public EstadoGasto EstadoGasto { get; set; } = EstadoGasto.PENDIENTE;
        public int? DocumentoId { get; set; }
        public string? Observaciones { get; set; }
        public List<GastoViaticoDetalleResponseDto> Detalles { get; set; } = new();



        public class GastoViaticoDetalleResponseDto

        {

            
            public int Id { get; set; }
            public int GastoViaticoId { get; set; }
            public string TipoGasto { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string? Descripcion { get; set; }
        }
}
}