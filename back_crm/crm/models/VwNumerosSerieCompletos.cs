// =====================================================================================
// VISTA NÚMEROS SERIE COMPLETOS - VwNumerosSerieCompletos.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Entidad que mapea la vista VwNumerosSerieCompletos.
// Une datos de catalog.numeros_serie con legacy.numeros_serie_ref y COMPAC.admNumerosSerie.
//
// CAMPOS:
// - Locales: NumeroSerieId, NumeroSerie, Estado, EstadoAnterior, Costo, FechaTimestamp, Activo
// - Legacy: LegacySerieId, LegacyProductoId, LegacyAlmacenId, ProductoLegacy, NumeroSerieLegacy, 
//           AlmacenLegacy, EstadoLegacy, EstadoAnteriorLegacy, CostoLegacy, TimestampLegacy
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models
{
    /// <summary>
    /// Vista que combina datos de números de serie locales y legacy
    /// Une catalog.numeros_serie con legacy.numeros_serie_ref y COMPAC.admNumerosSerie
    /// </summary>
    [Table("VwNumerosSerieCompletos")]
    public class VwNumerosSerieCompletos
    {
        // ═══════════════════════════════════════════════════════════════
        // DATOS LOCALES (catalog.numeros_serie)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del número de serie en el sistema local
        /// </summary>
        [Key]
        [Column("NumeroSerieId")]
        public int NumeroSerieId { get; set; }

        /// <summary>
        /// Número de serie único del producto
        /// </summary>
        [Column("NumeroSerie")]
        public string? NumeroSerie { get; set; }

        /// <summary>
        /// Estado actual del número de serie
        /// Ejemplos: Disponible, Vendido, En Reparación, etc.
        /// </summary>
        [Column("Estado")]
        public int? Estado { get; set; }

        /// <summary>
        /// Estado anterior del número de serie
        /// Útil para auditoría y seguimiento de cambios
        /// </summary>
        [Column("EstadoAnterior")]
        public int? EstadoAnterior { get; set; }

        /// <summary>
        /// Costo del artículo con este número de serie
        /// </summary>
        [Column("Costo")]
        public decimal? Costo { get; set; }

        /// <summary>
        /// Fecha y hora del último cambio/registro
        /// </summary>
        [Column("FechaTimestamp")]
        public DateTime? FechaTimestamp { get; set; }

        /// <summary>
        /// Indica si el número de serie está activo
        /// </summary>
        [Column("Activo")]
        public bool Activo { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS LEGACY (legacy.numeros_serie_ref + COMPAC)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del número de serie en el sistema legacy
        /// </summary>
        [Column("LegacySerieId")]
        public int? LegacySerieId { get; set; }

        /// <summary>
        /// ID del producto asociado en el sistema legacy
        /// </summary>
        [Column("LegacyProductoId")]
        public int? LegacyProductoId { get; set; }

        /// <summary>
        /// ID del almacén donde se encuentra en el sistema legacy
        /// </summary>
        [Column("LegacyAlmacenId")]
        public int? LegacyAlmacenId { get; set; }

        /// <summary>
        /// Producto legacy (de COMPAC - CIDPRODUCTO es INT)
        /// </summary>
        [Column("ProductoLegacy")]
        public int? ProductoLegacy { get; set; }

        /// <summary>
        /// Número de serie en sistema legacy (CNUMEROSERIE es VARCHAR(30))
        /// </summary>
        [Column("NumeroSerieLegacy")]
        public string? NumeroSerieLegacy { get; set; }

        /// <summary>
        /// Almacén en sistema legacy (CIDALMACEN es INT)
        /// </summary>
        [Column("AlmacenLegacy")]
        public int? AlmacenLegacy { get; set; }

        /// <summary>
        /// Estado en sistema legacy (CESTADO es INT)
        /// </summary>
        [Column("EstadoLegacy")]
        public int? EstadoLegacy { get; set; }

        /// <summary>
        /// Estado anterior en sistema legacy (CESTADOANTERIOR es INT)
        /// </summary>
        [Column("EstadoAnteriorLegacy")]
        public int? EstadoAnteriorLegacy { get; set; }

        /// <summary>
        /// Costo en sistema legacy
        /// </summary>
        [Column("CostoLegacy")]
        public decimal? CostoLegacy { get; set; }

        /// <summary>
        /// Timestamp en sistema legacy (VARCHAR(23) - formato de texto COMPAC)
        /// </summary>
        [Column("TimestampLegacy")]
        public string? TimestampLegacy { get; set; }
    }
}
