// =====================================================================================
// DTO RESPONSE DOCUMENTO MODELO - DocumentoModeloResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DTO para transferir datos de documentos modelo desde la API hacia clientes.
// Incluye las propiedades esenciales para catálogo de documentos modelo.
//
// PROPIEDADES CLAVE:
// - Descripcion: Nombre del tipo de documento (filtrable)
// - Naturaleza: 1=Cargo, 2=Abono
// - AfectaExistencia: Cómo afecta inventario
// - NoFolio: Número de folio actual
//
// USO:
// - Respuestas de GET /api/DocumentoModelo
// - Respuestas de GET /api/DocumentoModelo/search
// - Serialización JSON para cache Redis
//
// =====================================================================================

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO de respuesta para documentos modelo del catálogo
    /// Contiene información completa de tipos de documentos
    /// </summary>
    public class DocumentoModeloResponseDto
    {
        /// <summary>
        /// ID único del documento modelo en el sistema local
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del documento modelo en el sistema legacy
        /// </summary>
        public int? LegacyDocumentoModeloId { get; set; }

        /// <summary>
        /// Descripción del tipo de documento
        /// Campo filtrable en búsquedas
        /// Ejemplo: "Factura", "Remisión", "Pedido", "Devolución"
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Naturaleza del documento
        /// 1 = Cargo (aumenta saldo)
        /// 2 = Abono (disminuye saldo)
        /// </summary>
        public int? Naturaleza { get; set; }

        /// <summary>
        /// Indica cómo afecta la existencia
        /// 1 = Aumenta existencia
        /// 2 = Disminuye existencia
        /// 0 = No afecta
        /// </summary>
        public int? AfectaExistencia { get; set; }

        /// <summary>
        /// Número de folio actual para este tipo de documento
        /// Se incrementa automáticamente al crear documentos
        /// </summary>
        public double? NoFolio { get; set; }

        /// <summary>
        /// ID del concepto asumido por defecto para este modelo
        /// </summary>
        public int? ConceptoAsumidoId { get; set; }

        /// <summary>
        /// Indica si el documento modelo está activo
        /// </summary>
        public bool Activo { get; set; }
    }
}
