
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  FormularioInfoGeneral,
  DatosFase,
  EvaluacionCompletaDTO,
  mapFormularioToRequest
} from '../models/evaluaciones.interface';

@Injectable({
  providedIn: 'root'
})
export class SharedEvaluacionService {
  
  // ==================== STATE SUBJECTS ====================
  
  // ID de la evaluación (null para nueva, number para editar)
  private evaluacionIdSubject = new BehaviorSubject<number | null>(null);
  public evaluacionId$ = this.evaluacionIdSubject.asObservable();

  // Datos de información general
  private infoGeneralSubject = new BehaviorSubject<FormularioInfoGeneral | null>(null);
  public infoGeneral$ = this.infoGeneralSubject.asObservable();

  // Datos de fase ANTES
  private faseAntesSubject = new BehaviorSubject<DatosFase | null>(null);
  public faseAntes$ = this.faseAntesSubject.asObservable();

  // Datos de fase DESPUÉS
  private faseDespuesSubject = new BehaviorSubject<DatosFase | null>(null);
  public faseDespues$ = this.faseDespuesSubject.asObservable();

  // Estado de guardado
  private guardandoSubject = new BehaviorSubject<boolean>(false);
  public guardando$ = this.guardandoSubject.asObservable();

  // Estado de carga
  private cargandoSubject = new BehaviorSubject<boolean>(false);
  public cargando$ = this.cargandoSubject.asObservable();

  constructor() {
    // Log inicial del estado
    console.log(' SharedEvaluacionService inicializado');
  }

  // ==================== SETTERS ====================

  setEvaluacionId(id: number | null): void {
    console.log(' Estableciendo ID de evaluación:', id);
    this.evaluacionIdSubject.next(id);
  }

  setInfoGeneral(info: FormularioInfoGeneral): void {
    console.log('Guardando información general');
    this.infoGeneralSubject.next(info);
  }

  setFaseAntes(datos: DatosFase): void {
    console.log(' Guardando fase ANTES');
    this.faseAntesSubject.next(datos);
  }

  setFaseDespues(datos: DatosFase): void {
    console.log(' Guardando fase DESPUÉS');
    this.faseDespuesSubject.next(datos);
  }

  setGuardando(guardando: boolean): void {
    this.guardandoSubject.next(guardando);
    if (guardando) {
      console.log(' Iniciando guardado...');
    } else {
      console.log(' Guardado completado');
    }
  }

  setCargando(cargando: boolean): void {
    this.cargandoSubject.next(cargando);
    if (cargando) {
      console.log(' Cargando datos...');
    } else {
      console.log(' Carga completada');
    }
  }

  // ==================== GETTERS ====================

  getEvaluacionId(): number | null {
    return this.evaluacionIdSubject.value;
  }

  getInfoGeneral(): FormularioInfoGeneral | null {
    return this.infoGeneralSubject.value;
  }

  getFaseAntes(): DatosFase | null {
    return this.faseAntesSubject.value;
  }

  getFaseDespues(): DatosFase | null {
    return this.faseDespuesSubject.value;
  }

  isGuardando(): boolean {
    return this.guardandoSubject.value;
  }

  isCargando(): boolean {
    return this.cargandoSubject.value;
  }

  // ==================== MÉTODOS DE VERIFICACIÓN ====================

  /**
   *  Verificar si hay datos residuales de sesión anterior
   * Útil para detectar cuando el servicio no se limpió correctamente
   */
  tieneDatosSucios(): boolean {
    const tieneInfo = this.infoGeneralSubject.value !== null;
    const tieneAntes = this.faseAntesSubject.value !== null;
    const tieneDespues = this.faseDespuesSubject.value !== null;
    const tieneId = this.evaluacionIdSubject.value !== null;
    
    const tieneDatos = tieneInfo || tieneAntes || tieneDespues || tieneId;
    
    if (tieneDatos) {
      console.warn(' Se detectaron datos residuales:', {
        tieneInfo,
        tieneAntes,
        tieneDespues,
        tieneId,
        evaluacionId: this.evaluacionIdSubject.value
      });
    }
    
    return tieneDatos;
  }

  /**
   * Limpiar si el modo es 'crear' y hay datos
   * Método de seguridad para componentes
   */
  limpiarSiModoCrear(modoOperacion: 'crear' | 'editar'): void {
    if (modoOperacion === 'crear') {
      if (this.tieneDatosSucios()) {
        console.log(' Limpiando datos residuales para modo CREAR');
        this.limpiar();
      } else {
        console.log(' Servicio limpio para modo CREAR');
      }
    } else {
      console.log(' Modo EDITAR - manteniendo datos');
    }
  }

  /**
   * Verificar si el servicio está limpio
   */
  estaLimpio(): boolean {
    const limpio = !this.tieneDatosSucios();
    if (limpio) {
      console.log(' Servicio está limpio');
    }
    return limpio;
  }

  /**
   * Obtener resumen del estado actual
   * Útil para debugging y logs
   */
  obtenerResumenEstado(): {
    tieneId: boolean;
    tieneInfoGeneral: boolean;
    tieneFaseAntes: boolean;
    tieneFaseDespues: boolean;
    estaGuardando: boolean;
    estaCargando: boolean;
    evaluacionId: number | null;
    modoOperacion: 'crear' | 'editar' | 'indeterminado';
  } {
    const evaluacionId = this.evaluacionIdSubject.value;
    const modoOperacion = evaluacionId === null 
      ? 'crear' 
      : evaluacionId > 0 
        ? 'editar' 
        : 'indeterminado';

    return {
      tieneId: evaluacionId !== null,
      tieneInfoGeneral: this.infoGeneralSubject.value !== null,
      tieneFaseAntes: this.faseAntesSubject.value !== null,
      tieneFaseDespues: this.faseDespuesSubject.value !== null,
      estaGuardando: this.guardandoSubject.value,
      estaCargando: this.cargandoSubject.value,
      evaluacionId: evaluacionId,
      modoOperacion: modoOperacion
    };
  }

  /**
   * Verificar consistencia de datos
   * Detecta estados inválidos
   */
  verificarConsistencia(): {
    esConsistente: boolean;
    errores: string[];
  } {
    const errores: string[] = [];
    const resumen = this.obtenerResumenEstado();

    // Verificar que en modo crear no haya ID
    if (resumen.modoOperacion === 'crear' && resumen.tieneId) {
      errores.push('Modo CREAR pero hay ID de evaluación');
    }

    // Verificar que en modo editar haya ID
    if (resumen.modoOperacion === 'editar' && !resumen.tieneId) {
      errores.push('Modo EDITAR pero no hay ID de evaluación');
    }

    // Verificar que si hay fases, haya info general
    if ((resumen.tieneFaseAntes || resumen.tieneFaseDespues) && !resumen.tieneInfoGeneral) {
      errores.push('Hay fases guardadas pero falta información general');
    }

    return {
      esConsistente: errores.length === 0,
      errores
    };
  }

  // ==================== UTILIDADES ====================

  /**
   * Construir DTO completo para enviar al backend
   */
  construirDTO(): EvaluacionCompletaDTO | null {
    const infoGeneral = this.getInfoGeneral();
    
    if (!infoGeneral) {
      console.error(' No hay información general para construir DTO');
      return null;
    }

    const evaluacionId = this.getEvaluacionId();
    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();

    console.log('🔨 Construyendo DTO:', {
      evaluacionId,
      tieneFaseAntes: !!faseAntes,
      tieneFaseDespues: !!faseDespues,
      ordenTrabajoId: infoGeneral.ordenTrabajoId
    });

    const dto: EvaluacionCompletaDTO = {
      evaluacionId: evaluacionId || undefined,
      evaluacion: mapFormularioToRequest(infoGeneral),
      faseAntes: faseAntes || undefined,
      faseDespues: faseDespues || undefined
    };

    return dto;
  }

  /**
   * Validar datos completos antes de guardar
   */
  validar(): { valido: boolean; errores: string[] } {
    const errores: string[] = [];
    const info = this.getInfoGeneral();

    // Validar info general
    if (!info) {
      errores.push('Falta información general');
      return { valido: false, errores };
    }

    if (!info.ordenTrabajoId) {
      errores.push('Seleccione una orden de trabajo');
    }

    if (!info.evaluadorId) {
      errores.push('Seleccione un evaluador');
    }

    // Validar que haya al menos una fase
    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();

    if (!faseAntes && !faseDespues) {
      errores.push('Complete al menos una fase (ANTES o DESPUÉS)');
    }

    // Validar lugares en fases
    if (faseAntes && !faseAntes.lugar) {
      errores.push('Especifique el lugar en fase ANTES');
    }

    if (faseDespues && !faseDespues.lugar) {
      errores.push('Especifique el lugar en fase DESPUÉS');
    }

    // Validar scores (opcional pero si existe debe ser válido)
    if (faseAntes?.scoreFase !== undefined && 
        (faseAntes.scoreFase < 0 || faseAntes.scoreFase > 100)) {
      errores.push('Score de fase ANTES debe estar entre 0 y 100');
    }

    if (faseDespues?.scoreFase !== undefined && 
        (faseDespues.scoreFase < 0 || faseDespues.scoreFase > 100)) {
      errores.push('Score de fase DESPUÉS debe estar entre 0 y 100');
    }

    return {
      valido: errores.length === 0,
      errores
    };
  }

  /**
   * Calcular score total promedio
   */
  calcularScore(): number {
    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();

    let scoreTotal = 0;
    let fasesCompletadas = 0;

    if (faseAntes?.scoreFase !== undefined && faseAntes.scoreFase !== null) {
      scoreTotal += faseAntes.scoreFase;
      fasesCompletadas++;
    }

    if (faseDespues?.scoreFase !== undefined && faseDespues.scoreFase !== null) {
      scoreTotal += faseDespues.scoreFase;
      fasesCompletadas++;
    }

    const promedio = fasesCompletadas > 0 
      ? Math.round(scoreTotal / fasesCompletadas) 
      : 0;

    console.log(' Score calculado:', {
      scoreAntes: faseAntes?.scoreFase,
      scoreDespues: faseDespues?.scoreFase,
      promedio
    });

    return promedio;
  }

  /**
   * Actualizar score en info general
   */
  actualizarScore(): void {
    const info = this.getInfoGeneral();
    if (info) {
      const nuevoScore = this.calcularScore();
      info.scoreCalidad = nuevoScore;
      this.setInfoGeneral(info);
      console.log(' Score actualizado en info general:', nuevoScore);
    } else {
      console.warn(' No se puede actualizar score: falta info general');
    }
  }

  /**
   * Cargar evaluación existente
   */
  cargarEvaluacion(
    evaluacionId: number,
    infoGeneral: FormularioInfoGeneral,
    faseAntes?: DatosFase,
    faseDespues?: DatosFase
  ): void {
    console.log(' Cargando evaluación completa:', evaluacionId);
    
    this.setCargando(true);
    
    this.setEvaluacionId(evaluacionId);
    this.setInfoGeneral(infoGeneral);
    
    if (faseAntes) {
      this.setFaseAntes(faseAntes);
    }
    
    if (faseDespues) {
      this.setFaseDespues(faseDespues);
    }
    
    this.setCargando(false);
    
    console.log(' Evaluación cargada. Estado:', this.obtenerResumenEstado());
  }

  /**
   * LIMPIAR TODO EL SERVICIO
   * CRÍTICO: Debe llamarse al salir del formulario o después de guardar exitosamente
   */
  limpiar(): void {
    console.log(' ============================================');
    console.log(' INICIANDO LIMPIEZA DEL SERVICIO');
    console.log(' ============================================');
    
    // Mostrar estado antes de limpiar
    const estadoAntes = this.obtenerResumenEstado();
    console.log(' Estado ANTES de limpiar:', estadoAntes);
    
    // Si está guardando, advertir
    if (this.isGuardando()) {
      console.warn(' ADVERTENCIA: Limpiando mientras está guardando');
    }
    
    // Limpiar todos los subjects
    this.evaluacionIdSubject.next(null);
    this.infoGeneralSubject.next(null);
    this.faseAntesSubject.next(null);
    this.faseDespuesSubject.next(null);
    this.guardandoSubject.next(false);
    this.cargandoSubject.next(false);
    
    // Verificar que se limpió correctamente
    const estadoDespues = this.obtenerResumenEstado();
    console.log(' Estado DESPUÉS de limpiar:', estadoDespues);
    
    // Validar limpieza
    if (this.estaLimpio()) {
      console.log(' ============================================');
      console.log(' SERVICIO LIMPIADO CORRECTAMENTE');
      console.log(' ============================================');
    } else {
      console.error(' ============================================');
      console.error(' ERROR: EL SERVICIO NO SE LIMPIÓ CORRECTAMENTE');
      console.error(' Datos residuales detectados:', estadoDespues);
      console.error(' ============================================');
    }
  }

  /**
   * NUEVO: Limpiar con confirmación
   * Útil para componentes que quieren estar seguros
   */
  limpiarConConfirmacion(): Promise<boolean> {
    return new Promise((resolve) => {
      if (this.tieneDatosSucios()) {
        console.log(' Limpiando datos existentes...');
        this.limpiar();
        // Verificar después de limpiar
        setTimeout(() => {
          const limpio = this.estaLimpio();
          resolve(limpio);
        }, 100);
      } else {
        console.log(' Servicio ya estaba limpio');
        resolve(true);
      }
    });
  }

  /**
   * NUEVO: Reset completo con logs
   * Para casos extremos donde necesitas empezar de cero
   */
  resetCompleto(): void {
    console.log('============================================');
    console.log('RESET COMPLETO DEL SERVICIO');
    console.log('============================================');
    
    // Guardar estado anterior para logs
    const estadoAnterior = this.obtenerResumenEstado();
    console.log(' Estado anterior:', estadoAnterior);
    
    // Limpiar
    this.limpiar();
    
    // Verificar consistencia
    const consistencia = this.verificarConsistencia();
    if (consistencia.esConsistente) {
      console.log(' Servicio reseteado y consistente');
    } else {
      console.error(' Servicio reseteado pero con inconsistencias:', consistencia.errores);
    }
    
    console.log('============================================');
  }
}

