import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RecepcionTicket } from '../models/recepcion.model'; // Importamos la interfaz
import { environment } from '../../../environments/environment'; // Asumiendo que tienes environments

@Injectable({
  providedIn: 'root'
})
export class RecepcionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/Recepcion`; // Ajusta la URL base

  constructor() { }

  // Método para obtener los tickets paginados y filtrados
  getTickets(
    skip: number,
    take: number,
    estado?: string
  ): Observable<RecepcionTicket[]> { // <-- Tu API parece devolver un array directo
    
    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());

    if (estado && estado !== '') {
      params = params.set('estado', estado);
    }

    // Tu API (imagen 1) devuelve un array directamente en el 200 OK
    return this.http.get<RecepcionTicket[]>(this.apiUrl, { params });

    /* // NOTA: Si tu API devolviera un objeto { totalItems: 100, items: [...] }
    // deberías usar la interfaz PaginacionRecepcion y cambiar el tipo de retorno:
    // return this.http.get<PaginacionRecepcion>(this.apiUrl, { params });
    */
  }
}