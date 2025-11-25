import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

// Interfaces backend
export interface EvaluacionResponseDto {
  id: number;
  ordenId: number;
  ejecucionId?: number;
  clienteId?: number;
  evaluadorId: number;
  objetivo?: string;
  comentariosGenerales?: string;
  scoreCalidadTotal?: number;
  requiereSeguimiento: boolean;
  seguimientoNotas?: string;
  creadoEn: string;
  ordenTrabajo?: string;
  ejecucion?: string;
  cliente?: string;
  evaluador?: string;
}

export interface EvaluacionDetalleResponseDto {
  id: number;
  evaluacionId: number;
  fase: string; // 'antes' o 'despues'
  descripcion?: string;
  sugerencias?: string;
  scoreFase?: number;
  evidenciaNota?: string;
  lugar: string;
  creadoEn: string;
}

// Interfaces frontend
export interface Evaluacion {
  id: number;  
  ordenTrabajo: string;
  ejecucion: string;
  cliente: string;
  evaluador: string;
  objetivo: string;
  comentarios: string;
  scoreTotal: number;
  requiereSeguimiento: boolean;
  notasSeguimiento: string;
  fases: Fase[];
}

export interface Fase {
  tipo: 'antes' | 'despues';
  titulo: string;
  completada: boolean;
  id?: number;
}

@Injectable({
  providedIn: 'root'
})
export class EvaluacionService {
  private apiUrl = `${environment.apiUrl}/api`; // Base URL

  constructor(private http: HttpClient) {}


  // Obtener evaluación completa

  getEvaluacionCompleta(evaluacionId: number, token?: string): Observable<Evaluacion> {
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    const evaluacion$ = this.http.get<EvaluacionResponseDto>(
      `${this.apiUrl}/Evaluacion/${evaluacionId}`,
      { headers }
    );

    const detalles$ = this.http.get<EvaluacionDetalleResponseDto[]>(
      `${this.apiUrl}/EvaluacionDetalles/por-evaluacion/${evaluacionId}`,
      { headers }
    );

    return forkJoin([evaluacion$, detalles$]).pipe(
      map(([evaluacion, detalles]) => this.mapearEvaluacion(evaluacion, detalles))
    );
  }


  // Obtener solo la evaluación

  getEvaluacion(evaluacionId: number, token?: string): Observable<EvaluacionResponseDto> {
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get<EvaluacionResponseDto>(`${this.apiUrl}/Evaluacion/${evaluacionId}`, { headers });
  }


  // Obtener detalles de la evaluación

  getDetallesEvaluacion(evaluacionId: number, token?: string): Observable<EvaluacionDetalleResponseDto[]> {
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.get<EvaluacionDetalleResponseDto[]>(`${this.apiUrl}/EvaluacionDetalles/por-evaluacion/${evaluacionId}`, { headers });
  }


  // Mapeo seguro de backend a frontend

private mapearEvaluacion(
  evaluacion: EvaluacionResponseDto, 
  detalles: EvaluacionDetalleResponseDto[]
): Evaluacion {
  const fases: Fase[] = [
    {
      tipo: 'antes',
      titulo: 'Evaluación ANTES',
      completada: detalles.some(d => d.fase.toLowerCase() === 'antes'),
      id: detalles.find(d => d.fase.toLowerCase() === 'antes')?.id
    },
    {
      tipo: 'despues',
      titulo: 'Evaluación DESPUÉS',
      completada: detalles.some(d => d.fase.toLowerCase() === 'despues'),
      id: detalles.find(d => d.fase.toLowerCase() === 'despues')?.id
    }
  ];

  return {
    id: evaluacion.id,  // ← AGREGAR
    ordenTrabajo: evaluacion.ordenTrabajo || `OT-${evaluacion.ordenId}`,
    ejecucion: evaluacion.ejecucion || (evaluacion.ejecucionId ? `Ejecución ${evaluacion.ejecucionId}` : 'Sin ejecución específica'),
    cliente: evaluacion.cliente || (evaluacion.clienteId ? `Cliente ${evaluacion.clienteId}` : 'Sin cliente específico'),
    evaluador: evaluacion.evaluador || `Evaluador ${evaluacion.evaluadorId}`,
    objetivo: evaluacion.objetivo || '',
    comentarios: evaluacion.comentariosGenerales || '',
    scoreTotal: evaluacion.scoreCalidadTotal || 0,
    requiereSeguimiento: evaluacion.requiereSeguimiento,
    notasSeguimiento: evaluacion.seguimientoNotas || '',
    fases: fases
  };
}

}
