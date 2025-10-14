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
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.Services.Shared
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio de viáticos.
    /// Maneja creación, consulta y actualización de viáticos y sus detalles.
    /// </summary>
    public class GastoViaticoService
    {
        private readonly WriteContext _writeContext;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<GastoViaticoService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        /// <param name="writeContext">Contexto para operaciones de escritura.</param>
        /// <param name="readContext">Contexto para operaciones de lectura.</param>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        public GastoViaticoService(WriteContext writeContext, ReadOnlyContext readContext, ILogger<GastoViaticoService> logger)
        {
            _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
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

            // Validaciones de negocio
            if (dto.TipoViatico == TipoViatico.ORDEN && !dto.OrdenId.HasValue)
                throw new ArgumentException("El campo OrdenId es obligatorio para viáticos por orden.", nameof(dto.OrdenId));

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("Debe incluir al menos un detalle de gasto.", nameof(dto.Detalles));

            // Validar que el usuario existe (ejemplo básico)
            var usuarioExiste = await _readContext.UsuariosAuth.AnyAsync(u => u.Id == dto.UsuarioId);
            if (!usuarioExiste)
                throw new ArgumentException("El usuario especificado no existe.", nameof(dto.UsuarioId));

            // Crear entidad principal
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
                Observaciones = dto.Observaciones,
                Detalles = dto.Detalles.Select(d => new GastoViaticoDetalle
                {
                    TipoGasto = d.TipoGasto,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList()
            };

            // Guardar en transacción para asegurar integridad
            using var transaction = await _writeContext.Database.BeginTransactionAsync();
            try
            {
                _writeContext.GastosViaticosNuevos.Add(viatico);
                await _writeContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Viático creado exitosamente con ID {ViaticoId}", viatico.Id);
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
                _logger.LogError(ex, "Error al crear viático para usuario {UsuarioId}", dto.UsuarioId);
                throw new DbUpdateException("Error al guardar el viático en la base de datos.", ex);
            }
        }

        /// <summary>
        /// Consulta viáticos con filtros opcionales (GET /api/viaticos).
        /// Incluye detalles y soporta paginación.
        /// </summary>
        /// <param name="tipo">Filtro por tipo de viático.</param>
        /// <param name="usuarioId">Filtro por usuario.</param>
        /// <param name="estado">Filtro por estado.</param>
        /// <param name="fechaDesde">Filtro por fecha desde.</param>
        /// <param name="fechaHasta">Filtro por fecha hasta.</param>
        /// <param name="pageNumber">Número de página (1-based).</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <returns>Lista paginada de viáticos.</returns>
        public async Task<List<GastoViaticoResponseDto>> GetViaticosAsync(
            TipoViatico? tipo = null,
            int? usuarioId = null,
            EstadoGasto? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            _logger.LogInformation("Consultando viáticos con filtros: Tipo={Tipo}, Usuario={UsuarioId}", tipo, usuarioId);

            var query = _readContext.GastosViaticosNuevos
                .Include(v => v.Detalles)  // Incluir detalles para evitar N+1 queries
                .AsNoTracking()  // Optimización para lecturas
                .AsQueryable();

            // Aplicar filtros
            if (tipo.HasValue) query = query.Where(v => v.TipoViatico == tipo.Value);
            if (usuarioId.HasValue) query = query.Where(v => v.UsuarioId == usuarioId.Value);
            if (estado.HasValue) query = query.Where(v => v.EstadoGasto == estado.Value);
            if (fechaDesde.HasValue) query = query.Where(v => v.Fecha >= fechaDesde.Value);
            if (fechaHasta.HasValue) query = query.Where(v => v.Fecha <= fechaHasta.Value);

            // Paginación
            var totalCount = await query.CountAsync();
            var viaticos = await query
                .OrderByDescending(v => v.FechaRegistro)  // Ordenar por fecha de registro descendente
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Consulta completada: {Count} viáticos encontrados", viaticos.Count);

            // Mapear a DTOs
            return viaticos.Select(v => new GastoViaticoResponseDto
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
        }

        /// <summary>
        /// Consulta un viático específico por ID (GET /api/viaticos/{id}).
        /// </summary>
        /// <param name="id">ID del viático.</param>
        /// <returns>DTO del viático o null si no existe.</returns>
        public async Task<GastoViaticoResponseDto?> GetViaticoByIdAsync(int id)
        {
            _logger.LogInformation("Consultando viático con ID {ViaticoId}", id);

            var viatico = await _readContext.GastosViaticosNuevos
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

            var viatico = await _writeContext.GastosViaticosNuevos.FindAsync(id);
            if (viatico == null)
                throw new KeyNotFoundException($"Viático con ID {id} no encontrado.");

            // Validación adicional: no permitir cambiar de PAGADO a otro estado
            if (viatico.EstadoGasto == EstadoGasto.PAGADO && estado != EstadoGasto.PAGADO)
                throw new ArgumentException("No se puede cambiar el estado de un viático pagado.", nameof(estado));

            viatico.EstadoGasto = estado;
            await _writeContext.SaveChangesAsync();

            _logger.LogInformation("Estado de viático {ViaticoId} actualizado exitosamente", id);
        }
    }
}