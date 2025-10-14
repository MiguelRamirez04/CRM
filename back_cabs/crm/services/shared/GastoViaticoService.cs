using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;  // Para logging

namespace back_cabs.CRM.services.Shared
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio de viáticos.
    /// Maneja creación, consulta y actualización de viáticos y sus detalles.
    /// </summary>

    public class GastoViaticoService
    {
        private readonly WriteContext _writecontext;
        private readonly ReadOnlyContext _readonlycontext;
        private readonly ILogger<GastoViaticoService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        /// <param name="writeContext">Contexto para operaciones de escritura.</param>
        /// <param name="readContext">Contexto para operaciones de lectura.</param>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        /// 
        public GastoViaticoService(WriteContext writeContext, ReadOnlyContext readOnlyContext, ILogger<GastoViaticoService> logger)
        {
            _writecontext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readonlycontext = readOnlyContext ?? throw new ArgumentNullException(nameof(readOnlyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Crea un nuevo viático con sus detalles (POST /api/viaticos).
        /// Valida datos, crea en transacción y registra auditoría.
        /// </summary>
        /// <param name="dto">DTO con los datos del viático a crear.</param>
        /// <returns>ID del viático creado.</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos.</exception>
        /// <exception cref="DbUpdateException">Si hay error al guardar en BD.</exception>
        /// 

        public async Task<int> CreateViaticoAsync(GastoViaticoCreateRequestDto dto)
        {

            _logger.LogInformation("Iniciando creación de viático para usuario {UsuarioId}", dto.UsuarioId);
            if (dto.TipoViatico == TipoViatico.ORDEN && !dto.OrdenId.HasValue)
                throw new ArgumentException("OrdenId es requerido para TipoViatico 'ORDEN' ", nameof(dto.OrdenId));

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("Se requiere al menos un detalle de viático.", nameof(dto.Detalles));

            var usuarioExiste = await _readonlycontext.UsuariosAuth.AnyAsync(u => u.Id == dto.UsuarioId);
            if (!usuarioExiste)

                _logger.LogWarning("Usuario con ID {UsuarioId} no existe.", dto.UsuarioId);
            throw new ArgumentException($"Usuario con ID {dto.UsuarioId} no existe.", nameof(dto.UsuarioId));


            var viatico = new GastoViatico
            {
                TipoViatico = dto.TipoViatico,
                OrdenId = dto.OrdenId,
                UsuarioId = dto.UsuarioId,
                TieneFactura = dto.TieneFactura,
                Descripcion = dto.Descripcion,
                ProveedorNombre = dto.ProveedorNombre,
                Fecha = dto.Fecha,
                LugarDestino = dto.LugarDestino,
                Detalles = dto.Detalles.Select(d => new GastoViaticoDetalle
                {
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            };

            using var transaction = await _writecontext.Database.BeginTransactionAsync();
            try

            {

                _writecontext.GastosViaticosNuevos.Add(viatico);
                await _writecontext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"viatico creado exitosamente con ID {viatico.Id}");
                return viatico.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al crear viatico para el usuario {dto.UsuarioId}");
                throw new DbUpdateException("Error al guardar el viatico en la base de datos", ex);

            }
        }
        /// <summary>
        /// Obtiene los viáticos de un usuario con filtros opcionales (GET /api/viaticos).
        /// </summary>
        /// <param name="usuarioId">ID del usuario para filtrar.</param>
        /// <param name="tipoViatico">Tipo de viático para filtrar (op
        /// cional).</param>
        /// <param name="fechaDesde">Fecha inicial para filtrar (opcional).</param>
        /// <param name="fechaHasta">Fecha final para filtrar (opcional).</param>
        /// <returns>Lista de viáticos que cumplen los filtros.</returns>
        ///
        ///     
        ///  <exception cref="ArgumentException">Si el usuario no existe.</exception>
        ///     <exception cref="DbUpdateException">Si hay error al consultar en BD.</exception>
        ///     


        public async Task<List<GastoViaticoResponseDto>> GetViaticosAsync(
            TipoViatico? tipo = null,
            int? usuarioId = null,

            EstadoGasto? estado = null,

DateTime? fechaDesde = null,
DateTime? fechaHasta = null,
int pageNumber = 1,
int pageSize = 10
        )
        {
            // Implementación básica para evitar el error de compilación.
            // Puedes reemplazar esto con la lógica real de consulta.
            return await Task.FromResult(new List<GastoViaticoResponseDto>());
        }










    }
}



