import { Component, inject, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GastoViaticoService } from '../../../../../core/services/gasto-viatico.service';
import { GastoViaticoResponse } from '../../../../../core/models/gasto-viatico.interface';

@Component({
  selector: 'app-viaticos-detalle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './viaticos-detalle.component.html',
  styleUrl: './viaticos-detalle.component.css'
})
export class ViaticosDetalleComponent implements OnChanges {
  private viaticoService = inject(GastoViaticoService);

  @Input() viaticoId: number | null = null;
  @Input() mostrar = false;
  @Output() cerrar = new EventEmitter<void>();

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
}
