// =====================================================================================
// SERVICIO REPORTES COTIZACIONES - reportes-cotizaciones.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir las APIs de reportes de cotizaciones del backend.
// Proporciona métodos para obtener estadísticas, rankings y análisis de cotizaciones.
//
// ENDPOINTS DISPONIBLES:
// - GET /api/AdmDocumentos/estadisticas                    - Estadísticas generales
// - GET /api/AdmDocumentos/top-clientes                    - Top clientes
// - GET /api/AdmDocumentos/proximas-vencer                 - Cotizaciones próximas a vencer
// - GET /api/AdmDocumentos/rendimiento-agentes             - Rendimiento por agente
// - GET /api/AdmDocumentos/productos-mas-cotizados        - Productos más cotizados
// - GET /api/AdmDocumentos/cotizaciones-por-rango-monto    - Distribución por rangos
//
// AUTENTICACIÓN:
// - Todas las peticiones requieren JWT via HttpOnly cookies
// - El interceptor secure-auth.interceptor.ts añade el token automáticamente
//
// =====================================================================================

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  EstadisticasGeneralesDto,
  TopClienteDto,
  CotizacionVencimientoDto,
  RendimientoAgenteDto,
  ProductoCotizadoDto,
  CotizacionPorRangoDto,
  ApiResponse,
  FiltrosFecha
} from '../models/reportes-cotizaciones.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportesCotizacionesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/AdmDocumentos`;

  // =====================================================================================
  // ESTADÍSTICAS GENERALES
  // =====================================================================================

  /**
   * Obtiene estadísticas generales del dashboard de cotizaciones
   * @param fechaInicio Fecha inicial del rango en formato YYYY-MM-DD (opcional)
   * @param fechaFin Fecha final del rango en formato YYYY-MM-DD (opcional)
   * @returns Observable con estadísticas generales
   */
  getEstadisticasGenerales(fechaInicio?: string, fechaFin?: string): Observable<ApiResponse<EstadisticasGeneralesDto>> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.set('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.set('fechaFin', fechaFin);
    }

    return this.http.get<ApiResponse<EstadisticasGeneralesDto>>(`${this.apiUrl}/estadisticas`, { params });
  }

  // =====================================================================================
  // TOP CLIENTES
  // =====================================================================================

  /**
   * Obtiene el ranking de los mejores clientes por cotizaciones
   * @param top Número de clientes a retornar (default: 10)
   * @param fechaInicio Fecha inicial del rango en formato YYYY-MM-DD (opcional)
   * @param fechaFin Fecha final del rango en formato YYYY-MM-DD (opcional)
   * @returns Observable con lista de top clientes
   */
  getTopClientes(top: number = 10, fechaInicio?: string, fechaFin?: string): Observable<ApiResponse<TopClienteDto[]>> {
    let params = new HttpParams()
      .set('top', top.toString());

    if (fechaInicio) {
      params = params.set('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.set('fechaFin', fechaFin);
    }

    return this.http.get<ApiResponse<TopClienteDto[]>>(`${this.apiUrl}/top-clientes`, { params });
  }

  // =====================================================================================
  // COTIZACIONES PRÓXIMAS A VENCER
  // =====================================================================================

  /**
   * Obtiene cotizaciones próximas a vencer en los próximos N días con paginación
   * @param dias Número de días para considerar como "próximas a vencer" (default: 30)
   * @param page Número de página (default: 1)
   * @param pageSize Registros por página (default: 20)
   * @returns Observable con lista de cotizaciones próximas a vencer
   */
  getCotizacionesProximasVencer(
    dias: number = 30, 
    page: number = 1, 
    pageSize: number = 20
  ): Observable<ApiResponse<CotizacionVencimientoDto[]>> {
    const params = new HttpParams()
      .set('dias', dias.toString())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<CotizacionVencimientoDto[]>>(`${this.apiUrl}/proximas-vencer`, { params });
  }

  // =====================================================================================
  // RENDIMIENTO POR AGENTE
  // =====================================================================================

  /**
   * Obtiene métricas de rendimiento por agente de ventas
   * @param fechaInicio Fecha inicial del rango en formato YYYY-MM-DD (opcional)
   * @param fechaFin Fecha final del rango en formato YYYY-MM-DD (opcional)
   * @returns Observable con lista de rendimiento por agente
   */
  getRendimientoAgentes(fechaInicio?: string, fechaFin?: string): Observable<ApiResponse<RendimientoAgenteDto[]>> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.set('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.set('fechaFin', fechaFin);
    }

    return this.http.get<ApiResponse<RendimientoAgenteDto[]>>(`${this.apiUrl}/rendimiento-agentes`, { params });
  }

  // =====================================================================================
  // PRODUCTOS MÁS COTIZADOS
  // =====================================================================================

  /**
   * Obtiene ranking de productos más cotizados por frecuencia y volumen
   * @param top Número de productos a retornar (default: 10)
   * @param fechaInicio Fecha inicial del rango en formato YYYY-MM-DD (opcional)
   * @param fechaFin Fecha final del rango en formato YYYY-MM-DD (opcional)
   * @returns Observable con lista de productos más cotizados
   */
  getProductosMasCotizados(top: number = 10, fechaInicio?: string, fechaFin?: string): Observable<ApiResponse<ProductoCotizadoDto[]>> {
    let params = new HttpParams()
      .set('top', top.toString());

    if (fechaInicio) {
      params = params.set('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.set('fechaFin', fechaFin);
    }

    return this.http.get<ApiResponse<ProductoCotizadoDto[]>>(`${this.apiUrl}/productos-mas-cotizados`, { params });
  }

  // =====================================================================================
  // COTIZACIONES POR RANGO DE MONTO
  // =====================================================================================

  /**
   * Obtiene distribución de cotizaciones por rangos de monto
   * @param fechaInicio Fecha inicial del rango en formato YYYY-MM-DD (opcional)
   * @param fechaFin Fecha final del rango en formato YYYY-MM-DD (opcional)
   * @returns Observable con lista de rangos con estadísticas
   */
  getCotizacionesPorRangoMonto(fechaInicio?: string, fechaFin?: string): Observable<ApiResponse<CotizacionPorRangoDto[]>> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.set('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.set('fechaFin', fechaFin);
    }

    return this.http.get<ApiResponse<CotizacionPorRangoDto[]>>(`${this.apiUrl}/cotizaciones-por-rango-monto`, { params });
  }

  // =====================================================================================
  // MÉTODOS DE UTILIDAD
  // =====================================================================================

  /**
   * Construye filtros de fecha para usar en consultas
   * @param fechaInicio Fecha inicial
   * @param fechaFin Fecha final
   * @returns Objeto con filtros formateados
   */
  construirFiltrosFecha(fechaInicio?: Date, fechaFin?: Date): FiltrosFecha {
    const filtros: FiltrosFecha = {};

    if (fechaInicio) {
      filtros.fechaInicio = fechaInicio.toISOString().split('T')[0];
    }

    if (fechaFin) {
      filtros.fechaFin = fechaFin.toISOString().split('T')[0];
    }

    return filtros;
  }

  /**
   * Valida que las fechas sean coherentes
   * @param fechaInicio Fecha inicial
   * @param fechaFin Fecha final
   * @returns true si las fechas son válidas
   */
  validarRangoFechas(fechaInicio?: Date, fechaFin?: Date): boolean {
    if (!fechaInicio || !fechaFin) return true;
    return fechaInicio <= fechaFin;
  }
}