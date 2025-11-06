import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment'; // importa tu environment

export interface Evaluacion {
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
  evaluacionActualizada?: string;
  nuevaEvaluacion?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EvaluacionesService {
  private apiUrl = `${environment.apiUrl}/api/Evaluacion`; // usa environment.apiUrl

  constructor(private http: HttpClient) {}

  obtenerEvaluaciones(): Observable<Evaluacion[]> {
    return this.http.get<Evaluacion[]>(this.apiUrl, { withCredentials: environment.security.useHttpOnlyCookies });
  }

  // Ejemplo: obtener detalle por id
  obtenerEvaluacionPorId(id: number): Observable<Evaluacion> {
    return this.http.get<Evaluacion>(`${this.apiUrl}/${id}`, { withCredentials: environment.security.useHttpOnlyCookies });
  }
}
