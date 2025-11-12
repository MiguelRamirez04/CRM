// =====================================================================================
// DTO RESPONSE PRODUCTO - ProductoResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para transferir datos de productos desde la API hacia clientes.
// Incluye las propiedades esenciales para catálogo de productos.
//
// PROPIEDADES CLAVE:
// - CodigoProducto: SKU único del producto (filtrable)
// - NombreProducto: Nombre descriptivo (filtrable)
// - Precio1: Precio principal de venta
// - ClaveSat: Clave SAT para facturación electrónica
//
// USO:
// - Respuestas de GET /api/Producto
// - Respuestas de GET /api/Producto/search
// - Serialización JSON para cache Redis
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para productos del catálogo
    /// Contiene información completa de un producto para visualización
    /// </summary>
    public class ProductoResponseDto
    {
        /// <summary>
        /// ID único del producto en el sistema local
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del producto en el sistema legacy (si existe)
        /// </summary>
        public int? LegacyProductoId { get; set; }

        /// <summary>
        /// Código único del producto (SKU)
        /// Campo filtrable en búsquedas
        /// </summary>
        public string? CodigoProducto { get; set; }

        /// <summary>
        /// Nombre descriptivo del producto
        /// Campo filtrable en búsquedas
        /// </summary>
        public string? NombreProducto { get; set; }

        /// <summary>
        /// Tipo de producto (1=Producto, 2=Servicio, 3=Paquete, etc.)
        /// </summary>
        public int? TipoProducto { get; set; }

        /// <summary>
        /// Descripción detallada del producto
        /// </summary>
        public string? DescripcionProducto { get; set; }

        /// <summary>
        /// Precio de venta principal del producto
        /// </summary>
        public decimal? Precio1 { get; set; }

        /// <summary>
        /// Clave SAT del producto para facturación electrónica
        /// </summary>
        public string? ClaveSat { get; set; }

        /// <summary>
        /// Estado del producto (1=Activo, 2=Descontinuado, etc.)
        /// </summary>
        public int? StatusProducto { get; set; }

        /// <summary>
        /// Indica si el producto está activo en el sistema
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha de alta del producto en el sistema
        /// </summary>
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// ID de la unidad base de medida
        /// </summary>
        public int? UnidadBaseId { get; set; }

        /// <summary>
        /// Método de costeo (1=Promedio, 2=PEPS, 3=UEPS, etc.)
        /// </summary>
        public int? MetodoCosteo { get; set; }

        /// <summary>
        /// Subtipo o categoría del producto
        /// </summary>
        public int? Subtipo { get; set; }
    }
}
