// =====================================================================================
// INTERFACES CLIENTE LEGACY - cliente-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript para consumir las APIs Legacy de Clientes
// desde el sistema Adminpaq (adCABS2016).
//
// ENDPOINTS DEL BACKEND:
// - GET /api/AdmClientes/buscar?texto={search} - Búsqueda simplificada
// - GET /api/AdmClientes/search?... - Búsqueda paginada con filtros
// - GET /api/AdmClientes/{id} - Obtener cliente por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens)
// - Interceptor automático añade credenciales
//
// =====================================================================================

/**
 * Respuesta simplificada para búsqueda rápida de clientes
 * Endpoint: GET /api/AdmClientes/buscar
 */
export interface ClienteLegacyBusqueda {
  idCliente: number;
  codigoCliente: string;
  razonSocial: string;
  rfc: string;
  email: string | null;
  telefono: string | null;
}

/**
 * Respuesta completa de cliente Legacy
 * Endpoint: GET /api/AdmClientes/{id}
 */
export interface ClienteLegacyResponse {
  // === IDENTIFICACIÓN ===
  idCliente: number;
  codigoCliente: string;
  razonSocial: string;
  rfc: string;
  curp: string | null;

  // === INFORMACIÓN DE CONTACTO ===
  tipoCliente: number; // 1 = Cliente, 2 = Proveedor, 3 = Acreedor
  tipoClienteDescripcion: string;
  fechaAlta: string; // ISO date
  status: number; // 0 = Inactivo, 1 = Activo
  estaActivo: boolean;

  // === DOMICILIO FISCAL ===
  calleFiscal: string | null;
  numeroExteriorFiscal: string | null;
  numeroInteriorFiscal: string | null;
  coloniaFiscal: string | null;
  poblacionFiscal: string | null;
  estadoFiscal: string | null;
  paisFiscal: string | null;
  codigoPostalFiscal: string | null;

  // === DOMICILIO ENTREGA ===
  calleEntrega: string | null;
  numeroExteriorEntrega: string | null;
  numeroInteriorEntrega: string | null;
  coloniaEntrega: string | null;
  poblacionEntrega: string | null;
  estadoEntrega: string | null;
  paisEntrega: string | null;
  codigoPostalEntrega: string | null;

  // === INFORMACIÓN FINANCIERA ===
  usaCreditoCliente: number; // 0 = No, 1 = Sí
  limiteCredito: number;
  diasCredito: number;
  descuentoProntoPago: number;
  interesMoratorio: number;

  // === CONTACTO ===
  email: string | null;
  telefono1: string | null;
  telefono2: string | null;
  telefono3: string | null;
  fax: string | null;
  paginaWeb: string | null;
  contacto: string | null;

  // === CLASIFICACIONES ===
  clasificacion1: number;
  clasificacion2: number;
  clasificacion3: number;
  clasificacion4: number;
  clasificacion5: number;
  clasificacion6: number;

  // === IDs DE RELACIONES ===
  idMoneda: number;
  idAgente: number;
  idAlmacen: number;
  idListaPrecio: number;
  idPais: number;

  // === BANCARIZACIÓN ===
  bancoNombre: string | null;
  bancoCuenta: string | null;
  bancoSucursal: string | null;

  // === AUDITORÍA ===
  timestamp: string;
}

/**
 * Filtros para búsqueda paginada de clientes
 * Endpoint: GET /api/AdmClientes/search
 */
export interface ClienteLegacyFiltros {
  razonSocial?: string; // Búsqueda parcial
  rfc?: string; // Búsqueda parcial
  codigoCliente?: string; // Búsqueda parcial
  email?: string; // Búsqueda parcial
  telefono?: string; // Búsqueda parcial
  soloActivos?: boolean; // true = solo clientes activos
  page?: number; // Número de página (default: 1)
  pageSize?: number; // Registros por página (default: 50, max: 100)
}

/**
 * Respuesta paginada de búsqueda de clientes
 */
export interface ClienteLegacyPaginado {
  data: ClienteLegacyResponse[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  filters: ClienteLegacyFiltros;
}

/**
 * Respuesta genérica de API con estado
 */
export interface ClienteLegacyApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  executionTime?: string;
  errors?: string[];
}
