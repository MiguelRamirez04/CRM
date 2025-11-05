import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { 
  FormularioInfoGeneral, 
  FaseEvaluacionDto,
  EvaluacionCompletaDto,
  mapFormularioToEvaluacionDto,
  mapComponenteFaseToDto,
  Foto
} from '../models/evaluacion.dto';

//Servicio compartido para manejar el estado de la evaluación
//entre los 3 componentes (InfoGeneral, FaseAntes, FaseDespues)

@Injectable({
  providedIn: 'root'
})
export class SharedEvaluacionService {
  
  // Estado de la evaluación actual
  private evaluacionIdSubject = new BehaviorSubject<number | null>(null);
  public evaluacionId$ = this.evaluacionIdSubject.asObservable();

  // Datos de información general
  private infoGeneralSubject = new BehaviorSubject<FormularioInfoGeneral | null>(null);
  public infoGeneral$ = this.infoGeneralSubject.asObservable();

  // Datos de fase ANTES
  private faseAntesSubject = new BehaviorSubject<any>(null);
  public faseAntes$ = this.faseAntesSubject.asObservable();
  private faseAntesDetalleIdSubject = new BehaviorSubject<number | null>(null);
  public faseAntesDetalleId$ = this.faseAntesDetalleIdSubject.asObservable();

  // Datos de fase DESPUÉS
  private faseDespuesSubject = new BehaviorSubject<any>(null);
  public faseDespues$ = this.faseDespuesSubject.asObservable();
  private faseDespuesDetalleIdSubject = new BehaviorSubject<number | null>(null);
  public faseDespuesDetalleId$ = this.faseDespuesDetalleIdSubject.asObservable();

  // Estado de guardado
  private guardandoSubject = new BehaviorSubject<boolean>(false);
  public guardando$ = this.guardandoSubject.asObservable();

  constructor() { }

  // ==================== SETTERS ====================

  
//Establece el ID de la evaluación actual//en
  setEvaluacionId(id: number | null): void {
    console.log('🆔 Estableciendo ID de evaluación:', id);
    this.evaluacionIdSubject.next(id);
  }

  
//Guarda los datos de información general//en
  setInfoGeneral(info: FormularioInfoGeneral): void {
    this.infoGeneralSubject.next(info);
  }

  
//Guarda los datos de la fase ANTES//en
  setFaseAntes(datos: {
    lugar: string;
    fechaCreacion: string;
    scoreFase: number;
    descripcion: string;
    sugerencias: string;
    notaGeneral: string;
    fotos: Foto[];
    detalleId?: number;
  }): void {
    console.log('📥 Guardando fase ANTES:', { 
      lugar: datos.lugar, 
      detalleId: datos.detalleId,
      tieneFotos: datos.fotos.length 
    });
    
    this.faseAntesSubject.next(datos);
    
    if (datos.detalleId !== undefined) {
      this.faseAntesDetalleIdSubject.next(datos.detalleId);
      console.log('✅ DetalleId ANTES actualizado:', datos.detalleId);
    }
  }

  
//Guarda los datos de la fase DESPUÉS//en
  setFaseDespues(datos: {
    lugar: string;
    fechaCreacion: string;
    scoreFase: number;
    descripcion: string;
    sugerencias: string;
    notaGeneral: string;
    fotos: Foto[];
    detalleId?: number;
  }): void {
    console.log('📥 Guardando fase DESPUÉS:', { 
      lugar: datos.lugar, 
      detalleId: datos.detalleId,
      tieneFotos: datos.fotos.length 
    });
    
    this.faseDespuesSubject.next(datos);
    
    if (datos.detalleId !== undefined) {
      this.faseDespuesDetalleIdSubject.next(datos.detalleId);
      console.log('✅ DetalleId DESPUÉS actualizado:', datos.detalleId);
    }
  }

  
//Establece el estado de guardado//en
  setGuardando(guardando: boolean): void {
    this.guardandoSubject.next(guardando);
  }

  // ==================== GETTERS ====================

  
//Obtiene el ID de la evaluación actual//en
  getEvaluacionId(): number | null {
    return this.evaluacionIdSubject.value;
  }

  
//Obtiene los datos de información general//en
  getInfoGeneral(): FormularioInfoGeneral | null {
    return this.infoGeneralSubject.value;
  }

  
//Obtiene los datos de la fase ANTES//en
  getFaseAntes(): any {
    return this.faseAntesSubject.value;
  }

  
//Obtiene los datos de la fase DESPUÉS//en
  getFaseDespues(): any {
    return this.faseDespuesSubject.value;
  }

  
//Obtiene el ID del detalle de fase ANTES//en
  getFaseAntesDetalleId(): number | null {
    return this.faseAntesDetalleIdSubject.value;
  }

  
//Obtiene el ID del detalle de fase DESPUÉS//en
  getFaseDespuesDetalleId(): number | null {
    return this.faseDespuesDetalleIdSubject.value;
  }

  
//Verifica si se está guardando//en
  isGuardando(): boolean {
    return this.guardandoSubject.value;
  }

  // ==================== UTILIDADES ====================

  
//🔥 CORREGIDO: Construye el DTO completo para enviar al backend//enAhora incluye todos los IDs necesarios para UPDATE
  
  construirEvaluacionCompleta(): EvaluacionCompletaDto | null {
    const infoGeneral = this.getInfoGeneral();
    
    if (!infoGeneral) {
      console.error('❌ No hay información general para guardar');
      return null;
    }

    // Validar campos requeridos
    if (!infoGeneral.ordenTrabajoId || !infoGeneral.evaluadorId) {
      console.error('❌ Faltan campos obligatorios: ordenTrabajoId o evaluadorId');
      return null;
    }

    // Obtener datos de fases
    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();
    const evaluacionId = this.getEvaluacionId();

    // 🔥 CRÍTICO: Obtener los detalleId directamente de los datos de las fases
    const detalleAntesId = faseAntes?.detalleId ?? null;
    const detalleDespuesId = faseDespues?.detalleId ?? null;

    // Log de verificación detallado
    console.log('🔨 Construyendo DTO completo:');
    console.log('- Evaluación ID:', evaluacionId);
    console.log('- Fase ANTES:', {
      existe: !!faseAntes,
      detalleId: detalleAntesId,
      lugar: faseAntes?.lugar,
      fotos: faseAntes?.fotos?.length || 0
    });
    console.log('- Fase DESPUÉS:', {
      existe: !!faseDespues,
      detalleId: detalleDespuesId,
      lugar: faseDespues?.lugar,
      fotos: faseDespues?.fotos?.length || 0
    });

    // Mapear evaluación principal
    const evaluacionDto = mapFormularioToEvaluacionDto(infoGeneral);

    // Construir el DTO completo
    const evaluacionCompleta: EvaluacionCompletaDto = {
      evaluacionId: evaluacionId || undefined,
      evaluacion: evaluacionDto,
      faseAntes: faseAntes ? this.mapFase(faseAntes, detalleAntesId) : undefined,
      faseDespues: faseDespues ? this.mapFase(faseDespues, detalleDespuesId) : undefined
    };

    // 🔥 Verificación final del DTO
    console.log('✅ DTO construido:', {
      tieneEvaluacionId: !!evaluacionCompleta.evaluacionId,
      tieneFaseAntes: !!evaluacionCompleta.faseAntes,
      faseAntesDetalleId: evaluacionCompleta.faseAntes?.detalleId,
      faseAntesFotos: evaluacionCompleta.faseAntes?.fotos?.length || 0,
      tieneFaseDespues: !!evaluacionCompleta.faseDespues,
      faseDespuesDetalleId: evaluacionCompleta.faseDespues?.detalleId,
      faseDespuesFotos: evaluacionCompleta.faseDespues?.fotos?.length || 0
    });

    return evaluacionCompleta;
  }

  
//Mapea los datos de una fase del componente al DTO//en
  private mapFase(datos: any, detalleId: number | null): FaseEvaluacionDto {
    const faseDto = mapComponenteFaseToDto(
      datos.lugar,
      datos.fechaCreacion,
      datos.scoreFase,
      datos.descripcion,
      datos.sugerencias,
      datos.notaGeneral,
      datos.fotos,
      detalleId || undefined
    );
    
    console.log('🗺️ Fase mapeada:', {
      detalleId: faseDto.detalleId,
      lugar: faseDto.lugar,
      fotos: faseDto.fotos?.length || 0
    });
    
    return faseDto;
  }

  
//Valida si todos los datos necesarios están completos//en
  validarDatosCompletos(): { valido: boolean; errores: string[] } {
    const errores: string[] = [];
    const infoGeneral = this.getInfoGeneral();

    if (!infoGeneral) {
      errores.push('Falta completar la información general');
      return { valido: false, errores };
    }

    if (!infoGeneral.ordenTrabajoId) {
      errores.push('Debe seleccionar una orden de trabajo');
    }

    if (!infoGeneral.evaluadorId) {
      errores.push('Debe seleccionar un evaluador');
    }

    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();

    if (!faseAntes && !faseDespues) {
      errores.push('Debe completar al menos una fase (ANTES o DESPUÉS)');
    }

    if (faseAntes && !faseAntes.lugar) {
      errores.push('Debe especificar el lugar en la fase ANTES');
    }

    if (faseDespues && !faseDespues.lugar) {
      errores.push('Debe especificar el lugar en la fase DESPUÉS');
    }

    return {
      valido: errores.length === 0,
      errores: errores
    };
  }

  
//Calcula el score total basado en las fases completadas//en
  calcularScoreTotal(): number {
    const faseAntes = this.getFaseAntes();
    const faseDespues = this.getFaseDespues();

    let scoreTotal = 0;
    let fasesCompletadas = 0;

    if (faseAntes && faseAntes.scoreFase) {
      scoreTotal += faseAntes.scoreFase;
      fasesCompletadas++;
    }

    if (faseDespues && faseDespues.scoreFase) {
      scoreTotal += faseDespues.scoreFase;
      fasesCompletadas++;
    }

    const score = fasesCompletadas > 0 ? Math.round(scoreTotal / fasesCompletadas) : 0;
    console.log('📊 Score calculado:', score, '(fases completadas:', fasesCompletadas + ')');
    return score;
  }

  
//Actualiza el score en la información general//en
  actualizarScoreEnInfoGeneral(): void {
    const infoGeneral = this.getInfoGeneral();
    if (infoGeneral) {
      const nuevoScore = this.calcularScoreTotal();
      infoGeneral.scoreCalidad = nuevoScore;
      this.setInfoGeneral(infoGeneral);
      console.log('✅ Score actualizado en InfoGeneral:', nuevoScore);
    }
  }

  
//Limpia todos los datos (útil para crear nueva evaluación)//en
  limpiar(): void {
    console.log('🧹 Limpiando SharedEvaluacionService');
    this.evaluacionIdSubject.next(null);
    this.infoGeneralSubject.next(null);
    this.faseAntesSubject.next(null);
    this.faseDespuesSubject.next(null);
    this.faseAntesDetalleIdSubject.next(null);
    this.faseDespuesDetalleIdSubject.next(null);
    this.guardandoSubject.next(false);
  }

  
//🔥 CORREGIDO: Carga una evaluación existente con logs de verificación//en
  cargarEvaluacion(
    evaluacionId: number,
    infoGeneral: FormularioInfoGeneral,
    faseAntes?: any,
    faseDespues?: any
  ): void {
    console.log('📥 Cargando evaluación completa en SharedService');
    console.log('- ID:', evaluacionId);
    console.log('- Tiene fase ANTES:', !!faseAntes, 'detalleId:', faseAntes?.detalleId);
    console.log('- Tiene fase DESPUÉS:', !!faseDespues, 'detalleId:', faseDespues?.detalleId);

    this.setEvaluacionId(evaluacionId);
    this.setInfoGeneral(infoGeneral);
    
    if (faseAntes) {
      this.setFaseAntes(faseAntes);
      console.log('✅ Fase ANTES cargada con detalleId:', faseAntes.detalleId);
    } else {
      this.faseAntesSubject.next(null);
      this.faseAntesDetalleIdSubject.next(null);
      console.log('⚠️ No hay fase ANTES');
    }
    
    if (faseDespues) {
      this.setFaseDespues(faseDespues);
      console.log('✅ Fase DESPUÉS cargada con detalleId:', faseDespues.detalleId);
    } else {
      this.faseDespuesSubject.next(null);
      this.faseDespuesDetalleIdSubject.next(null);
      console.log('⚠️ No hay fase DESPUÉS');
    }

    console.log('✅ Evaluación cargada completamente en SharedService');
  }
}