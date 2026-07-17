// =====================================================================================
// SERVICIO CLIENTE LEGACY - cliente-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Clientes Legacy (Adminpaq).
// Maneja búsquedas, paginación y obtención de detalles de clientes.
//
// ENDPOINTS DISPONIBLES:
// - GET /api/AdmClientes/buscar?texto={search} - Búsqueda rápida simplificada
// - GET /api/AdmClientes/search - Búsqueda paginada con filtros avanzados
// - GET /api/AdmClientes/{id} - Obtener cliente específico por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens en headers)
// - El interceptor secure-auth.interceptor.ts añade withCredentials automáticamente
// - CSRF token se incluye automáticamente en métodos POST/PUT/DELETE
//
// EJEMPLO DE USO:
// constructor(private clienteService: ClienteLegacyService) {}
//
// buscarCliente(texto: string) {
//   this.clienteService.buscarSimplificado(texto).subscribe({
//     next: (response) => {
//       if (response.success) {
//         this.clientes = response.data || [];
//       }
//     },
//     error: (err) => console.error('Error:', err)
//   });
// }
//
// =====================================================================================

import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ClienteLegacyBusqueda,
  ClienteLegacyResponse,
  ClienteLegacyFiltros,
  ClienteLegacyPaginado,
  ClienteLegacyApiResponse
} from '../models/cliente-legacy.interface';

@Injectable({
  providedIn: 'root'
})
export class ClienteLegacyService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/AdmClientes`;

  constructor() {
    console.log('🔷 ClienteLegacyService inicializado');
    console.log(`📡 API URL: ${this.apiUrl}`);
  }

  /**
   * 🔍 Búsqueda simplificada de clientes
   * Endpoint: GET /api/AdmClientes/buscar
   * 
   * Busca clientes por razón social, RFC o código de forma rápida.
   * Retorna solo campos esenciales (ID, razón social, RFC, contacto).
   * 
   * @param texto - Texto a buscar (mínimo 3 caracteres recomendado)
   * @returns Observable con respuesta de API conteniendo lista de clientes
   * 
   * @example
   * this.clienteService.buscarSimplificado('ACME').subscribe({
   *   next: (response) => {
   *     if (response.success) {
   *       this.clientes = response.data || [];
   *       console.log(`Encontrados: ${this.clientes.length} clientes`);
   *     }
   *   },
   *   error: (err) => this.manejarError(err)
   * });
   */
  buscarSimplificado(texto: string): Observable<ClienteLegacyApiResponse<ClienteLegacyBusqueda[]>> {
    console.log(`🔍 Buscando clientes con texto: "${texto}"`);
    
    const params = new HttpParams().set('texto', texto);
    
    return this.http.get<ClienteLegacyApiResponse<ClienteLegacyBusqueda[]>>(
      `${this.apiUrl}/buscar`,
      { params }
    ).pipe(
      map(response => {
        console.log(`✅ Búsqueda completada: ${response.data?.length || 0} resultados`);
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 📋 Búsqueda paginada con filtros avanzados
   * Endpoint: GET /api/AdmClientes/search
   * 
   * Permite búsqueda avanzada con múltiples filtros y paginación.
   * Retorna información completa de clientes con metadata de paginación.
   * 
   * @param filtros - Objeto con criterios de búsqueda opcionales
   * @returns Observable con respuesta de API conteniendo datos paginados
   * 
   * @example
   * const filtros: ClienteLegacyFiltros = {
   *   razonSocial: 'Servicios',
   *   soloActivos: true,
   *   page: 1,
   *   pageSize: 20
   * };
   * 
   * this.clienteService.buscarPaginado(filtros).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.clientes = response.data.data;
   *       this.pagination = response.data.pagination;
   *     }
   *   }
   * });
   */
  buscarPaginado(filtros: ClienteLegacyFiltros = {}): Observable<ClienteLegacyApiResponse<ClienteLegacyPaginado>> {
    console.log('📋 Búsqueda paginada con filtros:', filtros);
    
    let params = new HttpParams();
    
    // Añadir filtros solo si tienen valor
    if (filtros.razonSocial) {
      params = params.set('razonSocial', filtros.razonSocial);
    }
    if (filtros.rfc) {
      params = params.set('rfc', filtros.rfc);
    }
    if (filtros.codigoCliente) {
      params = params.set('codigoCliente', filtros.codigoCliente);
    }
    if (filtros.email) {
      params = params.set('email', filtros.email);
    }
    if (filtros.telefono) {
      params = params.set('telefono', filtros.telefono);
    }
    if (filtros.estado) {
      params = params.set('estado', filtros.estado);
    }
    if (filtros.ciudad) {
      params = params.set('ciudad', filtros.ciudad);
    }
    if (filtros.estatus !== undefined && filtros.estatus !== null) {
      params = params.set('estatus', filtros.estatus.toString());
    }
    if (filtros.tipoDireccion) {
      params = params.set('tipoDireccion', filtros.tipoDireccion.toString());
    }
    if (filtros.incluirDetalleUbicacion !== undefined) {
      params = params.set('incluirDetalleUbicacion', filtros.incluirDetalleUbicacion.toString());
    }
    if (filtros.numeroPagina) {
      params = params.set('numeroPagina', filtros.numeroPagina.toString());
    }
    if (filtros.tamanoPagina) {
      params = params.set('tamanoPagina', filtros.tamanoPagina.toString());
    }

    return this.http.get<any>(
      `${this.apiUrl}/search`,
      { params }
    ).pipe(
      map(response => {
        // Adaptador para formato consistente
        const mappedResponse: ClienteLegacyApiResponse<ClienteLegacyPaginado> = {
          success: response.success || true,
          data: {
            data: response.data || [],
            pagination: response.pagination || {
              currentPage: 1,
              pageSize: 50,
              totalPages: 1,
              totalRecords: 0,
              hasNextPage: false,
              hasPreviousPage: false
            },
            filters: filtros
          },
          message: response.message
        };

        console.log(`✅ Búsqueda paginada: ${mappedResponse.data?.pagination.totalRecords || 0} registros totales`);
        return mappedResponse;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 👤 Obtener cliente específico por ID
   * Endpoint: GET /api/AdmClientes/{id}
   * 
   * Retorna información completa de un cliente incluyendo:
   * - Datos fiscales y de contacto
   * - Domicilios (fiscal y entrega)
   * - Información financiera (crédito, descuentos)
   * - Clasificaciones y relaciones
   * 
   * @param id - ID del cliente en el sistema Legacy
   * @returns Observable con respuesta de API conteniendo datos del cliente
   * 
   * @example
   * this.clienteService.obtenerPorId(123).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.cliente = response.data;
   *       console.log('Cliente:', response.data.razonSocial);
   *       console.log('RFC:', response.data.rfc);
   *       console.log('Límite crédito:', response.data.limiteCredito);
   *     } else {
   *       console.warn('Cliente no encontrado');
   *     }
   *   },
   *   error: (err) => {
   *     if (err.status === 404) {
   *       console.error('Cliente no existe');
   *     }
   *   }
   * });
   */
  obtenerPorId(id: number, incluirDetalleUbicacion: boolean = true): Observable<ClienteLegacyResponse> {
    console.log(`👤 Obteniendo cliente con ID: ${id}`);
    
    const params = new HttpParams().set('incluirDetalleUbicacion', incluirDetalleUbicacion.toString());
    
    return this.http.get<any>(
      `${this.apiUrl}/${id}`,
      { params }
    ).pipe(
      map(response => {
        const cliente = response.data || response;
        console.log(`✅ Cliente obtenido: ${cliente.razonSocial}`);
        return cliente;
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
          mensajeError = 'Cliente no encontrado';
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
