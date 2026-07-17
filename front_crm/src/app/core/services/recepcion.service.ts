import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RecepcionTicket } from '../models/recepcion.model';
import { OrdenTrabajo } from '../models/orden-trabajo.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RecepcionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/Recepcion`;

  constructor() { }

  // Método para obtener los tickets paginados y filtrados
  getTickets(
    skip: number,
    take: number,
    estado?: string
  ): Observable<RecepcionTicket[]> {
    
    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());

    if (estado && estado !== '') {
      params = params.set('estado', estado);
    }

    return this.http.get<RecepcionTicket[]>(this.apiUrl, { params });
  }

  // Método para crear una nueva orden
  crearOrden(data: any): Observable<OrdenTrabajo> {
    const wrapper = { requestDto: data };
    return this.http.post<OrdenTrabajo>(this.apiUrl, wrapper);
  }

  // Método para actualizar una orden existente
  actualizarOrden(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  // Método para obtener una orden por ID
  obtenerOrdenPorId(id: number): Observable<OrdenTrabajo> {
    return this.http.get<OrdenTrabajo>(`${this.apiUrl}/${id}`);
  }

  // Método para obtener estadísticas
  obtenerEstadisticas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estadisticas`);
  }
}