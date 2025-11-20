// =====================================================================================
// SERVICE MONEDA LEGACY - moneda-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir APIs de monedas legacy.
// Proporciona métodos para obtener monedas paginadas con manejo de errores.
//
// MÉTODOS:
// - getPaginated(): Obtener monedas paginadas
//
// CARACTERÍSTICAS:
// - HttpClient para llamadas REST
// - Manejo de errores con retry
// - Logging detallado
// - Tipado fuerte con interfaces
//
// =====================================================================================

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { MonedaLegacyResponse, MonedaLegacyPaginatedResponse } from '../models/moneda-legacy.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MonedaLegacyService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/AdmMonedas`;

  // ═══════════════════════════════════════════════════════════════
  // MÉTODOS PÚBLICOS
  // ═══════════════════════════════════════════════════════════════

  /**
   * 📄 Obtener monedas legacy paginadas
   * @param page Página actual (1-based, default: 1)
   * @param pageSize Tamaño de página (default: 30, max: 100)
   * @returns Observable con respuesta paginada
   */
  getPaginated(page: number = 1, pageSize: number = 30): Observable<MonedaLegacyPaginatedResponse> {
    console.log(`💰 Obteniendo monedas paginadas - Página: ${page}, Tamaño: ${pageSize}`);

    // Validar parámetros
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 30;
    if (pageSize > 100) pageSize = 100;

    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/paginated`, { params }).pipe(
      map(response => {
        console.log(`✅ Monedas obtenidas: ${response.items?.length || 0} registros de ${response.totalItems || 0}`);
        console.log('📦 Estructura de respuesta:', response);

        // Mapear respuesta del backend (camelCase: items, pagina, totalItems)
        const items = response.items || [];
        const currentPage = response.pagina || page;
        const totalPages = response.totalPaginas || 1;
        const totalCount = response.totalItems || 0;

        return {
          data: items.map((item: any) => this.mapToMonedaResponse(item)),
          pagination: {
            currentPage,
            totalPages,
            pageSize: response.resultadosPorPagina || pageSize,
            totalCount,
            hasPrevious: currentPage > 1,
            hasNext: currentPage < totalPages
          },
          meta: {
            timestamp: new Date().toISOString(),
            durationMs: 0,
            source: 'backend'
          }
        };
      }),
      catchError(this.manejarError)
    );
  }

  // ═══════════════════════════════════════════════════════════════
  // MÉTODOS PRIVADOS
  // ═══════════════════════════════════════════════════════════════

  /**
   * 🔄 Mapear respuesta del backend a interfaz TypeScript
   */
  private mapToMonedaResponse(item: any): MonedaLegacyResponse {
    return {
      idMoneda: item.idMoneda || item.IdMoneda,
      nombreMoneda: item.nombreMoneda || item.NombreMoneda || '',
      simboloMoneda: item.simboloMoneda || item.SimboloMoneda || '',
      posicionSimbolo: item.posicionSimbolo || item.PosicionSimbolo || 0,
      numeroDecimales: item.numeroDecimales || item.NumeroDecimales || 2,
      separadorMiles: item.separadorMiles || item.SeparadorMiles || ',',
      separadorDecimales: item.separadorDecimales || item.SeparadorDecimales || '.',
      timestamp: item.timestamp || item.Timestamp || new Date().toISOString()
    };
  }

  /**
   * ❌ Manejador de errores HTTP
   */
  private manejarError = (error: any): Observable<never> => {
    console.error('❌ Error en MonedaLegacyService:', error);

    let mensaje = 'Error desconocido al obtener monedas';

    if (error.status === 401) {
      mensaje = 'No autorizado para acceder a monedas';
    } else if (error.status === 403) {
      mensaje = 'Acceso denegado a monedas';
    } else if (error.status === 404) {
      mensaje = 'Endpoint de monedas no encontrado';
    } else if (error.status >= 500) {
      mensaje = 'Error del servidor al obtener monedas';
    } else if (error.error?.message) {
      mensaje = error.error.message;
    }

    return throwError(() => new Error(mensaje));
  };
}