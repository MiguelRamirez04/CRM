// =====================================================================================
// INTERFACES ORDEN TRABAJO - orden-trabajo.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript que corresponden a los DTOs del backend
// para el módulo de Recepción (OrdenTrabajo).
// Incluye interfaces para request, response, y modelos relacionados.
//
// CUÁNDO USARLO:
// - En servicios para tipado de respuestas HTTP
// - En componentes para tipado de datos
// - En formularios para validación
//
// =====================================================================================

import { EstadoOrden, TipoOrden, Modalidad, EstadoFacturado } from '../enums/estado-orden.enum';

export interface OrdenTrabajo {
  id: number;
  notas?: string;
  citaProgramadaInicio: string; // ISO date string
  citaProgramadaFin?: string;
  modalidad: Modalidad;
  tipoOrden: TipoOrden;
  cotizaciones?: string;
  nuevoCliente: boolean;
  nombreCliente: string;
  clienteId?: number;
  prioridad: number;
  estado: EstadoOrden;
  ubicacionText?: string;
  estadoFacturado?: EstadoFacturado;
  requiereFactura: boolean;
  facturaFolio?: number;
  costoReal?: number;
  costoEstimado?: number;
  creadoPorUserId: number;
  actualizadoEn?: string;
  creadoEn: string;
  asignadaAUserId?: number;
}

export interface OrdenTrabajoRequest {
  requestDto: {
    notas?: string;
    citaProgramadaInicio: string;
    citaProgramadaFin?: string;
    modalidad: Modalidad;
    tipoOrden: TipoOrden;
    cotizaciones?: string;
    nuevoCliente: boolean;
    nombreCliente?: string;
    clienteId?: number;
    prioridad: number;
    estado: EstadoOrden;
    ubicacionText?: string;
    estadoFacturado?: EstadoFacturado;
    requiereFactura: boolean;
    facturaFolio?: number;
    costoReal?: number;
    costoEstimado?: number;
    creadoPorUserId: number;
  };
}

export interface OrdenTrabajoUpdateRequest {
  requestDto: Partial<OrdenTrabajoRequest['requestDto']>;
}

export interface ClienteLegacy {
  id: number;
  nombre: string;
  rfc?: string;
  // Agregar otros campos según la API /api/ClientesCompletos
}

export interface EstadisticaRecepcion {
  ordenesHoy: number;
  pendientes: number;
  completadas: number;
  totalMes: number;
  // Agregar más métricas según backend
}

export interface ClienteBusquedaRequest {
  busqueda: string;
}

export interface ClienteResumenDto {
  id: number;
  nombre: string;
  rfc?: string;
  // Campos resumidos para búsqueda
}

export { EstadoOrden, TipoOrden, Modalidad };
