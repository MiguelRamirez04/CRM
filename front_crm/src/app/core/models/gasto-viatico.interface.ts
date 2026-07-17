// =====================================================================================
// INTERFACES GASTO VIATICO - gasto-viatico.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript que corresponden a los DTOs del backend
// para el módulo de Gastos de Viáticos.
// Incluye interfaces para request, response, y modelos relacionados.
//
// CUÁNDO USARLO:
// - En servicios para tipado de respuestas HTTP
// - En componentes para tipado de datos
// - En formularios para validación y binding
//
// NOTA IMPORTANTE:
// - Gastos es un string que puede contener descripción de conceptos
// - MontoTotal es decimal (almacena el total del gasto)
// - OrdenId es opcional (puede crearse viático sin orden asociada)
//
// =====================================================================================

/**
 * Interface para crear un nuevo gasto de viático (POST)
 * Corresponde a GastoViaticoCreateRequestDto del backend
 */
export interface GastoViaticoCreateRequest {
  ordenId?: number | null; // OPCIONAL - puede asociarse a una orden de trabajo
  tieneFactura: boolean; // Indica si el gasto tiene factura fiscal
  descripcion?: string | null; // Descripción del gasto
  proveedorNombre?: string | null; // Nombre del proveedor (si tiene factura)
  fecha: string | Date; // Fecha del gasto (ISO string o Date)
  kmRecorridos?: number | null; // Kilómetros recorridos (si aplica)
  gastos: string; // Descripción de conceptos (ej: "HOSPEDAJE, COMBUSTIBLE, ALIMENTOS")
  montoTotal: number; // Monto total del gasto
  lugarDestino?: string | null; // Lugar de destino del viaje
}

/**
 * Interface para actualizar un gasto de viático existente (PUT)
 * Corresponde a GastoViaticoUpdateRequestDto del backend
 * Misma estructura que Create (sin incluir ID)
 */
export interface GastoViaticoUpdateRequest {
  ordenId?: number | null;
  tieneFactura: boolean;
  descripcion?: string | null;
  proveedorNombre?: string | null;
  fecha: string | Date;
  kmRecorridos?: number | null;
  gastos: string;
  montoTotal: number;
  lugarDestino?: string | null;
}

/**
 * Interface para respuesta del backend (GET)
 * Corresponde a GastoViaticoResponseDto del backend
 * Incluye el ID generado y todos los campos
 */
export interface GastoViaticoResponse {
  id: number; // ID generado por la BD
  ordenId?: number | null;
  tieneFactura: boolean;
  descripcion?: string | null;
  proveedorNombre?: string | null;
  fecha: string; // ISO date string del backend
  kmRecorridos?: number | null;
  gastos: string;
  montoTotal: number;
  lugarDestino?: string | null;
}

/**
 * Interface simplificada para listados/tablas
 * Incluye solo campos esenciales para visualización rápida
 */
export interface GastoViaticoListItem {
  id: number;
  fecha: string;
  descripcion?: string | null;
  lugarDestino?: string | null;
  montoTotal: number;
  tieneFactura: boolean;
  ordenId?: number | null;
}

/**
 * Interface para respuesta paginada del backend
 * Corresponde a PaginatedResponseDto<GastoViaticoResponseDto>
 */
export interface GastoViaticoPaginatedResponse {
  items: GastoViaticoResponse[]; // Lista de viáticos
  pagina: number; // Página actual (1-indexed)
  resultadosPorPagina: number; // Cantidad de items por página
  totalItems?: number | null; // Total de items en la BD
  totalPaginas?: number | null; // Total de páginas calculado
}

/**
 * Interface para filtros de búsqueda/paginación
 */
export interface GastoViaticoFiltros {
  ordenId?: number | null; // Filtrar por orden de trabajo específica
  fechaDesde?: string | Date | null; // Fecha inicial del rango
  fechaHasta?: string | Date | null; // Fecha final del rango
  pageNumber?: number; // Número de página (por defecto 1)
  pageSize?: number; // Cantidad de resultados (por defecto 10)
}

/**
 * Interface para estadísticas de gastos (opcional - para futuros reportes)
 */
export interface GastoViaticoEstadisticas {
  totalGastos: number; // Suma total de todos los gastos
  cantidadRegistros: number; // Cantidad de viáticos registrados
  promedioGasto: number; // Promedio de gasto por viático
  gastosConFactura: number; // Cantidad de gastos con factura
  gastosSinFactura: number; // Cantidad de gastos sin factura
  totalKmRecorridos: number; // Total de kilómetros sumados
}
