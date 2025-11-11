// =====================================================================================
// ENTIDAD CATALOG MONEDAS - Moneda.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad de monedas del catálogo local con datos completos.
// Contiene información migrada del sistema legacy más datos locales.
//
// PROPÓSITO:
// - Almacenar catálogo de monedas del sistema
// - Mantener referencia con sistema legacy
// - Gestionar información de monedas para transacciones
//
// RELACIONES:
// - Muchos a Uno con legacy.monedas_ref (FK: LegacyMonedaId)
//
// USO:
// - Configuración de monedas en documentos
// - Conversiones de tipo de cambio
// - Reportes financieros
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Entidad de catálogo de monedas
    /// Almacena información completa de monedas del sistema
    /// </summary>
    [Table("monedas", Schema = "catalog")]
    public class Moneda
    {
        // ═══════════════════════════════════════════════════════════════
        // CLAVE PRIMARIA
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único de la moneda en el sistema local (autoincremental)
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CLAVE FORÁNEA LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID de la moneda en el sistema legacy
        /// Permite rastrear origen de la moneda migrada
        /// </summary>
        [Column("legacy_moneda_id")]
        public int? LegacyMonedaId { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // PROPIEDADES DE DATOS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Nombre completo de la moneda (ej: "Peso Mexicano", "Dólar Estadounidense")
        /// </summary>
        [Column("nombre_moneda")]
        [MaxLength(60)]
        public string? NombreMoneda { get; set; }

        /// <summary>
        /// Símbolo de la moneda (ej: "$", "USD", "€")
        /// </summary>
        [Column("simbolo_moneda")]
        [MaxLength(1)]
        public string? SimboloMoneda { get; set; }

        /// <summary>
        /// Clave SAT para facturación electrónica
        /// Debe coincidir con el catálogo de monedas del SAT
        /// </summary>
        [Column("clave_sat")]
        [MaxLength(3)]
        public string? ClaveSat { get; set; }

        /// <summary>
        /// Número de decimales para cálculos con esta moneda
        /// Ejemplo: 2 para pesos (centavos), 4 para criptomonedas
        /// </summary>
        [Column("decimales")]
        public int? Decimales { get; set; }

        /// <summary>
        /// Indica si la moneda está activa en el sistema
        /// true = disponible para uso, false = deshabilitada
        /// </summary>
        [Column("activo")]
        public bool Activo { get; set; } = true;

        // ═══════════════════════════════════════════════════════════════
        // RELACIONES DE NAVEGACIÓN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Referencia a la moneda en el sistema legacy
        /// Permite acceder a datos históricos si es necesario
        /// </summary>
        [ForeignKey(nameof(LegacyMonedaId))]
        public virtual MonedasRef? MonedasRef { get; set; }
    }
}
