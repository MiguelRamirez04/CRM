// Interfaces para reportes de cotizaciones

export interface EstadisticasGeneralesDto {
  totalCotizaciones: number;
  montoTotal: number;
  montoPromedio: number;
  montoMaximo: number;
  montoMinimo: number;
  cotizacionesActivas: number;
  cotizacionesCanceladas: number;
  clientesUnicos: number;
  productosUnicos: number;
  fechaInicio?: Date;
  fechaFin?: Date;
}

export interface TopClienteDto {
  idCliente: number;
  codigoCliente: string;
  razonSocial: string;  // Coincide con el backend
  rfc?: string;
  totalCotizaciones: number;
  montoTotal: number;
  montoPromedio: number;
  cotizacionesActivas?: number;
  ultimaCotizacion?: Date;
  ranking: number;
}

export interface CotizacionVencimientoDto {
  idDocumento?: number;
  serieDocumento?: string;
  folio: number;
  razonSocial?: string;
  montoTotal: number;
  fechaVencimiento: Date;
  diasRestantes: number;
  nivelUrgencia?: string;
  fechaCreacion?: Date;
  estatus?: string;
}

export interface RendimientoAgenteDto {
  idAgente: number;
  codigoAgente?: string;
  nombreAgente: string;
  totalCotizaciones: number;
  montoTotal: number;
  montoPromedio: number;
  cotizacionesActivas?: number;
  cotizacionesCanceladas?: number;
  tasaConversion: number;
  ultimaCotizacion?: Date;
  clientesUnicos?: number;
  ranking?: number;
}

export interface ProductoCotizadoDto {
  idProducto: number;
  codigoProducto: string;
  nombreProducto: string;
  totalCotizaciones: number;
  cantidadTotal: number;
  montoTotal: number;
  precioPromedio: number;
  clientesUnicos: number;
  ranking: number;
}

export interface CotizacionPorRangoDto {
  rangoMonto: string;
  montoMinimo: number;
  montoMaximo?: number;
  totalCotizaciones: number;
  montoTotal: number;
  montoPromedio: number;
  cotizacionesActivas: number;
  cotizacionesCanceladas: number;
  porcentajeDelTotal: number;
}

// Interfaces para respuestas de API
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  pagination?: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  filtros?: any;
  resumen?: any;
  executionTime?: string;
  message?: string;
  error?: string;
}

export interface FiltrosFecha {
  fechaInicio?: string;
  fechaFin?: string;
}

export interface ResumenEstadisticas {
  total: number;
  criticos?: number;
  altos?: number;
  medios?: number;
  bajos?: number;
}

export interface ResumenTopClientes {
  totalClientes: number;
  montoTotalGeneral: number;
  clienteTop: string;
}

export interface ResumenRendimiento {
  totalAgentes: number;
  totalCotizaciones: number;
  montoTotalGeneral: number;
  promedioConversion: number;
  mejorAgente?: string;
}

export interface ResumenProductos {
  totalProductos: number;
  montoTotalGeneral: number;
  cantidadTotalGeneral: number;
  clientesUnicosTotal: number;
}

export interface ResumenRangos {
  totalRangos: number;
  totalCotizaciones: number;
  montoTotalGeneral: number;
  rangoMasFrecuente?: string;
  rangoMayorMonto?: string;
}