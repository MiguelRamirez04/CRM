using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using back_cabs.CRM.models.Shared;
using Microsoft.VisualBasic;
using back_cabs.CRM.enums;

namespace back_cabs.CRM.models.Shared
{
    [Table("finance_gastos_viaticos")]
    public class GastoViatico
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("tipo_viatico")]
        [StringLength(20)]
        public TipoViatico TipoViatico { get; set; } = TipoViatico.GENERAL;

        [Column("orden_id")]
        public int? OrdenId { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("tiene_factura")]
        public bool TieneFactura { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("proveedor_nombre")]
        [StringLength(200)]
        public string? ProveedorNombre { get; set; }

        [Required]
        [Column("fecha", TypeName = "DATE")]
        public DateTime Fecha { get; set; }

        [Required]
        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("km_recorridos")]
        public int? KmRecorridos { get; set; }

        [Column("lugar_destino")]
        [StringLength(200)]
        public string? LugarDestino { get; set; }

        [Required]
        [Column("estado_gasto")]
        public EstadoGasto EstadoGasto { get; set; } = EstadoGasto.PENDIENTE;

        [Column("documento_id")]
        public int? DocumentoId { get; set; }

        [Column("observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

        public List<GastoViaticoDetalle> Detalles { get; set; } = new();
    }

 
}
