// =====================================================================================
// ENTIDAD LEGACY CLIENTES REFERENCIA - ClientesRef.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de referencia para clientes del sistema legacy.
// Tabla puente que solo almacena IDs para mantener integridad referencial.
//
// PROPÓSITO:
// - Puente entre sistema legacy y nuevo sistema
// - Mantiene integridad referencial con catalog.clientes (ya existente)
// - No contiene datos, solo referencias
//
// NOTA:
// Esta tabla ya existe de una migración previa de clientes
//
// RELACIONES:
// - Uno a Muchos con catalog.clientes (ya existente en el sistema)
// - Uno a Muchos con catalog.documentos
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de referencia para clientes del sistema legacy
    /// Tabla puente sin datos, solo IDs para integridad referencial
    /// NOTA: Ya existente de migración previa
    /// </summary>
    [Table("clientes_ref", Schema = "legacy")]
    public class ClientesRef
    {
        /// <summary>
        /// ID del cliente en el sistema legacy (Primary Key)
        /// Mantiene sincronización con admClientes del sistema anterior
        /// NOTA: Es BIGINT en vez de INT por compatibilidad con sistema legacy
        /// </summary>
        [Key]
        [Column("legacy_client_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long LegacyClientId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de clientes locales asociados a esta referencia legacy
        /// NOTA: Relación con catalog.clientes ya existente en el sistema
        /// </summary>
        // Pendiente: public virtual ICollection<Cliente> Clientes { get; set; }

        /// <summary>
        /// Colección de documentos asociados a este cliente legacy
        /// </summary>
        // Pendiente: public virtual ICollection<Documento> Documentos { get; set; }
    }
}
