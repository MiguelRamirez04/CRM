// =====================================================================================
// ENTIDAD REPARACIÓN - Reparacion.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para las reparaciones del módulo de Soporte.
// Representa la tabla 'reparaciones' en la base de datos con todas sus propiedades
// y sus relaciones, como la lista de componentes y fotos asociadas.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Gestión del ciclo de vida de una reparación (diagnóstico, solución, costos, etc.)
//
// =====================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using back_cabs.CRM.models.Auth; // Para la relación con UsuarioAuth
using back_cabs.CRM.models.Recepcion; // Para la relación con OrdenTrabajo

namespace back_cabs.CRM.models.Soporte
{
    /// <summary>
    /// Entidad que representa una reparación de un dispositivo asociado a una orden de trabajo.
    /// Mapea a la tabla 'reparaciones' en la base de datos.
    /// </summary>
    [Table("reparaciones")]
    public class Reparacion
    {
        /// <summary>
        /// Constructor para inicializar colecciones.
        /// </summary>
        public Reparacion()
        {
            Fotos = new HashSet<ReparacionFoto>();
        }

        /// <summary>
        /// Identificador único de la reparación (autoincremental).
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID de la orden de trabajo a la que pertenece esta reparación.
        /// </summary>
        [Required(ErrorMessage = "El ID de la orden es obligatorio.")]
        [Column("orden_id")]
        public int OrdenId { get; set; }

        /// <summary>
        /// Propiedad de navegación para la orden de trabajo asociada.
        /// </summary>
        /*
        [ForeignKey("OrdenId")]
        public virtual OrdenTrabajo Orden { get; set; } = null!; // Se asume que siempre habrá una orden
        */
        /// <summary>  
        /// ID del técnico asignado a esta reparación.
        /// </summary>
        [Required(ErrorMessage = "El ID del técnico es obligatorio.")]
        [Column("tecnico_id")]
        public int TecnicoId { get; set; }

        /// <summary>
        /// Propiedad de navegación para el usuario (técnico) asignado.
        /// </summary>
        /*
        [ForeignKey("TecnicoId")]
        public virtual UsuarioAuth Tecnico { get; set; } = null!; // Se asume que siempre habrá un técnico
        */
        /// <summary>
        /// Tipo de dispositivo a reparar (ej. "Laptop", "Celular", "Tablet").
        /// </summary>
        [Required(ErrorMessage = "El tipo de dispositivo es obligatorio.")]
        [StringLength(80)]
        [Column("dispositivo_tipo")]
        public string DispositivoTipo { get; set; } = string.Empty;

        /// <summary>
        /// Marca del dispositivo.
        /// </summary>
        [StringLength(80)]
        [Column("marca")]
        public string? Marca { get; set; }

        /// <summary>
        /// Modelo del dispositivo.
        /// </summary>
        [StringLength(120)]
        [Column("modelo")]
        public string? Modelo { get; set; }

        /// <summary>
        /// Lista de accesorios recibidos junto con el dispositivo.
        /// </summary>
        [StringLength(500)]
        [Column("accesorios_recibidos")]
        public string? AccesoriosRecibidos { get; set; }

        /// <summary>
        /// Descripción detallada de la falla reportada por el cliente.
        /// </summary>
        [Required(ErrorMessage = "La descripción de la falla es obligatoria.")]
        [Column("descripcion_falla", TypeName = "NVARCHAR(MAX)")]
        public string DescripcionFalla { get; set; } = string.Empty;

        /// <summary>
        /// Diagnóstico realizado por el técnico.
        /// </summary>
        [Column("diagnostico", TypeName = "NVARCHAR(MAX)")] // Changed to NVARCHAR(MAX)
        public string? Diagnostico { get; set; }

        /// <summary>
        /// Descripción de la solución aplicada al dispositivo.
        /// </summary>
        [Column("solucion_aplicada", TypeName = "NVARCHAR(MAX)")] // Changed to NVARCHAR(MAX)
        public string? SolucionAplicada { get; set; }

        /// <summary>
        /// Resultado final de la reparación (REPARADO, IRREPARABLE, COTIZAR, DEVUELTO_SIN_REPARAR).
        /// </summary>
        [Required(ErrorMessage = "El resultado de la reparación es obligatorio.")]
        [StringLength(20)]
        [Column("resultado")]
        public string Resultado { get; set; } = string.Empty; // Considerar un enum para valores fijos

        /// <summary>
        /// Causa detallada si el dispositivo fue declarado irreparable.
        /// </summary>
        [Column("causa_irreparable", TypeName = "NVARCHAR(MAX)")]
        public string? CausaIrreparable { get; set; }

        /// <summary>
        /// Indica si el respaldo de datos del cliente fue autorizado.
        /// </summary>
        [Required]
        [Column("respaldo_datos_autorizado")]
        public bool RespaldoDatosAutorizado { get; set; } = false;

        /// <summary>
        /// Costo de la mano de obra de la reparación.
        /// </summary>
        [Column("costo_mano_obra", TypeName = "decimal(12,2)")]
        public decimal? CostoManoObra { get; set; }

        /// <summary>
        /// Costo de compra de las refacciones utilizadas.
        /// </summary>
        [Column("costo_refacciones_compra", TypeName = "decimal(12,2)")]
        public decimal? CostoRefaccionesCompra { get; set; }

        /// <summary>
        /// Precio de venta al público de las refacciones utilizadas.
        /// </summary>
        [Column("costo_refacciones_publico", TypeName = "decimal(12,2)")]
        public decimal? CostoRefaccionesPublico { get; set; }

        /// <summary>
        /// Costo total de la reparación a precio de compra (mano de obra + refacciones compra).
        /// Esta es una propiedad calculada en la base de datos, no mapeada directamente.
        /// </summary>
        [Column("costo_total_compra", TypeName = "decimal(12,2)")]
        public decimal CostoTotalCompra { get; set; }

        /// <summary>
        /// Costo total de la reparación a precio público (mano de obra + refacciones público).
        /// Esta es una propiedad calculada en la base de datos, no mapeada directamente.
        /// </summary>
        [Column("costo_total_publico", TypeName = "decimal(12,2)")]
        public decimal CostoTotalPublico { get; set; }

        /// <summary>
        /// Margen estimado de ganancia por las refacciones.
        /// Esta es una propiedad calculada en la base de datos, no mapeada directamente.
        /// </summary>
        [Column("margen_estimado", TypeName = "decimal(12,2)")]
        public decimal MargenEstimado { get; set; }

        /// <summary>
        /// Días de garantía ofrecidos para la reparación.
        /// </summary>
        [Column("garantia_dias")]
        public int? GarantiaDias { get; set; }

        /// <summary>
        /// Fecha y hora en que el dispositivo llegó al taller.
        /// </summary>
        [Required]
        [Column("fecha_llegada", TypeName = "DATETIME2(0)")]
        public DateTime FechaLlegada { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora en que se comenzó a trabajar en la reparación.
        /// </summary>
        [Column("empezado_en", TypeName = "DATETIME2(0)")]
        public DateTime? EmpezadoEn { get; set; }

        /// <summary>
        /// Fecha y hora en que la reparación fue entregada al cliente.
        /// </summary>
        [Column("entregado_en", TypeName = "DATETIME2(0)")]
        public DateTime? EntregadoEn { get; set; }

        /// <summary>
        /// Tipo de entrega del dispositivo (DOMICILIO, RECOGE_CLIENTE).
        /// </summary>
        [Required(ErrorMessage = "El tipo de entrega es obligatorio.")]
        [StringLength(20)]
        [Column("tipo_entrega")]
        public string TipoEntrega { get; set; } = string.Empty; // Considerar un enum para valores fijos

        /// <summary>
        /// Ubicación física del dispositivo en el almacén o taller.
        /// </summary>
        [StringLength(200)]
        [Column("ubicacion_almacenamiento")]
        public string? UbicacionAlmacenamiento { get; set; }

        /// <summary>
        /// Notas adicionales sobre la reparación.
        /// </summary>
        [Column("notas", TypeName = "NVARCHAR(MAX)")] // Changed to NVARCHAR(MAX)
        public string? Notas { get; set; }

<<<<<<< HEAD
        [Column("nombre_cliente")]
        public string? NombreCliente {get; set;}

        [Column("telefono")]
        public long Telefono {get; set;}
        /// <summary>
        /// Colección de fotos asociadas a esta reparación.
        /// </summary>
        public virtual ICollection<ReparacionFoto> Fotos { get; set; }
    }
=======
        /// <summary>
        /// Colección de fotos asociadas a esta reparación.
        /// </summary>
        public virtual ICollection<ReparacionFoto> Fotos { get; set; }
    }
>>>>>>> 3a6bacfee886888ba16e7a8430bc6b20ed889301
}