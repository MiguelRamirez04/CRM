// =====================================================================================
// INTERFACES ALMACEN LEGACY - almacen-legacy.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las interfaces TypeScript para consumir las APIs Legacy de Almacenes
// desde el sistema Adminpaq (legacy_adm).
//
// ENDPOINTS DEL BACKEND:
// - GET /api/AdmAlmacenes - Obtener todos los almacenes
// - GET /api/AdmAlmacenes/{id} - Obtener almacén por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens)
// - Interceptor automático añade credenciales
//
// =====================================================================================

/**
 * Respuesta de almacén Legacy
 * Endpoint: GET /api/AdmAlmacenes
 */
export interface AlmacenLegacyResponse {
  // === IDENTIFICACIÓN ===
  idAlmacen: number;
  codigoAlmacen: string;
  nombreAlmacen: string;

  // === INFORMACIÓN ===
  tipoAlmacen: number; // 1 = Normal, 2 = En tránsito, 3 = Consignación
  status: number; // 0 = Inactivo, 1 = Activo
  estaActivo: boolean;

  // === DOMICILIO ===
  calle: string | null;
  numeroExterior: string | null;
  numeroInterior: string | null;
  colonia: string | null;
  poblacion: string | null;
  estado: string | null;
  pais: string | null;
  codigoPostal: string | null;

  // === CONTACTO ===
  telefono: string | null;
  encargado: string | null;

  // === AUDITORÍA ===
  timestamp: string;
}

/**
 * Respuesta simplificada de almacén
 * Usada para selección en cotizaciones
 */
export interface AlmacenLegacySimple {
  idAlmacen: number;
  codigoAlmacen: string;
  nombreAlmacen: string;
  estaActivo: boolean;
}

/**
 * Respuesta genérica de API con estado
 */
export interface AlmacenLegacyApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  executionTime?: string;
  errors?: string[];
}
