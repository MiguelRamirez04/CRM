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
  disponible: boolean;
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
  transmisionManual?: boolean; // Helper opcional para UI
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
 * DTO para el historial de cambios (auditoría)
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

/**
 * DTO para registrar la SALIDA de un vehículo
 */
export interface RegistrarSalidaDto {
  usuarioId: number;
  motivoUso: string;
  kilometrajeInicial: number;
  fechaSalida?: string; // ISO string form
}

/**
 * DTO para registrar la ENTRADA de un vehículo
 */
export interface RegistrarEntradaDto {
  kilometrajeFinal: number;
  observaciones?: string;
  fechaRegreso?: string; // ISO string form
  estado?: string; // 'COMPLETADO', etc.
}

/**
 * Modelo para el historial de USO (Viajes)
 */
export interface UsoVehiculo {
  id: number;
  vehiculoId: number;
  usuarioId: number;
  usuarioNombre?: string; // Propiedad auxiliar para mostrar nombre si se hace join en front o back
  fechaInicio: string; // DateTime
  fechaFin?: string; // DateTime
  horaSalida: string; // TimeSpan string "HH:mm:ss"
  horaRegreso?: string; // TimeSpan string
  motivoUso: string;
  kilometrajeInicial: number;
  kilometrajeFinal?: number;
  estado: string; // 'EN_USO', 'COMPLETADO', etc.
  observaciones?: string;
}