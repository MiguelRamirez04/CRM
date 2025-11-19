using back_cabs.CRM.DTOs.Legacy;
using back_cabs.CRM.models.legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el repositorio de AdmDocumentos
    /// </summary>
    public interface IAdmDocumentoRepository
    {
        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        Task<(List<AdmDocumento> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter);
        
        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos
        /// </summary>
        Task<AdmDocumento?> GetByIdWithMovimientosAsync(int idDocumento);
        
        /// <summary>
        /// Obtiene los movimientos de un documento específico
        /// </summary>
        Task<List<AdmMovimiento>> GetMovimientosByDocumentoIdAsync(int idDocumento);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en la base de datos
        /// </summary>
        /// <param name="documento">Entidad AdmDocumento a insertar</param>
        /// <returns>El ID del documento creado (CIDDOCUMENTO)</returns>
        Task<int> CreateAsync(AdmDocumento documento);

        /// <summary>
        /// Verifica si existe un documento modelo por su ID
        /// </summary>
        /// <param name="idDocumentoDe">ID del modelo de documento</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsDocumentoModeloAsync(int idDocumentoDe);

        /// <summary>
        /// Verifica si existe un concepto por su ID
        /// </summary>
        /// <param name="idConcepto">ID del concepto</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsConceptoAsync(int idConcepto);

        /// <summary>
        /// Verifica si existe un cliente/proveedor por su ID
        /// </summary>
        /// <param name="idClienteProveedor">ID del cliente o proveedor</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsClienteProveedorAsync(int idClienteProveedor);

        /// <summary>
        /// Verifica si existe un agente por su ID
        /// </summary>
        /// <param name="idAgente">ID del agente</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsAgenteAsync(int idAgente);

        /// <summary>
        /// Verifica si existe una moneda por su ID
        /// </summary>
        /// <param name="idMoneda">ID de la moneda</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsMonedaAsync(int idMoneda);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA COTIZACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene el folio actual del concepto y lo bloquea para transacción
        /// </summary>
        /// <param name="idConcepto">ID del concepto (1 para cotizaciones)</param>
        /// <returns>El folio actual</returns>
        Task<double> GetFolioActualAsync(int idConcepto);

        /// <summary>
        /// Actualiza el folio del concepto después de crear un documento
        /// </summary>
        /// <param name="idConcepto">ID del concepto</param>
        /// <param name="nuevoFolio">Nuevo valor del folio</param>
        Task UpdateFolioConceptoAsync(int idConcepto, double nuevoFolio);

        /// <summary>
        /// Crea un documento con sus movimientos en una transacción
        /// </summary>
        /// <param name="documento">Entidad AdmDocumento</param>
        /// <param name="movimientos">Lista de movimientos (productos)</param>
        /// <returns>El ID del documento creado</returns>
        Task<int> CreateDocumentoConMovimientosAsync(AdmDocumento documento, List<AdmMovimiento> movimientos);

        /// <summary>
        /// Verifica si existe un almacén por su ID
        /// </summary>
        /// <param name="idAlmacen">ID del almacén</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsAlmacenAsync(int idAlmacen);

        /// <summary>
        /// Verifica si existe un producto por su ID
        /// </summary>
        /// <param name="idProducto">ID del producto</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsProductoAsync(int idProducto);

        /// <summary>
        /// Verifica si existe una unidad de medida por su ID
        /// </summary>
        /// <param name="idUnidad">ID de la unidad</param>
        /// <returns>True si existe, False si no</returns>
        Task<bool> ExistsUnidadAsync(int idUnidad);

        /// <summary>
        /// Cancela un documento (cambia CCANCELADO a 1)
        /// </summary>
        /// <param name="idDocumento">ID del documento a cancelar</param>
        /// <param name="motivo">Motivo de cancelación</param>
        /// <param name="usuario">Usuario que cancela</param>
        Task CancelarDocumentoAsync(int idDocumento, string? motivo, string usuario);

        /// <summary>
        /// Obtiene un documento por ID (solo encabezado)
        /// </summary>
        /// <param name="idDocumento">ID del documento</param>
        /// <returns>El documento o null si no existe</returns>
        Task<AdmDocumento?> GetDocumentoByIdAsync(int idDocumento);
    }
}
