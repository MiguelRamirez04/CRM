// Esta interfaz se basa en el "Example Value" de tu API
export interface RecepcionTicket {
  id: number;
  notas: string;
  folio: string;
  citProgramada: string; // O tipo Date si lo prefieres
  tipoOrden: string;
  nombreCliente: string;
  telefono: string;
  estado: string;
  clienteId: number;
  falla: string;
  ubicacionText: string;
  requiereFactura: boolean;
  costoEstimado: number;
  actualizadoPor: string;
  actualizadoEn: string;
  asignadoAUsuarioId: number;
}

// Modelo para la respuesta paginada (opcional pero recomendado)
export interface PaginacionRecepcion {
  totalItems: number;
  items: RecepcionTicket[];
}