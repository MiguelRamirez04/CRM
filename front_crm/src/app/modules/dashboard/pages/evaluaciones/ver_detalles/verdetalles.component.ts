import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { environment } from '../../../../../../environments/environment';

// Importar servicio y modelos (SIN ClienteSelect)
import { 
  EvaluacionService,
  OrdenTrabajoSelect,
  EjecucionSelect,
  UsuarioSelect
} from '../../../../../core/services/evaluaciones.service';
import { 
  EvaluacionResponse, 
  EvaluacionDetalleResponse,
  UsuarioResponseDto
} from '../../../../../core/models/evaluaciones.interface';

// Importar componentes reutilizables desde index
import { SidePanelComponent } from '../../../../../shared/organisms/side-panel/side-panel.component';
import { DetailSectionComponent } from '../../../../../shared/molecules/detail-section/detail-section.component';
import { BadgeComponent } from '../../../../../shared/atoms/bage/badge.component';
import { AlertComponent } from '../../../../../shared/molecules/alert/alert.component';
import { LoadingSpinnerComponent } from '../../../../../shared/atoms/loading-spinner/loading-spinner.component';
import { UiBotonComponent } from '../../../../../shared/atoms/boton/boton.component';
import { UitipografiaComponent } from '../../../../../shared/atoms/tipografia/tipografia.component';
import { UiIconComponent } from '../../../../../shared/atoms/icono/icono.component';

import { FaseantesComponent } from './fases/faseantes.component';
import { FasedespuesComponent } from './fases/fasedespues.component';

// =====================================================================================
//   Tipos para estados de fases
// =====================================================================================
type EstadoFase = 'completada' | 'sin-completar' | 'sin-inicializar';

interface DatosFase {
  fase: string;
  titulo: string;
  estado: EstadoFase;
  detalle: EvaluacionDetalleResponse | undefined;
}

@Component({
  selector: 'app-verdetalles',
  standalone: true,
  imports: [
    CommonModule,
    SidePanelComponent,
    DetailSectionComponent,
    BadgeComponent,
    AlertComponent,
    LoadingSpinnerComponent,
    UiBotonComponent,
    FaseantesComponent,  
    FasedespuesComponent,
    UitipografiaComponent,
    UiIconComponent
  ],
  templateUrl: './verdetalles.component.htmL',
  styleUrls: ['./verdetalles.component.css']
})
export class VerdetallesComponent implements OnInit, OnChanges {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() evaluacionId: number = 0;
  @Input() mostrarPanel: boolean = false;
  @Output() cerrar = new EventEmitter<void>();

  // =====================================================================================
  // PROPIEDADES (CLIENTES ELIMINADO)
  // =====================================================================================
  
  evaluacion: EvaluacionResponse | null = null;
  detalles: EvaluacionDetalleResponse[] = [];
  
  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  evaluadores: UsuarioSelect[] = [];  

  textoOrden: string = '';
  textoEjecucion: string = '';
  textoCliente: string = '';
  textoEvaluador: string = '';  

  cargando: boolean = true;
  cargandoCatalogos: boolean = false;
  error: string = '';

  vistaActual: 'detalles' | 'faseAntes' | 'faseDespues' = 'detalles';

  //  Cache de estados de fases para evitar logs repetidos
  private estadoFaseAntesCache: EstadoFase | null = null;
  private estadoFaseDespuesCache: EstadoFase | null = null;

  constructor(
    public evaluacionService: EvaluacionService,
    public router: Router,
    private http: HttpClient  
  ) {}

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    if (this.evaluacionId && this.mostrarPanel) {
      this.cargarEvaluacionCompleta();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['evaluacionId'] || changes['mostrarPanel']) {
      if (this.evaluacionId && this.mostrarPanel) {
        this.cargarEvaluacionCompleta();
      }
    }
  }

  // =====================================================================================
  //  MÉTODOS PARA DETERMINAR ESTADO DE FASES
  // =====================================================================================

  /**
    Determina el estado de una fase con cache
   * @param fase - Nombre de la fase ('ANTES' o 'DESPUES')
   * @returns Estado de la fase
   */
  obtenerEstadoFase(fase: string): EstadoFase {
    const faseUpper = fase.toUpperCase();

    // Usar cache si está disponible
    if (faseUpper === 'ANTES' && this.estadoFaseAntesCache !== null) {
      return this.estadoFaseAntesCache;
    }
    if (faseUpper === 'DESPUES' && this.estadoFaseDespuesCache !== null) {
      return this.estadoFaseDespuesCache;
    }

    // Calcular estado
    const detalle = this.obtenerDetallePorFase(fase);
    let estado: EstadoFase;

    // 1. SIN INICIALIZAR: No existe registro en la BD
    if (!detalle) {
      estado = 'sin-inicializar';
    }
    // 2. COMPLETADA: Existe registro Y tiene scoreFase (no null)
    else if (detalle.scoreFase !== null && detalle.scoreFase !== undefined) {
      estado = 'completada';
    }
    // 3. SIN COMPLETAR: Existe registro PERO scoreFase es null
    else {
      estado = 'sin-completar';
    }

    // Guardar en cache
    if (faseUpper === 'ANTES') {
      this.estadoFaseAntesCache = estado;
    } else if (faseUpper === 'DESPUES') {
      this.estadoFaseDespuesCache = estado;
    }

    // Log solo la primera vez
    console.log(`📋 Fase ${fase}: ${estado.toUpperCase()}`);

    return estado;
  }

  /**
   * Verifica si una fase está completada (tiene score)
   */
  estaFaseCompletada(fase: string): boolean {
    return this.obtenerEstadoFase(fase) === 'completada';
  }

  /**
   * Verifica si una fase está sin completar (existe pero sin score)
   */
  estaFaseSinCompletar(fase: string): boolean {
    return this.obtenerEstadoFase(fase) === 'sin-completar';
  }

  /**
   * Verifica si una fase está sin inicializar (no existe registro)
   */
  estaFaseSinInicializar(fase: string): boolean {
    return this.obtenerEstadoFase(fase) === 'sin-inicializar';
  }

  /**
   * Verifica si una fase es navegable (completada o sin completar)
   */
  esFaseNavegable(fase: string): boolean {
    const estado = this.obtenerEstadoFase(fase);
    return estado === 'completada' || estado === 'sin-completar';
  }

  // =====================================================================================
  // MÉTODOS DE CARGA
  // =====================================================================================

  cargarEvaluacionCompleta(): void {
    console.log(' Cargando evaluación completa con catálogos...');
    
    this.cargando = true;
    this.error = '';
    
    //  LIMPIAR CACHE al cargar nueva evaluación
    this.limpiarCacheFases();

    this.evaluacionService.cargarEvaluacionCompleta(this.evaluacionId).subscribe({
      next: (resultado) => {
        console.log(' Datos de evaluación recibidos:', resultado);
        
        this.evaluacion = resultado.evaluacion;
        
        // Convertir detalleAntes y detalleDespues a array
        this.detalles = [];
        if (resultado.detalleAntes) {
          this.detalles.push(resultado.detalleAntes);
        }
        if (resultado.detalleDespues) {
          this.detalles.push(resultado.detalleDespues);
        }
        
        console.log('Detalles cargados:', this.detalles);
        
        this.ejecuciones = resultado.ejecuciones;
        this.cargando = false;
        
        this.cargarCatalogosAdicionales();
      },
      error: (err) => {
        console.error(' Error al cargar evaluación completa:', err);
        this.error = 'No se pudo cargar la evaluación. Intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  //  MÉTODO CORREGIDO: Ahora carga la lista completa de usuarios
  private cargarCatalogosAdicionales(): void {
    console.log(' Cargando catálogos adicionales (SIN CLIENTES)...');
    
    this.cargandoCatalogos = true;

    forkJoin({
      ordenes: this.evaluacionService.obtenerOrdenesTrabajo(),
      usuarios: this.http.get(`${environment.apiUrl}/api/Auth/usuarios`, {
        responseType: 'text'
      })
    }).subscribe({
      next: ({ ordenes, usuarios }) => {
        this.ordenesTrabajo = ordenes;
        
        //  PARSEAR USUARIOS
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
          console.error(' Error al parsear usuarios:', error);
          usuariosArray = [];
        }

        //  CONVERTIR A UsuarioSelect[]
        this.evaluadores = usuariosArray.map((u: UsuarioResponseDto) => ({
          id: u.id,
          nombreCompleto: u.nombreCompleto || `${u.nombre} ${u.apellido}`,
          rol: u.rol || 'Desconocido',
          displayText: `${u.nombreCompleto || u.nombre} ${u.apellido} (${u.rol || 'Sin rol'})`
        }));

        console.log(' Usuarios cargados:', this.evaluadores.length);
        
        this.construirTextosDescriptivos();
        this.cargandoCatalogos = false;
      },
      error: (err) => {
        console.error('Error al cargar catálogos adicionales:', err);
        this.construirTextosDescriptivos();
        this.cargandoCatalogos = false;
      }
    });
  }

  //  MÉTODO CORREGIDO: Ahora busca el evaluador en el array completo
  private construirTextosDescriptivos(): void {
    if (!this.evaluacion) return;

    // Orden de trabajo
    if (this.evaluacion.ordenId) {
      const orden = this.ordenesTrabajo.find(o => o.id === this.evaluacion!.ordenId);
      this.textoOrden = orden ? orden.displayText : `OT-${this.evaluacion.ordenId}`;
    } else {
      this.textoOrden = 'Sin orden asignada';
    }

    // Ejecución
    if (this.evaluacion.ejecucionId) {
      const ejecucion = this.ejecuciones.find(e => e.id === this.evaluacion!.ejecucionId);
      this.textoEjecucion = ejecucion ? ejecucion.displayText : `Ejecución #${this.evaluacion.ejecucionId}`;
    } else {
      this.textoEjecucion = 'Sin ejecución específica';
    }

    // Cliente (sin búsqueda)
    if (this.evaluacion.clienteId) {
      this.textoCliente = `Cliente #${this.evaluacion.clienteId}`;
    } else {
      this.textoCliente = 'Sin cliente asignado';
    }

    //  EVALUADOR - AHORA BUSCA EN EL ARRAY DE USUARIOS
    if (this.evaluacion.evaluadorId) {
      const evaluador = this.evaluadores.find(e => e.id === this.evaluacion!.evaluadorId);
      
      if (evaluador) {
        this.textoEvaluador = evaluador.displayText;
        console.log(' Evaluador encontrado:', this.textoEvaluador);
      } else {
        this.textoEvaluador = `Evaluador #${this.evaluacion.evaluadorId}`;
        console.warn('Evaluador no encontrado en array:', this.evaluacion.evaluadorId);
      }
    } else {
      this.textoEvaluador = 'Sin evaluador asignado';
    }

    console.log('Textos descriptivos construidos:', {
      orden: this.textoOrden,
      ejecucion: this.textoEjecucion,
      cliente: this.textoCliente,
      evaluador: this.textoEvaluador
    });
  }

  reintentar(): void {
    this.cargarEvaluacionCompleta();
  }

  // Método para limpiar cache de estados
  private limpiarCacheFases(): void {
    this.estadoFaseAntesCache = null;
    this.estadoFaseDespuesCache = null;
  }

  // =====================================================================================
  // NAVEGACIÓN
  // =====================================================================================

  navegarAFase(fase: string): void {
    // Solo permitir navegación si la fase es navegable (completada o sin completar)
    if (!this.esFaseNavegable(fase)) {
      console.warn(` La fase ${fase} no es navegable (estado: ${this.obtenerEstadoFase(fase)})`);
      return;
    }

    if (fase.toUpperCase() === 'ANTES') {
      this.vistaActual = 'faseAntes';
    } else if (fase.toUpperCase() === 'DESPUES') {
      this.vistaActual = 'faseDespues';
    }
  }

  volverADetallesDesdeVista(): void {
    this.vistaActual = 'detalles';
  }

  cerrarPanel(): void {
    this.cerrar.emit();
  }

  volverAlListado(): void {
    this.cerrarPanel();
  }

  // =====================================================================================
  // GETTERS Y HELPERS
  // =====================================================================================

  obtenerDetallePorFase(fase: string): EvaluacionDetalleResponse | undefined {
    return this.detalles.find(d => d.fase.toUpperCase() === fase.toUpperCase());
  }

  obtenerLabelFase(fase: string): string {
    return this.evaluacionService.obtenerLabelFase(fase);
  }

  get claseScore(): string {
    if (!this.evaluacion?.scoreCalidadTotal) return 'bg-gray-300 text-black';
    return this.evaluacionService.obtenerClaseScore(this.evaluacion.scoreCalidadTotal);
  }

  get porcentajeScore(): number {
    return this.evaluacion?.scoreCalidadTotal || 0;
  }

  get textoSeguimiento(): string {
    return this.evaluacionService.obtenerTextoSeguimiento(
      this.evaluacion?.requiereSeguimiento || false
    );
  }

  get tipoAlertaSeguimiento(): 'error' | 'warning' | 'info' | 'success' {
    return this.evaluacion?.requiereSeguimiento ? 'warning' : 'info';
  }

  get fechaCreacionFormateada(): string {
    if (!this.evaluacion?.creadoEn) return '';
    return this.evaluacionService.formatearFecha(this.evaluacion.creadoEn);
  }

  get tituloPanel(): string {
    switch (this.vistaActual) {
      case 'faseAntes':
        return 'Evaluación Fase ANTES';
      case 'faseDespues':
        return 'Evaluación Fase DESPUÉS';
      default:
        return 'Detalles de Evaluación';
    }
  }

  /**
   *  Incluye el estado de cada fase
   */
  get datosFases(): DatosFase[] {
    return [
      {
        fase: 'ANTES',
        titulo: 'Evaluación ANTES',
        estado: this.obtenerEstadoFase('ANTES'),
        detalle: this.obtenerDetallePorFase('ANTES')
      },
      {
        fase: 'DESPUES',
        titulo: 'Evaluación DESPUÉS',
        estado: this.obtenerEstadoFase('DESPUES'),
        detalle: this.obtenerDetallePorFase('DESPUES')
      }
    ];
  }

  /**
   * Cuenta solo las fases completadas (con score)
   */
  get fasesCompletadas(): number {
    return this.datosFases.filter(f => f.estado === 'completada').length;
  }

  get totalFases(): number {
    return 2;
  }

  get todasFasesCompletadas(): boolean {
    return this.fasesCompletadas === this.totalFases;
  }

  obtenerClaseScoreFase(score: number | null | undefined): string {
    if (score === null || score === undefined) return 'bg-gray-300 text-black';
    return this.evaluacionService.obtenerClaseScore(score);
  }

  obtenerTipoBadgeScore(score: number | null | undefined): 'success' | 'info' | 'warning' | 'error' | 'default' {
  if (score === null || score === undefined) return 'default';
  if (score >= 90) return 'success';
  if (score >= 80) return 'info';    
  if (score >= 60) return 'warning';
  return 'error';
}
}

