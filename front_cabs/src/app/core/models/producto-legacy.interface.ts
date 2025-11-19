// =====================================================================================
// INTERFACES PRODUCTO LEGACY - producto-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript para consumir las APIs Legacy de Productos
// desde el sistema Adminpaq (adCABS2016).
//
// ENDPOINTS DEL BACKEND:
// - GET /api/AdmProductos/buscar?texto={search} - Búsqueda simplificada
// - GET /api/AdmProductos/search?... - Búsqueda paginada con filtros
// - GET /api/AdmProductos/{id} - Obtener producto por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens)
// - Interceptor automático añade credenciales
//
// =====================================================================================

/**
 * Respuesta simplificada para búsqueda rápida de productos
 * Endpoint: GET /api/AdmProductos/buscar
 */
export interface ProductoLegacyBusqueda {
  idProducto: number;
  codigoProducto: string;
  nombreProducto: string;
  descripcion: string | null;
  precio: number; // Precio lista 1
  idUnidadBase: number;
}

/**
 * Respuesta completa de producto Legacy
 * Endpoint: GET /api/AdmProductos/{id}
 */
export interface ProductoLegacyResponse {
  // === IDENTIFICACIÓN ===
  idProducto: number;
  codigoProducto: string;
  nombreProducto: string;
  codigoAlternativo: string;
  nombreAlternativo: string;
  descripcionCorta: string;
  descripcion: string | null;

  // === INFORMACIÓN PRINCIPAL ===
  tipoProducto: number; // 1 = Producto, 2 = Servicio, 3 = Paquete
  tipoProductoDescripcion: string;
  fechaAlta: string; // ISO date
  status: number; // 0 = Inactivo, 1 = Activo
  estaActivo: boolean;
  controlExistencia: number; // 0 = No, 1 = Sí
  peso: number;

  // === PRECIOS (10 niveles) ===
  precio1: number;
  precio2: number;
  precio3: number;
  precio4: number;
  precio5: number;
  precio6: number;
  precio7: number;
  precio8: number;
  precio9: number;
  precio10: number;

  // === COSTOS ===
  costoEstandar: number;
  metodoCosteo: number; // 1 = Promedio, 2 = PEPS, 3 = UEPS, 4 = Identificado
  margenUtilidad: number;

  // === IMPUESTOS Y RETENCIONES ===
  impuesto1: number; // IVA (%)
  impuesto2: number; // IEPS (%)
  impuesto3: number;
  retencionIsr: number;
  retencionIva: number;

  // === UNIDADES DE MEDIDA (FKs) ===
  idUnidadBase: number;
  idUnidadNoConvertible: number;
  idUnidadCompra: number;
  idUnidadVenta: number;

  // === IDs DE RELACIONES ===
  idMoneda: number;
  idFamilia: number;
  idLinea: number;

  // === CLASIFICACIONES ===
  clasificacion1: number;
  clasificacion2: number;
  clasificacion3: number;
  clasificacion4: number;
  clasificacion5: number;
  clasificacion6: number;

  // === CLAVE SAT ===
  claveSat: string;
  existenciaNegativa: number; // 0 = No, 1 = Sí

  // === DIMENSIONES ===
  alto: number;
  largo: number;
  ancho: number;

  // === AUDITORÍA ===
  timestamp: string;
}

/**
 * Filtros para búsqueda paginada de productos
 * Endpoint: GET /api/AdmProductos/search
 */
export interface ProductoLegacyFiltros {
  nombreProducto?: string; // Búsqueda parcial
  codigoProducto?: string; // Búsqueda parcial
  tipoProducto?: number; // 1 = Producto, 2 = Servicio, 3 = Paquete
  soloActivos?: boolean; // true = solo productos activos
  conExistencias?: boolean; // true = solo con stock disponible
  idFamilia?: number; // Filtrar por familia de productos
  idLinea?: number; // Filtrar por línea de productos
  precioMinimo?: number; // Filtro de precio mínimo
  precioMaximo?: number; // Filtro de precio máximo
  page?: number; // Número de página (default: 1)
  pageSize?: number; // Registros por página (default: 50, max: 100)
}

/**
 * Respuesta paginada de búsqueda de productos
 */
export interface ProductoLegacyPaginado {
  data: ProductoLegacyResponse[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  filters: ProductoLegacyFiltros;
}

/**
 * Interface para existencias de producto por almacén
 * Usado en consultas de disponibilidad
 */
export interface ProductoExistencia {
  idProducto: number;
  idAlmacen: number;
  nombreAlmacen: string;
  existencia: number;
  disponible: number;
  apartado: number;
  pedido: number;
}

/**
 * Respuesta genérica de API con estado
 */
export interface ProductoLegacyApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  executionTime?: string;
  errors?: string[];
}
