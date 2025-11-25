import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// 1. IMPORTACIONES DE MODELOS (Asegúrate de tener estas dos líneas)
import { Reparacion, ReparacionDto } from '../../core/models/reparacion.interface';
import { ReparacionComponente, ReparacionComponenteDto } from '../../core/models/reparacion-componente.interface';

@Injectable({
  providedIn: 'root'
})
export class ReparacionService {
  private readonly http = inject(HttpClient);
  
  // URLs de la API
  private readonly apiUrl = `${environment.apiUrl}/api/soporte/reparaciones`;
  private readonly apiComponentesUrl = `${environment.apiUrl}/api/soporte/reparaciones/componentes`;

  // ==========================================
  // MÉTODOS PARA REPARACIONES (PADRE)
  // ==========================================

  getReparaciones(filtros: any = {}): Observable<Reparacion[]> {
    let params = new HttpParams();
    if (filtros.termino) params = params.set('termino', filtros.termino);
    return this.http.get<Reparacion[]>(this.apiUrl, { params });
  }

  createReparacion(dto: ReparacionDto): Observable<Reparacion> {
    return this.http.post<Reparacion>(this.apiUrl, dto);
  }

  updateReparacion(id: number, dto: ReparacionDto): Observable<Reparacion> {
    return this.http.put<Reparacion>(`${this.apiUrl}/${id}`, dto);
  }

  deleteReparacion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // ==========================================
  // MÉTODOS PARA COMPONENTES (HIJOS)
  // ==========================================

  getComponentes(filtros: any = {}): Observable<ReparacionComponente[]> {
    let params = new HttpParams();
    // Filtro clave: ID de la reparación padre
    if (filtros.reparacionId) params = params.set('reparacionId', filtros.reparacionId);
    return this.http.get<ReparacionComponente[]>(this.apiComponentesUrl, { params });
  }

  createComponente(dto: ReparacionComponenteDto): Observable<ReparacionComponente> {
    return this.http.post<ReparacionComponente>(this.apiComponentesUrl, dto);
  }

  updateComponente(id: number, dto: ReparacionComponenteDto): Observable<ReparacionComponente> {
    return this.http.put<ReparacionComponente>(`${this.apiComponentesUrl}/${id}`, dto);
  }
  
  deleteComponente(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiComponentesUrl}/${id}`);
  }
}