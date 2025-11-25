// =====================================================================================
// COMPONENTE INFO GENERAL - infogeneralregistro.component.ts (CORREGIDO - EJECUCIÓN)
// =====================================================================================
//
// 🔥 FIX APLICADO: Cargar y mostrar ejecuciones correctamente en modo EDITAR
// ✅ Ahora recibe las ejecuciones del servicio
// ✅ Puede construir el texto de la ejecución bloqueada correctamente
//
// =====================================================================================

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  EvaluacionService,
  OrdenTrabajoSelect,
  ClienteSelect,
  EjecucionSelect,
  UsuarioSelect
} from '../../../../../../core/services/evaluaciones.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { FormularioInfoGeneral } from '../../../../../../core/models/evaluaciones.interface';

@Component({
  selector: 'app-infogeneral',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './infogeneralregistro.component.html',
  styleUrls: ['./infogeneralregistro.component.css']
})
export class InfogeneralComponent implements OnInit, OnDestroy {
  faseActiva: 'antes' | 'despues' | 'infogeneral' = 'infogeneral';
  private destroy$ = new Subject<void>();
  guardando = false;
  cargando = false;

  // Detectar modo desde la ruta
  modoOperacion: 'crear' | 'editar' = 'crear';
  evaluacionId: number | null = null;

  // Catálogos
  ordenesTrabajo: OrdenTrabajoSelect[] = [];
  ejecuciones: EjecucionSelect[] = [];
  clientes: ClienteSelect[] = [];
  evaluadores: UsuarioSelect[] = [];
  usuarioActual: UsuarioSelect | null = null;

  // Textos para campos bloqueados
  textoOrdenBloqueada: string = '';
  textoEjecucionBloqueada: string = '';
  textoClienteBloqueado: string = '';
  textoEvaluadorBloqueado: string = '';

  // Estados de carga por catálogo
  cargandoCatalogos = {
    ordenes: false,
    clientes: false,
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
    private route: ActivatedRoute
  ) { }

  async ngOnInit(): Promise<void> {
    await this.detectarModoDesdeRuta();
    await this.cargarCatalogosAsync();

    if (this.modoOperacion === 'editar') {
      this.construirTextosCamposBloqueados();
    }

    this.suscribirCambios();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Detectar modo desde la ruta y parámetros
   */
  private async detectarModoDesdeRuta(): Promise<void> {
    const modoData = this.route.snapshot.data['modo'] as 'crear' | 'editar' | undefined;
    const idParam = this.route.snapshot.paramMap.get('id');

    console.log('Detectando modo:', { modoData, idParam });

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
        console.log('Ya tenía datos en SharedService, NO recargar BD');
        const infoGuardada = this.sharedService.getInfoGeneral();
        if (infoGuardada) {
          this.formulario = { ...infoGuardada };
        }
        return;
      }

      console.log('Modo EDITAR - Primera entrada, cargando evaluación ID:', this.evaluacionId);
      await this.cargarEvaluacionExistente(this.evaluacionId);

    } else if (modoData === 'crear') {
      this.modoOperacion = 'crear';
      this.evaluacionId = null;

      console.log('Modo CREAR - Nueva evaluación');

      const infoGuardada = this.sharedService.getInfoGeneral();
      if (infoGuardada) {
        this.formulario = { ...infoGuardada };
        console.log('Datos locales cargados');
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

  /**
   * 🔥 CORREGIDO: Construir textos para campos bloqueados
   * Ahora puede encontrar la ejecución porque this.ejecuciones está poblado
   */
  private construirTextosCamposBloqueados(): void {
    console.log('🔨 Construyendo textos de campos bloqueados...');
    console.log('📊 Formulario actual:', this.formulario);
    console.log('📚 Catálogos disponibles:', {
      ordenes: this.ordenesTrabajo.length,
      ejecuciones: this.ejecuciones.length,  // 🔥 Ahora debería tener datos
      clientes: this.clientes.length,
      evaluadores: this.evaluadores.length,
      usuarioActual: this.usuarioActual
    });

    // Orden de trabajo
    if (this.formulario.ordenTrabajoId) {
      const ordenId = this.formulario.ordenTrabajoId.toString();
      const orden = this.ordenesTrabajo.find(o => o.id.toString() === ordenId);

      if (orden) {
        this.textoOrdenBloqueada = orden.displayText;
        console.log('✅ Orden encontrada:', this.textoOrdenBloqueada);
      } else {
        this.textoOrdenBloqueada = `OT-${this.formulario.ordenTrabajoId}`;
        console.warn('⚠️ Orden no encontrada en catálogo, usando ID');
      }
    } else {
      this.textoOrdenBloqueada = 'Sin orden asignada';
    }

    // 🔥 EJECUCIÓN - CORREGIDO
    if (this.formulario.ejecucionId) {
      const ejecucionId = this.formulario.ejecucionId.toString();
      
      console.log('🔍 Buscando ejecución:', {
        ejecucionId,
        ejecucionesDisponibles: this.ejecuciones.length,
        ejecucionesIds: this.ejecuciones.map(e => e.id)
      });

      const ejecucion = this.ejecuciones.find(e => e.id.toString() === ejecucionId);

      if (ejecucion) {
        this.textoEjecucionBloqueada = ejecucion.displayText;
        console.log('✅ Ejecución encontrada:', this.textoEjecucionBloqueada);
      } else {
        // Si no se encuentra, construir texto básico
        this.textoEjecucionBloqueada = `Ejecución #${this.formulario.ejecucionId}`;
        console.warn('⚠️ Ejecución no encontrada en catálogo. IDs disponibles:', 
          this.ejecuciones.map(e => e.id).join(', '));
      }
    } else {
      this.textoEjecucionBloqueada = 'Sin ejecución específica';
      console.log('ℹ️ No hay ejecución asignada');
    }

    // Cliente
    if (this.formulario.clienteId) {
      const clienteId = this.formulario.clienteId.toString();
      const cliente = this.clientes.find(c => c.id.toString() === clienteId);

      if (cliente) {
        this.textoClienteBloqueado = cliente.displayText;
        console.log('✅ Cliente encontrado:', this.textoClienteBloqueado);
      } else {
        this.textoClienteBloqueado = `Cliente #${this.formulario.clienteId}`;
        console.warn('⚠️ Cliente no encontrado en catálogo');
      }
    } else {
      this.textoClienteBloqueado = 'Sin cliente específico';
    }

    // Evaluador
    if (this.formulario.evaluadorId) {
      const evaluadorId = this.formulario.evaluadorId.toString();
      let evaluador = this.evaluadores.find(e => e.id.toString() === evaluadorId);

      if (!evaluador && this.usuarioActual && this.usuarioActual.id.toString() === evaluadorId) {
        evaluador = this.usuarioActual;
      }

      if (evaluador) {
        this.textoEvaluadorBloqueado = evaluador.displayText;
        console.log('✅ Evaluador encontrado:', this.textoEvaluadorBloqueado);
      } else {
        this.textoEvaluadorBloqueado = `Evaluador #${this.formulario.evaluadorId}`;
        console.warn('⚠️ Evaluador no encontrado');
      }
    } else {
      this.textoEvaluadorBloqueado = 'Sin evaluador asignado';
    }

    console.log('✅ Textos bloqueados construidos:', {
      orden: this.textoOrdenBloqueada,
      ejecucion: this.textoEjecucionBloqueada,
      cliente: this.textoClienteBloqueado,
      evaluador: this.textoEvaluadorBloqueado
    });
  }

  /**
   * 🔥 CORREGIDO: Cargar evaluación completa desde el backend
   * Ahora recibe las ejecuciones junto con los datos de la evaluación
   */
  private async cargarEvaluacionExistente(id: number): Promise<void> {
    this.cargando = true;

    try {
      console.log('📥 Cargando evaluación completa desde BD...');

      const resultado = await this.evaluacionService.cargarEvaluacionCompleta(id).toPromise();

      if (!resultado) {
        throw new Error('No se pudo cargar la evaluación');
      }

      console.log('✅ Datos recibidos de BD:', {
        evaluacionId: resultado.evaluacion.id,
        ordenId: resultado.evaluacion.ordenId,
        ejecucionId: resultado.evaluacion.ejecucionId,
        ejecucionesDisponibles: resultado.ejecuciones?.length || 0  // 🔥 NUEVO
      });

      // 🔥 NUEVO: Guardar las ejecuciones recibidas
      if (resultado.ejecuciones) {
        this.ejecuciones = resultado.ejecuciones;
        console.log('✅ Ejecuciones cargadas:', this.ejecuciones.length);
        console.log('📋 IDs de ejecuciones:', this.ejecuciones.map(e => e.id));
      }

      // Mapear evaluación a formulario
      const infoGeneral: FormularioInfoGeneral = {
        ordenTrabajoId: resultado.evaluacion.ordenId?.toString() || '',
        ejecucionId: resultado.evaluacion.ejecucionId?.toString() || '',
        clienteId: resultado.evaluacion.clienteId?.toString() || '',
        evaluadorId: resultado.evaluacion.evaluadorId?.toString() || '',
        objetivo: resultado.evaluacion.objetivo || '',
        comentariosGenerales: resultado.evaluacion.comentariosGenerales || '',
        scoreCalidad: resultado.evaluacion.scoreCalidadTotal || 0,
        requiereSeguimiento: resultado.evaluacion.requiereSeguimiento || false,
        notasSeguimiento: resultado.evaluacion.seguimientoNotas || ''
      };

      console.log('✅ Formulario mapeado:', infoGeneral);

      // Mapear fase ANTES
      const faseAntes = resultado.detalleAntes ? {
        detalleId: resultado.detalleAntes.id,
        lugar: resultado.detalleAntes.lugar || '',
        fechaCreacion: resultado.detalleAntes.creadoEn?.split('T')[0] || '',
        scoreFase: resultado.detalleAntes.scoreFase || 0,
        descripcion: resultado.detalleAntes.descripcion || '',
        sugerencias: resultado.detalleAntes.sugerencias || '',
        notaGeneral: resultado.detalleAntes.evidenciasNota || '',
        fotos: resultado.fotosAntes.map(foto => ({
          id: foto.id.toString(),
          fotoIdBD: foto.id,
          tipo: foto.tipo || 'General',
          fecha: foto.creadoEn?.split('T')[0] || '',
          descripcion: foto.descripcion || '',
          preview: foto.urlDescarga
        }))
      } : undefined;

      // Mapear fase DESPUÉS
      const faseDespues = resultado.detalleDespues ? {
        detalleId: resultado.detalleDespues.id,
        lugar: resultado.detalleDespues.lugar || '',
        fechaCreacion: resultado.detalleDespues.creadoEn?.split('T')[0] || '',
        scoreFase: resultado.detalleDespues.scoreFase || 0,
        descripcion: resultado.detalleDespues.descripcion || '',
        sugerencias: resultado.detalleDespues.sugerencias || '',
        notaGeneral: resultado.detalleDespues.evidenciasNota || '',
        fotos: resultado.fotosDespues.map(foto => ({
          id: foto.id.toString(),
          fotoIdBD: foto.id,
          tipo: foto.tipo || 'General',
          fecha: foto.creadoEn?.split('T')[0] || '',
          descripcion: foto.descripcion || '',
          preview: foto.urlDescarga
        }))
      } : undefined;

      this.sharedService.cargarEvaluacion(id, infoGeneral, faseAntes, faseDespues);
      this.formulario = { ...infoGeneral };

      console.log('✅ Evaluación cargada en servicio compartido');

    } catch (error) {
      console.error('❌ Error al cargar evaluación:', error);
      alert('Error al cargar la evaluación. Volviendo al listado...');
      this.router.navigate(['/dashboard/evaluaciones']);
    } finally {
      this.cargando = false;
    }
  }

  /**
   * Suscribirse a cambios del servicio compartido
   */
  private suscribirCambios(): void {
    this.sharedService.infoGeneral$
      .pipe(takeUntil(this.destroy$))
      .subscribe(info => {
        if (info && info.scoreCalidad !== this.formulario.scoreCalidad) {
          this.formulario.scoreCalidad = info.scoreCalidad;
        }
      });

    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });

    this.sharedService.faseAntes$
      .pipe(takeUntil(this.destroy$))
      .subscribe(fase => {
        this.fases.antes.completada = fase !== null;
        this.fases.antes.estatus = fase ? 'Completada' : 'Sin completar';
      });

    this.sharedService.faseDespues$
      .pipe(takeUntil(this.destroy$))
      .subscribe(fase => {
        this.fases.despues.completada = fase !== null;
        this.fases.despues.estatus = fase ? 'Completada' : 'Sin completar';
      });

    this.sharedService.evaluacionId$
      .pipe(takeUntil(this.destroy$))
      .subscribe(id => {
        if (id !== this.evaluacionId) {
          this.evaluacionId = id;
        }
      });
  }

  /**
   * Cargar catálogos de forma asíncrona
   */
  private async cargarCatalogosAsync(): Promise<void> {
    console.log('📚 Cargando catálogos generales...');

    try {
      const [ordenes, clientes, usuario] = await Promise.all([
        this.evaluacionService.obtenerOrdenesTrabajo().toPromise(),
        this.evaluacionService.obtenerClientes().toPromise(),
        this.evaluacionService.obtenerUsuarioActual().toPromise()
      ]);

      this.ordenesTrabajo = ordenes || [];
      this.clientes = clientes || [];
      this.usuarioActual = usuario || null;
      this.evaluadores = usuario ? [usuario] : [];

      console.log('✅ Catálogos generales cargados:', {
        ordenes: this.ordenesTrabajo.length,
        clientes: this.clientes.length,
        usuario: this.usuarioActual?.nombreCompleto
      });

      // 🔥 NOTA: En modo EDITAR, las ejecuciones se cargan en cargarEvaluacionExistente()
      // Solo cargar ejecuciones aquí si estamos en modo CREAR y hay orden seleccionada
      if (this.modoOperacion === 'crear' && this.formulario.ordenTrabajoId) {
        const ordenId = parseInt(this.formulario.ordenTrabajoId);
        if (!isNaN(ordenId)) {
          const ejecuciones = await this.evaluacionService.obtenerEjecuciones(ordenId).toPromise();
          this.ejecuciones = ejecuciones || [];
          console.log('✅ Ejecuciones cargadas (modo CREAR):', this.ejecuciones.length);
        }
      }

      // Solo asignar evaluador actual en modo CREAR
      if (this.modoOperacion === 'crear' && !this.formulario.evaluadorId && this.usuarioActual) {
        this.formulario.evaluadorId = this.usuarioActual.id.toString();
        this.onFormularioChange();
      }

    } catch (error) {
      console.error('❌ Error al cargar catálogos:', error);
    }
  }

  cargarCatalogos(): void {
    this.cargarCatalogosAsync();
  }

  /**
   * Cuando cambia la orden, cargar sus ejecuciones (solo en modo CREAR)
   */
  onOrdenChange(): void {
    if (this.modoOperacion === 'editar') return;

    const ordenId = this.formulario.ordenTrabajoId;

    if (!ordenId) {
      this.ejecuciones = [];
      this.formulario.ejecucionId = '';
      this.onFormularioChange();
      return;
    }

    console.log('🔄 Cargando ejecuciones para orden:', ordenId);

    this.cargandoCatalogos.ejecuciones = true;
    this.evaluacionService.obtenerEjecuciones(parseInt(ordenId)).subscribe({
      next: (ejecuciones) => {
        this.ejecuciones = ejecuciones;
        console.log('✅ Ejecuciones cargadas:', ejecuciones.length);

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
        console.error('❌ Error al cargar ejecuciones:', error);
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

  seleccionarFase(tipoFase: 'antes' | 'despues'): void {
    this.cambiarFase(tipoFase);
  }

  /**
   * Cambiar fase con ruta según modo
   */
  cambiarFase(fase: 'antes' | 'despues' | 'infogeneral'): void {
    this.onFormularioChange();
    this.faseActiva = fase;

    const rutaBase = this.modoOperacion === 'editar' && this.evaluacionId
      ? `/dashboard/evaluaciones/editar/${this.evaluacionId}`
      : '/dashboard/evaluaciones/nueva';

    if (fase === 'antes') {
      this.router.navigate([`${rutaBase}/fase-antes`]);
    } else if (fase === 'despues') {
      this.router.navigate([`${rutaBase}/fase-despues`]);
    } else {
      this.router.navigate([rutaBase]);
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

  /**
   * Guardar evaluación completa
   */
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
      ? '¿Guardar cambios en la evaluación?\n\nNota: Los campos Orden, Ejecución, Cliente y Evaluador no se pueden modificar.'
      : '¿Guardar la evaluación completa?';

    if (!confirm(mensaje)) return;

    try {
      this.sharedService.setGuardando(true);

      const resultado = await this.evaluacionService.guardarEvaluacionCompleta(dto);

      console.log('✅ Evaluación guardada:', resultado);

      const mensajeExito = this.modoOperacion === 'editar'
        ? `Evaluación #${resultado.id} actualizada`
        : `Evaluación guardada con ID: ${resultado.id}`;

      alert(mensajeExito);

      this.sharedService.setEvaluacionId(resultado.id);

      if (this.modoOperacion === 'crear') {
        this.modoOperacion = 'editar';
        this.evaluacionId = resultado.id;
        this.router.navigate(['/dashboard/evaluaciones/editar', resultado.id], {
          replaceUrl: true
        });
      }

      const irAListado = confirm('¿Volver al listado?');
      if (irAListado) {
        this.sharedService.limpiar();
        this.router.navigate(['/dashboard/evaluaciones']);
      }

    } catch (error) {
      console.error('❌ Error al guardar:', error);
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
      this.router.navigate(['/dashboard/evaluaciones']);
    }
  }

  get algunCatalogoCargando(): boolean {
    return Object.values(this.cargandoCatalogos).some(cargando => cargando);
  }
}

/* 
=====================================================================================
📋 CAMBIOS REALIZADOS EN EL COMPONENTE
=====================================================================================

🔥 PROBLEMA SOLUCIONADO:
   - Antes: No se cargaban las ejecuciones al cargar evaluación existente
   - Resultado: this.ejecuciones estaba vacío []
   - El método construirTextosCamposBloqueados() no encontraba la ejecución
   - Mostraba: "Sin ejecución específica" aunque tuviera una

✅ SOLUCIÓN IMPLEMENTADA:

1. cargarEvaluacionExistente():
   - Ahora el servicio retorna { ..., ejecuciones: EjecucionSelect[] }
   - El componente guarda: this.ejecuciones = resultado.ejecuciones
   - Logs agregados para debugging

2. construirTextosCamposBloqueados():
   - Logs mejorados para detectar el problema
   - Ahora puede encontrar la ejecución porque this.ejecuciones tiene datos

3. cargarCatalogosAsync():
   - Comentario agregado explicando que en modo EDITAR
   - Las ejecuciones se cargan en cargarEvaluacionExistente()

🔄 FLUJO CORREGIDO (modo EDITAR):
   1. Detectar modo EDITAR desde ruta
   2. Llamar cargarEvaluacionExistente(id)
   3. Servicio carga evaluación + ejecuciones de esa orden
   4. Componente recibe y guarda ejecuciones
   5. construirTextosCamposBloqueados() encuentra la ejecución
   6. ✅ Muestra: "Ejecución #123 - Técnico XYZ (01/01/2025 10:00)"

📝 ARCHIVOS QUE DEBEN ACTUALIZARSE:
   ✅ evaluacion.service.ts (ya corregido)
   ✅ infogeneralregistro.component.ts (este archivo)
   ⚠️ NO tocar: shared-evaluacion.service.ts
   ⚠️ NO tocar: evaluaciones.interface.ts
*/