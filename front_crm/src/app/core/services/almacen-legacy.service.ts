// =====================================================================================
// SERVICIO ALMACEN LEGACY - almacen-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Almacenes Legacy (Adminpaq).
// Maneja obtención de almacenes disponibles para asignación en cotizaciones.
//
// ENDPOINTS DISPONIBLES:
// - GET /api/AdmAlmacenes - Obtener todos los almacenes
// - GET /api/AdmAlmacenes/{id} - Obtener almacén específico por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens en headers)
// - El interceptor secure-auth.interceptor.ts añade withCredentials automáticamente
//
// EJEMPLO DE USO:
// constructor(private almacenService: AlmacenLegacyService) {}
//
// cargarAlmacenes() {
//   this.almacenService.obtenerTodos().subscribe({
//     next: (response) => {
//       if (response.success) {
//         this.almacenes = response.data || [];
//       }
//     },
//     error: (err) => console.error('Error:', err)
//   });
// }
//
// =====================================================================================

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  AlmacenLegacyResponse,
  AlmacenLegacySimple,
  AlmacenLegacyApiResponse
} from '../models/almacen-legacy.interface';

@Injectable({
  providedIn: 'root'
})
export class AlmacenLegacyService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/AdmAlmacenes`;

  constructor() {
    console.log('🏢 AlmacenLegacyService inicializado');
    console.log(`📡 API URL: ${this.apiUrl}`);
  }

  /**
   * 📋 Obtener todos los almacenes
   * Endpoint: GET /api/AdmAlmacenes
   * 
   * Retorna lista completa de almacenes del sistema Legacy.
   * Útil para selección en formularios de cotizaciones.
   * 
   * @returns Observable con respuesta de API conteniendo lista de almacenes
   * 
   * @example
   * this.almacenService.obtenerTodos().subscribe({
   *   next: (response) => {
   *     if (response.success) {
   *       this.almacenes = response.data || [];
   *       console.log(`Total almacenes: ${this.almacenes.length}`);
   *     }
   *   },
   *   error: (err) => this.manejarError(err)
   * });
   */
  obtenerTodos(): Observable<AlmacenLegacyApiResponse<AlmacenLegacyResponse[]>> {
    console.log('📋 Obteniendo todos los almacenes');
    
    return this.http.get<AlmacenLegacyApiResponse<AlmacenLegacyResponse[]>>(
      this.apiUrl
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Almacenes obtenidos: ${response.data.length} registros`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 🏢 Obtener solo almacenes activos (simplificado)
   * 
   * Retorna solo almacenes activos con campos esenciales.
   * Optimizado para dropdowns y selección rápida.
   * 
   * @returns Observable con lista simplificada de almacenes activos
   * 
   * @example
   * this.almacenService.obtenerActivos().subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.almacenesActivos = response.data.filter(a => a.estaActivo);
   *     }
   *   }
   * });
   */
  obtenerActivos(): Observable<AlmacenLegacyApiResponse<AlmacenLegacySimple[]>> {
    console.log('🏢 Obteniendo almacenes activos');
    
    return this.http.get<AlmacenLegacyApiResponse<AlmacenLegacyResponse[]>>(
      this.apiUrl
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          // Transformar a formato simplificado y filtrar solo activos
          const almacenesSimplificados: AlmacenLegacySimple[] = response.data
            .filter(a => a.estaActivo)
            .map(a => ({
              idAlmacen: a.idAlmacen,
              codigoAlmacen: a.codigoAlmacen,
              nombreAlmacen: a.nombreAlmacen,
              estaActivo: a.estaActivo
            }));

          console.log(`✅ Almacenes activos: ${almacenesSimplificados.length} registros`);

          return {
            success: true,
            data: almacenesSimplificados,
            executionTime: response.executionTime
          };
        }
        return response as any;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 🏢 Obtener almacén específico por ID
   * Endpoint: GET /api/AdmAlmacenes/{id}
   * 
   * Retorna información completa de un almacén específico.
   * 
   * @param id - ID del almacén en el sistema Legacy
   * @returns Observable con respuesta de API conteniendo datos del almacén
   * 
   * @example
   * this.almacenService.obtenerPorId(1).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.almacen = response.data;
   *       console.log('Almacén:', response.data.nombreAlmacen);
   *       console.log('Encargado:', response.data.encargado);
   *     } else {
   *       console.warn('Almacén no encontrado');
   *     }
   *   },
   *   error: (err) => {
   *     if (err.status === 404) {
   *       console.error('Almacén no existe');
   *     }
   *   }
   * });
   */
  obtenerPorId(id: number): Observable<AlmacenLegacyApiResponse<AlmacenLegacyResponse>> {
    console.log(`🏢 Obteniendo almacén con ID: ${id}`);
    
    return this.http.get<AlmacenLegacyApiResponse<AlmacenLegacyResponse>>(
      `${this.apiUrl}/${id}`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Almacén obtenido: ${response.data.nombreAlmacen}`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * ❌ Manejador de errores HTTP
   * 
   * Procesa errores de peticiones HTTP y los transforma en mensajes legibles.
   * Maneja diferentes códigos de estado HTTP (400, 401, 404, 500, etc.).
   * 
   * @param error - Error HTTP capturado
   * @returns Observable que emite el error procesado
   */
  private manejarError(error: HttpErrorResponse): Observable<never> {
    let mensajeError = 'Error desconocido en la operación';

    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente o de red
      mensajeError = `Error de red: ${error.error.message}`;
      console.error('❌ Error de cliente:', error.error.message);
    } else {
      // Error del lado del servidor
      switch (error.status) {
        case 400:
          mensajeError = 'Datos de entrada inválidos';
          console.error('❌ Bad Request (400):', error.error?.message || error.message);
          break;
        case 401:
          mensajeError = 'No autenticado. Por favor inicie sesión';
          console.error('❌ No autenticado (401)');
          break;
        case 403:
          mensajeError = 'No tiene permisos para realizar esta acción';
          console.error('❌ Forbidden (403)');
          break;
        case 404:
          mensajeError = 'Almacén no encontrado';
          console.error('❌ Not Found (404)');
          break;
        case 500:
          mensajeError = 'Error interno del servidor';
          console.error('❌ Internal Server Error (500):', error.error?.message);
          break;
        default:
          mensajeError = `Error del servidor (${error.status}): ${error.message}`;
          console.error(`❌ Error ${error.status}:`, error.message);
      }
    }

    return throwError(() => ({
      mensaje: mensajeError,
      status: error.status,
      error: error.error
    }));
  }
}
