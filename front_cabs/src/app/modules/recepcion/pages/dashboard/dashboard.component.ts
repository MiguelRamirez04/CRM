import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  OrdenTrabajo,
  EstadisticaRecepcion,
  OrdenTrabajoRequest,
  OrdenTrabajoUpdateRequest,
} from '../../../../core/models/orden-trabajo.interface';
import { RecepcionService } from '../../services/recepcion.service';
import { OrdenListComponent } from '../../components/orden-list/orden-list.component';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';
import { DialogService } from '../../services/dialog.service';
import { NuevaOrdenClienteNuevoComponent } from '../../components/dialogs/nueva-orden-cliente-nuevo/nueva-orden-cliente-nuevo.component';
import { NuevaOrdenClienteLegacyComponent } from '../../components/dialogs/nueva-orden-cliente-legacy/nueva-orden-cliente-legacy.component';
import { EditarOrdenComponent } from '../../components/dialogs/editar-orden/editar-orden.component';

@Component({
  selector: 'app-recepcion-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    OrdenListComponent
],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class RecepcionDashboardComponent implements OnInit {
  private recepcionService = inject(RecepcionService);
  private authService = inject(SecureAuthService);
  private dialogService = inject(DialogService);

  // Signals para estado reactivo
  ordenes = signal<OrdenTrabajo[]>([]);
  estadisticas = signal<EstadisticaRecepcion | null>(null);
  loading = signal(false);
  mostrarDetallesOrden = signal(false);
  ordenSeleccionada = signal<OrdenTrabajo | null>(null);
  error = signal<string | null>(null);
  
  // Propiedad para fecha actual
  get fechaActual(): Date {
    return new Date();
  }

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

  onNuevaOrdenClienteNuevo() {
    const dialogRef = this.dialogService.open(NuevaOrdenClienteNuevoComponent);
    dialogRef.afterClosed().then(result => {
      if (result) {
        this.cargarDatosIniciales(); // Recargar lista si se guardó
      }
    });
  }

  onNuevaOrdenClienteLegacy() {
    const dialogRef = this.dialogService.open(NuevaOrdenClienteLegacyComponent);
    dialogRef.afterClosed().then(result => {
      if (result) {
        this.cargarDatosIniciales(); // Recargar lista si se guardó
      }
    });
  }

  onEditarOrden(id: number) {
    // Buscar la orden en la lista actual
    const orden = this.ordenes().find(o => o.id === id);
    if (orden) {
      const dialogRef = this.dialogService.open(EditarOrdenComponent, {
        data: { orden }
      });
      dialogRef.afterClosed().then(result => {
        if (result) {
          this.cargarDatosIniciales(); // Recargar lista si se guardó
        }
      });
    } else {
      this.handleError('No se encontró la orden seleccionada para editar.', null);
    }
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

  onLogout() {
    if (confirm('¿Está seguro de que desea cerrar sesión?')) {
      this.authService.logout().subscribe({
        next: () => {
          // El servicio maneja la redirección automáticamente
          console.log('Sesión cerrada exitosamente');
        },
        error: (error) => {
          console.error('Error al cerrar sesión:', error);
          // Forzar logout si falla la llamada al servidor
          this.authService.forceLogout();
        }
      });
    }
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