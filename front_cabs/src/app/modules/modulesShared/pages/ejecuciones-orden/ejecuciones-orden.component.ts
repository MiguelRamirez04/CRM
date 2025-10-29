import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
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

        // Pre-llenar el formulario
        const formActual = this.formularioCrear();
        console.log('📝 Formulario actual antes de actualizar:', formActual);
        formActual.ordenId = parseInt(ordenId);
        this.formularioCrear.set({ ...formActual });
        console.log('📝 Formulario actualizado:', this.formularioCrear());

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
    // Preservar el ordenId si viene de query params
    const ordenIdActual = this.formularioCrear().ordenId || 0;

    this.formularioCrear.set({
      ordenId: ordenIdActual, // Mantener el ordenId si ya está establecido
      tecnicoId: 0,
      tipoEjecucion: TipoEjecucion.CAMPO,
      hrInicio: new Date().toISOString(),
      comentarios: ''
    });
    this.mostrarModalCrear.set(true);

    console.log('📋 Modal abierto con formulario:', this.formularioCrear());
  }

  /**
   * Crea una nueva ejecución
   */
  crearEjecucion(): void {
    const dto = this.formularioCrear();

    console.log('📤 Intentando crear ejecución con DTO:', dto);
    console.log('📋 Contexto de orden:', this.ordenContexto());
    console.log('📋 Viene de orden:', this.vieneDeOrden());

    if (!this.validarFormularioCrear(dto)) {
      console.warn('⚠️ Validación fallida:', this.error());
      return;
    }    this.cargando.set(true);
    this.error.set(null);

    this.ejecucionService.createEjecucion(dto).subscribe({
      next: (response) => {
        console.log('✅ Ejecución creada exitosamente:', response);
        this.mostrarModalCrear.set(false);
        this.cargarEjecuciones();
        this.cargando.set(false);
      },
      error: (err) => {
        console.error('❌ Error al crear ejecución:', err);
        
        // Mensajes de error más específicos
        let mensajeError = 'Error al crear la ejecución';
        if (err.status === 400) {
          mensajeError = err.error?.message || 'Datos inválidos. Verifique los campos del formulario.';
        } else if (err.status === 401) {
          mensajeError = 'No autorizado. Por favor, inicie sesión nuevamente.';
        } else if (err.status === 404) {
          mensajeError = 'Recurso no encontrado. Verifique que la orden, técnico o vehículo existan.';
        } else if (err.status === 500) {
          mensajeError = 'Error interno del servidor. Por favor, contacte al administrador.';
        } else if (err.error?.message) {
          mensajeError = err.error.message;
        }
        
        this.error.set(mensajeError);
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
    console.log('🔍 Validando formulario:', dto);

    // El ordenId debe venir del contexto (parámetro de navegación)
    if (!dto.ordenId) {
      console.error('❌ Error: No se ha especificado una orden de trabajo. Por favor, cree la ejecución desde la página de órdenes.');
      this.error.set('No se ha especificado una orden de trabajo. Por favor, cree la ejecución desde la página de órdenes.');
      return false;
    }

    if (!dto.tecnicoId) {
      console.error('❌ Error: Debe seleccionar un técnico');
      this.error.set('Debe seleccionar un técnico');
      return false;
    }

    // Validaciones más flexibles: vehículo y kilometraje son opcionales para ambos tipos
    if (dto.vehiculoId && dto.kmInicial !== undefined && dto.kmInicial !== null && dto.kmInicial < 0) {
      console.error('❌ Error: El kilometraje inicial debe ser mayor o igual a 0');
      this.error.set('El kilometraje inicial debe ser mayor o igual a 0');
      return false;
    }

    console.log('✅ Validación exitosa');
    this.error.set('');
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
   * Actualiza la hora de inicio del formulario
   */
  actualizarHrInicio(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.formularioCrear.update(form => ({
      ...form,
      hrInicio: new Date(input.value).toISOString()
    }));
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

