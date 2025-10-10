import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  OrdenTrabajo,
  EstadisticaRecepcion,
  OrdenTrabajoRequest,
} from '../../../../core/models/orden-trabajo.interface';
import { RecepcionService } from '../../services/recepcion.service';
import { OrdenListComponent } from '../../components/orden-list/orden-list.component';
import { OrdenFormComponent } from '../../components/orden-form/orden-form.component';

@Component({
  selector: 'app-recepcion-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    OrdenListComponent,
    OrdenFormComponent
],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class RecepcionDashboardComponent implements OnInit {
  private recepcionService = inject(RecepcionService);

  // Signals para estado reactivo
  ordenes = signal<OrdenTrabajo[]>([]);
  estadisticas = signal<EstadisticaRecepcion | null>(null);
  loading = signal(false);
  mostrarFormulario = signal(false);
  mostrarDetallesOrden = signal(false);
  ordenSeleccionada = signal<OrdenTrabajo | null>(null);
  error = signal<string | null>(null);

  ngOnInit() {
    this.cargarDatosIniciales();
  }

  cargarDatosIniciales() {
    this.loading.set(true);
    this.error.set(null);
    this.recepcionService.getOrdenes().subscribe({
      next: (ordenes) => {
        this.ordenes.set(ordenes);
        this.loading.set(false);
      },
      error: (err) => this.handleError('Error al cargar las órdenes.', err),
    });
    this.recepcionService.getEstadisticas().subscribe({
      next: (stats) => this.estadisticas.set(stats),
      error: (err) =>
        this.handleError('No se pudieron cargar las estadísticas.', err, false),
    });
  }

  onNuevaOrden() {
    this.mostrarFormulario.set(true);
    // Prevenir scroll del body cuando el modal está abierto
    document.body.classList.add('modal-open');
  }

  onEditarOrden(id: number) {
    // TODO: Implementar edición de orden
    console.log('Editar orden:', id);
  }

  onVerDetalles(id: number) {
    // Buscar la orden en la lista actual
    const orden = this.ordenes().find(o => o.id === id);
    if (orden) {
      this.ordenSeleccionada.set(orden);
      this.mostrarDetallesOrden.set(true);
      // Prevenir scroll del body cuando el modal está abierto
      document.body.classList.add('modal-open');
    } else {
      this.handleError('No se encontró la orden seleccionada.', null);
    }
  }

  onEliminarOrden(id: number) {
    if (confirm('¿Está seguro de eliminar esta orden de trabajo?')) {
      this.loading.set(true);
      this.recepcionService.eliminarOrden(id).subscribe({
        next: () => {
          this.ordenes.update((ordenes) =>
            ordenes.filter((o) => o.id !== id)
          );
          this.loading.set(false);
        },
        error: (err) => this.handleError('Error al eliminar la orden.', err),
      });
    }
  }

  onGuardarOrden(ordenRequest: OrdenTrabajoRequest) {
    this.loading.set(true);
    this.error.set(null);
    
    this.recepcionService.crearOrden(ordenRequest).subscribe({
      next: (ordenCreada) => {
        console.log('Orden creada exitosamente:', ordenCreada);
        this.mostrarFormulario.set(false);
        this.loading.set(false);
        // Restaurar scroll del body
        document.body.classList.remove('modal-open');
        this.cargarDatosIniciales(); // Recargar lista de órdenes
      },
      error: (err: any) => {
        console.error('Error al crear la orden:', err);
        this.handleError('Error al guardar la orden. Por favor intente nuevamente.', err);
      },
    });
  }

  onCancelarFormulario() {
    this.mostrarFormulario.set(false);
    this.error.set(null);
    // Restaurar scroll del body cuando se cierra el modal
    document.body.classList.remove('modal-open');
  }

  onCerrarDetallesOrden() {
    this.mostrarDetallesOrden.set(false);
    this.ordenSeleccionada.set(null);
    // Restaurar scroll del body cuando se cierra el modal
    document.body.classList.remove('modal-open');
  }

  onBuscarCliente(term: string) {
    // Lógica para buscar cliente si es necesario en el dashboard
    console.log('Buscando cliente con término:', term);
  }

  private handleError(
    message: string,
    error: any,
    showToUser: boolean = true
  ) {
    console.error(message, error);
    if (showToUser) {
      this.error.set(message);
    }
    this.loading.set(false);
  }
}