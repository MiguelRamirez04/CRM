export interface ReparacionComponente {
  id: number;
  reparacionId: number;
  componente: string;
  cantidad: number;
  proveedor: string;
  garantiaMeses: number;
  costoUnitarioCompra: number;
  costoUnitarioPublico: number;
  subtotalCompra: number;  // Generalmente calculados por el backend
  subtotalPublico: number; // Generalmente calculados por el backend
  notas: string;
}

export interface ReparacionComponenteDto extends Omit<ReparacionComponente, 'id' | 'subtotalCompra' | 'subtotalPublico'> {}