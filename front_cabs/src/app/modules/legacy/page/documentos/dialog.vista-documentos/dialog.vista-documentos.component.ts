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
    if (this.cotizacion()?.cancelado === 1) {
      alert('⚠️ Esta cotización ya está cancelada');
      return;
    }

    const confirmar = confirm('¿Está seguro de que desea cancelar esta cotización?');
    if (confirmar) {
      this.solicitudCancelacion.emit(this.idDocumento);
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
    if (cotizacion.cancelado === 1) {
      return { text: 'CANCELADO', class: 'bg-red-100 text-red-800 border-red-200' };
    }
    if (cotizacion.afectado === 1) {
      return { text: 'AFECTADO', class: 'bg-green-100 text-green-800 border-green-200' };
    }
    return { text: 'PENDIENTE', class: 'bg-yellow-100 text-yellow-800 border-yellow-200' };
  }
}
