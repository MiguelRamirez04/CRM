// =====================================================================================
// SERVICIO PRODUCTO LEGACY - producto-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Productos Legacy (Adminpaq).
// Maneja búsquedas, paginación y obtención de detalles de productos.
//
// ENDPOINTS DISPONIBLES:
// - GET /api/AdmProductos/buscar?texto={search} - Búsqueda rápida simplificada
// - GET /api/AdmProductos/search - Búsqueda paginada con filtros avanzados
// - GET /api/AdmProductos/{id} - Obtener producto específico por ID
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens en headers)
// - El interceptor secure-auth.interceptor.ts añade withCredentials automáticamente
// - CSRF token se incluye automáticamente en métodos POST/PUT/DELETE
//
// EJEMPLO DE USO:
// constructor(private productoService: ProductoLegacyService) {}
//
// buscarProducto(texto: string) {
//   this.productoService.buscarSimplificado(texto).subscribe({
//     next: (response) => {
//       if (response.success) {
//         this.productos = response.data || [];
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
  ProductoLegacyBusqueda,
  ProductoLegacyResponse,
  ProductoLegacyFiltros,
  ProductoLegacyPaginado,
  ProductoLegacyApiResponse
} from '../models/producto-legacy.interface';

@Injectable({
  providedIn: 'root'
})
export class ProductoLegacyService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/AdmProductos`;

  constructor() {
    console.log('📦 ProductoLegacyService inicializado');
    console.log(`📡 API URL: ${this.apiUrl}`);
  }

  /**
   * 🔍 Búsqueda simplificada de productos
   * Endpoint: GET /api/AdmProductos/buscar
   * 
   * Busca productos por nombre o código de forma rápida.
   * Retorna solo campos esenciales (ID, código, nombre, precio, unidad).
   * Útil para autocompletado y selección rápida en cotizaciones.
   * 
   * @param texto - Texto a buscar (mínimo 3 caracteres recomendado)
   * @returns Observable con respuesta de API conteniendo lista de productos
   * 
   * @example
   * this.productoService.buscarSimplificado('tornillo').subscribe({
   *   next: (response) => {
   *     if (response.success) {
   *       this.productos = response.data || [];
   *       console.log(`Encontrados: ${this.productos.length} productos`);
   *     }
   *   },
   *   error: (err) => this.manejarError(err)
   * });
   */
  buscarSimplificado(texto: string): Observable<ProductoLegacyApiResponse<ProductoLegacyBusqueda[]>> {
    console.log(`🔍 Buscando productos con texto: "${texto}"`);
    
    const params = new HttpParams().set('texto', texto);
    
    return this.http.get<ProductoLegacyApiResponse<ProductoLegacyBusqueda[]>>(
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
   * Endpoint: GET /api/AdmProductos/search
   * 
   * Permite búsqueda avanzada con múltiples filtros y paginación.
   * Retorna información completa de productos con metadata de paginación.
   * 
   * @param filtros - Objeto con criterios de búsqueda opcionales
   * @returns Observable con respuesta de API conteniendo datos paginados
   * 
   * @example
   * const filtros: ProductoLegacyFiltros = {
   *   nombreProducto: 'Herramienta',
   *   tipoProducto: 1, // Productos (no servicios)
   *   soloActivos: true,
   *   precioMinimo: 100,
   *   precioMaximo: 1000,
   *   page: 1,
   *   pageSize: 20
   * };
   * 
   * this.productoService.buscarPaginado(filtros).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.productos = response.data.data;
   *       this.pagination = response.data.pagination;
   *     }
   *   }
   * });
   */
  buscarPaginado(filtros: ProductoLegacyFiltros = {}): Observable<ProductoLegacyApiResponse<ProductoLegacyPaginado>> {
    console.log('📋 Búsqueda paginada con filtros:', filtros);
    
    let params = new HttpParams();
    let endpoint = `${this.apiUrl}/paginated`;

    // Si hay término de búsqueda (nombre o código), usamos el endpoint de búsqueda
    const searchTerm = filtros.nombreProducto || filtros.codigoProducto;
    
    if (searchTerm) {
      endpoint = `${this.apiUrl}/search/paginated`;
      params = params.set('q', searchTerm);
    }

    // Parámetros de paginación
    if (filtros.page) {
      params = params.set('page', filtros.page.toString());
    }
    if (filtros.pageSize) {
      params = params.set('pageSize', filtros.pageSize.toString());
    }
    
    // Parámetro de estado (status)
    if (filtros.status !== undefined && filtros.status !== null) {
      params = params.set('status', filtros.status.toString());
    }

    return this.http.get<any>(
      endpoint,
      { params }
    ).pipe(
      map(response => {
        // ADAPTADOR: El backend retorna PaginatedResponseDto directamente, no envuelto en ApiResponse
        // Mapeamos la respuesta del backend al formato que espera el componente
        
        const backendData = response;
        const totalRecords = backendData.totalItems || 0;
        const totalPages = backendData.totalPaginas || 0;
        const currentPage = backendData.pagina || 1;
        const pageSize = backendData.resultadosPorPagina || 50;

        const mappedResponse: ProductoLegacyApiResponse<ProductoLegacyPaginado> = {
          success: true,
          data: {
            data: backendData.items || [],
            pagination: {
              currentPage: currentPage,
              pageSize: pageSize,
              totalRecords: totalRecords,
              totalPages: totalPages,
              hasNextPage: currentPage < totalPages,
              hasPreviousPage: currentPage > 1
            },
            filters: filtros
          }
        };

        console.log(`✅ Búsqueda paginada: ${totalRecords} registros totales`);
        console.log(`📄 Página ${currentPage}/${totalPages}`);
        
        return mappedResponse;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 📦 Obtener producto específico por ID
   * Endpoint: GET /api/AdmProductos/{id}
   * 
   * Retorna información completa de un producto incluyendo:
   * - Datos básicos (código, nombre, descripción)
   * - Precios (10 niveles de precio)
   * - Costos y márgenes
   * - Impuestos y retenciones
   * - Unidades de medida
   * - Clasificaciones
   * - Dimensiones
   * 
   * @param id - ID del producto en el sistema Legacy
   * @returns Observable con respuesta de API conteniendo datos del producto
   * 
   * @example
   * this.productoService.obtenerPorId(456).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.producto = response.data;
   *       console.log('Producto:', response.data.nombreProducto);
   *       console.log('Precio lista 1:', response.data.precio1);
   *       console.log('IVA:', response.data.impuesto1 + '%');
   *     } else {
   *       console.warn('Producto no encontrado');
   *     }
   *   },
   *   error: (err) => {
   *     if (err.status === 404) {
   *       console.error('Producto no existe');
   *     }
   *   }
   * });
   */
  obtenerPorId(id: number): Observable<ProductoLegacyResponse> {
    console.log(`📦 Obteniendo producto con ID: ${id}`);
    
    return this.http.get<ProductoLegacyResponse>(
      `${this.apiUrl}/${id}`
    ).pipe(
      map(response => {
        console.log(`✅ Producto obtenido: ${response.nombreProducto}`);
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 💰 Obtener precio del producto según lista de precios
   * 
   * Calcula el precio aplicable según el nivel de lista (1-10).
   * Útil para cálculos de cotizaciones con diferentes listas de precio.
   * 
   * @param producto - Producto con información de precios
   * @param listaPrecio - Nivel de lista de precios (1-10, default: 1)
   * @returns Precio del producto según la lista especificada
   * 
   * @example
   * const precioAplicable = this.productoService.obtenerPrecioSegunLista(producto, 2);
   * console.log(`Precio lista 2: $${precioAplicable}`);
   */
  obtenerPrecioSegunLista(producto: ProductoLegacyResponse, listaPrecio: number = 1): number {
    switch (listaPrecio) {
      case 1: return producto.precio1;
      case 2: return producto.precio2;
      case 3: return producto.precio3;
      case 4: return producto.precio4;
      case 5: return producto.precio5;
      case 6: return producto.precio6;
      case 7: return producto.precio7;
      case 8: return producto.precio8;
      case 9: return producto.precio9;
      case 10: return producto.precio10;
      default: return producto.precio1;
    }
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
          mensajeError = 'Producto no encontrado';
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
