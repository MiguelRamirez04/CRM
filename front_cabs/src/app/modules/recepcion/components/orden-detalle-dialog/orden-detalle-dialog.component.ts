import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { OrdenTrabajo } from '../../../../core/models/orden-trabajo.interface';

@Component({
  selector: 'app-orden-detalle-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './orden-detalle-dialog.component.html',
  styleUrls: ['./orden-detalle-dialog.component.css']
})
export class OrdenDetalleDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<OrdenDetalleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { orden: OrdenTrabajo }
  ) {}

  get orden(): OrdenTrabajo {
    return this.data.orden;
  }

  onCerrar(): void {
    this.dialogRef.close();
  }

  onEditarOrden(ordenId: number): void {
    this.dialogRef.close({ action: 'edit', ordenId });
  }

  getEstadoColor(estado: string): string {
    const colores: { [key: string]: string } = {
      'CAPTURADA': 'bg-blue-100 text-blue-800 border-blue-200',
      'ASIGNADA': 'bg-purple-100 text-purple-800 border-purple-200',
      'EN_PROCESO': 'bg-yellow-100 text-yellow-800 border-yellow-200',
      'COMPLETADA': 'bg-green-100 text-green-800 border-green-200',
      'CANCELADA': 'bg-red-100 text-red-800 border-red-200'
    };
    return colores[estado] || 'bg-gray-100 text-gray-800 border-gray-200';
  }

  getEstadoFacturadoColor(estado: string | undefined): string {
    const estadoStr = estado || 'PENDIENTE';
    const colores: { [key: string]: string } = {
      'PENDIENTE': 'bg-amber-100 text-amber-800 border-amber-200',
      'FACTURADO': 'bg-green-100 text-green-800 border-green-200',
      'NO_REQUIERE': 'bg-gray-100 text-gray-800 border-gray-200'
    };
    return colores[estadoStr] || 'bg-gray-100 text-gray-800 border-gray-200';
  }
}
