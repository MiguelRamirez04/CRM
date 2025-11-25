// =====================================================================================
// INTERFACES AGENTE LEGACY - agente-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACEN ESTOS ARCHIVOS?
// Interfaces TypeScript para comunicación con APIs de agentes legacy.
// Mapean DTOs del backend .NET para tipado fuerte en Angular.
//
// INTERFACES:
// - AgenteLegacyResponse: DTO de respuesta para agentes
// - AgenteLegacyPaginatedResponse: Respuesta paginada
//
// =====================================================================================

/**
 * DTO de respuesta para agentes legacy de admAgentes (adCABS2016)
 * Contiene información completa de agentes del sistema Adminpaq
 */
export interface AgenteLegacyResponse {
  // ═══════════════════════════════════════════════════════════════
  // IDENTIFICACIÓN
  // ═══════════════════════════════════════════════════════════════

  /** ID único del agente en el sistema legacy */
  idAgente: number;

  /** Código del agente (ej: "VEN001", "AGE-01") */
  codigoAgente: string;

  /** Nombre completo del agente */
  nombreAgente: string;

  // ═══════════════════════════════════════════════════════════════
  // INFORMACIÓN PRINCIPAL
  // ═══════════════════════════════════════════════════════════════

  /** Fecha de alta en el sistema */
  fechaAlta: string;

  /** Tipo de agente (1 = Venta, 2 = Cobro, etc.) */
  tipoAgente: number;

  /** Estatus del agente (0 = Inactivo, 1 = Activo) */
  estatus: number;

  // ═══════════════════════════════════════════════════════════════
  // COMISIONES
  // ═══════════════════════════════════════════════════════════════

  /** Comisión por venta (%) */
  comisionVenta: number;

  /** Comisión por cobro (%) */
  comisionCobro: number;

  /** Comisión por venta en efectivo (%) */
  comisionVentaEfectivo: number;

  /** Comisión por cobro en efectivo (%) */
  comisionCobroEfectivo: number;

  // ═══════════════════════════════════════════════════════════════
  // CLASIFICACIONES
  // ═══════════════════════════════════════════════════════════════

  /** ID de clasificación de cliente 1 */
  idValorClasifCliente1: number;

  /** ID de clasificación de cliente 2 */
  idValorClasifCliente2: number;

  /** ID de clasificación de cliente 3 */
  idValorClasifCliente3: number;

  /** ID de clasificación de cliente 4 */
  idValorClasifCliente4: number;

  /** ID de clasificación de cliente 5 */
  idValorClasifCliente5: number;

  /** ID de clasificación de cliente 6 */
  idValorClasifCliente6: number;

  // ═══════════════════════════════════════════════════════════════
  // METADATOS
  // ═══════════════════════════════════════════════════════════════

  /** Timestamp de última modificación */
  timestamp: string;
}

/**
 * Respuesta paginada para agentes legacy
 */
export interface AgenteLegacyPaginatedResponse {
  /** Lista de agentes en la página actual */
  data: AgenteLegacyResponse[];

  /** Información de paginación */
  pagination: {
    /** Página actual (1-based) */
    currentPage: number;

    /** Total de páginas */
    totalPages: number;

    /** Tamaño de página */
    pageSize: number;

    /** Total de registros */
    totalCount: number;

    /** Si hay página anterior */
    hasPrevious: boolean;

    /** Si hay página siguiente */
    hasNext: boolean;
  };

  /** Información adicional de la respuesta */
  meta: {
    /** Timestamp de la respuesta */
    timestamp: string;

    /** Duración de la consulta en ms */
    durationMs: number;

    /** Fuente de datos (cache o db) */
    source: string;
  };
}