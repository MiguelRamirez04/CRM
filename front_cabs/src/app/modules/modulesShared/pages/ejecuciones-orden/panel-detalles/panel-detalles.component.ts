import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EjecucionOrdenResponse, TipoEjecucion } from '../../../../../core/models/ejecucion-orden.interface';
import { EjecucionOrdenService } from '../../../../../core/services/ejecucion-orden.service';

@Component({
  selector: 'app-panel-detalles',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './panel-detalles.component.html',
  styleUrl: './panel-detalles.component.css'
})
export class PanelDetallesComponent {
  private readonly ejecucionService = inject(EjecucionOrdenService);

  // Inputs
  ejecucion = input<EjecucionOrdenResponse | null>(null);
  visible = input<boolean>(false);

  // Outputs
  cerrar = output<void>();

  // Enums para template
  readonly TipoEjecucion = TipoEjecucion;

  /**
   * Cierra el panel lateral
   */
  cerrarPanel(): void {
    this.cerrar.emit();
  }

  /**
   * Helpers para el template
   */
  calcularDuracion(ejecucion: EjecucionOrdenResponse): string {
    return this.ejecucionService.calcularDuracion(ejecucion);
  }

  getIconoTipo(tipo: string): string {
    return this.ejecucionService.getIconoTipo(tipo);
  }

  getColorTipo(tipo: string): string {
    return this.ejecucionService.getColorTipo(tipo);
  }

  formatearFecha(fecha: string): string {
    return new Date(fecha).toLocaleString('es-MX');
  }
}
