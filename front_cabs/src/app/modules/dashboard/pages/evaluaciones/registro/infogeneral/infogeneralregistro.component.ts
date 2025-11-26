import { Component, OnInit, OnDestroy } from '@angular/core';
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
  private destroy$ = new Subject<void>();
  guardando = false;
  cargando = false;

  // Control de modales
  modalAntesAbierto = false;
  modalDespuesAbierto = false;

  // Detectar modo desde la ruta
  modoOperacion: 'crear' | 'editar' = 'crear';
  evaluacionId: number | null = null;

  // Catálogos (se cargan UNA SOLA VEZ)
  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  evaluadores: UsuarioSelect[] = [];  // CAMBIADO: Ahora es array completo de usuarios
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
    clienteId: '',  // Mantener por compatibilidad con backend, pero no se usa en UI
    evaluadorId: '',
    objetivo: '',
    comentariosGenerales: '',
    scoreCalidad: 0,
    requiereSeguimiento: false,
    notasSeguimiento: ''
  };

  // Estado de fases
  fases = {
    antes: {
      completada: false,
      titulo: 'Evaluación Antes',
      estatus: 'Sin completar'
    },
    despues: {
      completada: false,
      titulo: 'Evaluación DESPUÉS',
      estatus: 'Sin completar'
    }
  };

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient  // AGREGADO: HttpClient para cargar usuarios
  ) {}

  async ngOnInit(): Promise<void> {
    console.log('InfoGeneral ngOnInit - Componente montado');
    
    await this.detectarModoDesdeRuta();
    
    // OPTIMIZACIÓN: Cargar catálogos solo UNA VEZ
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
    console.log(' InfoGeneral ngOnDestroy');
    this.destroy$.next();
    this.destroy$.complete();
  }

  private async detectarModoDesdeRuta(): Promise<void> {
    const modoData = this.route.snapshot.data['modo'] as 'crear' | 'editar' | undefined;
    const idParam = this.route.snapshot.paramMap.get('id');

    console.log(' Detectando modo:', { modoData, idParam });

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
        }
        return;
      }

      console.log('📥 Cargando evaluación ID:', this.evaluacionId);
      await this.cargarEvaluacionExistente(this.evaluacionId);

    } else if (modoData === 'crear') {
      this.modoOperacion = 'crear';
      this.evaluacionId = null;
      console.log('✨ Modo CREAR - Nueva evaluación');

      const infoGuardada = this.sharedService.getInfoGeneral();
      if (infoGuardada) {
        this.formulario = { ...infoGuardada };
        console.log('Datos locales restaurados');
      }

    } else {
      const idCompartido = this.sharedService.getEvaluacionId();

      if (idCompartido) {
        this.modoOperacion = 'editar';
        this.evaluacionId = idCompartido;
        console.log('Modo EDITAR detectado desde SharedService');
      } else {
        this.modoOperacion = 'crear';
        console.log('Modo CREAR por defecto');
      }
    }
  }

  private async cargarEvaluacionExistente(id: number): Promise<void> {
    try {
      this.cargando = true;
      console.log('📡 Cargando evaluación completa ID:', id);

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

          if (infoGeneral.ordenTrabajoId) {
            const ordenId = parseInt(infoGeneral.ordenTrabajoId);
            if (!isNaN(ordenId)) {
              try {
                this.ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise() || [];
                console.log('Ejecuciones cargadas:', this.ejecuciones.length);
                this.construirTextosCamposBloqueados();
              } catch (error) {
                console.error('Error al cargar ejecuciones:', error);
                this.ejecuciones = [];
              }
            }
          }

          this.sharedService.cargarEvaluacion(
            id,
            infoGeneral,
            datosAntes,
            datosDespues
          );

          console.log('Evaluación cargada en SharedService');
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

  /**
   * CORREGIDO: Ahora busca el evaluador en el array completo de usuarios
   */
  private construirTextosCamposBloqueados(): void {
    console.log('🔨 Construyendo textos bloqueados');

    // Orden
    if (this.formulario.ordenTrabajoId) {
      const orden = this.ordenesTrabajo.find(o => o.id.toString() === this.formulario.ordenTrabajoId.toString());
      this.textoOrdenBloqueada = orden ? orden.displayText : `OT-${this.formulario.ordenTrabajoId}`;
    } else {
      this.textoOrdenBloqueada = 'Sin orden asignada';
    }

    // Ejecución
    if (this.formulario.ejecucionId) {
      const ejecucion = this.ejecuciones.find(e => e.id.toString() === this.formulario.ejecucionId.toString());
      this.textoEjecucionBloqueada = ejecucion ? ejecucion.displayText : `Ejecución #${this.formulario.ejecucionId}`;
    } else {
      this.textoEjecucionBloqueada = 'Sin ejecución específica';
    }

    // EVALUADOR - AHORA BUSCA EN EL ARRAY COMPLETO
    if (this.formulario.evaluadorId) {
      const evaluador = this.evaluadores.find(e => e.id.toString() === this.formulario.evaluadorId.toString());
      
      if (evaluador) {
        this.textoEvaluadorBloqueado = evaluador.displayText;
        console.log('Evaluador encontrado:', this.textoEvaluadorBloqueado);
      } else {
        this.textoEvaluadorBloqueado = `Evaluador #${this.formulario.evaluadorId}`;
        console.warn(' Evaluador no encontrado en array:', this.formulario.evaluadorId);
      }
    } else {
      this.textoEvaluadorBloqueado = 'Sin evaluador asignado';
    }

    console.log('Textos bloqueados construidos:', {
      orden: this.textoOrdenBloqueada,
      ejecucion: this.textoEjecucionBloqueada,
      evaluador: this.textoEvaluadorBloqueado
    });
  }

  private suscribirCambios(): void {
    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });
  }

  private actualizarEstadoFases(): void {
    const datosAntes = this.sharedService.getFaseAntes();
    const datosDespues = this.sharedService.getFaseDespues();

    this.fases.antes.completada = !!datosAntes && !!datosAntes.lugar;
    this.fases.antes.estatus = this.fases.antes.completada ? 'Completada' : 'Sin completar';

    this.fases.despues.completada = !!datosDespues && !!datosDespues.lugar;
    this.fases.despues.estatus = this.fases.despues.completada ? 'Completada' : 'Sin completar';
    
    this.sharedService.actualizarScore();
    const infoActual = this.sharedService.getInfoGeneral();
    if (infoActual) {
      this.formulario.scoreCalidad = infoActual.scoreCalidad;
    }
  }

  /**
   * CORREGIDO: Ahora carga todos los usuarios, no solo el actual
   */
  private async cargarCatalogosAsync(): Promise<void> {
    try {
      console.log('📥 Iniciando carga de catálogos...');

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

    console.log('Cargando ejecuciones para orden:', ordenId);

    this.cargandoCatalogos.ejecuciones = true;
    this.evaluacionService.obtenerEjecuciones(parseInt(ordenId)).subscribe({
      next: (ejecuciones) => {
        this.ejecuciones = ejecuciones;
        console.log('Ejecuciones cargadas:', ejecuciones.length);

        if (this.formulario.ejecucionId) {
          const existe = ejecuciones.some(e => e.id.toString() === this.formulario.ejecucionId);
          if (!existe) {
            this.formulario.ejecucionId = '';
          }
        }

        this.cargandoCatalogos.ejecuciones = false;
        this.onFormularioChange();
      },
      error: (error) => {
        console.error('Error al cargar ejecuciones:', error);
        this.ejecuciones = [];
        this.cargandoCatalogos.ejecuciones = false;
      }
    });
  }

  onFormularioChange(): void {
    this.sharedService.setInfoGeneral({ ...this.formulario });
  }

  alternarSeguimiento(): void {
    this.formulario.requiereSeguimiento = !this.formulario.requiereSeguimiento;
    this.onFormularioChange();
  }

  // =====================================================================
  // 🎯 MÉTODOS PARA ABRIR/CERRAR MODALES
  // =====================================================================

  abrirModalAntes(): void {
    console.log(' Abriendo modal ANTES');
    this.modalAntesAbierto = true;
  }

  cerrarModalAntes(): void {
    console.log('Cerrando modal ANTES');
    this.modalAntesAbierto = false;
    this.actualizarEstadoFases();
  }

  abrirModalDespues(): void {
    console.log(' Abriendo modal DESPUÉS');
    this.modalDespuesAbierto = true;
  }

  cerrarModalDespues(): void {
    console.log('Cerrando modal DESPUÉS');
    this.modalDespuesAbierto = false;
    this.actualizarEstadoFases();
  }

  seleccionarFase(tipoFase: 'antes' | 'despues'): void {
    if (tipoFase === 'antes') {
      this.abrirModalAntes();
    } else {
      this.abrirModalDespues();
    }
  }

  // =====================================================================
  // GETTERS Y HELPERS
  // =====================================================================

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

  // =====================================================================
  // GUARDAR Y CERRAR
  // =====================================================================

  async guardarEvaluacion(): Promise<void> {
    if (!this.formulario.ordenTrabajoId || !this.formulario.evaluadorId) {
      alert('Complete campos obligatorios: Orden de Trabajo y Evaluador');
      return;
    }

    this.onFormularioChange();
    this.sharedService.actualizarScore();

    const validacion = this.sharedService.validar();
    if (!validacion.valido) {
      alert('Errores:\n' + validacion.errores.join('\n'));
      return;
    }

    const dto = this.sharedService.construirDTO();
    if (!dto) {
      alert('Error al preparar datos');
      return;
    }

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

      const irAListado = confirm('¿Volver al listado?');
      if (irAListado) {
        this.sharedService.limpiar();
        this.catalogosCargados = false;
        this.router.navigate(['/dashboard/evaluaciones']);
      }

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
      this.router.navigate(['/dashboard/evaluaciones']);
    }
  }
}

