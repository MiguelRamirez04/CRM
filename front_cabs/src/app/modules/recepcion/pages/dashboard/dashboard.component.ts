import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdenTrabajo, EstadisticaRecepcion } from '../../../../core/models/orden-trabajo/orden-trabajo.interface';
import { RecepcionService } from '../../services/recepcion.service';
import { OrdenListComponent } from '../../components/orden-list/orden-list.component';
import { OrdenFormComponent } from '../../components/orden-form/orden-form.component';

@Component({
  selector: 'app-recepcion-dashboard',
  standalone: true,
  imports: [CommonModule, OrdenListComponent, OrdenFormComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class RecepcionDashboardComponent implements OnInit {
  private recepcionService = inject(RecepcionService);

  // Signals para estado reactivo
  ordenes = signal<OrdenTrabajo[]>([]);
  estadisticas = signal<EstadisticaRecepcion | null>(null);
  loading = signal(false);
  mostrarFormulario = signal(false);
  ordenEditar = signal<OrdenTrabajo | null>(null);

  ngOnInit() {
    this.cargarOrdenes();
    this.cargarEstadisticas();
  }

  cargarOrdenes() {
    this.loading.set(true);
    this.recepcionService.getOrdenes().subscribe({
      next: (ordenes) => {
        this.ordenes.set(ordenes);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando órdenes:', error);
        this.loading.set(false);
      }
    });
  }

  cargarEstadisticas() {
    this.recepcionService.getEstadisticas().subscribe({
      next: (stats) => {
        this.estadisticas.set(stats);
      },
      error: (error) => {
        console.error('Error cargando estadísticas:', error);
      }
    });
  }

  onNuevaOrden() {
    this.ordenEditar.set(null);
    this.mostrarFormulario.set(true);
  }

  onEditarOrden(id: number) {
    const orden = this.ordenes().find(o => o.id === id);
    if (orden) {
      this.ordenEditar.set(orden);
      this.mostrarFormulario.set(true);
    }
  }

  onVerDetalles(id: number) {
    // TODO: Navegar a página de detalles
    console.log('Ver detalles de orden:', id);
  }

  onEliminarOrden(id: number) {
    if (confirm('¿Está seguro de eliminar esta orden?')) {
      // TODO: Implementar eliminación
      console.log('Eliminar orden:', id);
    }
  }

  onGuardarOrden(ordenRequest: any) {
    this.loading.set(true);
    if (this.ordenEditar()) {
      // Actualizar
      this.recepcionService.actualizarOrden(this.ordenEditar()!.id, ordenRequest).subscribe({
        next: () => {
          this.cargarOrdenes();
          this.mostrarFormulario.set(false);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error actualizando orden:', error);
          this.loading.set(false);
        }
      });
    } else {
      // Crear
      this.recepcionService.crearOrden(ordenRequest).subscribe({
        next: (nuevaOrden) => {
          this.ordenes.update(ordenes => [nuevaOrden, ...ordenes]);
          this.mostrarFormulario.set(false);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error creando orden:', error);
          this.loading.set(false);
        }
      });
    }
  }

  onCancelarFormulario() {
    this.mostrarFormulario.set(false);
    this.ordenEditar.set(null);
  }

  onBuscarCliente(termino: string) {
    // TODO: Implementar búsqueda de clientes legacy
    console.log('Buscar cliente:', termino);
  }
}