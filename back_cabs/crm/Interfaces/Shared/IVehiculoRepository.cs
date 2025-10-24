using back_cabs.CRM.models.Shared;

namespace back_cabs.CRM.Interfaces.Shared
{
    /// <summary>
    /// Contrato para operaciones de acceso a datos de Vehículos.
    /// Define qué operaciones están disponibles sin especificar cómo se implementan.
    /// </summary>
    public interface IVehiculoRepository
    {
        // 📖 OPERACIONES DE LECTURA
        /// <summary>
        /// Obtiene todos los vehículos ordenados por placas
        /// </summary>
        Task<IEnumerable<Vehiculo>> GetAllAsync();

        /// <summary>
        /// Obtiene un vehículo por su ID
        /// </summary>
        /// <param name="id">ID del vehículo</param>
        Task<Vehiculo?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene vehículos por tipo
        /// </summary>
        Task<IEnumerable<Vehiculo>> GetByTipoAsync(string tipoVehiculo);

        /// <summary>
        /// Obtiene solo vehículos activos
        /// </summary>
        Task<IEnumerable<Vehiculo>> GetActivosAsync();

        /// <summary>
        /// Busca vehículo por placas exactas
        /// </summary>
        Task<Vehiculo?> GetByPlacasAsync(string placas);

        /// <summary>
        /// Verifica si existe un vehículo con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica si las placas ya están registradas
        /// </summary>
        Task<bool> PlacasExistAsync(string placas);

        // OPERACIONES DE ESCRITURA
        /// <summary>
        /// Crea un nuevo vehículo
        /// </summary>
        Task<Vehiculo> CreateAsync(Vehiculo vehiculo);

        /// <summary>
        /// Actualiza un vehículo existente
        /// </summary>
        Task<Vehiculo> UpdateAsync(Vehiculo vehiculo);

        /// <summary>
        /// Elimina un vehículo por ID
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}