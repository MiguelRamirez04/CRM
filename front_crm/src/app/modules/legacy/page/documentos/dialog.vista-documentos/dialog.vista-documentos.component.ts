// =====================================================================================
// DIALOG VISTA DOCUMENTOS - Panel Lateral de Detalles
// =====================================================================================
import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CotizacionLegacyService } from '../../../../../core/services/cotizacion-legacy.service';
import { CotizacionLegacyResponse } from '../../../../../core/models/cotizacion-legacy.interface';

@Component({
  selector: 'app-dialog-vista-documentos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dialog.vista-documentos.component.html',
  styleUrl: './dialog.vista-documentos.component.css'
})
export class DialogVistaDocumentosComponent implements OnInit {
  @Input() idDocumento!: number;
  @Output() cerrar = new EventEmitter<void>();
  @Output() solicitudCancelacion = new EventEmitter<number>();
  @Output() solicitudEliminacion = new EventEmitter<number>();

  // Signals
  visible = signal<boolean>(false);
  loading = signal<boolean>(true);
  cotizacion = signal<CotizacionLegacyResponse | null>(null);
  error = signal<string | null>(null);

  constructor(private cotizacionService: CotizacionLegacyService) {}

  ngOnInit(): void {
    // Pequeño delay para animación de entrada
    setTimeout(() => this.visible.set(true), 50);
    this.cargarDetalle();
  }

  cargarDetalle(): void {
    this.loading.set(true);
    this.error.set(null);

    this.cotizacionService.obtenerPorId(this.idDocumento).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cotizacion.set(response.data);
        } else {
          this.error.set('No se pudo cargar la cotización');
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.mensaje || 'Error al cargar los detalles');
        this.loading.set(false);
      }
    });
  }

  cerrarPanel(): void {
    this.visible.set(false);
    setTimeout(() => this.cerrar.emit(), 300);
  }

  solicitarCancelacion(): void {
    if (this.cotizacion()?.estado === 'Cancelada') {
      alert('⚠️ Esta cotización ya está cancelada');
      return;
    }

    const confirmar = confirm('¿Está seguro de que desea cancelar esta cotización?');
    if (confirmar) {
      this.solicitudCancelacion.emit(this.idDocumento);
    }
  }

  solicitarEliminacion(): void {
    if (this.cotizacion()?.estado !== 'Cancelada') {
      alert('⚠️ Solo se pueden eliminar cotizaciones canceladas');
      return;
    }

    const confirmar = confirm('¿Está seguro de que desea eliminar permanentemente esta cotización?');
    if (confirmar) {
      this.solicitudEliminacion.emit(this.idDocumento);
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('es-MX', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getEstadoBadge(cotizacion: CotizacionLegacyResponse): { text: string; class: string } {
    if (cotizacion.estado === 'Cancelada') {
      return { text: 'CANCELADO', class: 'bg-red-100 text-red-800 border-red-200' };
    }
    if (cotizacion.estado === 'Activa') {
      return { text: 'ACTIVA', class: 'bg-green-100 text-green-800 border-green-200' };
    }
    return { text: cotizacion.estado, class: 'bg-gray-100 text-gray-800 border-gray-200' };
  }

  getDescuento1(): number {
    const cot = this.cotizacion();
    if (!cot) return 0;
    return cot.descuentoDoc1 || 0;
  }

  getDescuento2(): number {
    const cot = this.cotizacion();
    if (!cot) return 0;
    return cot.descuentoDoc2 || 0;
  }

  getDescuento3(): number {
    const cot = this.cotizacion();
    if (!cot) return 0;
    return cot.descuentoDoc3 || 0;
  }

  getTotalDescuentos(): number {
    return this.getDescuento1() + this.getDescuento2() + this.getDescuento3();
  }

  // Calcular el total correcto (Subtotal - Descuentos + IVA)
  getTotalCalculado(): number {
    const cot = this.cotizacion();
    if (!cot) return 0;
    
    const subtotal = cot.subtotal || 0;
    const descuentos = this.getTotalDescuentos();
    const iva = cot.iva || 0;
    
    return subtotal - descuentos + iva;
  }
}
