export interface Reparacion {
  id: number;
  ordenId: number;
  tecnicoId: number;
  dispositivoTipo: string;
  marca: string;
  modelo: string;
  accesoriosRecibidos: string;
  descripcionFalla: string;
  diagnostico: string;
  solucionAplicada: string;
  resultado: string;
  causaIrreparable: string;
  respaldoDatosAutorizado: boolean;
  garantiaDias: number;
  fechaLlegada: string; // O Date
  empezadoEn: string;
  entregadoEn: string;
  tipoEntrega: string;
  ubicacionAlmacenamiento: string;
  notas: string;
  costoManoObra: number;
  costoRefaccionesCompra: number;
  costoRefaccionesPublico: number;
  costoTotalCompra: number;
  costoTotalPublico: number;
  margenEstimado: number;
  nombreTecnico: string;
  folioOrden: string;
}

// DTO para crear/editar (puedes ajustar qué campos son obligatorios)
export interface ReparacionDto extends Partial<Reparacion> {}