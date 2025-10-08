using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace back_cabs.CRM.models.Soporte
{
    public class FinanceGastosViaticos
    {
        public int Id { get; set; }
        public int CordenId { get; set; }
        public bool TieneFactura { get; set; }
        public string? Descripcion { get; set; }
        public string? ProveedorNombre { get; set; }
        public DateTime Fecha { get; set; }
        public int? KmRecorridos { get; set; }
        public string Gastos { get; set; }
        public decimal MontoTotal { get; set; }
        public string? LugarDestino { get; set; }
    }
}