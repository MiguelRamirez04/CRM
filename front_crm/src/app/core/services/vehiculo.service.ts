import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
// Ajusta la ruta para subir a 'core/models'
import { Vehiculo, VehiculoCreateDto, VehiculoUpdateDto, VehiculoHistorial, RegistrarSalidaDto, RegistrarEntradaDto, UsoVehiculo } from '../models/vehiculo.interface';
import { environment } from '../../../environments/environment';

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
   * Obtiene un vehículo específico por ID.
   */
  getVehiculoById(id: number): Observable<Vehiculo> {
    // GET -> /api/Vehiculos/1
    return this.http.get<Vehiculo>(`${this.baseUrl}/${id}`);
  }

  /**
   * Obtiene el historial de cambios de un vehículo.
   */
  getVehiculoHistorial(id: number): Observable<VehiculoHistorial[]> {
    // GET -> /api/Vehiculos/1/historial
    return this.http.get<VehiculoHistorial[]>(`${this.baseUrl}/${id}/historial`);
  }

  /**
   * Registra la SALIDA de un vehículo (Check-out).
   */
  registrarSalida(id: number, dto: RegistrarSalidaDto): Observable<Vehiculo> {
    // POST -> /api/Vehiculos/1/salida
    return this.http.post<Vehiculo>(`${this.baseUrl}/${id}/salida`, dto);
  }

  /**
   * Registra la ENTRADA de un vehículo (Check-in).
   */
  registrarEntrada(id: number, dto: RegistrarEntradaDto): Observable<Vehiculo> {
    // POST -> /api/Vehiculos/1/entrada
    return this.http.post<Vehiculo>(`${this.baseUrl}/${id}/entrada`, dto);
  }

  /**
   * Obtiene el historial de USO de un vehículo (Viajes).
   */
  getHistorialUso(id: number): Observable<UsoVehiculo[]> {
    // GET -> /api/Vehiculos/1/historial-uso
    return this.http.get<UsoVehiculo[]>(`${this.baseUrl}/${id}/historial-uso`);
  }

  /**
   * Elimina un vehículo de la base de datos.
   */
  deleteVehiculo(id: number): Observable<void> {
    // DELETE -> /api/Vehiculos/1
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}