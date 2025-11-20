// =====================================================================================
// SERVICIO COTIZACION LEGACY - cotizacion-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Cotizaciones Legacy (Adminpaq).
// Maneja creación, cancelación, búsqueda y obtención de cotizaciones.
//
// ENDPOINTS DISPONIBLES:
// - POST /api/AdmDocumentos/cotizacion - Crear nueva cotización
// - PUT /api/AdmDocumentos/cotizacion/cancelar - Cancelar cotización
// - GET /api/AdmDocumentos/search - Búsqueda paginada con filtros
// - GET /api/AdmDocumentos/{id} - Obtener cotización específica
//
// AUTENTICACIÓN:
// - Usa cookies HttpOnly (sin Bearer tokens en headers)
// - El interceptor secure-auth.interceptor.ts añade withCredentials automáticamente
// - CSRF token se incluye automáticamente en métodos POST/PUT/DELETE
//
// FLUJO DE CREACIÓN DE COTIZACIÓN:
// 1. Buscar cliente (ClienteLegacyService.buscarSimplificado)
// 2. Buscar productos (ProductoLegacyService.buscarSimplificado)
// 3. Obtener almacenes (AlmacenLegacyService.obtenerActivos)
// 4. Crear cotización con IDs obtenidos (CotizacionLegacyService.crear)
//
// EJEMPLO DE USO:
// constructor(
//   private cotizacionService: CotizacionLegacyService,
//   private clienteService: ClienteLegacyService,
//   private productoService: ProductoLegacyService,
//   private almacenService: AlmacenLegacyService
// ) {}
//
// crearCotizacion() {
//   const request: CotizacionLegacyCreateRequest = {
//     idCliente: 123,
//     aplicarIVA: true,
//     porcentajeIVA: 16.0,
//     productos: [
//       {
//         idProducto: 456,
//         idAlmacen: 1,
//         unidades: 10,
//         precio: 100.0,
//         porcentajeDescuento: 5.0
//       }
//     ]
//   };
//
//   this.cotizacionService.crear(request).subscribe({
//     next: (response) => {
//       if (response.success && response.data) {
//         console.log('Cotización creada:', response.data.folio);
//         console.log('Total:', response.data.total);
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
  CotizacionLegacyCreateRequest,
  CotizacionLegacyCreateResponse,
  CotizacionLegacyCancelarRequest,
  CotizacionLegacyCancelarResponse,
  CotizacionLegacyFiltros,
  CotizacionLegacyResponse,
  CotizacionLegacyPaginado,
  CotizacionLegacyApiResponse,
  CotizacionLegacyResumen
} from '../models/cotizacion-legacy.interface';

@Injectable({
  providedIn: 'root'
})
export class CotizacionLegacyService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/AdmDocumentos`;

  constructor() {
    console.log('📄 CotizacionLegacyService inicializado');
    console.log(`📡 API URL: ${this.apiUrl}`);
  }

  /**
   * ✨ Crear nueva cotización
   * Endpoint: POST /api/AdmDocumentos/cotizacion
   * 
   * Crea una nueva cotización en el sistema Legacy Adminpaq.
   * Realiza los siguientes cálculos automáticos:
   * - Asigna folio auto-incremental
   * - Calcula subtotales por producto
   * - Aplica descuentos en cascada (producto → documento)
   * - Calcula IVA según porcentaje configurado
   * - Calcula total y saldo pendiente
   * 
   * @param request - Datos de la cotización a crear
   * @returns Observable con respuesta conteniendo datos de la cotización creada
   * 
   * @example
   * const request: CotizacionLegacyCreateRequest = {
   *   idCliente: 123,
   *   idAgente: 5,
   *   fechaVencimiento: '2025-12-31',
   *   productos: [
   *     {
   *       idProducto: 456,
   *       idAlmacen: 1,
   *       unidades: 10,
   *       precio: 100.0,
   *       porcentajeDescuento: 5.0
   *     }
   *   ],
   *   descuentoDoc1: 10.0,
   *   aplicarIVA: true,
   *   porcentajeIVA: 16.0,
   *   montoPagado: 500.0,
   *   observaciones: 'Cotización urgente',
   *   referencia: 'REF-2025-001'
   * };
   *
   * this.cotizacionService.crear(request).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       console.log(`✅ Cotización creada: Folio ${response.data.folio}`);
   *       console.log(`📊 Total: $${response.data.total}`);
   *       console.log(`💰 Pendiente: $${response.data.pendiente}`);
   *       // Navegar a detalle o mostrar mensaje de éxito
   *     }
   *   },
   *   error: (err) => {
   *     console.error('❌ Error al crear cotización:', err.mensaje);
   *     // Mostrar mensaje de error al usuario
   *   }
   * });
   */
  crear(request: CotizacionLegacyCreateRequest): Observable<CotizacionLegacyApiResponse<CotizacionLegacyCreateResponse>> {
    console.log('✨ Creando cotización Legacy');
    console.log('📋 Cliente ID:', request.idCliente);
    console.log('📦 Productos:', request.productos.length);
    
    return this.http.post<CotizacionLegacyApiResponse<CotizacionLegacyCreateResponse>>(
      `${this.apiUrl}/cotizacion`,
      request
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Cotización creada exitosamente`);
          console.log(`📄 ID: ${response.data.idDocumento}`);
          console.log(`🔢 Folio: ${response.data.folio}`);
          console.log(`💵 Total: $${response.data.total.toFixed(2)}`);
          console.log(`⏱️ Tiempo: ${response.executionTime}`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * ❌ Cancelar cotización
   * Endpoint: PUT /api/AdmDocumentos/cotizacion/cancelar
   * 
   * Cancela una cotización existente marcándola como cancelada.
   * El documento permanece en el sistema pero con estado CCANCELADO = 1.
   * Se registra el motivo de cancelación en las observaciones.
   * 
   * @param request - ID del documento y motivo de cancelación
   * @returns Observable con respuesta de la cancelación
   * 
   * @example
   * const request: CotizacionLegacyCancelarRequest = {
   *   idDocumento: 1234,
   *   motivo: 'Cliente solicitó cancelación por cambio de requerimientos'
   * };
   *
   * this.cotizacionService.cancelar(request).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       console.log(`✅ Cotización ${response.data.folio} cancelada`);
   *       console.log(`📅 Fecha: ${response.data.fechaCancelacion}`);
   *       console.log(`📝 Motivo: ${response.data.motivo}`);
   *       // Actualizar lista o mostrar mensaje
   *     }
   *   },
   *   error: (err) => {
   *     console.error('❌ Error al cancelar:', err.mensaje);
   *   }
   * });
   */
  cancelar(request: CotizacionLegacyCancelarRequest): Observable<CotizacionLegacyApiResponse<CotizacionLegacyCancelarResponse>> {
    console.log(`❌ Cancelando cotización ID: ${request.idDocumento}`);
    console.log(`📝 Motivo: ${request.motivo}`);
    
    return this.http.put<CotizacionLegacyApiResponse<CotizacionLegacyCancelarResponse>>(
      `${this.apiUrl}/cotizacion/cancelar`,
      request
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Cotización cancelada exitosamente`);
          console.log(`🔢 Folio: ${response.data.folio}`);
          console.log(`⏱️ Tiempo: ${response.executionTime}`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 🔍 Búsqueda paginada con filtros
   * Endpoint: GET /api/AdmDocumentos/search
   * 
   * Permite búsqueda avanzada de cotizaciones con múltiples filtros:
   * - Rango de fechas (documento y vencimiento)
   * - Folio específico
   * - Razón social del cliente
   * - Agente de ventas
   * - Paginación configurable
   * - Opción para incluir movimientos (productos)
   * 
   * @param filtros - Criterios de búsqueda opcionales
   * @returns Observable con respuesta paginada
   * 
   * @example
   * const filtros: CotizacionLegacyFiltros = {
   *   fechaInicio: '2025-01-01',
   *   fechaFin: '2025-12-31',
   *   razonSocial: 'ACME',
   *   incluirMovimientos: true,
   *   page: 1,
   *   pageSize: 20
   * };
   *
   * this.cotizacionService.buscar(filtros).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.cotizaciones = response.data.data;
   *       this.pagination = response.data.pagination;
   *       console.log(`📊 ${response.data.pagination.totalRecords} cotizaciones encontradas`);
   *     }
   *   }
   * });
   */
  buscar(filtros: CotizacionLegacyFiltros = {}): Observable<CotizacionLegacyApiResponse<CotizacionLegacyPaginado>> {
    console.log('🔍 Búsqueda de cotizaciones con filtros:', filtros);
    
    let params = new HttpParams();
    
    // Añadir filtros solo si tienen valor
    if (filtros.fechaInicio) {
      params = params.set('fechaInicio', filtros.fechaInicio);
    }
    if (filtros.fechaFin) {
      params = params.set('fechaFin', filtros.fechaFin);
    }
    if (filtros.fechaVencimientoInicio) {
      params = params.set('fechaVencimientoInicio', filtros.fechaVencimientoInicio);
    }
    if (filtros.fechaVencimientoFin) {
      params = params.set('fechaVencimientoFin', filtros.fechaVencimientoFin);
    }
    if (filtros.folio) {
      params = params.set('folio', filtros.folio);
    }
    if (filtros.razonSocial) {
      params = params.set('razonSocial', filtros.razonSocial);
    }
    if (filtros.idConcepto !== undefined) {
      params = params.set('idConcepto', filtros.idConcepto.toString());
    }
    if (filtros.idAgente !== undefined) {
      params = params.set('idAgente', filtros.idAgente.toString());
    }
    if (filtros.incluirMovimientos !== undefined) {
      params = params.set('incluirMovimientos', filtros.incluirMovimientos.toString());
    }
    if (filtros.page) {
      params = params.set('page', filtros.page.toString());
    }
    if (filtros.pageSize) {
      params = params.set('pageSize', filtros.pageSize.toString());
    }

    return this.http.get<any>(
      `${this.apiUrl}/search`,
      { params }
    ).pipe(
      map(response => {
        // Adaptar respuesta del backend (plana) a la estructura esperada por el frontend (anidada)
        if (response.success && Array.isArray(response.data)) {
          const adaptedResponse: CotizacionLegacyApiResponse<CotizacionLegacyPaginado> = {
            success: true,
            message: response.message,
            executionTime: response.executionTime,
            data: {
              data: response.data,
              pagination: response.pagination,
              filters: response.filters
            }
          };
          
          if (adaptedResponse.data?.pagination) {
            console.log(`✅ Búsqueda completada: ${adaptedResponse.data.pagination.totalRecords} registros totales`);
            console.log(`📄 Página ${adaptedResponse.data.pagination.currentPage}/${adaptedResponse.data.pagination.totalPages}`);
          }
          return adaptedResponse;
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 📄 Obtener cotización específica por ID
   * Endpoint: GET /api/AdmDocumentos/{id}
   * 
   * Retorna información completa de una cotización incluyendo:
   * - Datos del documento (folio, fecha, serie)
   * - Información del cliente
   * - Montos (subtotal, impuestos, total, pendiente)
   * - Movimientos (productos cotizados) si se solicitaron
   * 
   * @param id - ID del documento en el sistema Legacy
   * @returns Observable con respuesta conteniendo datos completos
   * 
   * @example
   * this.cotizacionService.obtenerPorId(1234).subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       this.cotizacion = response.data;
   *       console.log('📄 Folio:', response.data.folio);
   *       console.log('👤 Cliente:', response.data.razonSocial);
   *       console.log('💵 Total:', response.data.total);
   *       console.log('📦 Productos:', response.data.movimientos?.length || 0);
   *     }
   *   },
   *   error: (err) => {
   *     if (err.status === 404) {
   *       console.error('Cotización no encontrada');
   *     }
   *   }
   * });
   */
  obtenerPorId(id: number): Observable<CotizacionLegacyApiResponse<CotizacionLegacyResponse>> {
    console.log(`📄 Obteniendo cotización con ID: ${id}`);
    
    return this.http.get<CotizacionLegacyApiResponse<CotizacionLegacyResponse>>(
      `${this.apiUrl}/${id}`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Cotización obtenida: Folio ${response.data.folio}`);
          console.log(`👤 Cliente: ${response.data.razonSocial}`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 📊 Obtener resumen de cotizaciones por rango de fechas
   * Endpoint: GET /api/AdmDocumentos/resumen
   * 
   * Retorna el total de documentos en un rango de fechas específico.
   * Útil para dashboards o widgets de resumen.
   * 
   * @param fechaInicio - Fecha inicial (formato ISO: YYYY-MM-DD)
   * @param fechaFin - Fecha final (formato ISO: YYYY-MM-DD)
   * @returns Observable con respuesta conteniendo resumen
   * 
   * @example
   * this.cotizacionService.obtenerResumen('2025-01-01', '2025-12-31').subscribe({
   *   next: (response) => {
   *     if (response.success && response.data) {
   *       console.log(`📊 Total documentos: ${response.data.totalDocumentos}`);
   *     }
   *   }
   * });
   */
  obtenerResumen(fechaInicio: string, fechaFin: string): Observable<CotizacionLegacyApiResponse<{ fechaInicio: string; fechaFin: string; totalDocumentos: number }>> {
    console.log(`📊 Obteniendo resumen de cotizaciones: ${fechaInicio} - ${fechaFin}`);
    
    let params = new HttpParams()
      .set('fechaInicio', fechaInicio)
      .set('fechaFin', fechaFin);

    return this.http.get<CotizacionLegacyApiResponse<{ fechaInicio: string; fechaFin: string; totalDocumentos: number }>>(
      `${this.apiUrl}/resumen`,
      { params }
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          console.log(`✅ Resumen obtenido: ${response.data.totalDocumentos} documentos`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 🗑️ Eliminar cotización (solo si está cancelada)
   * Endpoint: DELETE /api/AdmDocumentos/{id}
   */
  eliminar(idDocumento: number): Observable<CotizacionLegacyApiResponse<null>> {
    console.log(`🗑️ Eliminando cotización ID: ${idDocumento}`);
    
    return this.http.delete<CotizacionLegacyApiResponse<null>>(
      `${this.apiUrl}/${idDocumento}`
    ).pipe(
      map(response => {
        if (response.success) {
          console.log(`✅ Cotización ${idDocumento} eliminada exitosamente`);
        }
        return response;
      }),
      catchError(this.manejarError)
    );
  }

  /**
   * 💰 Calcular totales de cotización (cliente-side)
   * 
   * Calcula totales de forma local antes de enviar al servidor.
   * Útil para preview en tiempo real mientras el usuario edita.
   * 
   * @param request - Request de cotización con productos y descuentos
   * @returns Objeto con cálculos detallados
   * 
   * @example
   * const totales = this.cotizacionService.calcularTotalesPreview(this.cotizacionForm.value);
   * console.log('Subtotal:', totales.subtotal);
   * console.log('IVA:', totales.iva);
   * console.log('Total:', totales.total);
   */
  calcularTotalesPreview(request: CotizacionLegacyCreateRequest): {
    subtotal: number;
    descuentoProductos: number;
    descuentoDocumento: number;
    neto: number;
    iva: number;
    total: number;
    pendiente: number;
  } {
    let subtotal = 0;
    let descuentoProductos = 0;

    // Calcular subtotal y descuentos por producto
    request.productos.forEach(prod => {
      const netoProducto = prod.unidades * prod.precio;
      const descProd = netoProducto * ((prod.porcentajeDescuento || 0) / 100);
      
      subtotal += netoProducto;
      descuentoProductos += descProd;
    });

    // Calcular neto después de descuentos de productos
    let neto = subtotal - descuentoProductos;

    // Aplicar descuentos a nivel documento (Importes directos)
    let descuentoDocumento = 0;
    if (request.descuentoDoc1) {
      descuentoDocumento += request.descuentoDoc1;
      neto -= request.descuentoDoc1;
    }
    if (request.descuentoDoc2) {
      descuentoDocumento += request.descuentoDoc2;
      neto -= request.descuentoDoc2;
    }
    if (request.descuentoDoc3) {
      descuentoDocumento += request.descuentoDoc3;
      neto -= request.descuentoDoc3;
    }

    // Calcular IVA
    const iva = request.aplicarIVA 
      ? neto * ((request.porcentajeIVA || 16) / 100)
      : 0;

    // Calcular total
    const total = neto + iva;

    // Calcular pendiente
    const pendiente = total - (request.montoPagado || 0);

    return {
      subtotal,
      descuentoProductos,
      descuentoDocumento,
      neto,
      iva,
      total,
      pendiente
    };
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
          mensajeError = error.error?.message || 'Datos de entrada inválidos';
          console.error('❌ Bad Request (400):', error.error?.message || error.message);
          if (error.error?.errors) {
            console.error('Errores de validación:', error.error.errors);
          }
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
          mensajeError = 'Cotización no encontrada';
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
