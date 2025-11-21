import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Reparacion, ReparacionDto } from '../../core/models/reparacion.interface';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class ReparacionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/soporte/reparaciones`;

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
}