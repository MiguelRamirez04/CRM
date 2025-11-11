// =====================================================================================
// ENTIDAD CATALOG AGENTES - Agente.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de agentes del catálogo local con datos completos.
// Representa vendedores o agentes de ventas en el sistema.
//
// PROPÓSITO:
// - Almacenar catálogo de agentes/vendedores
// - Mantener referencia con sistema legacy
// - Gestionar comisiones y asignaciones
//
// RELACIONES:
// - Muchos a Uno con legacy.agentes_ref (FK: LegacyAgenteId)
// - Uno a Muchos con catalog.documentos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de agentes/vendedores
    /// Almacena información completa de los agentes del sistema
    /// </summary>
    [Table("agentes", Schema = "catalog")]
    public class Agente
    {
        // ═══════════════════════════════════════════════════════════════
        // CLAVE PRIMARIA
        // ═══════════════════════════════════════════════════════════════

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLAVE FORÁNEA LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del agente en el sistema legacy
        /// </summary>
        [Column("legacy_agente_id")]
        public int? LegacyAgenteId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Código único del agente en el sistema
        /// </summary>
        [Column("codigo_agente")]
        [MaxLength(30)]
        public string? CodigoAgente { get; set; }

        /// <summary>
        /// Nombre completo del agente
        /// </summary>
        [Column("nombre_agente")]
        [MaxLength(60)]
        public string? NombreAgente { get; set; }

        /// <summary>
        /// Fecha de alta del agente en el sistema
        /// </summary>
        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Tipo de agente (1=Vendedor, 2=Cobrador, etc.)
        /// </summary>
        [Column("tipo_agente")]
        public int? TipoAgente { get; set; }

        /// <summary>
        /// Campo extra de texto para información adicional
        /// </summary>
        [Column("texto_extra2")]
        [MaxLength(50)]
        public string? TextoExtra2 { get; set; }

        /// <summary>
        /// Indica si el agente está activo
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia al agente en el sistema legacy
        /// </summary>
        [ForeignKey(nameof(LegacyAgenteId))]
        public virtual AgentesRef? AgentesRef { get; set; }

        /// <summary>
        /// Colección de documentos asignados a este agente
        /// </summary>
        // Pendiente: public virtual ICollection<Documento> Documentos { get; set; }
    }
}
