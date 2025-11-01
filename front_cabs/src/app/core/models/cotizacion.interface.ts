// =====================================================================================
// INTERFACES COTIZACION - cotizacion.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript que corresponden a los DTOs del backend
// para el módulo de Cotizaciones.
// Incluye interfaces para request, response, y modelos relacionados.
//
// CUÁNDO USARLO:
// - En servicios para tipado de respuestas HTTP
// - En componentes para tipado de datos
// - En formularios para validación y binding
//
// NOTA IMPORTANTE:
// - NO se incluye intakeLegacyId (siempre es null en contexto actual)
// - Total se calcula en BD como PERSISTED column: [subtotal] + [impuestos_total]
// - TotalFinal se calcula en cliente: Total - Descuento
//
// =====================================================================================

import { EstadoCotizacion } from '../enums/estado-cotizacion.enum';

/**
 * Interface para crear una nueva cotización (POST)
 * Corresponde a CotizacionCreateRequestDto del backend
 */
export interface CotizacionCreateRequest {
  ordenId: number;
  subtotal: number;
  impuestosTotal: number;
  descuento?: number | null;
  estado: EstadoCotizacion;
  observaciones?: string | null;
  cliente: string;
  rfc: string;
  folio: string;
  descripcionServicio: string;
  validezDias: number;
  horasCapacitacion?: number | null;
  paquetesCapacitacion?: number | null;
  costoCapacitacion?: number | null;
  telefono?: number | null;
  correo?: string | null;
}

/**
 * Interface para actualizar una cotización existente (PUT)
 * Misma estructura que Create
 */
export interface CotizacionUpdateRequest extends CotizacionCreateRequest {}

/**
 * Interface para respuesta del backend (GET)
 * Corresponde a CotizacionResponseDto del backend
 * Incluye campos calculados y de auditoría
 */
export interface CotizacionResponse {
  id: number;
  ordenId: number;
  subtotal: number;
  impuestosTotal: number;
  total: number; // Calculado en BD: subtotal + impuestosTotal
  descuento?: number | null;
  totalFinal: number; // Calculado en BD/Cliente: total - descuento
  estado: EstadoCotizacion;
  observaciones?: string | null;
  cliente: string;
  rfc: string;
  folio: string;
  descripcionServicio: string;
  validezDias: number;
  horasCapacitacion?: number | null;
  paquetesCapacitacion?: number | null;
  costoCapacitacion?: number | null;
  telefono?: number | null;
  correo?: string | null;
  fechaCreacion: string; // ISO date string
  fechaActualizacion?: string | null; // ISO date string
}

/**
 * Interface simplificada para listados
 * Incluye solo campos esenciales para tablas/cards
 */
export interface CotizacionListItem {
  id: number;
  folio: string;
  cliente: string;
  totalFinal: number;
  estado: EstadoCotizacion;
  fechaCreacion: string;
  validezDias: number;
}

/**
 * Interface para filtros de búsqueda
 */
export interface CotizacionFiltros {
  ordenId?: number;
  estado?: EstadoCotizacion;
  cliente?: string;
}
