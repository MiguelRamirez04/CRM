import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EjecucionOrdenService } from '../../../../core/services/ejecucion-orden.service';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import {
  EjecucionOrdenResponse,
  EjecucionOrdenCreateDto,
  EjecucionOrdenUpdateDto,
  DelegateEjecucionDto,
  TipoEjecucion,
  EjecucionOrdenFilters,
  OrdenTrabajoSimple,
  TecnicoSimple,
  VehiculoSimple
} from '../../../../core/models/ejecucion-orden.interface';
import { EjecucionesOrdenDialogsComponent } from './ejecuciones-orden-dialogs/ejecuciones-orden-dialogs.component';
import { PanelDetallesComponent } from './panel-detalles/panel-detalles.component';

@Component({
  selector: 'app-ejecuciones-orden',
  standalone: true,
  imports: [CommonModule, FormsModule, EjecucionesOrdenDialogsComponent, PanelDetallesComponent],
  templateUrl: './ejecuciones-orden.component.html',
  styleUrl: './ejecuciones-orden.component.css'
})
export class EjecucionesOrdenComponent implements OnInit {
  private readonly ejecucionService = inject(EjecucionOrdenService);
  private readonly authService = inject(SecureAuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Signals para estado reactivo
  ejecuciones = signal<EjecucionOrdenResponse[]>([]);
  ejecucionSeleccionada = signal<EjecucionOrdenResponse | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Catálogos (datos legibles para el usuario)
  ordenes = signal<OrdenTrabajoSimple[]>([]);
  tecnicos = signal<TecnicoSimple[]>([]);
  vehiculos = signal<VehiculoSimple[]>([]);
  cargandoCatalogos = signal(false);

  // Info de navegación desde Orden de Trabajo
  vieneDeOrden = signal(false);
  ordenContexto = signal<{ id: number; clienteNombre: string } | null>(null);
  ordenIdPrefill = signal(0);

  // Filtros
  filtros = signal<EjecucionOrdenFilters>({});
  filtroOrdenId = signal<number | undefined>(undefined);
  filtroTecnicoId = signal<number | undefined>(undefined);
  filtroTipo = signal<TipoEjecucion | undefined>(undefined);
  filtroEstado = signal<'EN_CURSO' | 'FINALIZADA' | undefined>(undefined);
  filtroClienteNombre = signal<string>(''); // Nuevo filtro por nombre de cliente

  // Modal states
  mostrarModalCrear = signal(false);
  mostrarModalFinalizar = signal(false);
  mostrarModalDelegar = signal(false);
  mostrarPanelDetalles = signal(false);

  // Enums para template
  readonly TipoEjecucion = TipoEjecucion;

  // Computed values
  ejecucionesFiltradas = computed(() => {
    let result = this.ejecuciones();

    const ordenId = this.filtroOrdenId();
    if (ordenId) {
      result = result.filter(e => e.ordenId === ordenId);
    }

    const tecnicoId = this.filtroTecnicoId();
    if (tecnicoId) {
      result = result.filter(e => e.tecnicoId === tecnicoId);
    }

    const tipo = this.filtroTipo();
    if (tipo) {
      result = result.filter(e => e.tipoEjecucion === tipo);
    }

    const estado = this.filtroEstado();
    if (estado) {
      if (estado === 'EN_CURSO') {
        result = result.filter(e => !e.hrFin);
      } else if (estado === 'FINALIZADA') {
        result = result.filter(e => e.hrFin);
      }
    }

    return result;
  });

  totalEjecuciones = computed(() => this.ejecucionesFiltradas().length);
  ejecucionesEnCurso = computed(() => 
    this.ejecucionesFiltradas().filter(e => !e.hrFin).length
  );
  ejecucionesFinalizadas = computed(() => 
    this.ejecucionesFiltradas().filter(e => e.hrFin).length
  );

  // Getters/Setters para compatibilidad con ngModel
  get filtroOrdenIdValue(): number | undefined {
    return this.filtroOrdenId();
  }

  set filtroOrdenIdValue(value: number | undefined) {
    this.filtroOrdenId.set(value);
  }

  get filtroTecnicoIdValue(): number | undefined {
    return this.filtroTecnicoId();
  }

  set filtroTecnicoIdValue(value: number | undefined) {
    this.filtroTecnicoId.set(value);
  }

  get filtroTipoValue(): TipoEjecucion | undefined {
    return this.filtroTipo();
  }

  set filtroTipoValue(value: TipoEjecucion | undefined) {
    this.filtroTipo.set(value);
  }

  get filtroEstadoValue(): 'EN_CURSO' | 'FINALIZADA' | undefined {
    return this.filtroEstado();
  }

  set filtroEstadoValue(value: 'EN_CURSO' | 'FINALIZADA' | undefined) {
    this.filtroEstado.set(value);
  }

  ngOnInit(): void {
    this.cargarCatalogos();
    this.cargarEjecuciones();
    this.detectarQueryParams();
  }

  /**
   * Detecta si viene desde una Orden de Trabajo con query params
   */
  detectarQueryParams(): void {
    this.route.queryParams.subscribe(params => {
      const ordenId = params['ordenId'];
      const clienteNombre = params['clienteNombre'];
      const autoOpen = params['autoOpen'] === 'true';

      console.log('🔍 Query params detectados:', { ordenId, clienteNombre, autoOpen });

      if (ordenId) {
        console.log('✅ Orden detectada, configurando contexto...');
        this.vieneDeOrden.set(true);
        this.ordenContexto.set({
          id: parseInt(ordenId),
          clienteNombre: clienteNombre || `Orden #${ordenId}`
        });

        // Pre-llenar el ordenId
        this.ordenIdPrefill.set(parseInt(ordenId));

        // Abrir modal automáticamente si viene con autoOpen
        if (autoOpen) {
          setTimeout(() => this.abrirModalCrear(), 500);
        }
      } else {
        console.log('❌ No se detectó ordenId en query params');
      }
    });
  }

  /**
   * Volver a la página de Órdenes de Trabajo
   */
  volverAOrdenes(): void {
    this.router.navigate(['/recepcion/ordenes-trabajo']);
  }

  /**
   * Carga los catálogos necesarios (órdenes, técnicos, vehículos)
   */
  cargarCatalogos(): void {
    this.cargandoCatalogos.set(true);

    // Cargar órdenes
    this.ejecucionService.getOrdenesDisponibles().subscribe({
      next: (data) => {
        this.ordenes.set(data);
      },
      error: (err) => {
        console.error('Error al cargar órdenes:', err);
      }
    });

    // Cargar técnicos
    this.ejecucionService.getTecnicosDisponibles().subscribe({
      next: (data) => {
        this.tecnicos.set(data);
      },
      error: (err) => {
        console.error('Error al cargar técnicos:', err);
      }
    });

    // Cargar vehículos
    this.ejecucionService.getVehiculosDisponibles().subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.cargandoCatalogos.set(false);
      },
      error: (err) => {
        console.error('Error al cargar vehículos:', err);
        this.cargandoCatalogos.set(false);
      }
    });
  }

  /**
   * Carga todas las ejecuciones desde el servidor
   */
  cargarEjecuciones(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.ejecucionService.getEjecuciones(this.filtros()).subscribe({
      next: (data) => {
        this.ejecuciones.set(data);
        this.cargando.set(false);
      },
      error: (err) => {
        this.error.set('Error al cargar las ejecuciones');
        console.error(err);
        this.cargando.set(false);
      }
    });
  }

  /**
   * Abre el modal para crear una nueva ejecución
   */
  abrirModalCrear(): void {
    this.mostrarModalCrear.set(true);
  }

  /**
   * Abre el modal para finalizar una ejecución
   */
  abrirModalFinalizar(ejecucion: EjecucionOrdenResponse): void {
    this.ejecucionSeleccionada.set(ejecucion);
    this.mostrarModalFinalizar.set(true);
  }

  /**
   * Abre el modal para delegar una ejecución
   */
  abrirModalDelegar(ejecucion: EjecucionOrdenResponse): void {
    this.ejecucionSeleccionada.set(ejecucion);
    this.mostrarModalDelegar.set(true);
  }

  /**
   * Ver detalles de una ejecución (abre panel lateral)
   */
  verDetalles(ejecucion: EjecucionOrdenResponse): void {
    this.ejecucionSeleccionada.set(ejecucion);
    this.mostrarPanelDetalles.set(true);
  }

  /**
   * Eliminar una ejecución
   */
  eliminarEjecucion(id: number): void {
    if (confirm('¿Está seguro de que desea eliminar esta ejecución? Esta acción no se puede deshacer.')) {
      this.ejecucionService.deleteEjecucion(id).subscribe({
        next: () => {
          this.notificationService.success('Ejecución eliminada exitosamente');
          this.cargarEjecuciones();
        },
        error: (error: any) => {
          console.error('❌ Error al eliminar ejecución:', error);
          this.notificationService.error('Error al eliminar la ejecución');
        }
      });
    }
  }

  /**
   * Aplicar filtros de búsqueda
   */
  aplicarFiltros(): void {
    const filtros: EjecucionOrdenFilters = {};

    const ordenId = this.filtroOrdenId();
    if (ordenId) filtros.ordenId = ordenId;

    const tecnicoId = this.filtroTecnicoId();
    if (tecnicoId) filtros.tecnicoId = tecnicoId;

    const tipo = this.filtroTipo();
    if (tipo) filtros.tipoEjecucion = tipo;

    const estado = this.filtroEstado();
    if (estado) filtros.estado = estado;

    this.filtros.set(filtros);
    this.cargarEjecuciones();
  }

  /**
   * Limpiar filtros
   */
  limpiarFiltros(): void {
    this.filtroOrdenId.set(undefined);
    this.filtroTecnicoId.set(undefined);
    this.filtroTipo.set(undefined);
    this.filtroEstado.set(undefined);
    this.filtros.set({});
    this.cargarEjecuciones();
  }

  /**
   * Cierra modales
   */
  cerrarModal(tipo: string): void {
    this.error.set(null);
    
    switch(tipo) {
      case 'crear':
        this.mostrarModalCrear.set(false);
        break;
      case 'finalizar':
        this.mostrarModalFinalizar.set(false);
        break;
      case 'delegar':
        this.mostrarModalDelegar.set(false);
        break;
    }
  }

  /**
   * Cierra el panel lateral de detalles
   */
  cerrarPanelDetalles(): void {
    this.mostrarPanelDetalles.set(false);
    this.ejecucionSeleccionada.set(null);
  }

  /**
   * Handlers para eventos del componente dialogs
   */
  onEjecucionCreada(): void {
    this.notificationService.success('Ejecución creada exitosamente');
    this.cargarEjecuciones();
    this.mostrarModalCrear.set(false);
  }

  onEjecucionFinalizada(): void {
    this.notificationService.success('Ejecución finalizada exitosamente');
    this.cargarEjecuciones();
    this.mostrarModalFinalizar.set(false);
  }

  onEjecucionDelegada(): void {
    this.notificationService.success('Ejecución delegada exitosamente');
    this.cargarEjecuciones();
    this.mostrarModalDelegar.set(false);
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

  /**
   * Obtiene el nombre del cliente desde la orden
   */
  getClienteNombre(ordenId: number): string {
    const orden = this.ordenes().find(o => o.id === ordenId);
    return orden?.clienteNombre || `Orden #${ordenId}`;
  }

  /**
   * Obtiene el nombre completo del técnico
   */
  getTecnicoNombre(tecnicoId: number): string {
    const tecnico = this.tecnicos().find(t => t.id === tecnicoId);
    return tecnico?.nombreCompleto || `Técnico #${tecnicoId}`;
  }

  /**
   * Obtiene la descripción del vehículo
   */
  getVehiculoDescripcion(vehiculoId?: number): string {
    if (!vehiculoId) return 'N/A';
    const vehiculo = this.vehiculos().find(v => v.id === vehiculoId);
    return vehiculo?.descripcion || `Vehículo #${vehiculoId}`;
  }

  /**
   * Formatea fecha para input datetime-local
   */
  getFormattedDateTime(fecha: Date | string): string {
    const date = typeof fecha === 'string' ? new Date(fecha) : fecha;
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  /**
   * Filtra órdenes por nombre de cliente
   */
  get ordenesFiltradas(): OrdenTrabajoSimple[] {
    const nombreFiltro = this.filtroClienteNombre().toLowerCase();
    if (!nombreFiltro) return this.ordenes();
    
    return this.ordenes().filter(orden => 
      orden.clienteNombre.toLowerCase().includes(nombreFiltro) ||
      orden.vehiculoPlacas?.toLowerCase().includes(nombreFiltro)
    );
  }

  /**
   * Formatea minutos a formato legible (ej: "2h 15m")
   */
  formatearDuracion(minutos: number): string {
    if (!minutos || minutos <= 0) return '0m';
    
    const horas = Math.floor(minutos / 60);
    const mins = minutos % 60;
    
    if (horas > 0) {
      return mins > 0 ? `${horas}h ${mins}m` : `${horas}h`;
    }
    return `${mins}m`;
  }
}

