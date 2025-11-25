import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';  // ✅ Agregar
import { forkJoin } from 'rxjs';  // ✅ Agregar
import { environment } from '../../../../../../environments/environment';  // ✅ Agregar (ajusta la ruta según tu estructura)

// Servicios e interfaces
import { EvaluacionService } from '../../../../../core/services/evaluaciones.service';
import { EvaluacionResponse, UsuarioResponseDto, EvaluacionResponseExtendida} from '../../../../../core/models/evaluaciones.interface';
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

// Componente de detalles
import { VerdetallesComponent } from '../ver_detalles/verdetalles.component';

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
  

  columnas: ConfiguracionColumna<EvaluacionResponseExtendida>[] = [];  // ✅ Cambiar aquí también
  acciones: AccionTabla<EvaluacionResponseExtendida>[] = [];  // ✅ Y aquí

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

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private http: HttpClient
  ) {}

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    console.log('🧹 Limpiando servicio compartido al entrar al listado');
    this.sharedService.limpiar();
    
    this.configurarTabla();
    this.configurarFiltros();
    this.cargarEvaluaciones();
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
    console.log('🔍 Abriendo modal de filtros');
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

    // FILTRAR POR FECHA
    if (resultado.fechas['fecha']) {
      const fechaSeleccionada = new Date(resultado.fechas['fecha']);
      fechaSeleccionada.setHours(0, 0, 0, 0);

      evaluacionesFiltradas = evaluacionesFiltradas.filter(ev => {
        const fechaEvaluacion = new Date(ev.creadoEn);
        fechaEvaluacion.setHours(0, 0, 0, 0);
        return fechaEvaluacion.getTime() === fechaSeleccionada.getTime();
      });
    }

    // FILTRAR POR SEGUIMIENTO
    if (resultado.selects['seguimiento']) {
      const tipoSeguimiento = resultado.selects['seguimiento'];
      
      evaluacionesFiltradas = evaluacionesFiltradas.filter(ev => {
        if (tipoSeguimiento === 'requiere') {
          return ev.requiereSeguimiento === true;
        } else if (tipoSeguimiento === 'completado') {
          return ev.requiereSeguimiento === false;
        }
        return true;
      });
    }

    this.evaluacionesFiltradas = evaluacionesFiltradas;
    this.paginaActual = 1;
    this.actualizarPaginacion();
  }

  onLimpiarFiltros(): void {
    console.log('🧹 Limpiando todos los filtros');
    this.filtrosAplicados = undefined;
    this.evaluacionesFiltradas = [...this.evaluacionesOriginales];
    this.searchTerm = '';
    this.paginaActual = 1;
    this.actualizarPaginacion();
  }

  // =====================================================================================
  // CONFIGURACIÓN DE TABLA
  // =====================================================================================

  configurarTabla(): void {
    this.columnas = [
      {
        encabezado: 'ID',
        campo: 'id',
        ancho: '80px',
        alineacion: 'center'
      },
      {
        encabezado: 'Objetivo',
        campo: 'objetivo',
        plantilla: this.plantillaObjetivo,
        alineacion: 'left'
      },
      {
        encabezado: 'Evaluador',
        campo: 'evaluadorNombre',
        ancho: '120px',
        alineacion: 'center'
      },
      {
        encabezado: 'Fecha',
        campo: 'creadoEn',
        plantilla: this.plantillaFecha,
        ancho: '180px',
        alineacion: 'center'
      },
      {
        encabezado: 'Score',
        campo: 'scoreCalidadTotal',
        plantilla: this.plantillaScore,
        ancho: '100px',
        alineacion: 'center'
      },
      {
        encabezado: 'Seguimiento',
        plantilla: this.plantillaSeguimiento,
        ancho: '200px',
        alineacion: 'center'
      }
    ];

    this.acciones = [
      {
        etiqueta: 'Ver detalles',
        icono: this.iconoVerSVG,
        variante: 'azul-2',
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
        console.error('❌ Error al parsear usuarios:', error);
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
          // ✅ Usar evaluadorId (correcto) en lugar de evaluadirId (typo)
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
      console.error('❌ Error al cargar datos:', err);
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
      ev.evaluadorId?.toString().includes(this.searchTerm) ||    // ✅ Cambiar
      ev.clienteId?.toString().includes(this.searchTerm) ||      // ✅ Cambiar
      ev.ejecucionId?.toString().includes(this.searchTerm) ||    // ✅ Cambiar
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
      ev.evaluadorId.toString().includes(termino) ||              // ✅ Cambiar
      ev.objetivo?.toLowerCase().includes(termino) ||
      ev.ordenId.toString().includes(termino) ||
      ev.clienteId?.toString().includes(termino)                  // ✅ Cambiar
    );
  }
  this.paginaActual = 1;
  this.actualizarPaginacion();
}

tieneClienteYEjecucion(evaluacion: EvaluacionResponse): boolean {
  return !!(evaluacion.clienteId && evaluacion.ejecucionId);      // ✅ Cambiar
}

  // =====================================================================================
  // ACCIONES
  // =====================================================================================

  onNuevaEvaluacion(): void {
    this.router.navigate(['/dashboard/evaluaciones/registro']);
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
    this.router.navigate(['/dashboard/evaluaciones/editar', evaluacion.id]);
  }

  alClickFila(evaluacion: EvaluacionResponse): void {
    console.log('Click en fila:', evaluacion.id);
  }

  alDobleClickFila(evaluacion: EvaluacionResponse): void {
    this.onVerDetalles(evaluacion);
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