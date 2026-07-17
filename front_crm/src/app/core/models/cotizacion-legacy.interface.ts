// =====================================================================================
// INTERFACES COTIZACION LEGACY - cotizacion-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript para consumir las APIs Legacy de Cotizaciones
// desde el sistema Adminpaq (legacy_adm).
//
// ENDPOINTS DEL BACKEND:
// - POST /api/AdmDocumentos/cotizacion - Crear nueva cotizacion
// - PUT /api/AdmDocumentos/cotizacion/cancelar - Cancelar cotizacion
// - GET /api/AdmDocumentos/search - Buscar cotizaciones con filtros
// - GET /api/AdmDocumentos/{id} - Obtener cotizacion por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens)
// - Interceptor automático añade credenciales y CSRF token
//
// =====================================================================================

/**
 * DTO para movimientos (productos) de la cotización
 * Representa un producto cotizado con cantidad, precio y descuentos
 */
export interface CotizacionMovimientoLegacyDto {
  idProducto: number; // FK a admProductos (obligatorio)
  idAlmacen: number; // FK a admAlmacenes (obligatorio)
  idUnidad?: number | null; // FK a admUnidadesMedidaPeso (opcional, usa unidad por defecto del producto)
  unidades: number; // Cantidad de unidades (obligatorio, puede ser 0)
  precio: number; // Precio unitario (obligatorio, puede modificarse del producto)
  porcentajeDescuento?: number | null; // Descuento % a nivel movimiento (0-100)
  descuentoImporte?: number | null; // Descuento en valores absolutos ($) a nivel movimiento
  observaciones?: string | null; // Observaciones del movimiento (max 200 caracteres)
}

/**
 * Request para crear una nueva cotización
 * POST /api/AdmDocumentos/cotizacion
 */
export interface CotizacionLegacyCreateRequest {
  // === DATOS PRINCIPALES ===
  idCliente: number; // FK a admClientes (obligatorio)
  idAgente?: number | null; // FK a admAgentes (opcional)
  
  // === FECHAS ===
  fechaVencimiento?: string | null; // ISO date (opcional, default: +30 días)
  fechaProntoPago?: string | null; // ISO date (opcional, default: fechaVencimiento)
  fechaEntregaRecepcion?: string | null; // ISO date (opcional, default: hoy)

  // === PRODUCTOS ===
  productos: CotizacionMovimientoLegacyDto[]; // Lista de productos (mínimo 1)

  // === DESCUENTOS ===
  descuentoDoc1?: number | null; // Descuento ($) a nivel documento 1
  descuentoDoc2?: number | null; // Descuento ($) a nivel documento 2
  descuentoDoc3?: number | null; // Descuento ($) a nivel documento 3

  // === TOTAL ===
  cTotal: number; // CTOTAL - Monto total final (OBLIGATORIO, ingresado manualmente)

  // === PAGOS ===
  montoPagado?: number | null; // Monto abonado o pagado (default: 0)

  // === INFORMACIÓN ADICIONAL ===
  observaciones?: string | null; // Observaciones generales (max 254 caracteres)
  referencia?: string | null; // Referencia del documento (max 20 caracteres)

  // === IMPUESTOS ===
  aplicarIVA?: boolean; // true = aplicar IVA (default: true)
  porcentajeIVA?: number; // Porcentaje de IVA (default: 16.0, rango 0-100)
}

/**
 * Response al crear una cotización exitosamente
 * POST /api/AdmDocumentos/cotizacion
 */
export interface CotizacionLegacyCreateResponse {
  // === IDENTIFICACIÓN ===
  idDocumento: number; // ID del documento creado
  serie: string; // Serie del documento (ej: "CA")
  folio: number; // Folio asignado automáticamente

  // === FECHA Y CLIENTE ===
  fecha: string; // ISO date - Fecha del documento
  razonSocial: string; // Razón social del cliente

  // === MONTOS ===
  neto: number; // Subtotal (suma de productos - descuentos)
  impuesto: number; // IVA aplicado
  total: number; // Total de la cotización (neto + impuesto)
  pendiente: number; // Saldo pendiente (total - montoPagado)

  // === INFORMACIÓN ADICIONAL ===
  cantidadProductos: number; // Cantidad de productos cotizados
  mensaje: string; // Mensaje informativo
}

/**
 * Request para cancelar una cotización
 * PUT /api/AdmDocumentos/cotizacion/cancelar
 */
export interface CotizacionLegacyCancelarRequest {
  idDocumento: number; // ID del documento a cancelar
  motivo: string; // Motivo de la cancelación (obligatorio)
}

/**
 * Response al cancelar una cotización
 * PUT /api/AdmDocumentos/cotizacion/cancelar
 */
export interface CotizacionLegacyCancelarResponse {
  idDocumento: number;
  folio: number;
  serie: string;
  fechaCancelacion: string; // ISO date
  motivo: string;
  mensaje: string;
}

/**
 * Filtros para búsqueda de cotizaciones
 * GET /api/AdmDocumentos/search
 */
export interface CotizacionLegacyFiltros {
  // === FECHAS ===
  fechaInicio?: string; // ISO date - Fecha inicial del documento
  fechaFin?: string; // ISO date - Fecha final del documento
  fechaVencimientoInicio?: string; // ISO date - Fecha inicial de vencimiento
  fechaVencimientoFin?: string; // ISO date - Fecha final de vencimiento

  // === BÚSQUEDA ===
  folio?: string; // Folio del documento (búsqueda exacta)
  razonSocial?: string; // Razón social del cliente (búsqueda parcial)

  // === FILTROS ESPECÍFICOS ===
  idConcepto?: number; // ID del concepto del documento
  idAgente?: number; // ID del agente de ventas

  // === PAGINACIÓN ===
  page?: number; // Número de página (default: 1)
  pageSize?: number; // Registros por página (default: 50, max: 100)

  // === OPCIONES ===
  incluirMovimientos?: boolean; // true = incluir productos cotizados (default: false)
}

/**
 * Response de cotización completa
 * GET /api/AdmDocumentos/{id}
 */
export interface CotizacionLegacyResponse {
  idDocumento: number;
  serieDocumento: string;
  folio: number;
  fecha: string;
  razonSocial: string;
  fechaVencimiento: string;
  fechaProntoPago: string;
  fechaEntregaRecepcion: string;
  
  // Totales
  subtotal: number;
  iva: number;
  total: number;
  
  // Descuentos
  descuentoDoc1: number;
  descuentoDoc2: number;
  descuentoDoc3: number;
  
  // Estado
  estado: string; // "Activa" | "Cancelada"

  // Agente
  agente?: string;

  // Movimientos
  movimientos?: CotizacionMovimientoLegacyResponse[];
}

/**
 * Response de movimiento (producto) de cotización
 */
export interface CotizacionMovimientoLegacyResponse {
  idMovimiento: number;
  numeroMovimiento: number;
  idProducto: number;
  codigoProducto: string;
  nombreProducto: string;
  descripcionProducto: string;
  idAlmacen: number;
  codigoAlmacen: string;
  nombreAlmacen: string;
  unidades: number;
  unidadesCapturadas: number;
  idUnidad: number;
  precio: number;
  precioCapturado: number;
  costoCapturado: number;
  porcentajeDescuento: number;
  descuentoLinea: number;
  impuesto1: number;
  impuesto2: number;
  impuesto3: number;
  retencion1: number;
  retencion2: number;
  neto: number;
  total: number;
  referencia: string;
  observaciones: string | null;
  afectado: number;
  venta: number;
}

/**
 * Respuesta paginada de búsqueda de cotizaciones
 * GET /api/AdmDocumentos/search
 */
export interface CotizacionLegacyPaginado {
  data: CotizacionLegacyResponse[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  filters: CotizacionLegacyFiltros;
}

/**
 * Resumen de cotizaciones por rango de fechas
 * GET /api/AdmDocumentos/resumen
 */
export interface CotizacionLegacyResumen {
  fechaInicio: string; // ISO date
  fechaFin: string; // ISO date
  totalDocumentos: number; // Total de documentos en el rango
}

/**
 * Respuesta genérica de API con estado
 */
export interface CotizacionLegacyApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  executionTime?: string;
  errors?: string[];
}
