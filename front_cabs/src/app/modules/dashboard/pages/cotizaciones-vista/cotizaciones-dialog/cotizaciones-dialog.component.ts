import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CotizacionService } from '../../../../../core/services/cotizacion.service';
import { CotizacionResponse } from '../../../../../core/models/cotizacion.interface';
import { PdfExportService } from '../../../../../core/services/pdf-export.service';

@Component({
  selector: 'app-cotizaciones-dialog',
  imports: [CommonModule],
  templateUrl: './cotizaciones-dialog.component.html',
  styleUrls: ['./cotizaciones-dialog.component.css']
})
export class CotizacionesDialogComponent implements OnInit {
  @Input() cotizacionId!: number;
  @Output() cerrar = new EventEmitter<void>();

  private cotizacionService = inject(CotizacionService);
  private pdfExportService = inject(PdfExportService);

  cotizacion: CotizacionResponse | null = null;
  cargando = true;
  error = '';

  ngOnInit(): void {
    this.cargarDetalle();
  }

  cargarDetalle(): void {
    this.cargando = true;
    this.error = '';

    this.cotizacionService.obtenerPorId(this.cotizacionId).subscribe({
      next: (response) => {
        this.cotizacion = response;
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar cotización:', err);
        this.error = err.error?.mensaje || 'Error al cargar los detalles';
        this.cargando = false;
      }
    });
  }

  cerrarModal(): void {
    this.cerrar.emit();
  }

  exportarPDF(): void {
    if (!this.cotizacion) {
      console.error('No hay datos de cotización para exportar');
      return;
    }

    // Support both synchronous and Promise-based implementations
    Promise.resolve(this.pdfExportService.exportarCotizacionPDF(this.cotizacion))
      .catch((err: any) => console.error('Error exportando PDF:', err));
  }

  obtenerLabelEstado(estado: string): string {
    return this.cotizacionService.obtenerLabelEstado(estado as any);
  }

  obtenerClaseEstado(estado: string): string {
    return this.cotizacionService.obtenerClaseEstado(estado as any);
  }

  formatearMoneda(valor: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(valor);
  }

  formatearFecha(fecha: string | Date): string {
    return new Date(fecha).toLocaleString('es-MX', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
