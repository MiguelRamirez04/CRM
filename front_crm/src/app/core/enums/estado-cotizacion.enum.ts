// =====================================================================================
// ENUMS COTIZACION - estado-cotizacion.enum.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el enum que corresponde al enum del backend
// para estados de cotizaciones.
//
// ESTADOS DISPONIBLES:
// - NUEVA: Cotización recién creada, pendiente de revisión
// - APROBADA: Cotización aceptada por el cliente
// - RECHAZADA: Cotización rechazada por el cliente
// - VENCIDA: Cotización que superó su validez en días
// - CANCELADA: Cotización cancelada internamente
//
// =====================================================================================

export enum EstadoCotizacion {
  NUEVA = 'NUEVA',
  APROBADA = 'APROBADA',
  RECHAZADA = 'RECHAZADA',
  VENCIDA = 'VENCIDA',
  CANCELADA = 'CANCELADA'
}
