using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Sales
{
    /// <summary>
    /// Entidad que representa una cotización de venta.
    /// Mapea a la tabla: dbo.sales_cotizaciones
    /// </summary>
    [Table("sales_cotizaciones")]  
    public class Cotizacion
    {
        // --- Llave Primaria ---
        [Key]
        [Column("CIDDOCUMENTO")]
        public int Id { get; set; }

        // --- Llaves Foráneas (IDs) ---
        [Column("CIDDOCUMENTODE")]
        public int DocumentoDeId { get; set; }

        [Column("CIDCONCEPTODOCUMENTO")]
        public int ConceptoDocumentoId { get; set; }

        [Column("CIDCLIENTEPROVEEDOR")]
        public int ClienteProveedorId { get; set; }

        [Column("CIDAGENTE")]
        public int AgenteId { get; set; }

        [Column("CIDDOCUMENTOORIGEN")]
        public int? DocumentoOrigenId { get; set; }

        // --- Datos del Documento (Folio, Fechas) ---
        [Column("CSERIEDOCUMENTO")]
        [StringLength(11)]
        public string? SerieDocumento { get; set; }

        [Column("CFOLIO")]
        public double Folio { get; set; }

        [Column("CFECHA")]
        public DateTime Fecha { get; set; }

        [Column("CFECHAVENCIMIENTO")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("CFECHAENTREGARECEPCION")]
        public DateTime? FechaEntregaRecepcion { get; set; }

        // --- Datos Descriptivos (Cliente, Texto) ---
        [Column("CRAZONSOCIAL")]
        [StringLength(60)]
        public string? RazonSocial { get; set; }

        [Column("CRFC")]
        [StringLength(20)]
        public string? Rfc { get; set; }

        [Column("CREFERENCIA")]
        [StringLength(20)]
        public string? Referencia { get; set; }

        [Column("COBSERVACIONES", TypeName = "varchar(max)")]
        public string? Observaciones { get; set; }

        // --- Banderas y Estados ---
        [Column("CNATURALEZA")]
        public int Naturaleza { get; set; }

        [Column("CUSACLIENTE")]
        public int UsaCliente { get; set; }

        [Column("CAFECTADO")]
        public int Afectado { get; set; }

        [Column("CIMPRESO")]
        public int Impreso { get; set; }

        [Column("CCANCELADO")]
        public int Cancelado { get; set; }

        // --- Importes y Totales ---
        [Column("CNETO")]
        public double Neto { get; set; }

        [Column("CIMPUESTO1")]
        public double Impuesto1 { get; set; }

        [Column("CDESCUENTOMOV")]
        public double DescuentoMovimiento { get; set; }

        [Column("CTOTAL")]
        public double Total { get; set; }

        [Column("CPENDIENTE")]
        public double Pendiente { get; set; }

        [Column("CTOTALUNIDADES")]
        public double TotalUnidades { get; set; }

        // Propiedades de navegación (opcionales, para relaciones con otras entidades)
        // Ejemplo:
        // [ForeignKey("ClienteProveedorId")]
        // public virtual ClienteProveedor Cliente { get; set; }
    }
}

