// =====================================================================================
// ENTIDAD LEGACY AGENTES REFERENCIA - AgentesRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para agentes del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.agentes
// - No contiene datos, solo referencias
//
// RELACIONES:
// - Uno a Muchos con catalog.agentes
// - Uno a Muchos con catalog.documentos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para agentes del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// </summary>
    [Table("agentes_ref", Schema = "legacy")]
    public class AgentesRef
    {
        /// <summary>
        /// ID del agente en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admAgentes.CIDAGENTE del sistema anterior
        /// </summary>
        [Key]
        [Column("legacy_agente_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LegacyAgenteId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de agentes locales asociados a esta referencia legacy
        /// </summary>
        public virtual ICollection<Agente> Agentes { get; set; } = new List<Agente>();

        /// <summary>
        /// Colección de documentos asociados a este agente legacy
        /// </summary>
        public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    }
}
