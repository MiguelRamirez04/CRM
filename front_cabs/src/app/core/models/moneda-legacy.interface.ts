// =====================================================================================
// INTERFACES MONEDA LEGACY - moneda-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACEN ESTOS ARCHIVOS?
// Interfaces TypeScript para comunicación con APIs de monedas legacy.
// Mapean DTOs del backend .NET para tipado fuerte en Angular.
//
// INTERFACES:
// - MonedaLegacyResponse: DTO de respuesta para monedas
// - MonedaLegacyPaginatedResponse: Respuesta paginada
//
// =====================================================================================

/**
 * DTO de respuesta para monedas legacy de admMonedas (adCABS2016)
 * Contiene información completa de monedas del sistema Adminpaq
 */
export interface MonedaLegacyResponse {
  // ═══════════════════════════════════════════════════════════════
  // IDENTIFICACIÓN
  // ═══════════════════════════════════════════════════════════════

  /** ID único de la moneda en el sistema legacy */
  idMoneda: number;

  /** Nombre de la moneda (ej: "PESO MEXICANO", "DOLAR AMERICANO") */
  nombreMoneda: string;

  /** Símbolo de la moneda (ej: "$", "€", "£") */
  simboloMoneda: string;

  // ═══════════════════════════════════════════════════════════════
  // CONFIGURACIÓN DE FORMATO
  // ═══════════════════════════════════════════════════════════════

  /** Posición del símbolo (0 = antes, 1 = después) */
  posicionSimbolo: number;

  /** Número de decimales a mostrar */
  numeroDecimales: number;

  /** Separador de miles (ej: ",", ".") */
  separadorMiles: string;

  /** Separador decimal (ej: ".", ",") */
  separadorDecimales: string;

  // ═══════════════════════════════════════════════════════════════
  // METADATOS
  // ═══════════════════════════════════════════════════════════════

  /** Timestamp de última modificación */
  timestamp: string;
}

/**
 * Respuesta paginada para monedas legacy
 */
export interface MonedaLegacyPaginatedResponse {
  /** Lista de monedas en la página actual */
  data: MonedaLegacyResponse[];

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