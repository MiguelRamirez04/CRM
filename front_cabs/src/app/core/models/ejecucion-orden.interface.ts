/**
 * Enums para EjecucionOrden
 */
export enum TipoEjecucion {
  REMOTO = 'REMOTO',
  CAMPO = 'CAMPO'
}

/**
 * DTO para crear una nueva ejecución de orden
 */
export interface EjecucionOrdenCreateDto {
  ordenId: number;
  tecnicoId: number;
  tipoEjecucion: TipoEjecucion;
  // Campos para ejecución CAMPO
  vehiculoId?: number;
  kmInicial?: number;
  // Campos para ejecución REMOTO
  herramientas?: string;
  codigoSesion?: string;
  contrasenaSesion?: string;
  // Comunes
  hrInicio: Date | string;
  comentarios?: string;
}

/**
 * DTO para actualizar una ejecución existente
 */
export interface EjecucionOrdenUpdateDto {
  hrFin?: Date | string;
  kmFinal?: number;
  comentarios?: string;
}

/**
 * DTO para delegar una ejecución a otro técnico
 */
export interface DelegateEjecucionDto {
  nuevoTecnicoId: number;
  motivo: string;
}

/**
 * Response DTO de una ejecución de orden
 */
export interface EjecucionOrdenResponse {
  id: number;
  ordenId: number;
  tecnicoId: number;
  tecnicoNombre: string;
  tipoEjecucion: TipoEjecucion;
  // Datos de ejecución CAMPO
  vehiculoId?: number;
  vehiculoPlacas?: string;
  kmInicial?: number;
  kmFinal?: number;
  // Datos de ejecución REMOTO
  herramientas?: string;
  codigoSesion?: string;
  contrasenaSesion?: string;
  // Tiempos
  hrInicio: string;
  hrFin?: string;
  duracionMinutos?: number;
  // Otros
  comentarios?: string;
  historialDelegaciones?: DelegacionHistorial[];
  fechaCreacion: string;
  fechaActualizacion?: string;
}

/**
 * Historial de delegaciones de una ejecución
 */
export interface DelegacionHistorial {
  id: number;
  fechaDelegacion: string;
  tecnicoAnteriorId: number;
  tecnicoAnteriorNombre: string;
  tecnicoNuevoId: number;
  tecnicoNuevoNombre: string;
  motivo: string;
}

/**
 * Filtros para listar ejecuciones
 */
export interface EjecucionOrdenFilters {
  ordenId?: number;
  tecnicoId?: number;
  tipoEjecucion?: TipoEjecucion;
  estado?: 'EN_CURSO' | 'FINALIZADA';
  fechaDesde?: Date | string;
  fechaHasta?: Date | string;
}

/**
 * Interfaces para catálogos (datos legibles para el usuario)
 */
export interface OrdenTrabajoSimple {
  id: number;
  clienteNombre: string;
  vehiculoPlacas?: string;
  descripcion?: string;
  estado: string;
}

export interface TecnicoSimple {
  id: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  correo?: string;
}

export interface VehiculoSimple {
  id: number;
  placas: string;
  marca?: string;
  modelo?: string;
  anio?: number;
  descripcion: string; // ej: "ABC123 - Toyota Corolla 2020"
}
