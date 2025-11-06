import { Component, OnInit, signal, inject, computed, WritableSignal, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';

import {
  OrdenTrabajo,
  EstadisticaRecepcion,
} from '../../../../core/models/orden-trabajo.interface';
import { RecepcionService } from '../../services/recepcion.service';
import { DialogService } from '../../services/dialog.service';
import { OrdenListComponent } from '../../components/orden-list/orden-list.component';
import { HeaderComponent } from '../../../../shared/components/header/header';
import { OrdenDetalleDialogComponent } from '../../components/orden-detalle-dialog/orden-detalle-dialog.component';
// import { SecureAuthService } from '../../../../core/services/secure-auth.service'; // Ya no es necesario

// Define los estados posibles para el filtro
type EstadoOrden = 'CAPTURADA' | 'ASIGNADA' | 'EN_PROCESO' | 'COMPLETADA' | 'CANCELADA';

@Component({
  selector: 'app-recepcion-dashboard',
  templateUrl: './ordenes.component.html',
  styleUrls: ['./ordenes.component.css'],
  standalone: true,
  imports: [CommonModule, HeaderComponent, DecimalPipe, OrdenListComponent]
})
export class RecepcionDashboardComponent implements OnInit {
  private recepcionService = inject(RecepcionService);
  private dialogService = inject(DialogService);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  // private authService = inject(SecureAuthService); // Eliminado

  // Signals de datos
  ordenes: WritableSignal<OrdenTrabajo[]> = signal([]);
  estadisticas: WritableSignal<EstadisticaRecepcion | null> = signal(null);
  loading: WritableSignal<boolean> = signal(false);
  error: WritableSignal<string | null> = signal(null);
  
  // --- NUEVO: Signals para Filtros ---
  filtroBusqueda: WritableSignal<string> = signal('');
  filtroEstado: WritableSignal<string> = signal('TODOS'); // 'TODOS' o un EstadoOrden
  
  // Lista de estados para el dropdown
  readonly estadosOrden: EstadoOrden[] = ['CAPTURADA', 'ASIGNADA', 'EN_PROCESO', 'COMPLETADA', 'CANCELADA'];

  // --- NUEVO: Signal Computada para Órdenes Filtradas ---
  ordenesFiltradas: Signal<OrdenTrabajo[]> = computed(() => {
    const busqueda = this.filtroBusqueda().toLowerCase().trim();
    const estado = this.filtroEstado();
    
    // Si no hay filtros, devuelve todo
    if (!busqueda && estado === 'TODOS') {
      return this.ordenes();
    }

    return this.ordenes().filter(orden => {
      // 1. Filtrar por Estado
      const matchEstado = (estado === 'TODOS') || (orden.estado === estado);
      if (!matchEstado) return false;

      // 2. Filtrar por Búsqueda
      if (!busqueda) return true; // Ya pasó el filtro de estado
      
      return (
        orden.id.toString().includes(busqueda) ||
        orden.nombreCliente.toLowerCase().includes(busqueda) ||
        orden.tipoOrden.toLowerCase().includes(busqueda) ||
        (orden.clienteId && orden.clienteId.toString().includes(busqueda))
      );
    });
  });

  getPorcentaje(valor: number, total: number): number {
    if (total === 0) {
      return 0;
    }
    return (valor / total) * 100;
  }

  get fechaActual(): Date {
    return new Date();
  }

  ngOnInit() {
    this.cargarDatosIniciales();
  }

  cargarDatosIniciales() {
    this.loading.set(true);
    this.error.set(null);
    
    // Carga de Órdenes
    this.recepcionService.getOrdenes().subscribe({
      next: (ordenes) => {
        this.ordenes.set(ordenes);
        this.loading.set(false);
      },
      error: (err) => this.handleError('Error al cargar las órdenes.', err),
    });

    // Carga de Estadísticas
    this.recepcionService.getEstadisticas().subscribe({
      next: (stats) => this.estadisticas.set(stats),
      error: (err) =>
        this.handleError('No se pudieron cargar las estadísticas.', err, false),
    });
  }

  // --- NUEVO: Métodos para actualizar filtros ---
  onBusquedaChange(event: Event) {
    const valor = (event.target as HTMLInputElement).value;
    this.filtroBusqueda.set(valor);
  }

  onEstadoChange(event: Event) {
    const valor = (event.target as HTMLSelectElement).value;
    this.filtroEstado.set(valor);
  }

  // Métodos de Acciones de Órdenes (sin cambios)
  onNuevaOrdenLegacy() {
    this.dialogService.openNuevaOrdenLegacy().subscribe(ordenCreada => {
      if (ordenCreada) this.cargarDatosIniciales();
    });
  }

  onNuevaOrdenNuevo() {
    this.dialogService.openNuevaOrdenNuevo().subscribe(ordenCreada => {
      if (ordenCreada) this.cargarDatosIniciales();
    });
  }

  onEditarOrden(id: number) {
    const orden = this.ordenes().find(o => o.id === id);
    if (orden) {
      this.dialogService.openEditarOrden(orden).subscribe(ordenActualizada => {
        if (ordenActualizada) this.cargarDatosIniciales();
      });
    } else {
      this.handleError('No se encontró la orden seleccionada para editar.', null);
    }
  }

  // Métodos del Sidebar (Modificados para usar Dialog)
  onVerDetalles(id: number) {
    const orden = this.ordenes().find(o => o.id === id);
    if (orden) {
      const dialogRef = this.dialog.open(OrdenDetalleDialogComponent, {
        data: { orden },
        panelClass: 'orden-detalle-dialog',
        position: { right: '0' },
        maxWidth: '600px',
        width: '600px',
        height: '100vh',
        hasBackdrop: true,
        backdropClass: 'bg-black/30'
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result?.action === 'edit') {
          this.onEditarOrden(result.ordenId);
        }
      });
    } else {
      this.handleError('No se encontró la orden seleccionada.', null);
    }
  }

  // Método de Logout Eliminado
  // onLogout() { ... }

  /**
   * Maneja la creación de una nueva ejecución desde una orden
   * Navega a la página de ejecuciones con datos pre-llenados
   */
  onCrearEjecucion(orden: OrdenTrabajo) {
    console.log('🚀 Creando ejecución para orden:', orden);
    
    // Navegar con query params para pre-llenar el formulario
    this.router.navigate(['/recepcion/ordenes-trabajo/ejecuciones'], {
      queryParams: {
        ordenId: orden.id,
        clienteNombre: orden.nombreCliente,
        clienteId: orden.clienteId,
        autoOpen: 'true' // Flag para abrir modal automáticamente
      }
    });
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