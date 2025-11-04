import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../../../environments/environment';
import {
  EvaluacionResponseDto,
  EvaluacionDetalleResponseDto,
  EvaluacionFotoResponseDto,
  FormularioInfoGeneral,
  Foto
} from '../models/evaluacion.dto';

/**
 * Interfaz para la evaluación completa cargada
 */
export interface EvaluacionCompletaCargada {
  evaluacion: EvaluacionResponseDto;
  detalleAntes?: EvaluacionDetalleResponseDto;
  detalleDespues?: EvaluacionDetalleResponseDto;
  fotosAntes: EvaluacionFotoResponseDto[];
  fotosDespues: EvaluacionFotoResponseDto[];
}

/**
 * Servicio especializado para cargar y editar evaluaciones existentes
 */
@Injectable({
  providedIn: 'root'
})
export class EvaluacionEditService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  /**
   * Carga una evaluación completa con todos sus detalles y fotos
   */
  cargarEvaluacionCompleta(evaluacionId: number): Observable<EvaluacionCompletaCargada> {
    return forkJoin({
      // 1. Cargar evaluación principal
      evaluacion: this.http.get<EvaluacionResponseDto>(`${this.apiUrl}/Evaluacion/${evaluacionId}`),
      
      // 2. Cargar detalles (ambas fases)
      detalles: this.http.get<EvaluacionDetalleResponseDto[]>(
        `${this.apiUrl}/EvaluacionDetalles/por-evaluacion/${evaluacionId}`
      )
    }).pipe(
      map(({ evaluacion, detalles }) => {
        // Separar detalles por fase
        const detalleAntes = detalles.find(d => d.fase === 'ANTES');
        const detalleDespues = detalles.find(d => d.fase === 'DESPUES');

        return {
          evaluacion,
          detalleAntes,
          detalleDespues,
          fotosAntes: [], 
          fotosDespues: []
        };
      })
    );
  }

  /**
   * Carga las fotos de un detalle específico
   */
  cargarFotosDetalle(detalleId: number): Observable<EvaluacionFotoResponseDto[]> {
    // Nota: Necesitarías un endpoint en el backend que devuelva fotos por detalleId
    // Por ahora, obtenemos todas las fotos y filtramos
    return this.http.get<EvaluacionFotoResponseDto[]>(`${this.apiUrl}/FotosEvaluacion`).pipe(
      map(fotos => fotos.filter(f => f.detalleId === detalleId))
    );
  }

  /**
   * Método conveniente para cargar evaluación con fotos
   */
  async cargarEvaluacionConFotos(evaluacionId: number): Promise<EvaluacionCompletaCargada> {
    // 1. Cargar evaluación y detalles
    const evaluacionCompleta = await this.cargarEvaluacionCompleta(evaluacionId).toPromise();

    if (!evaluacionCompleta) {
      throw new Error('No se pudo cargar la evaluación');
    }

    // 2. Cargar fotos de fase ANTES si existe
    if (evaluacionCompleta.detalleAntes) {
      try {
        evaluacionCompleta.fotosAntes = await this.cargarFotosDetalle(
          evaluacionCompleta.detalleAntes.id
        ).toPromise() || [];
      } catch (error) {
        console.warn('Error al cargar fotos ANTES:', error);
        evaluacionCompleta.fotosAntes = [];
      }
    }

    // 3. Cargar fotos de fase DESPUÉS si existe
    if (evaluacionCompleta.detalleDespues) {
      try {
        evaluacionCompleta.fotosDespues = await this.cargarFotosDetalle(
          evaluacionCompleta.detalleDespues.id
        ).toPromise() || [];
      } catch (error) {
        console.warn('Error al cargar fotos DESPUÉS:', error);
        evaluacionCompleta.fotosDespues = [];
      }
    }

    return evaluacionCompleta;
  }

  /**
   * Convierte EvaluacionResponseDto a FormularioInfoGeneral
   */
  mapearAFormularioInfoGeneral(evaluacion: EvaluacionResponseDto): FormularioInfoGeneral {
    return {
      ordenTrabajoId: evaluacion.ordenId.toString(),
      ejecucionId: evaluacion.ejecucionId?.toString() || '',
      clienteId: evaluacion.clienteId?.toString() || '',
      evaluadorId: evaluacion.evaluadorId.toString(),
      objetivo: evaluacion.objetivo || '',
      comentariosGenerales: evaluacion.comentariosGenerales || '',
      scoreCalidad: evaluacion.scoreCalidadTotal || 0,
      requiereSeguimiento: evaluacion.requiereSeguimiento,
      notasSeguimiento: evaluacion.seguimientoNotas || ''
    };
  }

  /**
   * Convierte EvaluacionDetalleResponseDto a datos de fase
   */
  mapearADatosFase(detalle: EvaluacionDetalleResponseDto, fotos: EvaluacionFotoResponseDto[]) {
    return {
      detalleId: detalle.id,
      lugar: detalle.lugar,
      fechaCreacion: this.formatearFecha(detalle.creadoEn),
      scoreFase: detalle.scoreFase || 0,
      descripcion: detalle.descripcion || '',
      sugerencias: detalle.sugerencias || '',
      notaGeneral: detalle.evidenciasNota || '',
      fotos: this.mapearFotosAComponente(fotos)
    };
  }

  /**
   * Convierte EvaluacionFotoResponseDto[] a Foto[] (formato del componente)
   */
  private mapearFotosAComponente(fotosDto: EvaluacionFotoResponseDto[]): Foto[] {
    return fotosDto.map(fotoDto => ({
      id: fotoDto.id.toString(), // ID temporal para el componente
      fotoIdBD: fotoDto.id, // ID real en la BD
      tipo: fotoDto.tipo || 'General',
      fecha: this.formatearFecha(fotoDto.creadoEn),
      descripcion: fotoDto.descripcion || '',
      preview: fotoDto.urlDescarga || `${this.apiUrl}/FotosEvaluacion/${fotoDto.id}/download`,
      // No incluimos 'archivo' porque la foto ya está subida
    }));
  }

  /**
   * Formatea una fecha para los inputs date de HTML
   */
  private formatearFecha(fecha: Date | string): string {
    const fechaObj = typeof fecha === 'string' ? new Date(fecha) : fecha;
    const year = fechaObj.getFullYear();
    const month = String(fechaObj.getMonth() + 1).padStart(2, '0');
    const day = String(fechaObj.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Verifica si una evaluación existe
   */
  existeEvaluacion(evaluacionId: number): Observable<boolean> {
    return this.http.get<EvaluacionResponseDto>(`${this.apiUrl}/Evaluacion/${evaluacionId}`).pipe(
      map(() => true),
      // Si hay error 404, la evaluación no existe
    );
  }
}
