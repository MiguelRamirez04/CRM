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
 * Detalle completo de ubicación
 */
export interface UbicacionDetalle {
  calle: string;
  numeroExterior: string;
  numeroInterior: string;
  colonia: string;
  codigoPostal: string;
  ciudad: string;
  municipio: string;
  estado: string;
  pais: string;
  telefono1: string;
  telefono2: string;
  telefonoCompleto: string;
}

/**
 * Respuesta completa de cliente Legacy
 * Endpoint: GET /api/AdmClientes/{id}
 * Endpoint: GET /api/AdmClientes/search
 */
export interface ClienteLegacyResponse {
  id: number;
  codigoCliente: string;
  nombre: string;
  rfc: string;
  telefono: string;
  email: string;
  ubicacion: string;
  estado: string;
  ubicacionDetalle?: UbicacionDetalle;
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
  estado?: string; // Estado del domicilio
  ciudad?: string; // Ciudad del domicilio
  estatus?: number | null; // 0 = Inactivo, 1 = Activo, null = Todos
  tipoDireccion?: number; // 1 = Fiscal, 2 = Envío, 3 = Otros
  incluirDetalleUbicacion?: boolean; // Incluir detalle de ubicación
  numeroPagina?: number; // Número de página (default: 1)
  tamanoPagina?: number; // Registros por página (default: 50, max: 100)
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
