using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Fleet;
using back_cabs.CRM.models.Fleet;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.services.Fleet;

public class VehiculosService
{
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

    public async Task<IEnumerable<VehiculoResponseDto>> ObtenerTodosAsync()
    {
        var vehiculos = await _readContext.Vehiculos
            .AsNoTracking()
            .OrderBy(v => v.Placas)
            .ToListAsync();

        return vehiculos.Select(MapToResponseDto);
    }

    public async Task<VehiculoResponseDto?> ObtenerPorIdAsync(int id)
    {
        var vehiculo = await _readContext.Vehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehiculo == null ? null : MapToResponseDto(vehiculo);
    }

    public async Task<VehiculoResponseDto> CrearAsync(VehiculoRequestDto request)
    {
        await ValidarPlacaUnica(request.Placas);

        var vehiculo = new Vehiculo
        {
            TipoVehiculo = request.TipoVehiculo,
            Transmision = request.Transmision,
            EsDeEmpresa = request.EsDeEmpresa,
            Placas = request.Placas,
            Activo = request.Activo,
            Observaciones = request.Observaciones
        };

        _writeContext.Vehiculos.Add(vehiculo);
        await _writeContext.SaveChangesAsync();

        _logger.LogInformation("Vehículo creado con ID: {VehiculoId} y Placas: {Placas}", vehiculo.Id, vehiculo.Placas);
        return MapToResponseDto(vehiculo);
    }

    public async Task<VehiculoResponseDto?> ActualizarAsync(int id, VehiculoRequestDto request)
    {
        var vehiculo = await _writeContext.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return null;
        }

        if (vehiculo.Placas != request.Placas)
        {
            await ValidarPlacaUnica(request.Placas);
        }

        vehiculo.TipoVehiculo = request.TipoVehiculo;
        vehiculo.Transmision = request.Transmision;
        vehiculo.EsDeEmpresa = request.EsDeEmpresa;
        vehiculo.Placas = request.Placas;
        vehiculo.Activo = request.Activo;
        vehiculo.Observaciones = request.Observaciones;

        await _writeContext.SaveChangesAsync();
        _logger.LogInformation("Vehículo actualizado con ID: {VehiculoId}", vehiculo.Id);

        return MapToResponseDto(vehiculo);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var vehiculo = await _writeContext.Vehiculos.FindAsync(id);
        if (vehiculo == null)
        {
            return false;
        }

        _writeContext.Vehiculos.Remove(vehiculo);
        await _writeContext.SaveChangesAsync();
        _logger.LogInformation("Vehículo eliminado con ID: {VehiculoId}", id);

        return true;
    }

    private async Task ValidarPlacaUnica(string? placas)
    {
        if (string.IsNullOrWhiteSpace(placas))
        {
            return;
        }

        var existente = await _readContext.Vehiculos
            .AnyAsync(v => v.Placas == placas);
            
        if (existente)
        {
            _logger.LogWarning("Intento de crear/actualizar vehículo con placas duplicadas: {Placas}", placas);
            throw new InvalidOperationException($"Las placas '{placas}' ya están registradas.");
        }
    }

    private VehiculoResponseDto MapToResponseDto(Vehiculo vehiculo)
    {
        return new VehiculoResponseDto
        {
            Id = vehiculo.Id,
            TipoVehiculo = vehiculo.TipoVehiculo,
            Transmision = vehiculo.Transmision,
            EsDeEmpresa = vehiculo.EsDeEmpresa,
            Placas = vehiculo.Placas,
            Activo = vehiculo.Activo,
            Observaciones = vehiculo.Observaciones
        };
    }
}
