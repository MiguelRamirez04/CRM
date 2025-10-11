export interface ClienteCompleto {
  // Campos principales de la respuesta real del API
  clienteId: number;
  nombreComercial: string | null;
  rfc: string | null;
  activo: boolean;
  legacyClientId: number;
  calle: string | null;
  numeroExterior: string | null;
  colonia: string | null;
  codigoPostal: string | null;
  ciudad: string | null;
  estado: string | null;
  pais: string | null;
  telefonoPrincipal: string | null;
  emailPrincipal: string | null;
  
  // Campos adicionales para compatibilidad con el frontend
  id?: number; // Mapeado desde clienteId
  nombre?: string; // Mapeado desde nombreComercial
  direccion?: string | null; // Construido desde calle + numeroExterior
  telefono?: string | null; // Mapeado desde telefonoPrincipal
  email?: string | null; // Mapeado desde emailPrincipal
  fechaRegistro?: string; // Para mostrar si está disponible
}

export interface ApiClientesCompletosResponse {
  items: ClienteCompleto[];
  pagina: number;
  resultadosPorPagina: number;
  totalItems: number | null;
  totalPaginas: number | null;
}

export interface PagedResponse<T> {
  data: T[];
  paginaActual: number;
  totalPaginas: number;
  totalRegistros: number;
  tieneSiguiente: boolean;
  tieneAnterior: boolean;
}

export interface BusquedaAvanzadaFiltros {
  nombre?: string;
  rfc?: string;
  telefono?: string;
  email?: string;
}