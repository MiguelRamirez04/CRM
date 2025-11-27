import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { environment } from '../../../../../../environments/environment';

// Servicios e interfaces
import { EvaluacionService } from '../../../../../core/services/evaluaciones.service';
import {
  EvaluacionResponse,
  UsuarioResponseDto,
  EvaluacionResponseExtendida,
  mapResponseToFormulario,
  mapDetalleResponseToDatos
} from '../../../../../core/models/evaluaciones.interface';
import { SharedEvaluacionService } from '../../../../../core/services/shared-evaluacion.service';

// Componentes compartidos
import { BuscadorFiltroComponent } from '../../../../../shared/components/buscador-filtro/buscador-filtro.component';
import { TablaListadoComponent, ConfiguracionColumna, AccionTabla } from '../../../../../shared/molecules/tabla-base/tabla-listado.component';
import { PaginacionComponent } from '../../../../../shared/components/paginacion/paginacion.component';
import { StatusDotComponent } from '../../../../../shared/atoms/status-dot/status-dot.component';
import { UiHeaderComponent } from '../../../../../shared/molecules/header/header.component';

// Componente modal de filtros
import {
  ModalFiltrosComponent,
  ConfiguracionModalFiltros,
  ResultadoFiltros
} from '../../../../../shared/components/modal-filtros/modal-filtros.component';


import { VerdetallesComponent } from '../ver_detalles/verdetalles.component';


import { InfogeneralComponent } from '../registro/infogeneral/infogeneralregistro.component';


import { UiBotonComponent } from '../../../../../shared/atoms/boton/boton.component';

@Component({
  selector: 'app-evaluaciones',
  standalone: true,
  imports: [
    CommonModule,
    BuscadorFiltroComponent,
    TablaListadoComponent,
    PaginacionComponent,
    StatusDotComponent,
    ModalFiltrosComponent,
    VerdetallesComponent,
    UiHeaderComponent,
    InfogeneralComponent,
    UiBotonComponent  
  ],
  templateUrl: './evaluaciones.component.html',
  styleUrls: ['./evaluaciones.component.css']
})
export class EvaluacionesComponent implements OnInit {
  // =====================================================================================
  // PROPIEDADES EXISTENTES
  // =====================================================================================

  searchTerm: string = '';
  paginaActual: number = 1;
  elementosPorPagina: number = 9;

  evaluacionesOriginales: EvaluacionResponseExtendida[] = [];
  evaluacionesFiltradas: EvaluacionResponseExtendida[] = [];
  evaluaciones: EvaluacionResponseExtendida[] = [];

  cargando: boolean = false;
  error: string = '';

  // Referencias a templates
  @ViewChild('plantillaObjetivo', { static: true }) plantillaObjetivo!: TemplateRef<any>;
  @ViewChild('plantillaSeguimiento', { static: true }) plantillaSeguimiento!: TemplateRef<any>;
  @ViewChild('plantillaScore', { static: true }) plantillaScore!: TemplateRef<any>;
  @ViewChild('plantillaFecha', { static: true }) plantillaFecha!: TemplateRef<any>;
  @ViewChild('iconoExito', { static: true }) iconoExito!: TemplateRef<any>;
  @ViewChild('iconoAlerta', { static: true }) iconoAlerta!: TemplateRef<any>;

  // ViewChild para acceder al componente hijo
  @ViewChild(InfogeneralComponent) infogeneralComponent!: InfogeneralComponent;

  columnas: ConfiguracionColumna<EvaluacionResponseExtendida>[] = [];
  acciones: AccionTabla<EvaluacionResponseExtendida>[] = [];

  // Iconos como strings de SVG
  private readonly iconoVerSVG = `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z" />
      <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
    </svg>
  `;

  private readonly iconoEditarSVG = `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
    </svg>
  `;

  // ICONOS SVG PARA LOS BOTONES UI
  iconoInfo = `
    <svg class="w-full h-full" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
        d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z" />
    </svg>
  `;

  iconoCheck = `
    <svg class="w-full h-full" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
        d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  `;

  iconoEditar = `
    <svg class="w-full h-full" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
        d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
    </svg>
  `;

  iconoCerrar = `
    <svg class="w-full h-full" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
        d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  `;

  // =====================================================================================
  // PROPIEDADES PARA FILTROS
  // =====================================================================================

  mostrarModalFiltros: boolean = false;
  configuracionFiltros: ConfiguracionModalFiltros = {};
  filtrosAplicados?: ResultadoFiltros;

  // =====================================================================================
  // PROPIEDADES PARA PANEL DE DETALLES
  // =====================================================================================

  mostrarPanelDetalles: boolean = false;
  evaluacionIdSeleccionada: number = 0;

  // =====================================================================================
  // PROPIEDADES PARA MODAL DE REGISTRO
  // =====================================================================================

  mostrarModalRegistro: boolean = false;
  evaluacionIdParaEditar: number | null = null;
  modoModal: 'crear' | 'editar' = 'crear';

  // =====================================================================================
  // PROPIEDADES PARA EL FOOTER DEL MODAL
  // =====================================================================================

  vistaActualModal: 'infoGeneral' | 'faseAntes' | 'faseDespues' = 'infoGeneral';
  estadoFaseAntes: 'completada' | 'sin-completar' | 'sin-inicializar' = 'sin-inicializar';
  estadoFaseDespues: 'completada' | 'sin-completar' | 'sin-inicializar' = 'sin-inicializar';
  guardandoModal: boolean = false;

  get badgeEstadoModal(): { texto: string; clase: string } {
    return this.modoModal === 'editar'
      ? { texto: 'EDITANDO', clase: 'badge-editar' }
      : { texto: 'NUEVA', clase: 'badge-crear' };
  }

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private http: HttpClient
  ) { }

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    console.log('Limpiando servicio compartido al entrar al listado');
    this.sharedService.limpiar();

    this.configurarTabla();
    this.configurarFiltros();
    this.cargarEvaluaciones();

    // Suscribirse a cambios en las fases
    this.suscribirCambiosFases();
  }

  // =====================================================================================
  // CONFIGURACIÓN DE FILTROS
  // =====================================================================================

  configurarFiltros(): void {
    this.configuracionFiltros = {
      titulo: 'Filtro',

      gruposCheckbox: [
        {
          id: 'puntaje',
          titulo: 'Puntaje',
          opciones: [
            {
              valor: { min: 90, max: 100 },
              etiqueta: '90-100',
              descripcion: 'Excelente'
            },
            {
              valor: { min: 80, max: 89 },
              etiqueta: '80 - 89',
              descripcion: 'Bueno'
            },
            {
              valor: { min: 60, max: 79 },
              etiqueta: '60 - 79',
              descripcion: 'Regular'
            },
            {
              valor: { min: 0, max: 59 },
              etiqueta: '0 - 59',
              descripcion: 'Deficiente'
            }
          ]
        }
      ],

      filtrosFecha: [
        {
          id: 'fecha',
          titulo: 'Fecha',
          placeholder: 'año/día/mes',
          tipo: 'date'
        }
      ],

      filtrosSelect: [
        {
          id: 'seguimiento',
          titulo: 'Seguimiento',
          placeholder: 'Seleccione un tipo de seguimiento',
          opciones: [
            { valor: 'requiere', etiqueta: 'Requiere seguimiento' },
            { valor: 'completado', etiqueta: 'Completado' }
          ]
        }
      ],

      mostrarBotonLimpiar: true,
      textoBotonAplicar: 'Aplicar',
      textoBotonLimpiar: 'Limpiar',
      textoBotonCerrar: 'Cerrar'
    };
  }

  // =====================================================================================
  // HANDLERS DE FILTROS
  // =====================================================================================

  onFilter(): void {
    console.log(' Abriendo modal de filtros');
    this.mostrarModalFiltros = true;
  }

  onCerrarFiltros(): void {
    console.log('Cerrando modal de filtros');
    this.mostrarModalFiltros = false;
  }

  onAplicarFiltros(resultado: ResultadoFiltros): void {
    console.log('Aplicando filtros:', resultado);

    this.filtrosAplicados = resultado;

    let evaluacionesFiltradas = [...this.evaluacionesOriginales];

    // FILTRAR POR PUNTAJE
    if (resultado.checkboxes['puntaje']?.length > 0) {
      evaluacionesFiltradas = evaluacionesFiltradas.filter(ev => {
        const score = ev.scoreCalidadTotal || 0;
        return resultado.checkboxes['puntaje'].some((rango: any) => {
          return score >= rango.min && score <= rango.max;
        });
      });
    }

    // FILTRAR POR SEGUIMIENTO
    if (resultado.selects['seguimiento']) {
      const valorSeguimiento = resultado.selects['seguimiento'];
      evaluacionesFiltradas = evaluacionesFiltradas.filter(ev => {
        if (valorSeguimiento === 'requiere') {
          return ev.requiereSeguimiento === true;
        } else if (valorSeguimiento === 'completado') {
          return ev.requiereSeguimiento === false;
        }
        return true;
      });
    }

    // FILTRAR POR FECHA
    if (resultado.fechas['fecha']) {
      const fechaSeleccionada = new Date(resultado.fechas['fecha']);
      evaluacionesFiltradas = evaluacionesFiltradas.filter(ev => {
        const fechaEv = new Date(ev.creadoEn);
        return fechaEv.toDateString() === fechaSeleccionada.toDateString();
      });
    }

    this.evaluacionesFiltradas = evaluacionesFiltradas;
    this.paginaActual = 1;
    this.actualizarPaginacion();
    this.mostrarModalFiltros = false;
  }

  onLimpiarFiltros(): void {
    console.log('Limpiando filtros');
    this.filtrosAplicados = undefined;
    this.evaluacionesFiltradas = [...this.evaluacionesOriginales];

    if (this.searchTerm.trim()) {
      this.onSearchTermChange(this.searchTerm);
    } else {
      this.paginaActual = 1;
      this.actualizarPaginacion();
    }
  }

  // =====================================================================================
  // CONFIGURACIÓN DE TABLA
  // =====================================================================================

  configurarTabla(): void {
    this.columnas = [
      {
        encabezado: 'ID',
        campo: 'id',
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Objetivo',
        campo: 'objetivo',
        plantilla: this.plantillaObjetivo,
        ancho: '15%',
        alineacion: 'left'
      },
      {
        encabezado: 'Evaluador',
        campo: 'evaluadorNombre',
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Fecha',
        campo: 'creadoEn',
        plantilla: this.plantillaFecha,
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Score',
        campo: 'scoreCalidadTotal',
        plantilla: this.plantillaScore,
        ancho: '10%',
        alineacion: 'center'
      },
      {
        encabezado: 'Seguimiento',
        plantilla: this.plantillaSeguimiento,
        ancho: '15%',
        alineacion: 'center'
      }
    ];

    this.acciones = [
      {
        etiqueta: 'Ver detalles',
        icono: this.iconoVerSVG,
        variante: 'primario',
        accion: (evaluacion: EvaluacionResponse) => this.onVerDetalles(evaluacion)
      },
      {
        etiqueta: 'Editar',
        icono: this.iconoEditarSVG,
        variante: 'azul-2',
        accion: (evaluacion: EvaluacionResponse) => this.onEditar(evaluacion)
      }
    ];
  }

  // =====================================================================================
  // CARGA DE DATOS
  // =====================================================================================

  cargarEvaluaciones(): void {
    this.cargando = true;
    this.error = '';

    forkJoin({
      evaluaciones: this.evaluacionService.obtenerTodas(),
      usuarios: this.http.get(`${environment.apiUrl}/api/Auth/usuarios`, {
        responseType: 'text'
      })
    }).subscribe({
      next: ({ evaluaciones, usuarios }) => {
        let usuariosArray: UsuarioResponseDto[] = [];
        try {
          const parsed = typeof usuarios === 'string'
            ? JSON.parse(usuarios)
            : usuarios;

          if (Array.isArray(parsed)) {
            usuariosArray = parsed;
          } else if (parsed && typeof parsed === 'object') {
            usuariosArray = parsed.data || parsed.items || parsed.usuarios || [];
          }
        } catch (error) {
          console.error('Error al parsear usuarios:', error);
          usuariosArray = [];
        }

        if (!Array.isArray(usuariosArray) || usuariosArray.length === 0) {
          this.evaluacionesOriginales = evaluaciones.map((ev: any) => ({
            ...ev,
            evaluadorNombre: 'Sin datos'
          }));
        } else {
          const mapaUsuarios = new Map(
            usuariosArray.map((u: UsuarioResponseDto) => [
              u.id,
              u.nombreCompleto || `${u.nombre} ${u.apellido}`
            ])
          );

          this.evaluacionesOriginales = evaluaciones.map((ev: any) => {
            const evaluadorId = ev.evaluadorId || ev.evaluadirId;
            const nombreEvaluador = evaluadorId
              ? (mapaUsuarios.get(evaluadorId) || 'Desconocido')
              : 'Sin evaluador';

            return {
              ...ev,
              evaluadorNombre: nombreEvaluador
            };
          });
        }

        this.evaluacionesFiltradas = this.evaluacionesOriginales;
        this.actualizarPaginacion();
        this.cargando = false;
      },
      error: (err: any) => {
        console.error('Error al cargar datos:', err);
        this.error = 'Error al cargar las evaluaciones.';
        this.cargando = false;
      }
    });
  }

  reintentar(): void {
    this.cargarEvaluaciones();
  }

  // =====================================================================================
  // BÚSQUEDA Y FILTRADO
  // =====================================================================================

  onSearchTermChange(termino: string): void {
    this.searchTerm = termino.toLowerCase().trim();

    if (!this.searchTerm) {
      if (this.filtrosAplicados) {
        this.onAplicarFiltros(this.filtrosAplicados);
      } else {
        this.evaluacionesFiltradas = [...this.evaluacionesOriginales];
      }
    } else {
      const baseDatos = this.filtrosAplicados
        ? this.evaluacionesFiltradas
        : this.evaluacionesOriginales;

      this.evaluacionesFiltradas = baseDatos.filter(ev =>
        ev.id?.toString().includes(this.searchTerm) ||
        ev.evaluadorId?.toString().includes(this.searchTerm) ||
        ev.clienteId?.toString().includes(this.searchTerm) ||
        ev.ejecucionId?.toString().includes(this.searchTerm) ||
        ev.objetivo?.toLowerCase().includes(this.searchTerm)
      );
    }

    this.paginaActual = 1;
    this.actualizarPaginacion();
  }

  onSearch(): void {
    if (!this.searchTerm.trim()) {
      this.evaluacionesFiltradas = this.evaluacionesOriginales;
    } else {
      const termino = this.searchTerm.toLowerCase().trim();
      this.evaluacionesFiltradas = this.evaluacionesOriginales.filter(ev =>
        ev.id.toString().includes(termino) ||
        ev.evaluadorId.toString().includes(termino) ||
        ev.objetivo?.toLowerCase().includes(termino) ||
        ev.clienteId?.toString().includes(termino)
      );
    }
    this.paginaActual = 1;
    this.actualizarPaginacion();
  }

  tieneClienteYEjecucion(evaluacion: EvaluacionResponse): boolean {
    return !!(evaluacion.clienteId && evaluacion.ejecucionId);
  }

  // =====================================================================================
  // ACCIONES
  // =====================================================================================

  onNuevaEvaluacion(): void {
    console.log('🆕 Abriendo modal de nueva evaluación');
    this.modoModal = 'crear';
    this.evaluacionIdParaEditar = null;

    // Inicializar estados del footer
    this.vistaActualModal = 'infoGeneral';
    this.estadoFaseAntes = 'sin-inicializar';
    this.estadoFaseDespues = 'sin-inicializar';

    this.sharedService.limpiar();
    this.mostrarModalRegistro = true;
  }

  onCerrarModalRegistro(): void {
    console.log('Cerrando modal de registro');
    this.mostrarModalRegistro = false;
    this.evaluacionIdParaEditar = null;
    this.modoModal = 'crear';

    // Resetear estados
    this.vistaActualModal = 'infoGeneral';
    this.estadoFaseAntes = 'sin-inicializar';
    this.estadoFaseDespues = 'sin-inicializar';

    // Recargar las evaluaciones para reflejar cambios
    this.cargarEvaluaciones();
  }

  confirmarCerrarModal(): void {
    const mensaje = this.modoModal === 'editar'
      ? '¿Cerrar sin guardar los cambios?'
      : '¿Cerrar? Los datos no guardados se perderán';

    if (confirm(mensaje)) {
      this.onCerrarModalRegistro();
    }
  }

  onEvaluacionGuardada(id: number): void {
    console.log(' Evaluación guardada con ID:', id);
    // El modal se cerrará automáticamente
  }

  get tituloModal(): string {
    const tituloBase = this.modoModal === 'editar' && this.evaluacionIdParaEditar
      ? `Editar Evaluación #${this.evaluacionIdParaEditar}`
      : 'Nueva Evaluación';

    // Agregar el nombre de la sección activa
    switch (this.vistaActualModal) {
      case 'infoGeneral':
        return `${tituloBase} - Información General`;
      case 'faseAntes':
        return `${tituloBase} - Fase Antes`;
      case 'faseDespues':
        return `${tituloBase} - Fase Después`;
      default:
        return tituloBase;
    }
  }

  onVerDetalles(evaluacion: EvaluacionResponse): void {
    console.log('Abriendo detalles de evaluación:', evaluacion.id);
    this.evaluacionIdSeleccionada = evaluacion.id;
    this.mostrarPanelDetalles = true;
  }

  onCerrarDetalles(): void {
    console.log('Cerrando panel de detalles');
    this.mostrarPanelDetalles = false;
    this.evaluacionIdSeleccionada = 0;
  }

  onEditar(evaluacion: EvaluacionResponse): void {
    console.log('Abriendo modal de edición para evaluación:', evaluacion.id);
    this.modoModal = 'editar';
    this.evaluacionIdParaEditar = evaluacion.id;

    // Inicializar vista
    this.vistaActualModal = 'infoGeneral';

    // Cargar la evaluación en el servicio compartido antes de abrir el modal
    this.evaluacionService.cargarEvaluacionCompleta(evaluacion.id).subscribe({
      next: (data) => {
        const infoGeneral = mapResponseToFormulario(data.evaluacion);
        const datosAntes = data.detalleAntes ? mapDetalleResponseToDatos(data.detalleAntes, data.fotosAntes) : undefined;
        const datosDespues = data.detalleDespues ? mapDetalleResponseToDatos(data.detalleDespues, data.fotosDespues) : undefined;

        this.sharedService.cargarEvaluacion(evaluacion.id, infoGeneral, datosAntes, datosDespues);

        // Actualizar estados de las fases
        this.actualizarEstadosFases();

        this.mostrarModalRegistro = true;
      },
      error: (error) => {
        console.error('Error al cargar evaluación:', error);
        alert('Error al cargar la evaluación para editar');
      }
    });
  }

  alClickFila(evaluacion: EvaluacionResponse): void {
    console.log('Click en fila:', evaluacion.id);
  }

  alDobleClickFila(evaluacion: EvaluacionResponse): void {
    this.onVerDetalles(evaluacion);
  }

  // =====================================================================================
  // MÉTODOS PARA COLORES PERSONALIZADOS DE TABS SEGÚN ESTADO
  // =====================================================================================

  /**
   * Obtiene el color del borde según el estado de la fase
   */
  obtenerColorBordeFase(estado: 'completada' | 'sin-completar' | 'sin-inicializar', esActivo: boolean): string {
    if (esActivo) {
      // Cuando el tab está activo, usar colores más intensos
      switch (estado) {
        case 'completada':
          return '#10b981'; // Verde esmeralda
        case 'sin-completar':
          return '#f59e0b'; // Amarillo ámbar
        case 'sin-inicializar':
          return '#9ca3af'; // Gris medio
      }
    } else {
      // Cuando el tab está inactivo, mismo color pero menos intenso
      switch (estado) {
        case 'completada':
          return '#10b981'; // Verde esmeralda
        case 'sin-completar':
          return '#f59e0b'; // Amarillo ámbar
        case 'sin-inicializar':
          return '#9ca3af'; // Gris medio
      }
    }
  }

  /**
   * Obtiene el color del texto según el estado de la fase
   */
  obtenerColorTextoFase(estado: 'completada' | 'sin-completar' | 'sin-inicializar', esActivo: boolean): string {
    if (esActivo) {
      // Cuando el tab está activo, usar colores más oscuros para mejor contraste
      switch (estado) {
        case 'completada':
          return '#065f46'; // Verde muy oscuro
        case 'sin-completar':
          return '#92400e'; // Marrón oscuro
        case 'sin-inicializar':
          return '#374151'; // Gris muy oscuro
      }
    } else {
      // Cuando el tab está inactivo
      switch (estado) {
        case 'completada':
          return '#059669'; // Verde oscuro
        case 'sin-completar':
          return '#d97706'; // Naranja
        case 'sin-inicializar':
          return '#6b7280'; // Gris oscuro
      }
    }
  }

  /**
   * Obtiene el color del icono según el estado de la fase
   */
  obtenerColorIconoFase(estado: 'completada' | 'sin-completar' | 'sin-inicializar'): string {
    switch (estado) {
      case 'completada':
        return '#10b981'; // Verde esmeralda
      case 'sin-completar':
        return '#f59e0b'; // Amarillo ámbar
      case 'sin-inicializar':
        return '#9ca3af'; // Gris medio
    }
  }

  /**
   * Obtiene el color de fondo según el estado de la fase
   */
  obtenerColorFondoFase(estado: 'completada' | 'sin-completar' | 'sin-inicializar', esActivo: boolean): string {
    if (esActivo) {
      // Cuando el tab está activo, usar fondos degradados
      switch (estado) {
        case 'completada':
          return 'linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%)'; // Degradado verde
        case 'sin-completar':
          return 'linear-gradient(135deg, #fef3c7 0%, #fde68a 100%)'; // Degradado amarillo
        case 'sin-inicializar':
          return 'linear-gradient(135deg, #f3f4f6 0%, #e5e7eb 100%)'; // Degradado gris
      }
    } else {
      // Cuando el tab está inactivo, fondo blanco
      return '#ffffff';
    }
  }

  // =====================================================================================
  // MÉTODOS PARA EL FOOTER DEL MODAL
  // =====================================================================================

  /**
   * Cambia la vista activa en el modal
   */
  cambiarVistaModal(vista: 'infoGeneral' | 'faseAntes' | 'faseDespues'): void {
    console.log('Cambiando vista del modal a:', vista);

    this.vistaActualModal = vista;

    // Llamar al método del componente hijo si está disponible
    if (this.infogeneralComponent) {
      this.infogeneralComponent.navegarASeccion(vista);
    }

    // IMPORTANTE: No inicializar las fases automáticamente al navegar
    // Solo actualizar los estados visuales basados en datos existentes
    this.actualizarEstadosFases();

    // Si navegamos a una fase que no existe, NO la creamos aquí
    // El usuario debe llenar al menos un campo para que se cree la fase
    if (vista === 'faseAntes') {
      const datosAntes = this.sharedService.getFaseAntes();
      if (!datosAntes) {
        console.log(' Fase ANTES aún no inicializada (esperando datos del usuario)');
      }
    }

    if (vista === 'faseDespues') {
      const datosDespues = this.sharedService.getFaseDespues();
      if (!datosDespues) {
        console.log(' Fase DESPUÉS aún no inicializada (esperando datos del usuario)');
      }
    }
  }

  /**
   * Actualiza los estados visuales de las fases desde el SharedService
   */
  actualizarEstadosFases(): void {
    const datosAntes = this.sharedService.getFaseAntes();
    const datosDespues = this.sharedService.getFaseDespues();

    // FASE ANTES
    this.estadoFaseAntes = this.determinarEstadoFase(datosAntes);

    // FASE DESPUÉS
    this.estadoFaseDespues = this.determinarEstadoFase(datosDespues);

    console.log(' Estados actualizados:', {
      antes: this.estadoFaseAntes,
      despues: this.estadoFaseDespues
    });
  }

  /**
   * Determina el estado de una fase individual
   */
  private determinarEstadoFase(datosFase: any): 'completada' | 'sin-completar' | 'sin-inicializar' {
    // SIN INICIALIZAR: No hay datos o todos los campos están vacíos
    if (!datosFase || this.estaFaseVacia(datosFase)) {
      return 'sin-inicializar';
    }

    // COMPLETADA: Tiene scoreFase Y es mayor a 0
    if (datosFase.scoreFase !== null &&
      datosFase.scoreFase !== undefined &&
      datosFase.scoreFase > 0) {
      return 'completada';
    }

    // SIN COMPLETAR: Tiene al menos un campo lleno pero sin score válido
    if (this.tieneAlgunCampoLleno(datosFase)) {
      return 'sin-completar';
    }

    return 'sin-inicializar';
  }

  /**
   * Verifica si una fase está completamente vacía (sin datos)
   */
  private estaFaseVacia(datosFase: any): boolean {
    if (!datosFase) return true;

    const camposVacios = (
      (!datosFase.lugar || datosFase.lugar.trim() === '') &&
      (!datosFase.descripcion || datosFase.descripcion.trim() === '') &&
      (!datosFase.sugerencias || datosFase.sugerencias.trim() === '') &&
      (!datosFase.evidenciasNota || datosFase.evidenciasNota.trim() === '') &&
      (!datosFase.fotos || datosFase.fotos.length === 0) &&
      (datosFase.scoreFase === null || datosFase.scoreFase === undefined || datosFase.scoreFase === 0)
    );

    return camposVacios;
  }

  /**
   * Verifica si la fase tiene al menos un campo con datos (excluyendo score)
   */
  private tieneAlgunCampoLleno(datosFase: any): boolean {
    if (!datosFase) return false;

    return (
      (datosFase.lugar && datosFase.lugar.trim() !== '') ||
      (datosFase.descripcion && datosFase.descripcion.trim() !== '') ||
      (datosFase.sugerencias && datosFase.sugerencias.trim() !== '') ||
      (datosFase.evidenciasNota && datosFase.evidenciasNota.trim() !== '') ||
      (datosFase.fotos && datosFase.fotos.length > 0)
    );
  }

  /**
   * Guarda la evaluación desde el modal (llamado por el botón del footer)
   */
  async guardarEvaluacionDesdeModal(): Promise<void> {
    console.log('Guardando evaluación desde modal...');

    // Verificar que exista el componente hijo
    if (!this.infogeneralComponent) {
      alert('Error: No se puede acceder al formulario');
      return;
    }

    this.guardandoModal = true;

    try {
      // VALIDACIÓN: Verificar estado de las fases antes de guardar
      const datosAntes = this.sharedService.getFaseAntes();
      const datosDespues = this.sharedService.getFaseDespues();

      const faseAntesValida = this.esFaseValidaParaGuardar(datosAntes);
      const faseDespeusValida = this.esFaseValidaParaGuardar(datosDespues);

      console.log(' Validación de fases:', {
        faseAntesValida,
        faseDespeusValida,
        datosAntes: datosAntes ? 'con datos' : 'null/undefined',
        datosDespues: datosDespues ? 'con datos' : 'null/undefined'
      });

      // Advertencias en consola si hay datos inválidos
      if (!faseAntesValida && datosAntes) {
        console.warn('Fase ANTES tiene un objeto con campos vacíos');
      }

      if (!faseDespeusValida && datosDespues) {
        console.warn('Fase DESPUÉS tiene un objeto con campos vacíos');
      }

      // Llamar al método de guardar del componente hijo
      // El componente hijo debe manejar correctamente los datos vacíos
      await this.infogeneralComponent.guardarEvaluacion();

      // Actualizar estados después de guardar
      this.actualizarEstadosFases();
    } catch (error) {
      console.error('Error al guardar desde modal:', error);
      alert('Error al guardar la evaluación');
    } finally {
      this.guardandoModal = false;
    }
  }

  /**
   * Verifica si una fase es válida para guardar
   * Una fase es válida si:
   * 1. Es null/undefined (fase sin inicializar)
   * 2. Tiene al menos un campo con datos válidos
   * 
   * Una fase NO es válida si:
   * - Tiene un objeto con todos los campos vacíos 
   * 
   * NOTA: Este método solo identifica fases inválidas para logging.
   * La limpieza real debe hacerse en el componente que guarda (infogeneralregistro).
   */
  private esFaseValidaParaGuardar(datosFase: any): boolean {
    // Si la fase es null o undefined, es válida (fase sin inicializar)
    if (!datosFase) {
      return true;
    }

    // Si la fase tiene al menos un campo con datos, es válida
    if (this.tieneAlgunCampoLleno(datosFase)) {
      return true;
    }

    // Si la fase tiene scoreFase válido, es válida
    if (datosFase.scoreFase !== null && 
        datosFase.scoreFase !== undefined && 
        datosFase.scoreFase > 0) {
      return true;
    }

    // Si llegamos aquí, la fase tiene un objeto con todos los campos vacíos
    // Esto puede causar errores en el backend
    return false;
  }

  /**
   * Suscribirse a cambios en el SharedService para actualizar estados
   */
  private suscribirCambiosFases(): void {
    this.sharedService.faseAntes$.subscribe(() => {
      if (this.mostrarModalRegistro) {
        this.actualizarEstadosFases();
      }
    });

    this.sharedService.faseDespues$.subscribe(() => {
      if (this.mostrarModalRegistro) {
        this.actualizarEstadosFases();
      }
    });

    this.sharedService.guardando$.subscribe(guardando => {
      this.guardandoModal = guardando;
    });
  }

  // =====================================================================================
  // PAGINACIÓN
  // =====================================================================================

  alCambiarPagina(numeroPagina: number): void {
    this.paginaActual = numeroPagina;
    this.actualizarPaginacion();
  }

  alCambiarRangoPagina(rango: { inicio: number; fin: number }): void {
    console.log('Mostrando elementos del', rango.inicio, 'al', rango.fin);
  }

  actualizarPaginacion(): void {
    const inicio = (this.paginaActual - 1) * this.elementosPorPagina;
    const fin = inicio + this.elementosPorPagina;
    this.evaluaciones = this.evaluacionesFiltradas.slice(inicio, fin);
  }

  // =====================================================================================
  // MÉTODOS HELPER PARA TEMPLATES
  // =====================================================================================

  obtenerClaseScore(score: number | null | undefined): string {
    if (!score) return 'bg-gray-300 text-black';
    return this.evaluacionService.obtenerClaseScore(score);
  }

  obtenerTextoSeguimiento(requiereSeguimiento: boolean): string {
    return this.evaluacionService.obtenerTextoSeguimiento(requiereSeguimiento);
  }

  obtenerClaseSeguimiento(requiereSeguimiento: boolean): string {
    return this.evaluacionService.obtenerClaseSeguimiento(requiereSeguimiento);
  }

  formatearFecha(fechaISO: string): string {
    return this.evaluacionService.formatearFecha(fechaISO);
  }

  obtenerObjetivoAbreviado(objetivo: string | null | undefined, maxLength: number = 50): string {
    if (!objetivo) return 'Sin objetivo definido';
    if (objetivo.length <= maxLength) return objetivo;
    return objetivo.substring(0, maxLength) + '...';
  }
}