// =====================================================================================
// ENTIDAD CLIENTE - Cliente.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para clientes del módulo de Recepción.
// Representa la tabla Cliente en la base de datos con todas sus propiedades.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Gestión de clientes y sus órdenes de trabajo
//
// =====================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Recepcion
{
    /// <summary>
    /// Entidad que representa un cliente del sistema CRM
    /// </summary>
    [Table("Cliente")]
    public class Cliente
    {
        /// <summary>
        /// Identificador único del cliente (autoincremental)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del cliente
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del cliente
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el cliente está activo en el sistema
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado fiscal del cliente
        /// </summary>
        [StringLength(50)]
        [Column("estado_fiscal")]
        public string? EstadoFiscal { get; set; }

        /// <summary>
        /// RFC (Registro Federal de Contribuyentes) del cliente
        /// </summary>
        [StringLength(13)]
        [Column("RFC")]
        public string? RFC { get; set; }

        /// <summary>
        /// ID del cliente en el sistema legacy (para compatibilidad)
        /// </summary>
        [Column("legacy_client_id")]
        public int? LegacyClientId { get; set; }

        // Propiedades de navegación para Entity Framework
        
        /// <summary>
        /// Órdenes de trabajo asociadas a este cliente
        /// </summary>
        public virtual ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
    }
}

