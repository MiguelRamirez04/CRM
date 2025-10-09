// =====================================================================================
// ENUMS ORDEN TRABAJO - estado-orden.enum.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los enums que corresponden a los enums del backend
// para estados, tipos y modalidades de órdenes de trabajo.
//
// =====================================================================================

export enum EstadoOrden {
  CAPTURADA = 'CAPTURADA',
  ASIGNADA = 'ASIGNADA',
  EN_PROCESO = 'EN_PROCESO',
  COMPLETADA = 'COMPLETADA',
  CANCELADA = 'CANCELADA'
}

export enum TipoOrden {
  ASESORIA = 'ASESORIA',
  COTIZACION = 'COTIZACION',
  SERVICIO = 'SERVICIO'
}

export enum Modalidad {
  PRESENCIAL = 'PRESENCIAL',
  REMOTO = 'REMOTO',
  HIBRIDO = 'HIBRIDO'
}

export enum EstadoFacturado {
  PENDIENTE = 'PENDIENTE',
  FACTURADO = 'FACTURADO',
  NO_REQUIERE = 'NO_REQUIERE'
}