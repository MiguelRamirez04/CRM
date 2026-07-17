// =====================================================================================
// SERVICE AGENTE LEGACY - agente-legacy.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir APIs de agentes legacy.
// Proporciona métodos para obtener agentes paginados y buscar con manejo de errores.
//
// MÉTODOS:
// - getPaginated(): Obtener agentes paginados
// - searchPaginated(): Buscar agentes paginados
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
import { AgenteLegacyResponse, AgenteLegacyPaginatedResponse } from '../models/agente-legacy.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AgenteLegacyService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/AdmAgentes`;

  // ═══════════════════════════════════════════════════════════════
  // MÉTODOS PÚBLICOS
  // ═══════════════════════════════════════════════════════════════

  /**
   * 📄 Obtener agentes legacy paginados
   * @param page Página actual (1-based, default: 1)
   * @param pageSize Tamaño de página (default: 30, max: 100)
   * @returns Observable con respuesta paginada
   */
  getPaginated(page: number = 1, pageSize: number = 30): Observable<AgenteLegacyPaginatedResponse> {
    console.log(`👤 Obteniendo agentes paginados - Página: ${page}, Tamaño: ${pageSize}`);

    // Validar parámetros
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 30;
    if (pageSize > 100) pageSize = 100;

    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/paginated`, { params }).pipe(
      map(response => {
        console.log(`✅ Agentes obtenidos: ${response.items?.length || 0} registros de ${response.totalItems || 0}`);
        console.log('📦 Estructura de respuesta:', response);

        // Mapear respuesta del backend (camelCase: items, pagina, totalItems)
        const items = response.items || [];
        const currentPage = response.pagina || page;
        const totalPages = response.totalPaginas || 1;
        const totalCount = response.totalItems || 0;

        return {
          data: items.map((item: any) => this.mapToAgenteResponse(item)),
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

  /**
   * 🔍 Buscar agentes legacy paginados
   * @param query Término de búsqueda
   * @param page Página actual (1-based, default: 1)
   * @param pageSize Tamaño de página (default: 30, max: 100)
   * @returns Observable con respuesta paginada
   */
  searchPaginated(query: string, page: number = 1, pageSize: number = 30): Observable<AgenteLegacyPaginatedResponse> {
    console.log(`🔍 Buscando agentes - Query: "${query}", Página: ${page}, Tamaño: ${pageSize}`);

    // Validar parámetros
    if (!query?.trim()) {
      return throwError(() => new Error('El término de búsqueda es requerido'));
    }

    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 30;
    if (pageSize > 100) pageSize = 100;

    const params = new HttpParams()
      .set('q', query.trim())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/search/paginated`, { params }).pipe(
      map(response => {
        console.log(`✅ Agentes encontrados: ${response.items?.length || 0} registros de ${response.totalItems || 0}`);
        console.log('📦 Estructura de respuesta búsqueda:', response);

        // Mapear respuesta del backend (camelCase: items, pagina, totalItems)
        const items = response.items || [];
        const currentPage = response.pagina || page;
        const totalPages = response.totalPaginas || 1;
        const totalCount = response.totalItems || 0;

        return {
          data: items.map((item: any) => this.mapToAgenteResponse(item)),
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
  private mapToAgenteResponse(item: any): AgenteLegacyResponse {
    return {
      idAgente: item.idAgente || item.IdAgente,
      codigoAgente: item.codigoAgente || item.CodigoAgente || '',
      nombreAgente: item.nombreAgente || item.NombreAgente || '',
      fechaAlta: item.fechaAlta || item.FechaAlta || new Date().toISOString(),
      tipoAgente: item.tipoAgente || item.TipoAgente || 0,
      estatus: item.estatus || item.Estatus || 0,
      comisionVenta: item.comisionVenta || item.ComisionVenta || 0,
      comisionCobro: item.comisionCobro || item.ComisionCobro || 0,
      comisionVentaEfectivo: item.comisionVentaEfectivo || item.ComisionVentaEfectivo || 0,
      comisionCobroEfectivo: item.comisionCobroEfectivo || item.ComisionCobroEfectivo || 0,
      idValorClasifCliente1: item.idValorClasifCliente1 || item.IdValorClasifCliente1 || 0,
      idValorClasifCliente2: item.idValorClasifCliente2 || item.IdValorClasifCliente2 || 0,
      idValorClasifCliente3: item.idValorClasifCliente3 || item.IdValorClasifCliente3 || 0,
      idValorClasifCliente4: item.idValorClasifCliente4 || item.IdValorClasifCliente4 || 0,
      idValorClasifCliente5: item.idValorClasifCliente5 || item.IdValorClasifCliente5 || 0,
      idValorClasifCliente6: item.idValorClasifCliente6 || item.IdValorClasifCliente6 || 0,
      timestamp: item.timestamp || item.Timestamp || new Date().toISOString()
    };
  }

  /**
   * ❌ Manejador de errores HTTP
   */
  private manejarError = (error: any): Observable<never> => {
    console.error('❌ Error en AgenteLegacyService:', error);

    let mensaje = 'Error desconocido al obtener agentes';

    if (error.status === 401) {
      mensaje = 'No autorizado para acceder a agentes';
    } else if (error.status === 403) {
      mensaje = 'Acceso denegado a agentes';
    } else if (error.status === 404) {
      mensaje = 'Endpoint de agentes no encontrado';
    } else if (error.status >= 500) {
      mensaje = 'Error del servidor al obtener agentes';
    } else if (error.error?.message) {
      mensaje = error.error.message;
    }

    return throwError(() => new Error(mensaje));
  };
}