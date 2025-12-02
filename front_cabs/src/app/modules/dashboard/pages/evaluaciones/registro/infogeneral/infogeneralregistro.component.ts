import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../../../../../../environments/environment';
import {
  EvaluacionService,
  OrdenTrabajoSelect,
  EjecucionSelect,
  UsuarioSelect
} from '../../../../../../core/services/evaluaciones.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { 
  FormularioInfoGeneral,
  mapResponseToFormulario,
  mapDetalleResponseToDatos,
  UsuarioResponseDto
} from '../../../../../../core/models/evaluaciones.interface';
import { FaseAntesModalComponent } from '../fases/faseantesregistro.component';
import { FaseDespuesModalComponent } from '../fases/fasedespuesregistro.component';

// ==================== SHARED COMPONENTS ====================
import {
  FormInputComponent,
  FormSelectComponent,
  SelectOption,
  FormTextareaComponent,
  FormToggleComponent,
  LockedFieldComponent,
  FormRowComponent,
  FormSectionComponent,
  FormInfoAlertComponent,
  ScoreDisplayComponent,
  LoadingOverlayComponent
} from '../../../../../../shared/~exports/form-system.index';

@Component({
  selector: 'app-infogeneral',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    FaseAntesModalComponent,
    FaseDespuesModalComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
    FormToggleComponent,
    LockedFieldComponent,
    FormRowComponent,
    FormSectionComponent,
    FormInfoAlertComponent,
    ScoreDisplayComponent,
    LoadingOverlayComponent
  ],
  templateUrl: './infogeneralregistro.component.html',
  styleUrls: ['./infogeneralregistro.component.css']
})
export class InfogeneralComponent implements OnInit, OnDestroy {
  @Output() cerrarModal = new EventEmitter<void>();
  @Output() evaluacionGuardada = new EventEmitter<number>();

  private destroy$ = new Subject<void>();
  guardando = false;
  cargando = false;

  vistaActual: 'infoGeneral' | 'faseAntes' | 'faseDespues' = 'infoGeneral';
  modoOperacion: 'crear' | 'editar' = 'crear';
  evaluacionId: number | null = null;

  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  evaluadores: UsuarioSelect[] = [];
  usuarioActual: UsuarioSelect | null = null;

  // ✅ PROPIEDADES CACHEADAS (en lugar de getters)
  ordenesOptions: SelectOption[] = [];
  ejecucionesOptions: SelectOption[] = [];
  evaluadoresOptions: SelectOption[] = [];

  private catalogosCargados = false;

  textoOrdenBloqueada: string = '';
  textoEjecucionBloqueada: string = '';
  textoEvaluadorBloqueado: string = '';

  cargandoCatalogos = {
    ordenes: false,
    ejecuciones: false,
    usuario: false
  };

  formulario: FormularioInfoGeneral = {
    ordenTrabajoId: '',
    ejecucionId: '',
    clienteId: '',
    evaluadorId: '',
    objetivo: '',
    comentariosGenerales: '',
    scoreCalidad: 0,
    requiereSeguimiento: false,
    notasSeguimiento: ''
  };

  fases = {
    antes: {
      estado: 'sin-inicializar' as 'completada' | 'sin-completar' | 'sin-inicializar',
      titulo: 'Evaluación Antes',
      estatus: 'No inicializada'
    },
    despues: {
      estado: 'sin-inicializar' as 'completada' | 'sin-completar' | 'sin-inicializar',
      titulo: 'Evaluación DESPUÉS',
      estatus: 'No inicializada'
    }
  };

  // ✅ SOLO GETTERS SIMPLES (no arrays)
  get mensajeInformativo(): string {
    return this.modoOperacion === 'editar'
      ? 'Los campos Orden, Ejecución y Evaluador no se pueden modificar'
      : 'Complete todos los campos requeridos para crear la evaluación';
  }

  get tituloSeccionActual(): string {
    switch (this.vistaActual) {
      case 'infoGeneral': return 'Información General';
      case 'faseAntes': return 'Evaluación ANTES';
      case 'faseDespues': return 'Evaluación DESPUÉS';
      default: return 'Información General';
    }
  }

  get tituloFormulario(): string {
    return this.modoOperacion === 'editar' && this.evaluacionId
      ? `Editar Evaluación #${this.evaluacionId}`
      : 'Nueva Evaluación';
  }

  get badgeEstado(): { texto: string; clase: string } {
    return this.modoOperacion === 'editar'
      ? { texto: 'EDITANDO', clase: 'badge-editar' }
      : { texto: 'NUEVA', clase: 'badge-crear' };
  }

  get estatusSeguimiento(): string {
    return this.formulario.requiereSeguimiento ? 'Requiere seguimiento' : 'Sin seguimiento';
  }

  get camposPrincipalesBloqueados(): boolean {
    return this.modoOperacion === 'editar';
  }

  get algunCatalogoCargando(): boolean {
    return Object.values(this.cargandoCatalogos).some(cargando => cargando);
  }

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    console.log('InfoGeneral ngOnInit - Componente montado');
    this.detectarModoDesdeRuta();

    if (!this.catalogosCargados) {
      this.cargarCatalogosAsync();
      this.catalogosCargados = true;
      console.log('Catálogos iniciando carga');
    } else {
      console.log('Usando catálogos en caché');
    }

    if (this.modoOperacion === 'editar') {
      this.construirTextosCamposBloqueados();
    }

    this.suscribirCambios();
    this.actualizarEstadoFases();
  }

  ngOnDestroy(): void {
    console.log('InfoGeneral ngOnDestroy');
    this.destroy$.next();
    this.destroy$.complete();
  }

  navegarASeccion(seccion: 'infoGeneral' | 'faseAntes' | 'faseDespues'): void {
    console.log('Navegando a:', seccion);
    this.vistaActual = seccion;
    if (seccion === 'infoGeneral') {
      this.actualizarEstadoFases();
    }
  }

  private detectarModoDesdeRuta(): void {
    const idCompartido = this.sharedService.getEvaluacionId();
    
    if (idCompartido) {
      this.modoOperacion = 'editar';
      this.evaluacionId = idCompartido;
      console.log('Modo EDITAR detectado desde SharedService, ID:', idCompartido);
      
      const infoGuardada = this.sharedService.getInfoGeneral();
      if (infoGuardada) {
        this.formulario = { ...infoGuardada };
        console.log('Datos cargados desde SharedService');
        
        if (infoGuardada.ordenTrabajoId) {
          const ordenId = parseInt(infoGuardada.ordenTrabajoId);
          if (!isNaN(ordenId)) {
            this.cargarEjecucionesPorOrden(ordenId);
          }
        }
        
        this.actualizarEstadoFases();
        return;
      }
    }

    const modoData = this.route.snapshot.data['modo'] as 'crear' | 'editar' | undefined;
    const idParam = this.route.snapshot.paramMap.get('id');
    console.log('🔍 Detectando modo desde ruta:', { modoData, idParam });

    if (modoData === 'editar' && idParam) {
      this.modoOperacion = 'editar';
      this.evaluacionId = parseInt(idParam, 10);

      if (isNaN(this.evaluacionId)) {
        console.error('ID inválido');
        alert('ID de evaluación inválido');
        this.router.navigate(['/dashboard/evaluaciones']);
        return;
      }

      const yaCargado = this.sharedService.getEvaluacionId() === this.evaluacionId;

      if (yaCargado) {
        console.log('Datos ya en SharedService');
        const infoGuardada = this.sharedService.getInfoGeneral();
        if (infoGuardada) {
          this.formulario = { ...infoGuardada };
          
          if (infoGuardada.ordenTrabajoId) {
            const ordenId = parseInt(infoGuardada.ordenTrabajoId);
            if (!isNaN(ordenId)) {
              this.cargarEjecucionesPorOrden(ordenId);
            }
          }
        }
        this.actualizarEstadoFases();
        return;
      }

      this.cargarEvaluacionExistente(this.evaluacionId);
    } else {
      this.modoOperacion = 'crear';
      this.evaluacionId = null;
      console.log('Modo CREAR detectado');
      this.sharedService.limpiar();
    }
  }

  // ✅ ELIMINADO async/await y toPromise()
  private cargarEvaluacionExistente(id: number): void {
    this.cargando = true;
    console.log('Cargando evaluación completa ID:', id);

    this.evaluacionService.cargarEvaluacionCompleta(id).subscribe({
      next: (data) => {
        console.log('Datos recibidos:', data);

        const infoGeneral = mapResponseToFormulario(data.evaluacion);
        const datosAntes = data.detalleAntes ? mapDetalleResponseToDatos(data.detalleAntes, data.fotosAntes) : undefined;
        const datosDespues = data.detalleDespues ? mapDetalleResponseToDatos(data.detalleDespues, data.fotosDespues) : undefined;

        this.formulario = { ...infoGeneral };

        // ✅ Cargar ejecuciones si hay orden
        if (infoGeneral.ordenTrabajoId) {
          const ordenId = parseInt(infoGeneral.ordenTrabajoId);
          if (!isNaN(ordenId)) {
            this.cargarEjecucionesPorOrden(ordenId);
          }
        }

        this.construirTextosCamposBloqueados();
        this.sharedService.cargarEvaluacion(id, infoGeneral, datosAntes, datosDespues);
        this.actualizarEstadoFases();
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar evaluación:', error);
        alert('Error al cargar la evaluación');
        this.router.navigate(['/dashboard/evaluaciones']);
        this.cargando = false;
      }
    });
  }

  private actualizarEstadoFases(): void {
    const datosAntes = this.sharedService.getFaseAntes();
    const datosDespues = this.sharedService.getFaseDespues();

    this.fases.antes.estado = this.determinarEstadoFase(datosAntes);
    this.fases.despues.estado = this.determinarEstadoFase(datosDespues);
    this.fases.antes.estatus = this.obtenerTextoEstado(this.fases.antes.estado);
    this.fases.despues.estatus = this.obtenerTextoEstado(this.fases.despues.estado);
  }

  private determinarEstadoFase(datosFase: any): 'completada' | 'sin-completar' | 'sin-inicializar' {
    if (!datosFase || this.estaFaseVacia(datosFase)) return 'sin-inicializar';
    if (datosFase.scoreFase !== null && datosFase.scoreFase !== undefined && datosFase.scoreFase > 0) return 'completada';
    return 'sin-completar';
  }

  private estaFaseVacia(datosFase: any): boolean {
    if (!datosFase) return true;
    const camposConValor = [
      datosFase.lugar,
      datosFase.comentarios,
      datosFase.fotosInicio?.length,
      datosFase.fotosFin?.length,
      datosFase.materialesUsados?.length,
      datosFase.evaluacionDetalle?.length
    ].filter(valor => {
      if (typeof valor === 'string') return valor.trim() !== '';
      if (typeof valor === 'number') return valor > 0;
      return false;
    });
    return camposConValor.length === 0;
  }

  private obtenerTextoEstado(estado: string): string {
    switch (estado) {
      case 'completada': return 'Completada';
      case 'sin-completar': return 'Sin completar';
      case 'sin-inicializar': return 'No inicializada';
      default: return 'Desconocido';
    }
  }

  private suscribirCambios(): void {
    this.sharedService.infoGeneral$.pipe(takeUntil(this.destroy$)).subscribe(info => {
      if (info && JSON.stringify(info) !== JSON.stringify(this.formulario)) {
        this.formulario = { ...info };
      }
    });
  }

  private construirTextosCamposBloqueados(): void {
    const ordenSeleccionada = this.ordenesTrabajo.find(o => o.id.toString() === this.formulario.ordenTrabajoId);
    this.textoOrdenBloqueada = ordenSeleccionada?.displayText || `OT-${this.formulario.ordenTrabajoId}`;

    const ejecucionSeleccionada = this.ejecuciones.find(e => e.id.toString() === this.formulario.ejecucionId);
    this.textoEjecucionBloqueada = ejecucionSeleccionada?.displayText || 'Sin ejecución';

    const evaluadorSeleccionado = this.evaluadores.find(e => e.id.toString() === this.formulario.evaluadorId);
    this.textoEvaluadorBloqueado = evaluadorSeleccionado?.displayText || `Evaluador #${this.formulario.evaluadorId}`;
  }

  private cargarCatalogosAsync(): void {
    console.log('Iniciando carga de catálogos...');

    // ==================== USUARIOS ====================
    this.cargandoCatalogos.usuario = true;

    forkJoin({
      usuarioActual: this.evaluacionService.obtenerUsuarioActual(),
      todosUsuarios: this.http.get(`${environment.apiUrl}/api/Auth/usuarios`, {
        responseType: 'text'
      })
    }).subscribe({
      next: ({ usuarioActual, todosUsuarios }) => {
        this.usuarioActual = usuarioActual;
        console.log('Usuario actual:', this.usuarioActual?.nombreCompleto);

        let usuariosArray: UsuarioResponseDto[] = [];
        try {
          const parsed = typeof todosUsuarios === 'string' 
            ? JSON.parse(todosUsuarios) 
            : todosUsuarios;
          
          if (Array.isArray(parsed)) {
            usuariosArray = parsed;
          } else if (parsed && typeof parsed === 'object') {
            usuariosArray = parsed.data || parsed.items || parsed.usuarios || [];
          }
        } catch (error) {
          console.error('Error al parsear usuarios:', error);
          usuariosArray = [];
        }

        this.evaluadores = usuariosArray.map((u: UsuarioResponseDto) => ({
          id: u.id,
          nombreCompleto: u.nombreCompleto || `${u.nombre} ${u.apellido}`,
          rol: u.rol || 'Desconocido',
          displayText: `${u.nombreCompleto || u.nombre} ${u.apellido} (${u.rol || 'Sin rol'})`
        }));

        // ✅ ACTUALIZAR OPTIONS CACHEADAS
        this.evaluadoresOptions = this.evaluadores.map(evaluador => ({
          value: evaluador.id.toString(),
          label: evaluador.displayText
        }));

        console.log('Total de usuarios cargados:', this.evaluadores.length);
        this.cargandoCatalogos.usuario = false;

        if (this.modoOperacion === 'crear' && !this.formulario.evaluadorId && this.usuarioActual) {
          this.formulario.evaluadorId = this.usuarioActual.id.toString();
          this.onFormularioChange();
        }

        if (this.modoOperacion === 'editar') {
          this.construirTextosCamposBloqueados();
        }
      },
      error: (error) => {
        console.error('Error al cargar usuarios:', error);
        this.usuarioActual = null;
        this.evaluadores = [];
        this.evaluadoresOptions = [];
        this.cargandoCatalogos.usuario = false;
      }
    });

    // ==================== ÓRDENES ====================
    this.cargandoCatalogos.ordenes = true;
    
    this.evaluacionService.obtenerOrdenesTrabajo().subscribe({
      next: (ordenes) => {
        this.ordenesTrabajo = ordenes || [];
        
        // ✅ ACTUALIZAR OPTIONS CACHEADAS
        this.ordenesOptions = this.ordenesTrabajo.map(orden => ({
          value: orden.id.toString(),
          label: orden.displayText
        }));
        
        console.log('Órdenes cargadas:', this.ordenesTrabajo.length);
        this.cargandoCatalogos.ordenes = false;

        if (this.modoOperacion === 'crear' && this.formulario.ordenTrabajoId) {
          const ordenId = parseInt(this.formulario.ordenTrabajoId);
          if (!isNaN(ordenId)) {
            this.cargarEjecucionesPorOrden(ordenId);
          }
        }
      },
      error: (error) => {
        console.error('Error al cargar órdenes:', error);
        this.ordenesTrabajo = [];
        this.ordenesOptions = [];
        this.cargandoCatalogos.ordenes = false;
      }
    });

    console.log('Catálogos en proceso de carga...');
  }

  private cargarEjecucionesPorOrden(ordenId: number): void {
    this.cargandoCatalogos.ejecuciones = true;
    
    this.evaluacionService.obtenerEjecuciones(ordenId).subscribe({
      next: (ejecuciones) => {
        this.ejecuciones = ejecuciones || [];
        
        // ✅ ACTUALIZAR OPTIONS CACHEADAS
        this.ejecucionesOptions = this.ejecuciones.map(ejecucion => ({
          value: ejecucion.id.toString(),
          label: ejecucion.displayText
        }));
        
        console.log('Ejecuciones cargadas:', this.ejecuciones.length);
        this.cargandoCatalogos.ejecuciones = false;
      },
      error: (error) => {
        console.error('Error al cargar ejecuciones:', error);
        this.ejecuciones = [];
        this.ejecucionesOptions = [];
        this.cargandoCatalogos.ejecuciones = false;
      }
    });
  }

  onOrdenChange(): void {
    if (this.modoOperacion === 'editar') return;

    const ordenId = this.formulario.ordenTrabajoId;
    if (!ordenId) {
      this.ejecuciones = [];
      this.ejecucionesOptions = []; // ✅ Limpiar options también
      this.formulario.ejecucionId = '';
      this.onFormularioChange();
      return;
    }

    const ordenIdNum = parseInt(ordenId);
    if (isNaN(ordenIdNum)) return;

    this.cargarEjecucionesPorOrden(ordenIdNum);
    this.onFormularioChange();
  }

  onFormularioChange(): void {
    this.sharedService.setInfoGeneral({ ...this.formulario });
  }

  alternarSeguimiento(): void {
    this.formulario.requiereSeguimiento = !this.formulario.requiereSeguimiento;
    this.onFormularioChange();
  }

  async guardarEvaluacion(): Promise<void> {
    if (!this.formulario.ordenTrabajoId || !this.formulario.evaluadorId) {
      alert('Complete campos obligatorios: Orden de Trabajo y Evaluador');
      return;
    }

    this.onFormularioChange();
    this.sharedService.actualizarScore();

    const datosAntes = this.sharedService.getFaseAntes();
    const datosDespues = this.sharedService.getFaseDespues();

    const erroresValidacion: string[] = [];

    if (datosAntes && !this.estaFaseVacia(datosAntes)) {
      if (!datosAntes.lugar || datosAntes.lugar.trim() === '') {
        erroresValidacion.push('La Evaluación ANTES tiene datos pero falta el campo "Lugar"');
      }
    }

    if (datosDespues && !this.estaFaseVacia(datosDespues)) {
      if (!datosDespues.lugar || datosDespues.lugar.trim() === '') {
        erroresValidacion.push('La Evaluación DESPUÉS tiene datos pero falta el campo "Lugar"');
      }
    }

    if (erroresValidacion.length > 0) {
      const mensajeError = 'No se puede guardar la evaluación:\n\n' + 
        erroresValidacion.map((err, idx) => `${idx + 1}. ${err}`).join('\n') +
        '\n\nPor favor, complete el campo "Lugar" en las fases correspondientes o elimine todos los datos de la fase si no desea incluirla.';
      alert(mensajeError);
      return;
    }

    const dto = this.sharedService.construirDTO();
    if (!dto) {
      alert('Error al preparar datos');
      return;
    }

    if (dto.faseAntes && this.estaFaseVacia(datosAntes)) {
      delete dto.faseAntes;
    }

    if (dto.faseDespues && this.estaFaseVacia(datosDespues)) {
      delete dto.faseDespues;
    }

    const mensaje = this.modoOperacion === 'editar'
      ? '¿Guardar cambios en la evaluación?\n\nNota: Los campos Orden, Ejecución y Evaluador no se pueden modificar.'
      : '¿Guardar la evaluación completa?';

    if (!confirm(mensaje)) return;

    try {
      this.sharedService.setGuardando(true);
      const resultado = await this.evaluacionService.guardarEvaluacionCompleta(dto);

      const mensajeExito = this.modoOperacion === 'editar'
        ? `Evaluación #${resultado.id} actualizada`
        : `Evaluación guardada con ID: ${resultado.id}`;

      alert(mensajeExito);
      this.sharedService.setEvaluacionId(resultado.id);

      if (this.modoOperacion === 'crear') {
        this.modoOperacion = 'editar';
        this.evaluacionId = resultado.id;
      }

      this.evaluacionGuardada.emit(resultado.id);
      this.sharedService.limpiar();
      this.catalogosCargados = false;
      this.cerrarModal.emit();
    } catch (error) {
      console.error('Error al guardar:', error);
      alert('Error al guardar la evaluación');
    } finally {
      this.sharedService.setGuardando(false);
    }
  }

  cerrarFormulario(): void {
    const mensaje = this.modoOperacion === 'editar'
      ? '¿Salir sin guardar cambios?'
      : '¿Cerrar? Los datos no guardados se perderán';

    if (confirm(mensaje)) {
      this.sharedService.limpiar();
      this.catalogosCargados = false;
      this.cerrarModal.emit();
    }
  }
}