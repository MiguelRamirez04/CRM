// =====================================================================================
// DTO RESPUESTA ALMACEN - AlmacenResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el contrato de respuesta para la API de almacenes.
// Contiene los datos que se devuelven al frontend.
//
// PROPIEDADES:
// - Datos básicos del almacén (id, código, nombre, etc.)
// - Información de configuración (clasificación, fecha alta, activo)
// - Referencia al sistema legacy
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para operaciones de almacenes
    /// </summary>
    public class AlmacenResponseDto
    {
        /// <summary>
        /// ID único del almacén en el sistema local
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del almacén en el sistema legacy (opcional)
        /// </summary>
        public int? LegacyAlmacenId { get; set; }

        /// <summary>
        /// Código único del almacén
        /// </summary>
        public string? CodigoAlmacen { get; set; }

        /// <summary>
        /// Nombre descriptivo del almacén
        /// </summary>
        public string? NombreAlmacen { get; set; }

        /// <summary>
        /// Fecha de alta del almacén en el sistema
        /// </summary>
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Clasificación del almacén (tipo, categoría)
        /// </summary>
        public int? Clasificacion1 { get; set; }

        /// <summary>
        /// Indica si el almacén está activo
        /// </summary>
        public bool Activo { get; set; }
    }
}