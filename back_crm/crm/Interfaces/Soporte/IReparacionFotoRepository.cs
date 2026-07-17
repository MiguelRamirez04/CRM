using back_cabs.CRM.models.Files;
using back_cabs.CRM.models.Soporte;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back_cabs.CRM.Interfaces.Soporte
{
    /// <summary>
    /// Contrato de acceso a datos para las entidades ReparacionFoto y FilesDocumento.
    /// Abstrae las operaciones de base de datos (lectura y escritura transaccional).
    /// </summary>
    public interface IReparacionFotoRepository
    {
        // --- Operaciones de Validación ---

        /// <summary>
        /// Verifica si una reparación existe (para validación de FK).
        /// </summary>
        Task<bool> ReparacionExistsAsync(int reparacionId);

        // --- Operaciones de Lectura (Queries) ---

        /// <summary>
        /// Obtiene todas las fotos de una reparación, incluyendo el documento y el usuario que lo creó.
        /// </summary>
        Task<IEnumerable<ReparacionFoto>> GetByReparacionIdWithDetailsAsync(int reparacionId);

        /// <summary>
        /// Obtiene una foto por ID (lectura simple), incluyendo su documento.
        /// </summary>
        Task<ReparacionFoto?> GetByIdWithDocumentoAsync(int fotoId);

        /// <summary>
        /// Obtiene una foto por ID (para eliminación), incluyendo su documento y con seguimiento (tracking).
        /// </summary>
        Task<ReparacionFoto?> GetByIdWithDocumentoForDeleteAsync(int fotoId);
        
        // --- Operaciones de Escritura (Commands) ---

        /// <summary>
        /// Guarda una nueva foto en una transacción (tabla Documentos y ReparacionFoto).
        /// </summary>
        /// <param name="reparacionFoto">La entidad ReparacionFoto (sin DocumentoId aún).</param>
        /// <param name="documento">La entidad FilesDocumento.</param>
        /// <returns>La entidad ReparacionFoto guardada (con su ID y DocumentoId actualizados).</returns>
        Task<ReparacionFoto> CreateFotoInTransactionAsync(ReparacionFoto reparacionFoto, FilesDocumento documento);

        /// <summary>
        /// Elimina un documento (y la ReparacionFoto asociada por cascada) de la base de datos.
        /// </summary>
        Task<bool> DeleteDocumentAndFotoAsync(FilesDocumento documento);
    }
}