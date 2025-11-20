// =====================================================================================
// INTERFACES COTIZACION LEGACY - cotizacion-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript para consumir las APIs Legacy de Cotizaciones
// desde el sistema Adminpaq (adCABS2016).
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
  descuentoDoc1?: number | null; // Descuento % a nivel documento 1 (0-100)
  descuentoDoc2?: number | null; // Descuento % a nivel documento 2 (0-100)
  descuentoDoc3?: number | null; // Descuento en valores absolutos ($) a nivel documento 3

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
  // === IDENTIFICACIÓN ===
  idDocumento: number;
  idConceptoDocumento: number;
  idDocumentoDe: number;
  serie: string;
  folio: number;
  fecha: string; // ISO date

  // === CLIENTE ===
  idCliente: number;
  razonSocial: string;
  rfc: string;

  // === FECHAS ===
  fechaVencimiento: string | null; // ISO date
  fechaProntoPago: string | null; // ISO date
  fechaEntregaRecepcion: string | null; // ISO date

  // === MONTOS ===
  subtotal: number; // CSUBTOTAL
  impuesto1: number; // CIMPUESTO1 (IVA)
  impuesto2: number; // CIMPUESTO2
  impuesto3: number; // CIMPUESTO3
  retencion1: number; // CRETENCION1
  retencion2: number; // CRETENCION2
  descuento1: number; // CDESCUENTO1
  descuento2: number; // CDESCUENTO2
  neto: number; // CNETO (calculado)
  total: number; // CTOTAL (calculado)
  pendiente: number; // CPENDIENTE (calculado)

  // === INFORMACIÓN ADICIONAL ===
  observaciones: string | null;
  referencia: string | null;
  idAgente: number | null;
  idMoneda: number;
  tipoCambio: number;

  // === ESTADOS ===
  afectado: number; // 0 = No, 1 = Sí
  impreso: number; // 0 = No, 1 = Sí
  cancelado: number; // 0 = No, 1 = Sí
  devuelto: number; // 0 = No, 1 = Sí

  // === MOVIMIENTOS (PRODUCTOS) ===
  movimientos?: CotizacionMovimientoLegacyResponse[]; // Solo si incluirMovimientos = true
}

/**
 * Response de movimiento (producto) de cotización
 */
export interface CotizacionMovimientoLegacyResponse {
  idMovimiento: number;
  idProducto: number;
  codigoProducto: string;
  nombreProducto: string;
  idAlmacen: number;
  nombreAlmacen: string;
  unidades: number;
  precio: number;
  porcentajeDescuento: number;
  descuento: number; // Monto de descuento calculado
  neto: number; // Total del movimiento (calculado)
  observaciones: string | null;
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
