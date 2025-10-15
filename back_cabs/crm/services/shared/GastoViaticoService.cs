using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using CRM.DTOs.Response;
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
        /// <returns>DTO del viático creado.</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos.</exception>
        /// <exception cref="DbUpdateException">Si hay error al guardar en BD.</exception>
        public async Task<GastoViaticoResponseDto> CreateViaticoAsync(GastoViaticoCreateRequestDto dto)
        {

            _logger.LogInformation("Iniciando creación de viático para usuario {UsuarioId}", dto.UsuarioId);
            if (dto.TipoViatico == TipoViatico.ORDEN && !dto.OrdenId.HasValue)
                throw new ArgumentException("OrdenId es requerido para TipoViatico 'ORDEN' ", nameof(dto.OrdenId));

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("Se requiere al menos un detalle de viático.", nameof(dto.Detalles));

            var usuarioExiste = await _readonlycontext.UsuariosAuth.AnyAsync(u => u.Id == dto.UsuarioId);
            if (!usuarioExiste)
            {
                _logger.LogWarning("Usuario con ID {UsuarioId} no existe.", dto.UsuarioId);
                throw new ArgumentException($"Usuario con ID {dto.UsuarioId} no existe.", nameof(dto.UsuarioId));
            }


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
                return new GastoViaticoResponseDto
                {
                    Id = viatico.Id,
                    TipoViatico = viatico.TipoViatico,
                    OrdenId = viatico.OrdenId,
                    UsuarioId = viatico.UsuarioId,
                    TieneFactura = viatico.TieneFactura,
                    Descripcion = viatico.Descripcion,
                    ProveedorNombre = viatico.ProveedorNombre,
                    Fecha = viatico.Fecha,
                    FechaRegistro = viatico.FechaRegistro,
                    KmRecorridos = viatico.KmRecorridos,
                    LugarDestino = viatico.LugarDestino,
                    EstadoGasto = viatico.EstadoGasto,
                    DocumentoId = viatico.DocumentoId,
                    Observaciones = viatico.Observaciones,
                    Detalles = viatico.Detalles.Select(d => new GastoViaticoResponseDto.GastoViaticoDetalleResponseDto
                    {
                        Id = d.Id,
                        TipoGasto = d.TipoGasto,
                        Monto = d.Monto,
                        Descripcion = d.Descripcion
                    }).ToList()
                };
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


        public async Task<PaginatedResponseDto<GastoViaticoResponseDto>> GetViaticosAsync(
            int? usuarioId = null,
            TipoViatico? tipoViatico = null,
            EstadoGasto? estadoGasto = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? ordenId = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            _logger.LogInformation("Consultando viáticos con filtros: UsuarioId={UsuarioId}, Tipo={Tipo}, Estado={Estado}, Desde={Desde}, Hasta={Hasta}, Pagina={Pagina}, Tamano={Tamano}",
                usuarioId, tipoViatico, estadoGasto, fechaDesde, fechaHasta, pageNumber, pageSize);

            var query = _readonlycontext.GastosViaticosNuevos
                .Include(v => v.Detalles)
                .AsNoTracking()
                .AsQueryable();

            if (tipoViatico.HasValue)
                query = query.Where(v => v.TipoViatico == tipoViatico.Value);
            if (usuarioId.HasValue)
                query = query.Where(v => v.UsuarioId == usuarioId.Value);
            if (estadoGasto.HasValue)
                query = query.Where(v => v.EstadoGasto == estadoGasto.Value);
            if (fechaDesde.HasValue)
                query = query.Where(v => v.Fecha >= fechaDesde.Value);
            if (fechaHasta.HasValue)
                query = query.Where(v => v.Fecha <= fechaHasta.Value);

            // Paginación
            var viaticos = await query
                .OrderByDescending(v => v.FechaRegistro)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = viaticos.Select(v => new GastoViaticoResponseDto
            {
                Id = v.Id,
                TipoViatico = v.TipoViatico,
                OrdenId = v.OrdenId,
                UsuarioId = v.UsuarioId,
                TieneFactura = v.TieneFactura,
                Descripcion = v.Descripcion,
                ProveedorNombre = v.ProveedorNombre,
                Fecha = v.Fecha,
                FechaRegistro = v.FechaRegistro,
                KmRecorridos = v.KmRecorridos,
                LugarDestino = v.LugarDestino,
                EstadoGasto = v.EstadoGasto,
                DocumentoId = v.DocumentoId,
                Observaciones = v.Observaciones,
                Detalles = v.Detalles.Select(d => new GastoViaticoResponseDto.GastoViaticoDetalleResponseDto
                {
                    Id = d.Id,
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            }).ToList();

            var totalCount = await query.CountAsync();

            _logger.LogInformation("Se encontraron {Count} viáticos", result.Count);
            return new PaginatedResponseDto<GastoViaticoResponseDto>
            {
                Items = result,
                TotalItems = totalCount,
                Pagina = pageNumber,
                ResultadosPorPagina = pageSize
            };
        }

        /// <summary>
        /// Consulta un viático específico por ID (GET /api/viaticos/{id}).
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <returns>DTO del viático o null si no existe.</returns>
        public async Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id)
        {
            _logger.LogInformation("Consultando viático con ID {ViaticoId}", id);

            var viatico = await _readonlycontext.GastosViaticosNuevos
                .Include(v => v.Detalles)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viatico == null)
            {
                _logger.LogWarning("Viático con ID {ViaticoId} no encontrado", id);
                return null;
            }

            return new GastoViaticoResponseDto
            {
                Id = viatico.Id,
                TipoViatico = viatico.TipoViatico,
                OrdenId = viatico.OrdenId,
                UsuarioId = viatico.UsuarioId,
                TieneFactura = viatico.TieneFactura,
                Descripcion = viatico.Descripcion,
                ProveedorNombre = viatico.ProveedorNombre,
                Fecha = viatico.Fecha,
                FechaRegistro = viatico.FechaRegistro,
                KmRecorridos = viatico.KmRecorridos,
                LugarDestino = viatico.LugarDestino,
                EstadoGasto = viatico.EstadoGasto,
                DocumentoId = viatico.DocumentoId,
                Observaciones = viatico.Observaciones,
                Detalles = viatico.Detalles.Select(d => new GastoViaticoResponseDto.GastoViaticoDetalleResponseDto
                {
                    Id = d.Id,
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            };
        }

        /// <summary>
        /// Actualiza el estado de un viático (PATCH /api/viaticos/{id}/estado).
        /// Solo permite cambios de estado para auditoría.
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <param name="estado">Nuevo estado.</param>
        /// <exception cref="KeyNotFoundException">Si el viático no existe.</exception>
        /// <exception cref="ArgumentException">Si el estado no es válido.</exception>
        public async Task UpdateEstadoAsync(int id, EstadoGasto estado)
        {
            _logger.LogInformation("Actualizando estado de viático {ViaticoId} a {Estado}", id, estado);

            var viatico = await _writecontext.GastosViaticosNuevos.FindAsync(id);
            if (viatico == null)
                throw new KeyNotFoundException($"Viático con ID {id} no encontrado.");

            // Validación adicional: no permitir cambiar de PAGADO a otro estado
            if (viatico.EstadoGasto == EstadoGasto.PAGADO && estado != EstadoGasto.PAGADO)
                throw new ArgumentException("No se puede cambiar el estado de un viático pagado.", nameof(estado));

            viatico.EstadoGasto = estado;
            await _writecontext.SaveChangesAsync();

            _logger.LogInformation("Estado de viático {ViaticoId} actualizado exitosamente", id);
        }
    }
}



