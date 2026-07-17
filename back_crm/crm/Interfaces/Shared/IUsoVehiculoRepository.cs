using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.Interfaces.Shared
{
    public interface IUsoVehiculoRepository
    {
        Task<UsoVehiculo> CreateAsync(UsoVehiculo usoVehiculo);
        Task<UsoVehiculo> UpdateAsync(UsoVehiculo usoVehiculo);
        Task<UsoVehiculo?> GetUsoActivoPorVehiculoAsync(int vehiculoId);
        Task<IEnumerable<UsoVehiculo>> GetHistorialPorVehiculoAsync(int vehiculoId);
        Task<UsoVehiculo?> GetByIdAsync(int id);
    }
}
