import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EjecucionOrdenService } from '../../services/ejecucion-orden.service';
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
} from '../../interfaces/ejecucion-orden.interface';

@Component({
  selector: 'app-ejecuciones-orden',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ejecuciones-orden.component.html',
  styleUrl: './ejecuciones-orden.component.css'
})
export class EjecucionesOrdenComponent implements OnInit {
  private readonly ejecucionService = inject(EjecucionOrdenService);

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

  // Filtros
  filtros = signal<EjecucionOrdenFilters>({});
  filtroOrdenId = signal<number | undefined>(undefined);
  filtroTecnicoId = signal<number | undefined>(undefined);
  filtroTipo = signal<TipoEjecucion | undefined>(undefined);
  filtroClienteNombre = signal<string>(''); // Nuevo filtro por nombre de cliente

  // Modal states
  mostrarModalCrear = signal(false);
  mostrarModalFinalizar = signal(false);
  mostrarModalDelegar = signal(false);

  // Formularios
  formularioCrear = signal<EjecucionOrdenCreateDto>({
    ordenId: 0,
    tecnicoId: 0,
    tipoEjecucion: TipoEjecucion.CAMPO,
    hrInicio: new Date().toISOString(),
    comentarios: ''
  });

  formularioFinalizar = signal<EjecucionOrdenUpdateDto>({
    hrFin: new Date().toISOString(),
    comentarios: ''
  });

  formularioDelegacion = signal<DelegateEjecucionDto>({
    nuevoTecnicoId: 0,
    motivo: ''
  });

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

    return result;
  });

  totalEjecuciones = computed(() => this.ejecucionesFiltradas().length);
  ejecucionesEnCurso = computed(() => 
    this.ejecucionesFiltradas().filter(e => !e.hrFin).length
  );
  ejecucionesFinalizadas = computed(() => 
    this.ejecucionesFiltradas().filter(e => e.hrFin).length
  );

  ngOnInit(): void {
    this.cargarCatalogos();
    this.cargarEjecuciones();
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
    this.formularioCrear.set({
      ordenId: 0,
      tecnicoId: 0,
      tipoEjecucion: TipoEjecucion.CAMPO,
      hrInicio: new Date().toISOString(),
      comentarios: ''
    });
    this.mostrarModalCrear.set(true);
  }

  /**
   * Crea una nueva ejecución
   */
  crearEjecucion(): void {
    const dto = this.formularioCrear();
    
    if (!this.validarFormularioCrear(dto)) {
      return;
    }

    this.cargando.set(true);

    this.ejecucionService.createEjecucion(dto).subscribe({
      next: (response) => {
        console.log('✅ Ejecución creada exitosamente:', response);
        this.mostrarModalCrear.set(false);
        this.cargarEjecuciones();
      },
      error: (err) => {
        this.error.set('Error al crear la ejecución');
        console.error(err);
        this.cargando.set(false);
      }
    });
  }

  /**
   * Abre el modal para finalizar una ejecución
   */
  abrirModalFinalizar(ejecucion: EjecucionOrdenResponse): void {
    this.ejecucionSeleccionada.set(ejecucion);
    this.formularioFinalizar.set({
      hrFin: new Date().toISOString(),
      kmFinal: ejecucion.tipoEjecucion === TipoEjecucion.CAMPO ? ejecucion.kmInicial : undefined,
      comentarios: ''
    });
    this.mostrarModalFinalizar.set(true);
  }

  /**
   * Finaliza una ejecución
   */
  finalizarEjecucion(): void {
    const ejecucion = this.ejecucionSeleccionada();
    if (!ejecucion) return;

    const dto = this.formularioFinalizar();
    this.cargando.set(true);

    this.ejecucionService.updateEjecucion(ejecucion.id, dto).subscribe({
      next: () => {
        console.log('✅ Ejecución finalizada exitosamente');
        this.mostrarModalFinalizar.set(false);
        this.cargarEjecuciones();
      },
      error: (err) => {
        this.error.set('Error al finalizar la ejecución');
        console.error(err);
        this.cargando.set(false);
      }
    });
  }

  /**
   * Abre el modal para delegar una ejecución
   */
  abrirModalDelegar(ejecucion: EjecucionOrdenResponse): void {
    this.ejecucionSeleccionada.set(ejecucion);
    this.formularioDelegacion.set({
      nuevoTecnicoId: 0,
      motivo: ''
    });
    this.mostrarModalDelegar.set(true);
  }

  /**
   * Delega una ejecución a otro técnico
   */
  delegarEjecucion(): void {
    const ejecucion = this.ejecucionSeleccionada();
    if (!ejecucion) return;

    const dto = this.formularioDelegacion();
    if (!dto.nuevoTecnicoId || !dto.motivo) {
      this.error.set('Complete todos los campos requeridos');
      return;
    }

    this.cargando.set(true);

    this.ejecucionService.delegateEjecucion(ejecucion.id, dto).subscribe({
      next: () => {
        console.log('✅ Ejecución delegada exitosamente');
        this.mostrarModalDelegar.set(false);
        this.cargarEjecuciones();
      },
      error: (err) => {
        this.error.set('Error al delegar la ejecución');
        console.error(err);
        this.cargando.set(false);
      }
    });
  }

  /**
   * Cambia el tipo de ejecución y resetea campos específicos
   */
  cambiarTipoEjecucion(tipo: TipoEjecucion): void {
    const form = this.formularioCrear();
    
    if (tipo === TipoEjecucion.CAMPO) {
      // Resetear campos de REMOTO
      form.herramientas = undefined;
      form.codigoSesion = undefined;
      form.contrasenaSesion = undefined;
    } else {
      // Resetear campos de CAMPO
      form.vehiculoId = undefined;
      form.kmInicial = undefined;
    }

    form.tipoEjecucion = tipo;
    this.formularioCrear.set({ ...form });
  }

  /**
   * Valida el formulario de creación según el tipo de ejecución
   */
  private validarFormularioCrear(dto: EjecucionOrdenCreateDto): boolean {
    if (!dto.ordenId || !dto.tecnicoId) {
      this.error.set('Orden y Técnico son obligatorios');
      return false;
    }

    if (dto.tipoEjecucion === TipoEjecucion.CAMPO) {
      if (!dto.vehiculoId || !dto.kmInicial) {
        this.error.set('Vehículo y Km Inicial son obligatorios para ejecución en campo');
        return false;
      }
    }

    return true;
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
    this.filtros.set({});
    this.cargarEjecuciones();
  }

  /**
   * Cerrar modales
   */
  cerrarModal(tipo: 'crear' | 'finalizar' | 'delegar'): void {
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
}

