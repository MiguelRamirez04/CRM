using System;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para gastos de viáticos
    /// </summary>
    public class GastoViaticoResponseDto
    {
        public int Id { get; set; }
        public int? OrdenId { get; set; }
        
        /// <summary>
        /// ID del vehículo usado (opcional)
        /// </summary>
        public int? VehiculoId { get; set; }
        
        /// <summary>
        /// Nombre del vehículo (si se usó)
        /// </summary>
        public string? VehiculoNombre { get; set; }
        
        /// <summary>
        /// Placas del vehículo (si se usó)
        /// </summary>
        public string? VehiculoPlacas { get; set; }
        public bool TieneFactura { get; set; }
        public string? Descripcion { get; set; }
        public string? ProveedorNombre { get; set; }
        public DateTime Fecha { get; set; }
        public int? KmRecorridos { get; set; }
        public string Gastos { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public string? LugarDestino { get; set; }
    }
}