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
  nombreVehiculo: string;
  kilometraje: number | null;
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
  nombreVehiculo: string;
  kilometraje: number | null;
}

/**
 * DTO para ACTUALIZAR un vehículo.
 * Solo permite modificar: kilometraje, placas y activo
 */
export interface VehiculoUpdateDto {
  kilometraje?: number | null;
  placas?: string | null;
  activo?: boolean;
}

/**
 * DTO para el historial de cambios del vehículo
 */
export interface VehiculoHistorial {
  id: number;
  vehiculoId: number;
  campoModificado: string;
  valorAnterior: string | null;
  valorNuevo: string | null;
  usuarioId: number;
  usuarioNombre: string;
  fechaCambio: Date;
  tipoCambio: string;
}