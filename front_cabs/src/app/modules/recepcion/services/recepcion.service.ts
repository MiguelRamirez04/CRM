// =====================================================================================
// SERVICIO RECEPCIÓN - recepcion.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir las APIs del módulo de Recepción.
// Incluye métodos para CRUD de órdenes de trabajo, búsqueda de clientes,
// estadísticas y estados.
//
// DEPENDENCIAS:
// - HttpClient (inyectado)
// - Interceptors para JWT y headers
//
// =====================================================================================

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  OrdenTrabajo,
  OrdenTrabajoRequest,
  OrdenTrabajoUpdateRequest,
  ClienteLegacy,
  EstadisticaRecepcion,
  ClienteResumenDto
} from '../../../core/models/orden-trabajo/orden-trabajo.interface';

@Injectable({
  providedIn: 'root'
})
export class RecepcionService {
  private apiUrl = `${environment.apiUrl}/api/Recepcion`;
  private clientesApiUrl = `${environment.apiUrl}/api/ClientesCompletos`;

  constructor(private http: HttpClient) {}

  // GET /api/Recepcion - Lista de órdenes con filtros opcionales
  getOrdenes(filtros?: {
    estado?: string;
    fechaDesde?: string;
    fechaHasta?: string;
    busqueda?: string;
    page?: number;
    size?: number;
  }): Observable<OrdenTrabajo[]> {
    let params = new HttpParams();
    if (filtros) {
      Object.entries(filtros).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params = params.set(key, value.toString());
        }
      });
    }
    return this.http.get<OrdenTrabajo[]>(this.apiUrl, { params });
  }

  // POST /api/Recepcion - Crear nueva orden
  crearOrden(orden: OrdenTrabajoRequest): Observable<OrdenTrabajo> {
    return this.http.post<OrdenTrabajo>(this.apiUrl, orden);
  }

  // GET /api/Recepcion/test-data/{userId}/{clienteId} - Datos de prueba
  getTestData(userId: number, clienteId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/test-data/${userId}/${clienteId}`);
  }

  // GET /api/Recepcion/{id} - Detalles de orden específica
  getOrdenById(id: number): Observable<OrdenTrabajo> {
    return this.http.get<OrdenTrabajo>(`${this.apiUrl}/${id}`);
  }

  // PUT /api/Recepcion/{id} - Actualizar orden
  actualizarOrden(id: number, orden: OrdenTrabajoUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, orden);
  }

  // DELETE /api/Recepcion/{id} - Eliminar orden
  eliminarOrden(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // GET /api/Recepcion/estadisticas - Estadísticas del módulo
  getEstadisticas(): Observable<EstadisticaRecepcion> {
    return this.http.get<EstadisticaRecepcion>(`${this.apiUrl}/estadisticas`);
  }

  // GET /api/Recepcion/clientes/buscar - Búsqueda de clientes para selección
  buscarClientes(busqueda: string): Observable<ClienteResumenDto[]> {
    const params = new HttpParams().set('busqueda', busqueda);
    return this.http.get<ClienteResumenDto[]>(`${this.apiUrl}/clientes/buscar`, { params });
  }

  // GET /api/Recepcion/estados - Lista de estados posibles
  getEstados(): Observable<{ key: string; value: string }[]> {
    return this.http.get<{ key: string; value: string }[]>(`${this.apiUrl}/estados`);
  }

  // APIs adicionales para clientes legacy
  // GET /api/ClientesCompletos/por-nombre
  buscarClientePorNombre(nombre: string): Observable<ClienteLegacy[]> {
    const params = new HttpParams().set('nombre', nombre);
    return this.http.get<ClienteLegacy[]>(`${this.clientesApiUrl}/por-nombre`, { params });
  }

  // GET /api/ClientesCompletos/por-rfc
  buscarClientePorRfc(rfc: string): Observable<ClienteLegacy[]> {
    const params = new HttpParams().set('rfc', rfc);
    return this.http.get<ClienteLegacy[]>(`${this.clientesApiUrl}/por-rfc`, { params });
  }
}