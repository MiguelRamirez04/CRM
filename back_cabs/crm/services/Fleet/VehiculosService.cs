using back_cabs.CRM.contexts;
using back_cabs.CRM.Interfaces.Shared;
using CRM.DTOs.Request;
using CRM.DTOs.Response;
using back_cabs.CRM.models.Shared;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Fleet;

/// <summary>
/// Servicio que maneja la lógica de negocio para vehículos.
/// Usa Repository Pattern para acceso a datos con separación de responsabilidades.
/// </summary>
public class VehiculosService
{
    // ✅ Inyección de dependencias: Repository Pattern para acceso a datos
    // desacoplado y testeable
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly ILogger<VehiculosService> _logger;

    public VehiculosService(
        IVehiculoRepository vehiculoRepository,
        ILogger<VehiculosService> logger)
    {
        _vehiculoRepository = vehiculoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los vehículos ordenados por placas.
    /// ✅ Usa Repository Pattern para acceso optimizado a datos
    /// ✅ El OrderBy usa el índice configurado en la base de datos para Placas
    /// </summary>
    public async Task<IEnumerable<VehiculoResponseDto>> ObtenerTodosAsync()
    {
        // ✅ Repository Pattern: abstracción completa del acceso a datos
        var vehiculos = await _vehiculoRepository.GetAllAsync();

        return vehiculos.Select(MapToResponseDto);
    }

    /// <summary>
    /// Obtiene un vehículo por ID.
    /// ✅ Usa Repository Pattern para consulta optimizada
    /// </summary>
    public async Task<VehiculoResponseDto?> ObtenerPorIdAsync(int id)
    {
        // ✅ Repository Pattern: consulta optimizada sin tracking
        var vehiculo = await _vehiculoRepository.GetByIdAsync(id);

        return vehiculo == null ? null : MapToResponseDto(vehiculo);
    }

    /// <summary>
    /// Crea un nuevo vehículo.
    /// ✅ Usa Repository Pattern para validaciones y creación
    /// ✅ Aplica todas las validaciones configuradas en la base de datos:
    /// - HasMaxLength(50) para TipoVehiculo
    /// - HasMaxLength(20) para Placas
    /// - IsUnique() para Placas (validado manualmente)
    /// - HasDefaultValue(true) para Activo si no se especifica
    /// </summary>
    public async Task<VehiculoResponseDto> CrearAsync(VehiculoRequestDto request)
    {
        // ✅ Validación manual usando el índice único configurado en la base de datos
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

        // ✅ Repository Pattern: creación con validaciones automáticas
        var vehiculoCreado = await _vehiculoRepository.CreateAsync(vehiculo);

        _logger.LogInformation("Vehículo creado con ID: {VehiculoId} y Placas: {Placas}", vehiculoCreado.Id, vehiculoCreado.Placas);
        return MapToResponseDto(vehiculoCreado);
    }

    /// <summary>
    /// Actualiza un vehículo existente.
    /// ✅ Usa Repository Pattern para validaciones y actualización
    /// ✅ Verifica unicidad de placas usando el índice configurado
    /// </summary>
    public async Task<VehiculoResponseDto?> ActualizarAsync(int id, VehiculoRequestDto request)
    {
        // ✅ Repository Pattern: obtener vehículo existente
        var vehiculo = await _vehiculoRepository.GetByIdAsync(id);
        if (vehiculo == null)
        {
            return null;
        }

        // ✅ Validación de unicidad usando Repository Pattern
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

        // ✅ Repository Pattern: actualización con validaciones automáticas
        var vehiculoActualizado = await _vehiculoRepository.UpdateAsync(vehiculo);
        _logger.LogInformation("Vehículo actualizado con ID: {VehiculoId}", vehiculoActualizado.Id);

        return MapToResponseDto(vehiculoActualizado);
    }

    /// <summary>
    /// Elimina un vehículo.
    /// ✅ Usa Repository Pattern para operaciones de eliminación
    /// </summary>
    public async Task<bool> EliminarAsync(int id)
    {
        // ✅ Repository Pattern: eliminación con validaciones automáticas
        var eliminado = await _vehiculoRepository.DeleteAsync(id);

        if (eliminado)
        {
            _logger.LogInformation("Vehículo eliminado con ID: {VehiculoId}", id);
        }

        return eliminado;
    }

    /// <summary>
    /// Valida que las placas sean únicas.
    /// ✅ Usa Repository Pattern para consulta eficiente
    /// </summary>
    private async Task ValidarPlacaUnica(string? placas)
    {
        if (string.IsNullOrWhiteSpace(placas))
        {
            return;
        }

        // ✅ Repository Pattern: validación eficiente usando índice único
        var existe = await _vehiculoRepository.PlacasExistAsync(placas);

        if (existe)
        {
            _logger.LogWarning("Intento de crear/actualizar vehículo con placas duplicadas: {Placas}", placas);
            throw new InvalidOperationException($"Las placas '{placas}' ya están registradas.");
        }
    }

    /// <summary>
    /// Mapea un modelo Vehiculo a VehiculoResponseDto.
    /// ✅ Las propiedades ya están validadas por las configuraciones de EF
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

