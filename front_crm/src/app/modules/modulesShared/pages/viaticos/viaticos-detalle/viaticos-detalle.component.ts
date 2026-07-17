import { Component, inject, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GastoViaticoService } from '../../../../../core/services/gasto-viatico.service';
import { GastoViaticoResponse } from '../../../../../core/models/gasto-viatico.interface';

import { SidePanelComponent } from '../../../../../shared/organisms/side-panel/side-panel.component';
import { DetailSectionComponent } from '../../../../../shared/molecules/detail-section/detail-section.component';
import { LoadingSpinnerComponent } from '../../../../../shared/atoms/loading-spinner/loading-spinner.component';
import { AlertComponent } from '../../../../../shared/molecules/alert/alert.component';
import { BadgeComponent } from '../../../../../shared/atoms/bage/badge.component';
import { UiBotonComponent } from '../../../../../shared/atoms/boton/boton.component';
import { UiIconComponent } from '../../../../../shared/atoms/icono/icono.component';
import { UitipografiaComponent } from '../../../../../shared/atoms/tipografia/tipografia.component';

@Component({
  selector: 'app-viaticos-detalle',
  standalone: true,
  imports: [
    CommonModule,
    SidePanelComponent,
    DetailSectionComponent,
    LoadingSpinnerComponent,
    AlertComponent,
    BadgeComponent,
    UiBotonComponent,
    UiIconComponent,
    UitipografiaComponent
  ],
  templateUrl: './viaticos-detalle.component.html'
})
export class ViaticosDetalleComponent implements OnChanges {
  private viaticoService = inject(GastoViaticoService);

  @Input() viaticoId: number | null = null;
  @Input() mostrar = false;
  @Output() cerrar = new EventEmitter();

  viatico: GastoViaticoResponse | null = null;
  cargando = false;
  error = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['viaticoId'] && this.viaticoId && this.mostrar) {
      this.cargarDetalle();
    }
  }

  private cargarDetalle(): void {
    if (!this.viaticoId) return;

    this.cargando = true;
    this.error = '';

    this.viaticoService.obtenerPorId(this.viaticoId).subscribe({
      next: (data) => {
        this.viatico = data;
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar viático:', err);
        this.error = 'No se pudo cargar el viático. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  onCerrar(): void {
    this.viatico = null;
    this.error = '';
    this.cerrar.emit();
  }

  get tituloDialog(): string {
    return this.viatico ? `Viático #${this.viatico.id}` : 'Detalles del Viático';
  }

  reintentar(): void {
    this.cargarDetalle();
  }
}