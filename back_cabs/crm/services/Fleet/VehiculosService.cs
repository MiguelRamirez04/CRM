using back_cabs.CRM.contexts;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Fleet;

/// <summary>
/// Servicio que maneja la lógica de negocio para vehículos.
/// Usa las configuraciones mapeadas en WriteContext.cs para validaciones automáticas.
/// </summary>
public class VehiculosService
{
    // ✅ Inyección de dependencias: ReadOnlyContext para lecturas optimizadas (sin tracking)
    // WriteContext para operaciones de escritura con validaciones completas
    private readonly ReadOnlyContext _readContext;
    private readonly WriteContext _writeContext;
    private readonly ILogger<VehiculosService> _logger;

    public VehiculosService(
        ReadOnlyContext readContext,
        WriteContext writeContext,
        ILogger<VehiculosService> logger)
    {
        _readContext = readContext;
        _writeContext = writeContext;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los vehículos ordenados por placas.
    /// ✅ Usa AsNoTracking() para rendimiento (configurado en ReadOnlyContext)
    /// ✅ El OrderBy usa el índice configurado en WriteContext para Placas
    /// </summary>
    public async Task<IEnumerable<VehiculoResponseDto>> ObtenerTodosAsync()
    {
        // ✅ ReadOnlyContext optimizado para lecturas (QueryTrackingBehavior.NoTracking)
        var vehiculos = await _readContext.Vehiculos
            .AsNoTracking() // Optimización adicional (ya configurada en contexto)
            .OrderBy(v => v.Placas) // ✅ Usa índice único configurado: HasIndex(e => e.Placas).IsUnique()
            .ToListAsync();

        return vehiculos.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un vehículo por ID.
    /// ✅ Usa AsNoTracking() para evitar overhead de tracking
    /// </summary>
    public async Task<VehiculoResponseDto?> ObtenerPorIdAsync(int id)
    {
        // ✅ ReadOnlyContext sin tracking para consulta rápida
        var vehiculo = await _readContext.Vehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehiculo == null ? null : MapToResponseDto(vehiculo);
    }

    /// <summary>
    /// Crea un nuevo vehículo.
    /// ✅ Aplica todas las validaciones configuradas en WriteContext:
    /// - HasMaxLength(50) para TipoVehiculo
    /// - HasMaxLength(20) para Placas
    /// - IsUnique() para Placas (validado manualmente)
    /// - HasDefaultValue(true) para Activo si no se especifica
    /// </summary>
    public async Task<VehiculoResponseDto> CrearAsync(VehiculoRequestDto request)
    {
        // ✅ Validación manual usando el índice único configurado en contexto
        await ValidarPlacaUnica(request.Placas);

        // ✅ Creación del modelo - EF aplicará automáticamente:
        // - Mapeo TipoVehiculo → "tipo_vehiculo" VARCHAR(50)
        // - Mapeo Placas → "placas" VARCHAR(20) UNIQUE
        // - Mapeo Activo → "activo" BIT DEFAULT 1
        // - Mapeo EsDeEmpresa → "es_de_empresa" BIT DEFAULT 1
        var vehiculo = new Vehiculo
        {
            TipoVehiculo = request.TipoVehiculo,    // ✅ HasMaxLength(50) aplicado automáticamente
            Transmision = request.Transmision,      // ✅ HasMaxLength(20) aplicado automáticamente
            EsDeEmpresa = request.EsDeEmpresa,      // ✅ HasDefaultValue(true) si no se especifica
            Placas = request.Placas,                // ✅ HasMaxLength(20) + UNIQUE constraint
            Activo = request.Activo,                // ✅ HasDefaultValue(true) si no se especifica
            Observaciones = request.Observaciones   // ✅ NVARCHAR(MAX) aplicado automáticamente
        };

        // ✅ WriteContext aplica todas las configuraciones al guardar
        _writeContext.Vehiculos.Add(vehiculo);
        await _writeContext.SaveChangesAsync(); // ✅ Valida constraints y aplica defaults

        _logger.LogInformation("Vehículo creado con ID: {VehiculoId} y Placas: {Placas}", vehiculo.Id, vehiculo.Placas);
        return MapToResponseDto(vehiculo);
    }

    /// <summary>
    /// Actualiza un vehículo existente.
    /// ✅ Aplica validaciones de WriteContext durante la actualización
    /// ✅ Verifica unicidad de placas usando el índice configurado
    /// </summary>
    public async Task<VehiculoResponseDto?> ActualizarAsync(int id, VehiculoRequestDto request)
    {
        // ✅ WriteContext permite tracking para actualizaciones
        var vehiculo = await _writeContext.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return null;
        }

        // ✅ Validación de unicidad usando el índice único configurado
        if (vehiculo.Placas != request.Placas)
        {
            await ValidarPlacaUnica(request.Placas);
        }

        // ✅ Asignación de propiedades - EF valida automáticamente:
        // - Longitudes máximas (HasMaxLength)
        // - Tipos de datos (HasColumnType)
        vehiculo.TipoVehiculo = request.TipoVehiculo;    // ✅ VARCHAR(50)
        vehiculo.Transmision = request.Transmision;      // ✅ VARCHAR(20)
        vehiculo.EsDeEmpresa = request.EsDeEmpresa;      // ✅ BIT DEFAULT 1
        vehiculo.Placas = request.Placas;                // ✅ VARCHAR(20) UNIQUE
        vehiculo.Activo = request.Activo;                // ✅ BIT DEFAULT 1
        vehiculo.Observaciones = request.Observaciones;  // ✅ NVARCHAR(MAX)

        // ✅ WriteContext valida todas las constraints al guardar
        await _writeContext.SaveChangesAsync();
        _logger.LogInformation("Vehículo actualizado con ID: {VehiculoId}", vehiculo.Id);

        return MapToResponseDto(vehiculo);
    }

    /// <summary>
    /// Elimina un vehículo.
    /// ✅ Usa WriteContext para operaciones de eliminación
    /// </summary>
    public async Task<bool> EliminarAsync(int id)
    {
        // ✅ WriteContext maneja eliminación con constraints
        var vehiculo = await _writeContext.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return false;
        }

        _writeContext.Vehiculos.Remove(vehiculo);
        await _writeContext.SaveChangesAsync(); // ✅ Verifica FKs y constraints
        _logger.LogInformation("Vehículo eliminado con ID: {VehiculoId}", id);

        return true;
    }

    /// <summary>
    /// Valida que las placas sean únicas.
    /// ✅ Usa el índice único configurado en WriteContext para consulta eficiente
    /// </summary>
    private async Task ValidarPlacaUnica(string? placas)
    {
        if (string.IsNullOrWhiteSpace(placas))
        {
            return;
        }

        // ✅ ReadOnlyContext + índice único hacen esta validación eficiente
        var existente = await _readContext.Vehiculos
            .AnyAsync(v => v.Placas == placas); // ✅ Usa HasIndex(e => e.Placas).IsUnique()

        if (existente)
        {
            _logger.LogWarning("Intento de crear/actualizar vehículo con placas duplicadas: {Placas}", placas);
            throw new InvalidOperationException($"Las placas '{placas}' ya están registradas.");
        }
    }

    /// <summary>
    /// Mapea un modelo Vehiculo a VehiculoResponseDto.
    /// ✅ Las propiedades ya están validadas por las configuraciones del contexto
    /// </summary>
    private VehiculoResponseDto MapToResponseDto(Vehiculo vehiculo)
    {
        return new VehiculoResponseDto
        {
            Id = vehiculo.Id,                          // ✅ ValueGeneratedOnAdd() aplicado
            TipoVehiculo = vehiculo.TipoVehiculo,      // ✅ Mapeado correctamente
            Transmision = vehiculo.Transmision,        // ✅ Mapeado correctamente
            EsDeEmpresa = vehiculo.EsDeEmpresa,        // ✅ Mapeado correctamente
            Placas = vehiculo.Placas,                  // ✅ Mapeado correctamente
            Activo = vehiculo.Activo,                  // ✅ Mapeado correctamente
            Observaciones = vehiculo.Observaciones     // ✅ Mapeado correctamente
        };
    }
}

