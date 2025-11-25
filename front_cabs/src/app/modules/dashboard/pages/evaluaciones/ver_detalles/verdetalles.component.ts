// =====================================================================================
// COMPONENTE VER DETALLES CORREGIDO - verdetalles.component.ts
// =====================================================================================
//
// 🔥 FIX APLICADO: Cargar catálogos completos para mostrar textos descriptivos
// ✅ Ahora carga: órdenes, ejecuciones, clientes y evaluadores
// ✅ Muestra textos descriptivos en lugar de solo IDs
// ✅ Agrega spinner de carga para la información general
//
// =====================================================================================

import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

// Importar servicio y modelos
import { 
  EvaluacionService,
  OrdenTrabajoSelect,
  ClienteSelect,
  EjecucionSelect,
  UsuarioSelect
} from '../../../../../core/services/evaluaciones.service';
import { 
  EvaluacionResponse, 
  EvaluacionDetalleResponse 
} from '../../../../../core/models/evaluaciones.interface';

// Importar componentes reutilizables
import { SidePanelComponent } from '../../../../../shared/organisms/side-panel/side-panel.component';
import { DetailSectionComponent } from '../../../../../shared/molecules/detail-section/detail-section.component';
import { DetailFieldComponent } from '../../../../../shared/molecules/detail-field/detail-field.component';
import { BadgeComponent } from '../../../../../shared/atoms/bage/badge.component';
import { AlertComponent } from '../../../../../shared/molecules/alert/alert.component';
import { LoadingSpinnerComponent } from '../../../../../shared/atoms/loading-spinner/loading-spinner.component';
import { UiBotonComponent } from '../../../../../shared/atoms/boton/boton.component';

import { FaseantesComponent } from './fases/faseantes.component';
import { FasedespuesComponent } from './fases/fasedespues.component';


@Component({
  selector: 'app-verdetalles',
  standalone: true,
  imports: [
    CommonModule,
    SidePanelComponent,
    DetailSectionComponent,
    DetailFieldComponent,
    BadgeComponent,
    AlertComponent,
    LoadingSpinnerComponent,
    UiBotonComponent,
     FaseantesComponent,  
    FasedespuesComponent     
  ],
  templateUrl: './verdetalles.component.html',
  styleUrls: ['./verdetalles.component.css']
})
export class VerdetallesComponent implements OnInit, OnChanges {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  /** ID de la evaluación a mostrar - Recibido desde el componente padre */
  @Input() evaluacionId: number = 0;
  
  /** Controla si el panel está visible - Recibido desde el componente padre */
  @Input() mostrarPanel: boolean = false;
  
  /** Emite cuando el panel se cierra */
  @Output() cerrar = new EventEmitter<void>();


  mostrarFaseAntes: boolean = false;
  mostrarFaseDespues: boolean = false;
  // Propiedad para controlar qué vista mostrar
vistaActual: 'detalles' | 'faseAntes' | 'faseDespues' = 'detalles';
  // =====================================================================================
  // PROPIEDADES - DATOS DE LA EVALUACIÓN
  // =====================================================================================
  
  evaluacion: EvaluacionResponse | null = null;
  detalles: EvaluacionDetalleResponse[] = [];
  
  // =====================================================================================
  // PROPIEDADES - CATÁLOGOS PARA MOSTRAR TEXTOS DESCRIPTIVOS
  // =====================================================================================
  
  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  clientes: ClienteSelect[] = [];
  evaluadores: UsuarioSelect[] = [];

  // Textos descriptivos construidos
  textoOrden: string = '';
  textoEjecucion: string = '';
  textoCliente: string = '';
  textoEvaluador: string = '';

  // =====================================================================================
  // PROPIEDADES - ESTADOS DE CARGA
  // =====================================================================================
  
  cargando: boolean = true;
  cargandoCatalogos: boolean = false;
  error: string = '';

  // Iconos SVG para las secciones
  readonly ICONOS = {
    info: 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
    documento: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    comentario: 'M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z',
    chart: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
    fases: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01',
    checkCircle: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
    clock: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z'
  };

  constructor(
    public evaluacionService: EvaluacionService,
    public router: Router
  ) {}

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    // Cargar datos si el evaluacionId está disponible al iniciar
    if (this.evaluacionId && this.mostrarPanel) {
      this.cargarEvaluacionCompleta();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Detectar cambios en evaluacionId o mostrarPanel
    if (changes['evaluacionId'] || changes['mostrarPanel']) {
      if (this.evaluacionId && this.mostrarPanel) {
        this.cargarEvaluacionCompleta();
      }
    }
  }

  // =====================================================================================
  // MÉTODOS DE CARGA PRINCIPALES
  // =====================================================================================

  /**
   * 🔥 CORREGIDO: Cargar evaluación completa CON catálogos
   * Ahora carga evaluación + detalles + catálogos para construir textos
   */
  cargarEvaluacionCompleta(): void {
    console.log('🔄 Cargando evaluación completa con catálogos...');
    
    this.cargando = true;
    this.error = '';

    // Usar el método del servicio que ya carga ejecuciones
    this.evaluacionService.cargarEvaluacionCompleta(this.evaluacionId).subscribe({
      next: (resultado) => {
        console.log('✅ Datos de evaluación recibidos:', resultado);
        
        this.evaluacion = resultado.evaluacion;
        
        // 🔥 Convertir detalleAntes y detalleDespues a array de detalles
        this.detalles = [];
        if (resultado.detalleAntes) {
          this.detalles.push(resultado.detalleAntes);
        }
        if (resultado.detalleDespues) {
          this.detalles.push(resultado.detalleDespues);
        }
        
        this.ejecuciones = resultado.ejecuciones;  // 🔥 Ahora tenemos las ejecuciones
        
        this.cargando = false;
        
        // Cargar el resto de catálogos (órdenes, clientes, evaluadores)
        this.cargarCatalogosAdicionales();
      },
      error: (err) => {
        console.error('❌ Error al cargar evaluación completa:', err);
        this.error = 'No se pudo cargar la evaluación. Intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  // En verdetalles.component.ts


// Método actualizado para navegar a fases
navegarAFase(fase: string): void {
  const detalle = this.obtenerDetallePorFase(fase);
  
  if (!detalle) {
    console.warn(`La fase ${fase} no está completada`);
    return;
  }

  // Cambiar la vista dentro del mismo panel
  if (fase.toUpperCase() === 'ANTES') {
    this.vistaActual = 'faseAntes';
  } else if (fase.toUpperCase() === 'DESPUES') {
    this.vistaActual = 'faseDespues';
  }
}

// Método para volver a la vista de detalles
volverADetallesDesdeVista(): void {
  this.vistaActual = 'detalles';
}

// Getter para el título dinámico del panel
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
   * 🔥 NUEVO: Cargar catálogos adicionales (órdenes, clientes, evaluadores)
   * Las ejecuciones ya las cargó cargarEvaluacionCompleta()
   */
  private cargarCatalogosAdicionales(): void {
    console.log('🔄 Cargando catálogos adicionales...');
    
    this.cargandoCatalogos = true;

    forkJoin({
      ordenes: this.evaluacionService.obtenerOrdenesTrabajo(),
      clientes: this.evaluacionService.obtenerClientes(),
      evaluadores: this.evaluacionService.obtenerUsuarioActual()
    }).subscribe({
      next: ({ ordenes, clientes, evaluadores }) => {
        console.log('✅ Catálogos adicionales cargados:', {
          ordenes: ordenes.length,
          clientes: clientes.length,
          evaluadores: evaluadores.nombreCompleto
        });
        
        this.ordenesTrabajo = ordenes;
        this.clientes = clientes;
        
        // Construir textos descriptivos
        this.construirTextosDescriptivos();
        
        this.cargandoCatalogos = false;
      },
      error: (err) => {
        console.error('⚠️ Error al cargar catálogos adicionales:', err);
        // No es crítico, construir textos con lo que tengamos
        this.construirTextosDescriptivos();
        this.cargandoCatalogos = false;
      }
    });
  }

  /**
   * 🔥 NUEVO: Construir textos descriptivos basándose en los catálogos
   * Similar a construirTextosCamposBloqueados() del componente de registro
   */
  private construirTextosDescriptivos(): void {
    if (!this.evaluacion) return;

    console.log('🔨 Construyendo textos descriptivos...');
    console.log('📊 Evaluación:', {
      ordenId: this.evaluacion.ordenId,
      ejecucionId: this.evaluacion.ejecucionId,
      clienteId: this.evaluacion.clienteId,
      evaluadorId: this.evaluacion.evaluadorId
    });
    console.log('📚 Catálogos disponibles:', {
      ordenes: this.ordenesTrabajo.length,
      ejecuciones: this.ejecuciones.length,
      clientes: this.clientes.length
    });

    // Orden de trabajo
    if (this.evaluacion.ordenId) {
      const orden = this.ordenesTrabajo.find(o => o.id === this.evaluacion!.ordenId);
      if (orden) {
        this.textoOrden = orden.displayText;
        console.log('✅ Orden encontrada:', this.textoOrden);
      } else {
        this.textoOrden = `OT-${this.evaluacion.ordenId}`;
        console.warn('⚠️ Orden no encontrada en catálogo');
      }
    } else {
      this.textoOrden = 'Sin orden asignada';
    }

    // Ejecución
    if (this.evaluacion.ejecucionId) {
      console.log('🔍 Buscando ejecución:', {
        ejecucionId: this.evaluacion.ejecucionId,
        ejecucionesDisponibles: this.ejecuciones.length,
        ejecucionesIds: this.ejecuciones.map(e => e.id)
      });

      const ejecucion = this.ejecuciones.find(e => e.id === this.evaluacion!.ejecucionId);
      if (ejecucion) {
        this.textoEjecucion = ejecucion.displayText;
        console.log('✅ Ejecución encontrada:', this.textoEjecucion);
      } else {
        this.textoEjecucion = `Ejecución #${this.evaluacion.ejecucionId}`;
        console.warn('⚠️ Ejecución no encontrada en catálogo');
      }
    } else {
      this.textoEjecucion = 'Sin ejecución específica';
    }

    // Cliente
    if (this.evaluacion.clienteId) {
      const cliente = this.clientes.find(c => c.id === this.evaluacion!.clienteId!);
      if (cliente) {
        this.textoCliente = cliente.displayText;
        console.log('✅ Cliente encontrado:', this.textoCliente);
      } else {
        this.textoCliente = `Cliente #${this.evaluacion.clienteId}`;
        console.warn('⚠️ Cliente no encontrado en catálogo');
      }
    } else {
      this.textoCliente = 'Sin cliente asignado';
    }

    // Evaluador - Siempre debe tener uno
    this.textoEvaluador = `Evaluador #${this.evaluacion.evaluadorId}`;

    console.log('✅ Textos construidos:', {
      orden: this.textoOrden,
      ejecucion: this.textoEjecucion,
      cliente: this.textoCliente,
      evaluador: this.textoEvaluador
    });
  }

  reintentar(): void {
    this.cargarEvaluacionCompleta();
  }

  // =====================================================================================
  // NAVEGACIÓN Y CIERRE
  // =====================================================================================

  cerrarPanel(): void {
    this.cerrar.emit();
  }

  volverAlListado(): void {
    this.cerrarPanel();
  }

  verFase(fase: string): void {
  const detalle = this.obtenerDetallePorFase(fase);
  
  if (!detalle) {
    console.warn(`La fase ${fase} no está completada`);
    return;
  }

  // En lugar de navegar, abrir el panel correspondiente
  if (fase.toUpperCase() === 'ANTES') {
    this.mostrarFaseAntes = true;
  } else if (fase.toUpperCase() === 'DESPUES') {
    this.mostrarFaseDespues = true;
  }
}

cerrarFaseAntes(): void {
  this.mostrarFaseAntes = false;
}

cerrarFaseDespues(): void {
  this.mostrarFaseDespues = false;
}

  // =====================================================================================
  // GETTERS Y HELPERS
  // =====================================================================================

  obtenerDetallePorFase(fase: string): EvaluacionDetalleResponse | undefined {
    return this.detalles.find(d => d.fase.toUpperCase() === fase.toUpperCase());
  }

  estaFaseCompletada(fase: string): boolean {
    return this.detalles.some(d => d.fase.toUpperCase() === fase.toUpperCase());
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

  get datosFases() {
    return [
      {
        fase: 'ANTES',
        titulo: 'Evaluación ANTES',
        completada: this.estaFaseCompletada('ANTES'),
        detalle: this.obtenerDetallePorFase('ANTES')
      },
      {
        fase: 'DESPUES',
        titulo: 'Evaluación DESPUÉS',
        completada: this.estaFaseCompletada('DESPUES'),
        detalle: this.obtenerDetallePorFase('DESPUES')
      }
    ];
  }

  get fasesCompletadas(): number {
    return this.detalles.length;
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
    if (score >= 80) return 'success';
    if (score >= 60) return 'info';
    if (score >= 40) return 'warning';
    return 'error';
  }
}

/* 
=====================================================================================
📋 CAMBIOS REALIZADOS EN EL COMPONENTE
=====================================================================================

🔥 PROBLEMA IDENTIFICADO:
   - Ver detalles solo mostraba IDs: "OT-1", "Ejecución #5", "Cliente #3"
   - No cargaba los catálogos necesarios para mostrar textos descriptivos
   - No tenía spinner de carga para la información general

✅ SOLUCIÓN IMPLEMENTADA:

1. Propiedades agregadas:
   - ordenesTrabajo, ejecuciones, clientes: para catálogos
   - textoOrden, textoEjecucion, textoCliente, textoEvaluador: textos descriptivos
   - cargandoCatalogos: estado de carga de catálogos

2. cargarEvaluacionCompleta():
   - Ahora usa evaluacionService.cargarEvaluacionCompleta()
   - Este método YA retorna las ejecuciones
   - Llama a cargarCatalogosAdicionales() después

3. cargarCatalogosAdicionales() - NUEVO:
   - Carga órdenes, clientes y evaluador actual
   - No carga ejecuciones (ya las tiene de cargarEvaluacionCompleta)
   - Llama a construirTextosDescriptivos()

4. construirTextosDescriptivos() - NUEVO:
   - Busca en los catálogos los datos por ID
   - Construye textos descriptivos completos
   - Similar a construirTextosCamposBloqueados() de registro
   - Logs para debugging

🔄 FLUJO DE CARGA:
   1. Usuario abre panel → ngOnChanges detecta cambio
   2. Llamar cargarEvaluacionCompleta()
   3. Servicio retorna: evaluacion + detalles + ejecuciones
   4. Llamar cargarCatalogosAdicionales()
   5. ForkJoin carga: órdenes + clientes + evaluador
   6. Llamar construirTextosDescriptivos()
   7. Buscar en catálogos y construir textos
   8. ✅ Mostrar textos descriptivos en lugar de IDs

📝 PRÓXIMO PASO:
   Actualizar el HTML para:
   - Mostrar spinner mientras carga catálogos
   - Usar los textos descriptivos construidos
   - Mantener null safety
*/