import { Component, OnInit, AfterViewInit, signal, computed, inject, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { VehiculoService } from '../../../../core/services/vehiculo.service';
import { Vehiculo, VehiculoCreateDto, VehiculoUpdateDto } from '../../../../core/models/vehiculo.interface';

// Componentes reutilizables
import { TablaListadoComponent, ConfiguracionColumna, AccionTabla } from '../../../../shared/molecules/tabla-base/tabla-listado.component';
import { StatusDotComponent } from '../../../../shared/atoms/status-dot/status-dot.component';
import { BuscadorFiltroComponent, ConfiguracionBuscador } from '../../../../shared/components/buscador-filtro/buscador-filtro.component';
import { PaginacionComponent, ConfiguracionPaginacion } from '../../../../shared/components/paginacion/paginacion.component';
import { ModalFiltrosComponent, ConfiguracionModalFiltros, ResultadoFiltros, GrupoFiltroCheckbox, FiltroSelect } from '../../../../shared/components/modal-filtros/modal-filtros.component';

// Nuevos componentes reutilizables
import { UitipografiaComponent } from '../../../../shared/atoms/tipografia/tipografia.component';
import { UiBotonComponent } from '../../../../shared/atoms/boton/boton.component';
import { SidePanelComponent } from '../../../../shared/organisms/side-panel/side-panel.component';
import { DetailSectionComponent } from '../../../../shared/molecules/detail-section/detail-section.component';
import { DetailFieldComponent } from '../../../../shared/molecules/detail-field/detail-field.component';
import { LoadingSpinnerComponent } from '../../../../shared/atoms/loading-spinner/loading-spinner.component';
import { AlertComponent } from '../../../../shared/molecules/alert/alert.component';
import { BadgeComponent } from '../../../../shared/atoms/bage/badge.component';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';

@Component({
  selector: 'app-vehiculos',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    // Componentes de tabla y navegación
    TablaListadoComponent,
    StatusDotComponent,
    BuscadorFiltroComponent,
    PaginacionComponent,
    ModalFiltrosComponent,
    // Componentes UI reutilizables
    UitipografiaComponent,
    UiBotonComponent,
    UiHeaderComponent,
    SidePanelComponent,
    DetailSectionComponent,
    DetailFieldComponent,
    LoadingSpinnerComponent,
    AlertComponent,
    BadgeComponent
  ],
  templateUrl: './vehiculos.component.html',
  styleUrls: ['./vehiculos.component.css']
})
export class VehiculosComponent implements OnInit, AfterViewInit {
  private readonly vehiculoService = inject(VehiculoService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  // Templates para columnas personalizadas
  @ViewChild('transmisionTemplate') transmisionTemplate!: TemplateRef<any>;
  @ViewChild('kilometrajeTemplate') kilometrajeTemplate!: TemplateRef<any>;
  @ViewChild('estadoTemplate') estadoTemplate!: TemplateRef<any>;

  // Signals para estado reactivo
  vehiculos = signal<Vehiculo[]>([]);
  vehiculosFiltrados = signal<Vehiculo[]>([]);
  vehiculosPaginados = signal<Vehiculo[]>([]);
  vehiculoSeleccionado = signal<Vehiculo | null>(null);
  vehiculoParaEditar = signal<Vehiculo | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Filtros
  filtroTermino = signal<string>('');
  filtrosActivos = signal<ResultadoFiltros | null>(null);
  
  // Paginación
  paginaActual = signal<number>(1);
  elementosPorPagina = signal<number>(10);

  // Computed values para estadísticas
  vehiculosActivos = computed(() => this.vehiculos().filter(v => v.activo).length);
  totalElementos = computed(() => this.vehiculosFiltrados().length);

  // Modal states
  mostrarModal = signal(false);
  modoModal = signal<'crear' | 'editar'>('crear');
  mostrarPanelDetalles = signal(false);
  mostrarModalFiltros = signal(false);

  // Formulario Reactivo
  formularioVehiculo: FormGroup;

  // Configuración de componentes reutilizables
  configuracionBuscador: ConfiguracionBuscador = {
    placeholderBusqueda: 'Buscar por placas, nombre o tipo...',
    mostrarBotonFiltro: true,
    mostrarBotonNuevo: true,
    textoBotonNuevo: 'Nuevo Vehículo',
    tiempoEsperaBusqueda: 300
  };

  configuracionPaginacion: ConfiguracionPaginacion = {
    elementosPorPagina: 10,
    paginasVisiblesMaximas: 6,
    textoAnterior: 'Anterior',
    textoSiguiente: 'Siguiente',
    textoMostrandoRegistros: 'Mostrando',
    textoDeRegistros: 'de',
    mostrarInfoRegistros: true,
    mostrarBotonesPagina: true
  };

  configuracionModalFiltros: ConfiguracionModalFiltros = {
    titulo: 'Filtros de Vehículos',
    mostrarBotonLimpiar: true,
    textoBotonAplicar: 'Aplicar Filtros',
    textoBotonLimpiar: 'Limpiar',
    textoBotonCerrar: 'Cerrar',
    gruposCheckbox: [],
    filtrosSelect: []
  };

  // Configuración de la tabla
  columnasTabla: ConfiguracionColumna<Vehiculo>[] = [];
  accionesTabla: AccionTabla<Vehiculo>[] = [];

  constructor() {
    // Inicializa el formulario con los nuevos campos
    this.formularioVehiculo = this.fb.group({
      nombreVehiculo: ['', [Validators.required, Validators.maxLength(100)]],
      placas: ['', [Validators.required, Validators.maxLength(20)]],
      tipoVehiculo: ['', [Validators.required, Validators.maxLength(50)]],
      transmision: ['', Validators.required],
      esDeEmpresa: [true, Validators.required],
      activo: [true, Validators.required],
      kilometraje: [null, [Validators.required, Validators.min(0)]],
      observaciones: ['']
    });
  }

  ngOnInit(): void {
    this.configurarModalFiltros();
    this.cargarVehiculos();
  }

  ngAfterViewInit(): void {
    // Configurar la tabla después de que la vista esté inicializada
    // para poder acceder a los templates
    setTimeout(() => this.configurarTabla());
  }

  configurarModalFiltros(): void {
    // Configurar grupos de checkboxes para el modal de filtros
    const grupoTransmision: GrupoFiltroCheckbox = {
      id: 'transmision',
      titulo: 'Transmisión',
      opciones: [
        { valor: 'Manual', etiqueta: 'Manual', descripcion: 'Vehículos con transmisión manual' },
        { valor: 'Automática', etiqueta: 'Automática', descripcion: 'Vehículos con transmisión automática' }
      ]
    };

    const grupoEstado: GrupoFiltroCheckbox = {
      id: 'estado',
      titulo: 'Estado',
      opciones: [
        { valor: true, etiqueta: 'Activos', descripcion: 'Vehículos activos' },
        { valor: false, etiqueta: 'Inactivos', descripcion: 'Vehículos inactivos' }
      ]
    };

    const grupoPropiedad: GrupoFiltroCheckbox = {
      id: 'propiedad',
      titulo: 'Propiedad',
      opciones: [
        { valor: true, etiqueta: 'De la Empresa', descripcion: 'Vehículos propiedad de la empresa' },
        { valor: false, etiqueta: 'Externos', descripcion: 'Vehículos externos o alquilados' }
      ]
    };

    const filtroTipoVehiculo: FiltroSelect = {
      id: 'tipoVehiculo',
      titulo: 'Tipo de Vehículo',
      placeholder: 'Seleccione un tipo',
      opciones: [
        { valor: 'Sedán', etiqueta: 'Sedán' },
        { valor: 'SUV', etiqueta: 'SUV' },
        { valor: 'Pickup', etiqueta: 'Pickup' },
        { valor: 'Hatchback', etiqueta: 'Hatchback' },
        { valor: 'Van', etiqueta: 'Van' }
      ]
    };

    this.configuracionModalFiltros = {
      titulo: 'Filtros de Vehículos',
      mostrarBotonLimpiar: true,
      textoBotonAplicar: 'Aplicar Filtros',
      textoBotonLimpiar: 'Limpiar Todo',
      textoBotonCerrar: 'Cerrar',
      gruposCheckbox: [grupoTransmision, grupoEstado, grupoPropiedad],
      filtrosSelect: [filtroTipoVehiculo]
    };
  }

  configurarTabla(): void {
    // Configuración de columnas
    this.columnasTabla = [
      {
        encabezado: 'Vehículo',
        campo: 'nombreVehiculo',
        ancho: '15%',
        alineacion: 'left'
      },
      {
        encabezado: 'Placas',
        campo: 'placas',
        ancho: '15%',
        alineacion: 'center'
      },
      {
        encabezado: 'Transmisión',
        campo: 'transmision',
        plantilla: this.transmisionTemplate,
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Kilometraje',
        campo: 'kilometraje',
        plantilla: this.kilometrajeTemplate,
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Estado',
        campo: 'activo',
        plantilla: this.estadoTemplate,
        ancho: '10%',
        alineacion: 'center'
      }
    ];

    // Configuración de acciones
    this.accionesTabla = [
      {
        etiqueta: 'Ver',
        icono: `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
        </svg>`,
        variante: 'secundario',
        accion: (vehiculo: Vehiculo) => this.verDetalles(vehiculo)
      },
      {
        etiqueta: 'Historial',
        icono: `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>`,
        variante: 'anil-suave',
        accion: (vehiculo: Vehiculo) => this.verHistorial(vehiculo)
      },
      {
        etiqueta: 'Editar',
        icono: `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
        </svg>`,
        variante: 'primario',
        accion: (vehiculo: Vehiculo) => this.abrirModalEditar(vehiculo)
      },
      {
        etiqueta: 'Eliminar',
        icono: `<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
        </svg>`,
        variante: 'eliminar',
        accion: (vehiculo: Vehiculo) => this.onEliminar(vehiculo)
      }
    ];
  }

  cargarVehiculos(): void {
    this.cargando.set(true);
    this.error.set(null);
    const filtros: { [key: string]: string } = {};
    
    // No aplicar el filtro de término aquí, lo manejamos en el cliente
    this.vehiculoService.getVehiculos(filtros).subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.aplicarFiltrosYBusqueda();
        this.cargando.set(false);
      },
      error: (err) => this.handleError('Error al cargar los vehículos.', err),
    });
  }

  // Métodos del buscador
  onCambioBusqueda(termino: string): void {
    this.filtroTermino.set(termino);
    this.aplicarFiltrosYBusqueda();
  }

  onAbrirFiltros(): void {
    this.mostrarModalFiltros.set(true);
  }

  // Métodos del modal de filtros
  onCerrarModalFiltros(): void {
    this.mostrarModalFiltros.set(false);
  }

  onAplicarFiltros(filtros: ResultadoFiltros): void {
    console.log('Filtros aplicados:', filtros);
    this.filtrosActivos.set(filtros);
    this.aplicarFiltrosYBusqueda();
    this.mostrarModalFiltros.set(false);
  }

  onLimpiarFiltros(): void {
    console.log('Limpiando filtros');
    this.filtrosActivos.set(null);
    this.aplicarFiltrosYBusqueda();
  }

  // Método central para aplicar búsqueda y filtros
  aplicarFiltrosYBusqueda(): void {
    let vehiculosFiltrados = [...this.vehiculos()];

    // Aplicar búsqueda por término
    const termino = this.filtroTermino().toLowerCase().trim();
    if (termino) {
      vehiculosFiltrados = vehiculosFiltrados.filter(vehiculo => 
        vehiculo.nombreVehiculo?.toLowerCase().includes(termino) ||
        vehiculo.placas?.toLowerCase().includes(termino) ||
        vehiculo.tipoVehiculo?.toLowerCase().includes(termino)
      );
    }

    // Aplicar filtros del modal
    const filtros = this.filtrosActivos();
    if (filtros) {
      // Filtro de transmisión
      if (filtros.checkboxes['transmision']?.length > 0) {
        vehiculosFiltrados = vehiculosFiltrados.filter(v => 
          filtros.checkboxes['transmision'].includes(v.transmision)
        );
      }

      // Filtro de estado
      if (filtros.checkboxes['estado']?.length > 0) {
        vehiculosFiltrados = vehiculosFiltrados.filter(v => 
          filtros.checkboxes['estado'].includes(v.activo)
        );
      }

      // Filtro de propiedad
      if (filtros.checkboxes['propiedad']?.length > 0) {
        vehiculosFiltrados = vehiculosFiltrados.filter(v => 
          filtros.checkboxes['propiedad'].includes(v.esDeEmpresa)
        );
      }

      // Filtro de tipo de vehículo
      if (filtros.selects['tipoVehiculo'] && filtros.selects['tipoVehiculo'] !== '') {
        vehiculosFiltrados = vehiculosFiltrados.filter(v => 
          v.tipoVehiculo === filtros.selects['tipoVehiculo']
        );
      }
    }

    this.vehiculosFiltrados.set(vehiculosFiltrados);
    
    // Resetear a la primera página cuando cambian los filtros
    this.paginaActual.set(1);
    this.aplicarPaginacion();
  }

  // Métodos de paginación
  onCambioPagina(pagina: number): void {
    this.paginaActual.set(pagina);
    this.aplicarPaginacion();
  }

  aplicarPaginacion(): void {
    const inicio = (this.paginaActual() - 1) * this.elementosPorPagina();
    const fin = inicio + this.elementosPorPagina();
    const vehiculosPaginados = this.vehiculosFiltrados().slice(inicio, fin);
    this.vehiculosPaginados.set(vehiculosPaginados);
  }

  abrirModalCrear(): void {
    // Cerrar panel de detalles si está abierto
    this.cerrarPanelDetalles();
    
    // Abrir modal inmediatamente con indicador de carga
    this.modoModal.set('crear');
    this.vehiculoParaEditar.set(null);
    this.mostrarModal.set(true);
    this.cargando.set(true);
    this.error.set(null);

    // Usar setTimeout para permitir que el modal se renderice primero
    setTimeout(() => {
      // Configurar validaciones para crear (todos los campos requeridos)
      this.formularioVehiculo.get('nombreVehiculo')?.setValidators([Validators.required, Validators.maxLength(100)]);
      this.formularioVehiculo.get('placas')?.setValidators([Validators.required, Validators.maxLength(20)]);
      this.formularioVehiculo.get('tipoVehiculo')?.setValidators([Validators.required, Validators.maxLength(50)]);
      this.formularioVehiculo.get('transmision')?.setValidators([Validators.required]);
      this.formularioVehiculo.get('esDeEmpresa')?.setValidators([Validators.required]);
      this.formularioVehiculo.get('activo')?.setValidators([Validators.required]);
      this.formularioVehiculo.get('kilometraje')?.setValidators([Validators.required, Validators.min(0)]);

      // Actualizar validaciones
      Object.keys(this.formularioVehiculo.controls).forEach(key => {
        this.formularioVehiculo.get(key)?.updateValueAndValidity();
      });

      this.formularioVehiculo.reset({
        nombreVehiculo: '',
        placas: '',
        tipoVehiculo: '',
        transmision: '',
        esDeEmpresa: true,
        activo: true,
        kilometraje: null,
        observaciones: ''
      });
      
      this.cargando.set(false);
    }, 0);
  }

  abrirModalEditar(vehiculo: Vehiculo): void {
    // Cerrar panel de detalles si está abierto
    this.cerrarPanelDetalles();
    
    // Configurar validaciones para editar (kilometraje requerido, activo opcional)
    this.formularioVehiculo.get('nombreVehiculo')?.clearValidators();
    this.formularioVehiculo.get('placas')?.clearValidators();
    this.formularioVehiculo.get('tipoVehiculo')?.clearValidators();
    this.formularioVehiculo.get('transmision')?.clearValidators();
    this.formularioVehiculo.get('esDeEmpresa')?.clearValidators();
    // El campo 'activo' se mantiene opcional para edición
    this.formularioVehiculo.get('kilometraje')?.setValidators([Validators.required, Validators.min(0)]);

    // Actualizar validaciones
    Object.keys(this.formularioVehiculo.controls).forEach(key => {
      this.formularioVehiculo.get(key)?.updateValueAndValidity();
    });

    this.formularioVehiculo.patchValue(vehiculo);
    this.modoModal.set('editar');
    this.vehiculoParaEditar.set(vehiculo);
    this.mostrarModal.set(true);
    this.error.set(null);
  }

  cerrarModal(event?: string): void {
    this.mostrarModal.set(false);
    this.vehiculoParaEditar.set(null);
    this.modoModal.set('crear');
    this.error.set(null);
  }

  guardarVehiculo(): void {
    if (this.formularioVehiculo.invalid) {
      this.formularioVehiculo.markAllAsTouched();
      this.error.set('Por favor, completa los campos requeridos.');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);

    if (this.modoModal() === 'crear') {
      // Para crear, envía todos los campos
      const dto = this.formularioVehiculo.value as VehiculoCreateDto;
      this.vehiculoService.createVehiculo(dto).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cerrarModal();
          this.cargarVehiculos();
        },
        error: (err) => this.handleError('Error al guardar el vehículo.', err, false),
      });
    } else {
      // Para editar, solo envía los campos permitidos: kilometraje (obligatorio), placas, observaciones y activo (opcionales)
      const formValue = this.formularioVehiculo.value;
      const dto: VehiculoUpdateDto = {
        kilometraje: formValue.kilometraje,
        placas: formValue.placas,
        observaciones: formValue.observaciones,
        activo: formValue.activo
      };

      this.vehiculoService.updateVehiculo(this.vehiculoParaEditar()!.id, dto).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cerrarModal();
          this.cargarVehiculos();
        },
        error: (err) => this.handleError('Error al guardar el vehículo.', err, false),
      });
    }
  }

  onEliminar(vehiculo: Vehiculo): void {
    if (confirm(`¿Estás seguro de eliminar el vehículo ${vehiculo.placas}?`)) {
      this.cargando.set(true);
      this.vehiculoService.deleteVehiculo(vehiculo.id).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cargarVehiculos();
          if (this.vehiculoParaEditar()?.id === vehiculo.id) {
            this.cerrarModal();
          }
          if (this.vehiculoSeleccionado()?.id === vehiculo.id) {
            this.cerrarPanelDetalles();
          }
        },
        error: (err) => this.handleError('Error al eliminar el vehículo.', err),
      });
    }
  }

  /**
   * Handlers para eventos del componente dialogs
   */
  onVehiculoCreado(): void {
    this.cargarVehiculos();
    this.mostrarModal.set(false);
    this.vehiculoParaEditar.set(null);
    this.modoModal.set('crear');
  }

  onVehiculoActualizado(): void {
    this.cargarVehiculos();
    this.mostrarModal.set(false);
    this.vehiculoParaEditar.set(null);
    this.modoModal.set('crear');
  }

  /**
   * Métodos para el panel lateral de detalles
   */
  seleccionarVehiculo(vehiculo: Vehiculo): void {
    this.vehiculoSeleccionado.set(vehiculo);
  }

// 1. Agregar signal para controlar la carga de detalles
cargandoDetalles = signal(false);

// 2. REEMPLAZAR el método verDetalles() existente con este:
verDetalles(vehiculo: Vehiculo): void {
  // ABRIR EL PANEL INMEDIATAMENTE
  this.vehiculoSeleccionado.set(vehiculo); // Establecer el vehículo básico primero
  this.mostrarPanelDetalles.set(true);     // Abrir el panel
  this.cargandoDetalles.set(true);         // Activar spinner
  this.error.set(null);                     // Limpiar errores previos

  // LUEGO CARGAR LOS DATOS COMPLETOS
  this.vehiculoService.getVehiculoById(vehiculo.id).subscribe({
    next: (vehiculoCompleto) => {
      this.vehiculoSeleccionado.set(vehiculoCompleto);
      this.cargandoDetalles.set(false);
    },
    error: (err) => {
      console.error('Error al obtener detalles del vehículo:', err);
      this.error.set('Error al cargar los detalles del vehículo');
      this.cargandoDetalles.set(false);
      // Opcional: cerrar el panel si falla la carga
      // this.cerrarPanelDetalles();
    }
  });
}

// 3. ACTUALIZAR el método cerrarPanelDetalles() para limpiar el estado de carga:
cerrarPanelDetalles(): void {
  this.mostrarPanelDetalles.set(false);
  this.vehiculoSeleccionado.set(null);
  this.cargandoDetalles.set(false); 
  this.error.set(null);              
}

  verHistorial(vehiculo: Vehiculo): void {
    this.router.navigate(['historial', vehiculo.id], { relativeTo: this.route });
  }

  // Helper para mostrar errores en el formulario
  campoInvalido(campo: string): boolean {
    const control = this.formularioVehiculo.get(campo);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  // Helpers para formatear datos en la tabla
  formatearKilometraje(vehiculo: Vehiculo): string {
    return vehiculo.kilometraje ? vehiculo.kilometraje.toLocaleString() + ' km' : 'N/A';
  }

  obtenerTipoEstado(vehiculo: Vehiculo): 'completado' | 'rechazado' {
    return vehiculo.activo ? 'completado' : 'rechazado';
  }

  obtenerTextoEstado(vehiculo: Vehiculo): string {
    return vehiculo.activo ? 'Activo' : 'Inactivo';
  }

  obtenerTipoTransmision(vehiculo: Vehiculo): 'personalizado' | 'neutral' {
    return vehiculo.transmision?.toLowerCase() === 'automatica' ? 'personalizado' : 'neutral';
  }

  /**
   * Manejador de errores
   * @param message Mensaje amigable para el usuario
   * @param error Error original
   * @param setGlobalError Si es 'true', muestra el error en la página. Si es 'false', lo prepara para el modal.
   */
  private handleError(message: string, error: any, setGlobalError: boolean = true): void {
    const apiError = error?.error?.message || error?.error || error?.message || 'Error desconocido';
    const fullMessage = `${message} Detalles: ${apiError}`;
    
    // Si el modal está abierto, siempre muestra el error ahí.
    if (this.mostrarModal()) {
      this.error.set(fullMessage);
    } else {
      // Si el modal está cerrado, usa el parámetro global
      if (setGlobalError) {
        this.error.set(fullMessage);
      }
    }
    
    this.cargando.set(false);
    console.error(message, error);
  }
}