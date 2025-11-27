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

@Component({
  selector: 'app-infogeneral',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    FaseAntesModalComponent,
    FaseDespuesModalComponent
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

  // Control de pestañas/tabs en lugar de modales
  vistaActual: 'infoGeneral' | 'faseAntes' | 'faseDespues' = 'infoGeneral';

  // Detectar modo desde la ruta
  modoOperacion: 'crear' | 'editar' = 'crear';
  evaluacionId: number | null = null;

  // Catálogos (se cargan UNA SOLA VEZ)
  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  evaluadores: UsuarioSelect[] = [];
  usuarioActual: UsuarioSelect | null = null;

  // Flags para controlar carga única
  private catalogosCargados = false;

  // Textos para campos bloqueados
  textoOrdenBloqueada: string = '';
  textoEjecucionBloqueada: string = '';
  textoEvaluadorBloqueado: string = '';

  // Estados de carga por catálogo
  cargandoCatalogos = {
    ordenes: false,
    ejecuciones: false,
    usuario: false
  };

  // Formulario
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

  // ACTUALIZACIÓN: Estado de fases con 3 estados posibles
  // - 'completada': Existe registro Y tiene scoreFase
  // - 'sin-completar': Existe registro PERO scoreFase es null
  // - 'sin-inicializar': NO existe registro en la BD
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

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  async ngOnInit(): Promise<void> {
    console.log('InfoGeneral ngOnInit - Componente montado');
    
    await this.detectarModoDesdeRuta();
    
    if (!this.catalogosCargados) {
      await this.cargarCatalogosAsync();
      this.catalogosCargados = true;
      console.log('Catálogos cargados y cacheados');
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

  // Métodos para navegación entre pestañas
  navegarASeccion(seccion: 'infoGeneral' | 'faseAntes' | 'faseDespues'): void {
    console.log('Navegando a:', seccion);
    this.vistaActual = seccion;
    // Actualizar estado de fases cuando se navega
    if (seccion === 'infoGeneral') {
      this.actualizarEstadoFases();
    }
  }

  get tituloSeccionActual(): string {
    switch (this.vistaActual) {
      case 'infoGeneral':
        return 'Información General';
      case 'faseAntes':
        return 'Evaluación ANTES';
      case 'faseDespues':
        return 'Evaluación DESPUÉS';
      default:
        return 'Información General';
    }
  }

  private async detectarModoDesdeRuta(): Promise<void> {
  // Primero verificar si hay un ID en el servicio compartido (modo modal de edición)
  const idCompartido = this.sharedService.getEvaluacionId();
  
  if (idCompartido) {
    this.modoOperacion = 'editar';
    this.evaluacionId = idCompartido;
    console.log('Modo EDITAR detectado desde SharedService, ID:', idCompartido);
    
    const infoGuardada = this.sharedService.getInfoGeneral();
    if (infoGuardada) {
      this.formulario = { ...infoGuardada };
      console.log('Datos cargados desde SharedService');
      
      // CARGAR EJECUCIONES SI HAY ORDEN
      if (infoGuardada.ordenTrabajoId) {
        const ordenId = parseInt(infoGuardada.ordenTrabajoId);
        if (!isNaN(ordenId)) {
          try {
            this.ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise() || [];
            console.log('Ejecuciones cargadas:', this.ejecuciones.length);
          } catch (error) {
            console.error('Error al cargar ejecuciones:', error);
            this.ejecuciones = [];
          }
        }
      }
      
      this.actualizarEstadoFases();
      return;
    }
  }

  // Si no hay datos en el servicio, verificar la ruta
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
        
        // CARGAR EJECUCIONES
        if (infoGuardada.ordenTrabajoId) {
          const ordenId = parseInt(infoGuardada.ordenTrabajoId);
          if (!isNaN(ordenId)) {
            try {
              this.ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise() || [];
              console.log('Ejecuciones cargadas:', this.ejecuciones.length);
            } catch (error) {
              console.error('Error al cargar ejecuciones:', error);
              this.ejecuciones = [];
            }
          }
        }
      }
      this.actualizarEstadoFases();
      return;
    }

    await this.cargarEvaluacionExistente(this.evaluacionId);
  } else {
    this.modoOperacion = 'crear';
    this.evaluacionId = null;
    console.log('Modo CREAR detectado');
    this.sharedService.limpiar();
  }
}

  private async cargarEvaluacionExistente(id: number): Promise<void> {
  try {
    this.cargando = true;
    console.log('Cargando evaluación completa ID:', id);

    this.evaluacionService.cargarEvaluacionCompleta(id).subscribe({
      next: async (data) => {
        console.log('Datos recibidos:', data);

        const infoGeneral = mapResponseToFormulario(data.evaluacion);
        
        const datosAntes = data.detalleAntes 
          ? mapDetalleResponseToDatos(data.detalleAntes, data.fotosAntes)
          : undefined;

        const datosDespues = data.detalleDespues
          ? mapDetalleResponseToDatos(data.detalleDespues, data.fotosDespues)
          : undefined;

        this.formulario = { ...infoGeneral };

        // CARGAR EJECUCIONES ANTES DE CONSTRUIR TEXTOS
        if (infoGeneral.ordenTrabajoId) {
          const ordenId = parseInt(infoGeneral.ordenTrabajoId);
          if (!isNaN(ordenId)) {
            try {
              console.log('Cargando ejecuciones para orden:', ordenId);
              this.ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise() || [];
              console.log('Ejecuciones cargadas:', this.ejecuciones.length);
            } catch (error) {
              console.error('Error al cargar ejecuciones:', error);
              this.ejecuciones = [];
            }
          }
        }

        // AHORA SÍ CONSTRUIR TEXTOS (después de cargar ejecuciones)
        this.construirTextosCamposBloqueados();

        // Cargar datos en SharedService
        this.sharedService.cargarEvaluacion(
          id,
          infoGeneral,
          datosAntes,
          datosDespues
        );

        console.log('Evaluación cargada en SharedService');
        
        // Actualizar estados de fases
        this.actualizarEstadoFases();
        console.log('Estados de fases actualizados después de cargar');
        
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar evaluación:', error);
        alert('Error al cargar la evaluación');
        this.router.navigate(['/dashboard/evaluaciones']);
        this.cargando = false;
      }
    });
  } catch (error) {
    console.error('Error al cargar evaluación:', error);
    alert('Error al cargar la evaluación');
    this.router.navigate(['/dashboard/evaluaciones']);
    this.cargando = false;
  }
}

  // ACTUALIZACIÓN: Nuevo método para determinar estado de fase según lógica correcta
  /**
   * Determina el estado de una fase basándose en:
   * - COMPLETADA: Tiene scoreFase Y scoreFase > 0
   * - SIN COMPLETAR: Tiene al menos un campo lleno (que no sea score) PERO scoreFase es null/0
   * - SIN INICIALIZAR: Todos los campos están vacíos (no hay registro o registro completamente vacío)
   * @returns Estado: 'completada' | 'sin-completar' | 'sin-inicializar'
   */
  private actualizarEstadoFases(): void {
    const datosAntes = this.sharedService.getFaseAntes();
    const datosDespues = this.sharedService.getFaseDespues();

    console.log('Actualizando estado de fases:', { 
      datosAntes: datosAntes ? 'TIENE DATOS' : 'NULL', 
      datosDespues: datosDespues ? 'TIENE DATOS' : 'NULL',
      datosAntesCompleto: datosAntes,
      datosDespuesCompleto: datosDespues
    });

    // FASE ANTES
    this.fases.antes.estado = this.determinarEstadoFase(datosAntes);
    this.fases.antes.estatus = this.obtenerTextoEstado(this.fases.antes.estado, datosAntes?.scoreFase);

    // FASE DESPUÉS
    this.fases.despues.estado = this.determinarEstadoFase(datosDespues);
    this.fases.despues.estatus = this.obtenerTextoEstado(this.fases.despues.estado, datosDespues?.scoreFase);

    console.log('Estados actualizados:', {
      antes: `${this.fases.antes.estado} - ${this.fases.antes.estatus}`,
      despues: `${this.fases.despues.estado} - ${this.fases.despues.estatus}`
    });
  }

  /**
   * Determina el estado de una fase individual
   */
  private determinarEstadoFase(datosFase: any): 'completada' | 'sin-completar' | 'sin-inicializar' {
    // 1. SIN INICIALIZAR: No hay datos o todos los campos están vacíos
    if (!datosFase || this.estaFaseVacia(datosFase)) {
      return 'sin-inicializar';
    }

    // 2. COMPLETADA: Tiene scoreFase Y es mayor a 0
    if (datosFase.scoreFase !== null && 
        datosFase.scoreFase !== undefined && 
        datosFase.scoreFase > 0) {
      return 'completada';
    }

    // 3. SIN COMPLETAR: Tiene al menos un campo lleno pero sin score válido
    if (this.tieneAlgunCampoLleno(datosFase)) {
      return 'sin-completar';
    }

    // Por defecto, sin inicializar
    return 'sin-inicializar';
  }

  /**
   * Verifica si una fase está completamente vacía (sin datos)
   */
  private estaFaseVacia(datosFase: any): boolean {
    if (!datosFase) return true;

    // Verificar si todos los campos relevantes están vacíos
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
   * Obtiene el texto de estatus según el estado
   */
  private obtenerTextoEstado(estado: 'completada' | 'sin-completar' | 'sin-inicializar', score?: number): string {
    switch (estado) {
      case 'completada':
        return `Completada - Score: ${score}/100`;
      case 'sin-completar':
        return 'Iniciada - Sin completar';
      case 'sin-inicializar':
        return 'No inicializada';
      default:
        return 'No inicializada';
    }
  }

  private suscribirCambios(): void {
    this.sharedService.faseAntes$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('Datos ANTES actualizados en shared service');
        this.actualizarEstadoFases();
      });

    this.sharedService.faseDespues$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('Datos DESPUÉS actualizados en shared service');
        this.actualizarEstadoFases();
      });

    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });
  }

  private construirTextosCamposBloqueados(): void {
    const ordenSeleccionada = this.ordenesTrabajo.find(
      o => o.id.toString() === this.formulario.ordenTrabajoId
    );
    this.textoOrdenBloqueada = ordenSeleccionada 
      ? ordenSeleccionada.displayText 
      : `OT-${this.formulario.ordenTrabajoId}`;

    const ejecucionSeleccionada = this.ejecuciones.find(
      e => e.id.toString() === this.formulario.ejecucionId
    );
    this.textoEjecucionBloqueada = ejecucionSeleccionada 
      ? ejecucionSeleccionada.displayText 
      : 'Sin ejecución';

    const evaluadorSeleccionado = this.evaluadores.find(
      e => e.id.toString() === this.formulario.evaluadorId
    );
    this.textoEvaluadorBloqueado = evaluadorSeleccionado 
      ? evaluadorSeleccionado.displayText 
      : `Evaluador #${this.formulario.evaluadorId}`;
  }

  private async cargarCatalogosAsync(): Promise<void> {
  try {
    console.log('Iniciando carga de catálogos...');

    this.cargandoCatalogos.usuario = true;

    // CARGAR USUARIO ACTUAL Y TODOS LOS USUARIOS EN PARALELO
    forkJoin({
      usuarioActual: this.evaluacionService.obtenerUsuarioActual(),
      todosUsuarios: this.http.get(`${environment.apiUrl}/api/Auth/usuarios`, {
        responseType: 'text'
      })
    }).subscribe({
      next: ({ usuarioActual, todosUsuarios }) => {
        // Usuario actual
        this.usuarioActual = usuarioActual;
        console.log('Usuario actual:', this.usuarioActual?.nombreCompleto);

        // PARSEAR TODOS LOS USUARIOS
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

        // CONVERTIR A UsuarioSelect[]
        this.evaluadores = usuariosArray.map((u: UsuarioResponseDto) => ({
          id: u.id,
          nombreCompleto: u.nombreCompleto || `${u.nombre} ${u.apellido}`,
          rol: u.rol || 'Desconocido',
          displayText: `${u.nombreCompleto || u.nombre} ${u.apellido} (${u.rol || 'Sin rol'})`
        }));

        console.log('Total de usuarios cargados:', this.evaluadores.length);

        this.cargandoCatalogos.usuario = false;

        // En modo CREAR, asignar usuario actual
        if (this.modoOperacion === 'crear' && !this.formulario.evaluadorId && this.usuarioActual) {
          this.formulario.evaluadorId = this.usuarioActual.id.toString();
          this.onFormularioChange();
        }

        // Si estamos en modo EDITAR, construir textos
        if (this.modoOperacion === 'editar') {
          this.construirTextosCamposBloqueados();
        }
      },
      error: (error) => {
        console.error('Error al cargar usuarios:', error);
        this.usuarioActual = null;
        this.evaluadores = [];
        this.cargandoCatalogos.usuario = false;
      }
    });

    // Cargar órdenes
    this.cargandoCatalogos.ordenes = true;
    try {
      this.ordenesTrabajo = await this.evaluacionService.obtenerOrdenesTrabajo().toPromise() || [];
      console.log('Órdenes cargadas:', this.ordenesTrabajo.length);
    } catch (error) {
      console.error('Error al cargar órdenes:', error);
      this.ordenesTrabajo = [];
    } finally {
      this.cargandoCatalogos.ordenes = false;
    }

    // En modo CREAR, cargar ejecuciones si ya hay orden seleccionada
    if (this.modoOperacion === 'crear' && this.formulario.ordenTrabajoId) {
      const ordenId = parseInt(this.formulario.ordenTrabajoId);
      if (!isNaN(ordenId)) {
        const ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise();
        this.ejecuciones = ejecuciones || [];
        console.log('Ejecuciones cargadas:', this.ejecuciones.length);
      }
    }

    console.log('Todos los catálogos cargados');

  } catch (error) {
    console.error('Error general al cargar catálogos:', error);
  }
}

  onOrdenChange(): void {
    if (this.modoOperacion === 'editar') return;

    const ordenId = this.formulario.ordenTrabajoId;

    if (!ordenId) {
      this.ejecuciones = [];
      this.formulario.ejecucionId = '';
      this.onFormularioChange();
      return;
    }

    const ordenIdNum = parseInt(ordenId);
    if (isNaN(ordenIdNum)) return;

    this.cargandoCatalogos.ejecuciones = true;
    this.evaluacionService.obtenerEjecuciones(ordenIdNum).subscribe({
      next: (ejecuciones) => {
        this.ejecuciones = ejecuciones || [];
        console.log('Ejecuciones cargadas para orden:', ordenIdNum, this.ejecuciones);
        this.cargandoCatalogos.ejecuciones = false;
      },
      error: (error) => {
        console.error('Error al cargar ejecuciones:', error);
        this.ejecuciones = [];
        this.cargandoCatalogos.ejecuciones = false;
      }
    });

    this.onFormularioChange();
  }

  onFormularioChange(): void {
    this.sharedService.setInfoGeneral({ ...this.formulario });
  }

  alternarSeguimiento(): void {
    this.formulario.requiereSeguimiento = !this.formulario.requiereSeguimiento;
    this.onFormularioChange();
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

async guardarEvaluacion(): Promise<void> {
  if (!this.formulario.ordenTrabajoId || !this.formulario.evaluadorId) {
    alert('Complete campos obligatorios: Orden de Trabajo y Evaluador');
    return;
  }

  this.onFormularioChange();
  this.sharedService.actualizarScore();

  // VALIDACIÓN: Verificar si las fases tienen datos válidos
  const datosAntes = this.sharedService.getFaseAntes();
  const datosDespues = this.sharedService.getFaseDespues();

  console.log('Datos antes de guardar:', {
    datosAntes: datosAntes ? 'existe' : 'null',
    datosDespues: datosDespues ? 'existe' : 'null',
    datosAntesVacio: datosAntes ? this.estaFaseVacia(datosAntes) : 'N/A',
    datosDespuesVacio: datosDespues ? this.estaFaseVacia(datosDespues) : 'N/A'
  });

  // Verificar que las fases con datos tengan lugar
  const erroresValidacion: string[] = [];

  // Validar FASE ANTES
  if (datosAntes && !this.estaFaseVacia(datosAntes)) {
    // Si la fase tiene datos, el lugar es obligatorio
    if (!datosAntes.lugar || datosAntes.lugar.trim() === '') {
      erroresValidacion.push('La Evaluación ANTES tiene datos pero falta el campo "Lugar"');
    }
  }

  // Validar FASE DESPUÉS
  if (datosDespues && !this.estaFaseVacia(datosDespues)) {
    // Si la fase tiene datos, el lugar es obligatorio
    if (!datosDespues.lugar || datosDespues.lugar.trim() === '') {
      erroresValidacion.push('La Evaluación DESPUÉS tiene datos pero falta el campo "Lugar"');
    }
  }

  // Si hay errores de validación, mostrarlos y detener el guardado
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

  // FILTRAR FASES VACÍAS DEL DTO
  // Si una fase está vacía, no la incluyas en el DTO
  if (dto.faseAntes && this.estaFaseVacia(datosAntes)) {
    console.log('Fase ANTES está vacía - se omitirá del guardado');
    delete dto.faseAntes;
  }

  if (dto.faseDespues && this.estaFaseVacia(datosDespues)) {
    console.log('Fase DESPUÉS está vacía - se omitirá del guardado');
    delete dto.faseDespues;
  }

  console.log('DTO final a enviar:', {
    tieneDetalleAntes: !!dto.faseAntes,
    tieneDetalleDespues: !!dto.faseDespues
  });

  const mensaje = this.modoOperacion === 'editar'
    ? '¿Guardar cambios en la evaluación?\n\nNota: Los campos Orden, Ejecución y Evaluador no se pueden modificar.'
    : '¿Guardar la evaluación completa?';

  if (!confirm(mensaje)) return;

  try {
    this.sharedService.setGuardando(true);

    const resultado = await this.evaluacionService.guardarEvaluacionCompleta(dto);

    console.log('Evaluación guardada:', resultado);

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