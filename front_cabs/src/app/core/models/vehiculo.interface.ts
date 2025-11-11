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
  kilometraje: number;
  creadoEn?: string;
  creadoPorUsuarioId?: number;
  actualizadoEn?: string;
  actualizadoPorUsuarioId?: number;
  historialCambios?: string;
}

/**
 * DTO (Data Transfer Object) para CREAR un vehículo.
 * ¡Esto AHORA SÍ coincide con tu API!
 */
export interface VehiculoCreateDto {
  tipoVehiculo: string;
  transmision: string;
  esDeEmpresa: boolean;
  placas: string;
  activo: boolean;
  observaciones: string;
  nombreVehiculo: string;
  kilometraje: number;
}

/**
 * DTO para ACTUALIZAR un vehículo.
 * Solo permite modificar: kilometraje (obligatorio), placas (opcional), observaciones (opcional) y activo (opcional)
 */
export interface VehiculoUpdateDto {
  kilometraje: number;
  placas?: string;
  observaciones?: string;
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