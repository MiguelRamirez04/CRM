// =====================================================================================
// NÚMERO SERIE RESPONSE DTO - NumeroSerieResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO de respuesta para números de serie basado en la vista VwNumerosSerieCompletos.
// Mapea datos de catálogo local y referencias legacy.
//
// PROPIEDADES:
// - Datos locales: Id, NumeroSerie, Estado, EstadoAnterior, Costo, FechaTimestamp, Activo
// - Datos legacy: LegacySerieId, LegacyProductoId, LegacyAlmacenId, ProductoLegacy, 
//                 NumeroSerieLegacy, AlmacenLegacy, EstadoLegacy, EstadoAnteriorLegacy, 
//                 CostoLegacy, TimestampLegacy
//
// =====================================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para números de serie
    /// Basado en la vista VwNumerosSerieCompletos con datos locales y legacy
    /// </summary>
    public class NumeroSerieResponseDto
    {
        // ═══════════════════════════════════════════════════════════════
        // DATOS LOCALES (catalog.numeros_serie)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID único del número de serie en el sistema local
        /// </summary>
        [Required]
        public int NumeroSerieId { get; set; }

        /// <summary>
        /// Número de serie único del producto
        /// </summary>
        public string? NumeroSerie { get; set; }

        /// <summary>
        /// Estado actual del número de serie
        /// Ejemplos: Disponible, Vendido, En Reparación, etc.
        /// </summary>
        public int? Estado { get; set; }

        /// <summary>
        /// Estado anterior del número de serie
        /// Útil para auditoría y seguimiento de cambios
        /// </summary>
        public int? EstadoAnterior { get; set; }

        /// <summary>
        /// Costo del artículo con este número de serie
        /// </summary>
        public decimal? Costo { get; set; }

        /// <summary>
        /// Fecha y hora del último cambio/registro
        /// </summary>
        public DateTime? FechaTimestamp { get; set; }

        /// <summary>
        /// Indica si el número de serie está activo
        /// </summary>
        [Required]
        public bool Activo { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // DATOS LEGACY (legacy.numeros_serie_ref + COMPAC)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// ID del número de serie en el sistema legacy
        /// </summary>
        public int? LegacySerieId { get; set; }

        /// <summary>
        /// ID del producto asociado en el sistema legacy
        /// </summary>
        public int? LegacyProductoId { get; set; }

        /// <summary>
        /// ID del almacén donde se encuentra en el sistema legacy
        /// </summary>
        public int? LegacyAlmacenId { get; set; }

        /// <summary>
        /// Producto legacy (de COMPAC - CIDPRODUCTO es INT)
        /// </summary>
        public int? ProductoLegacy { get; set; }

        /// <summary>
        /// Número de serie en sistema legacy (CNUMEROSERIE es VARCHAR(30))
        /// </summary>
        public string? NumeroSerieLegacy { get; set; }

        /// <summary>
        /// Almacén en sistema legacy (CIDALMACEN es INT)
        /// </summary>
        public int? AlmacenLegacy { get; set; }

        /// <summary>
        /// Estado en sistema legacy (CESTADO es INT)
        /// </summary>
        public int? EstadoLegacy { get; set; }

        /// <summary>
        /// Estado anterior en sistema legacy (CESTADOANTERIOR es INT)
        /// </summary>
        public int? EstadoAnteriorLegacy { get; set; }

        /// <summary>
        /// Costo en sistema legacy
        /// </summary>
        public decimal? CostoLegacy { get; set; }

        /// <summary>
        /// Timestamp en sistema legacy (VARCHAR(23) - formato de texto COMPAC)
        /// </summary>
        public string? TimestampLegacy { get; set; }
    }
}
