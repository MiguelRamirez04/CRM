import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
// Ajusta la ruta para subir a 'core/models'
import { Vehiculo, VehiculoCreateDto, VehiculoUpdateDto } from '../../../core/models/vehiculo.interface'; 
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {
  private http = inject(HttpClient);
  // URL base de tu API de Vehículos
  private baseUrl = `${environment.apiUrl}/api/Vehiculos`; 

  /**
   * Obtiene la lista de vehículos desde la API.
   * Acepta filtros (ej. { termino: 'ABC-123' })
   */
  getVehiculos(filtros: { [key: string]: string } = {}): Observable<Vehiculo[]> {
    const params = new HttpParams({ fromObject: filtros });
    // GET -> /api/Vehiculos
    return this.http.get<Vehiculo[]>(this.baseUrl, { params });
  }

  /**
   * Obtiene los tipos de combustible.
   */

  /**
   * Crea un nuevo vehículo en la base de datos.
   */
  createVehiculo(dto: VehiculoCreateDto): Observable<Vehiculo> {
    // POST -> /api/Vehiculos
    return this.http.post<Vehiculo>(this.baseUrl, dto);
  }

  /**
   * Actualiza un vehículo existente en la base de datos.
   */
  updateVehiculo(id: number, dto: VehiculoUpdateDto): Observable<Vehiculo> {
    // PUT -> /api/Vehiculos/1
    return this.http.put<Vehiculo>(`${this.baseUrl}/${id}`, dto);
  }

  /**
   * Elimina un vehículo de la base de datos.
   */
  deleteVehiculo(id: number): Observable<void> {
    // DELETE -> /api/Vehiculos/1
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}