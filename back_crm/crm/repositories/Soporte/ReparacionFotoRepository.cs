using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Soporte;
using back_cabs.CRM.models.Files;
using back_cabs.CRM.models.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace back_cabs.CRM.Repositories.Soporte
{
    /// <summary>
    /// Implementación de IReparacionFotoRepository usando EF Core.
    /// </summary>
    public class ReparacionFotoRepository : IReparacionFotoRepository
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<ReparacionFotoRepository> _logger;

        public ReparacionFotoRepository(
            WriteContext writeContext,
            ReadOnlyContext readContext,
            ILogger<ReparacionFotoRepository> logger)
        {
            _writeContext = writeContext;
            _readContext = readContext;
            _logger = logger;
        }

        public async Task<bool> ReparacionExistsAsync(int reparacionId)
        {
            try
            {
                return await _readContext.Reparaciones.AnyAsync(r => r.Id == reparacionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Reparacion ID {ReparacionId}", reparacionId);
                throw;
            }
        }

        public async Task<IEnumerable<ReparacionFoto>> GetByReparacionIdWithDetailsAsync(int reparacionId)
        {
            try
            {
                // Esta consulta incluye todo lo necesario para el mapeo del DTO
                return await _readContext.Set<ReparacionFoto>()
                    .Include(f => f.Documento)
                        .ThenInclude(d => d!.CreadoPorUsuario)
                    .Where(f => f.ReparacionId == reparacionId)
                    .OrderBy(f => f.CreadoEn)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByReparacionIdWithDetailsAsync para {ReparacionId}", reparacionId);
                throw;
            }
        }

        public async Task<ReparacionFoto?> GetByIdWithDocumentoAsync(int fotoId)
        {
            try
            {
                // Para descarga (Solo Lectura)
                return await _readContext.Set<ReparacionFoto>()
                    .Include(f => f.Documento)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == fotoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByIdWithDocumentoAsync para foto ID {FotoId}", fotoId);
                throw;
            }
        }

        public async Task<ReparacionFoto?> GetByIdWithDocumentoForDeleteAsync(int fotoId)
        {
            try
            {
                // Para eliminación (Con Tracking, usando WriteContext)
                return await _writeContext.Set<ReparacionFoto>()
                    .Include(f => f.Documento)
                    .FirstOrDefaultAsync(f => f.Id == fotoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByIdWithDocumentoForDeleteAsync para foto ID {FotoId}", fotoId);
                throw;
            }
        }

        public async Task<ReparacionFoto> CreateFotoInTransactionAsync(ReparacionFoto reparacionFoto, FilesDocumento documento)
{
    // 1. Obtener la estrategia de ejecución desde el DbContext
    var strategy = _writeContext.Database.CreateExecutionStrategy();

    // 2. Envolver TODA la lógica transaccional dentro del método ExecuteAsync de la estrategia
    return await strategy.ExecuteAsync(async () =>
    {
        // 3. La transacción manual ahora se ejecuta DENTRO del bloque reintentable
        using var transaction = await _writeContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Crear el documento primero para obtener su ID
            _writeContext.Documentos.Add(documento);
            await _writeContext.SaveChangesAsync();

            // 2. Vincular el ID del documento a la foto
            reparacionFoto.DocumentoId = documento.Id;
            
            // 3. Crear la foto
            _writeContext.Set<ReparacionFoto>().Add(reparacionFoto);
            await _writeContext.SaveChangesAsync();

            // 4. Confirmar la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Foto (ID {FotoId}) y Documento (ID {DocId}) creados en transacción.", reparacionFoto.Id, documento.Id);
            return reparacionFoto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error en CreateFotoInTransactionAsync. Transacción revertida.");
            // Re-lanzar la excepción para que la estrategia sepa que falló
            throw; 
        }
    });
}

        public async Task<bool> DeleteDocumentAndFotoAsync(FilesDocumento documento)
        {
            try
            {
                // Asumimos que la relación ReparacionFoto -> FilesDocumento
                // tiene OnDelete(Cascade) configurado en el DbContext.
                // Al eliminar el Documento, la ReparacionFoto se elimina automáticamente.
                _writeContext.Documentos.Remove(documento);
                await _writeContext.SaveChangesAsync();
                
                _logger.LogInformation("Documento ID {DocId} y ReparacionFoto asociada eliminados.", documento.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar Documento ID {DocId}", documento.Id);
                throw;
            }
        }
    }
}