/**
 * Representa un vehículo como viene de la API y la BD
 * (Basado en la tabla fleet_vehiculos)
 */
export interface Vehiculo {
  id: number;
  tipoVehiculo: string | null;
  transmision: string | null;
  esDeEmpresa: boolean;
  placas: string | null;
  activo: boolean;
  observaciones: string | null;
}

/**
 * DTO (Data Transfer Object) para CREAR un vehículo.
 * ¡Esto AHORA SÍ coincide con tu API!
 */
export interface VehiculoCreateDto {
  tipoVehiculo: string | null;
  transmision: string | null;
  esDeEmpresa: boolean;
  placas: string | null;
  activo: boolean;
  observaciones: string | null;
}

/**
 * DTO para ACTUALIZAR un vehículo.
 */
export interface VehiculoUpdateDto extends Partial<VehiculoCreateDto> {}