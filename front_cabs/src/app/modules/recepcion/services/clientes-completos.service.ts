// =====================================================================================
// SERVICIO CLIENTES COMPLETOS - clientes-completos.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir las APIs de ClientesCompletos (BD Legacy).
// Incluye métodos para búsqueda, filtrado y paginación de clientes.
//
// DEPENDENCIAS:
// - HttpClient (inyectado)
// - Interceptors para JWT y headers
//
// =====================================================================================

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ClienteCompleto,
  PagedResponse,
  ApiClientesCompletosResponse
} from '../../../core/models/cliente-completo.interface';

@Injectable({
  providedIn: 'root'
})
export class ClientesCompletosService {
  private apiUrl = `${environment.apiUrl}/api/ClientesCompletos`;

  constructor(private http: HttpClient) {}

  // GET /api/ClientesCompletos - Lista paginada de todos los clientes
  getClientesPaginados(pagina: number = 1, porPagina: number = 20, busqueda?: string): Observable<PagedResponse<ClienteCompleto>> {
    let params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('porPagina', porPagina.toString());
    
    if (busqueda && busqueda.trim()) {
      params = params.set('busqueda', busqueda.trim());
    }

    return this.http.get<ApiClientesCompletosResponse>(this.apiUrl, {
      params,
      withCredentials: true
    }).pipe(
      map(response => this.transformApiResponse(response, pagina, porPagina)),
      catchError(this.handleError)
    );
  }

  // GET /api/ClientesCompletos/por-nombre - Búsqueda por nombre
  buscarPorNombre(nombre: string, pagina: number = 1, porPagina: number = 20): Observable<PagedResponse<ClienteCompleto>> {
    if (!nombre || nombre.trim().length < 2) {
      return throwError(() => new Error('El nombre debe tener al menos 2 caracteres'));
    }

    const params = new HttpParams()
      .set('nombre', nombre.trim())
      .set('pagina', pagina.toString())
      .set('porPagina', porPagina.toString());

    return this.http.get<ApiClientesCompletosResponse>(`${this.apiUrl}/por-nombre`, {
      params,
      withCredentials: true
    }).pipe(
      map(response => this.transformApiResponse(response, pagina, porPagina)),
      catchError(this.handleError)
    );
  }

  // GET /api/ClientesCompletos/por-rfc - Búsqueda por RFC
  buscarPorRfc(rfc: string, pagina: number = 1, porPagina: number = 20): Observable<PagedResponse<ClienteCompleto>> {
    if (!rfc || rfc.trim().length === 0) {
      return throwError(() => new Error('RFC es requerido'));
    }

    const params = new HttpParams()
      .set('rfc', rfc.trim())
      .set('pagina', pagina.toString())
      .set('porPagina', porPagina.toString());

    return this.http.get<ApiClientesCompletosResponse>(`${this.apiUrl}/por-rfc`, {
      params,
      withCredentials: true
    }).pipe(
      map(response => this.transformApiResponse(response, pagina, porPagina)),
      catchError(this.handleError)
    );
  }



  // Transformar la respuesta del API a la estructura esperada por el frontend
  private transformApiResponse(apiResponse: ApiClientesCompletosResponse, paginaActual: number, porPagina: number): PagedResponse<ClienteCompleto> {
    // Validar que la respuesta tenga la estructura esperada
    if (!apiResponse || !apiResponse.items) {
      console.warn('Respuesta del API no tiene la estructura esperada:', apiResponse);
      return {
        data: [],
        paginaActual: paginaActual,
        totalPaginas: 1,
        totalRegistros: 0,
        tieneSiguiente: false,
        tieneAnterior: false
      };
    }

    // Transformar cada cliente para agregar campos de compatibilidad
    const clientesTransformados = apiResponse.items.map(cliente => {
      // Construir dirección completa solo con valores válidos
      const direccionPartes = [
        cliente.calle,
        cliente.numeroExterior,
        cliente.colonia,
        cliente.ciudad,
        cliente.estado
      ].filter(parte => 
        parte && 
        parte.trim() && 
        parte.toLowerCase() !== 'null' && 
        parte !== '(Ninguno)'
      );
      
      const direccionCompleta = direccionPartes.length > 0 ? direccionPartes.join(', ') : null;

      return {
        ...cliente,
        // Campos de compatibilidad con el frontend
        id: cliente.clienteId,
        nombre: cliente.nombreComercial && cliente.nombreComercial !== '(Ninguno)' 
          ? cliente.nombreComercial 
          : 'Sin nombre comercial',
        direccion: direccionCompleta,
        telefono: cliente.telefonoPrincipal && cliente.telefonoPrincipal.trim() 
          ? cliente.telefonoPrincipal 
          : null,
        email: cliente.emailPrincipal && cliente.emailPrincipal.trim() 
          ? cliente.emailPrincipal 
          : null,
        fechaRegistro: new Date().toISOString() // Valor por defecto ya que no viene en el API
      };
    });

    // Calcular totales - usar datos aproximados si no vienen del API
    let totalRegistros = apiResponse.totalItems;
    let totalPaginas = apiResponse.totalPaginas;

    // Si no hay totales, estimarlos basándose en si hay datos completos
    if (totalRegistros === null || totalRegistros === undefined) {
      // Si tenemos menos resultados que el tamaño de página, probablemente es la última página
      if (apiResponse.items.length < porPagina) {
        totalRegistros = ((paginaActual - 1) * porPagina) + apiResponse.items.length;
        totalPaginas = paginaActual;
      } else {
        // Estimación conservadora
        totalRegistros = paginaActual * porPagina;
        totalPaginas = paginaActual + 1; // Asumir que hay al menos una página más
      }
    }

    if (totalPaginas === null || totalPaginas === undefined) {
      totalPaginas = Math.ceil(totalRegistros / porPagina);
    }

    return {
      data: clientesTransformados,
      paginaActual: paginaActual,
      totalPaginas: totalPaginas,
      totalRegistros: totalRegistros,
      tieneSiguiente: paginaActual < totalPaginas,
      tieneAnterior: paginaActual > 1
    };
  }

  // Método privado para manejo de errores
  private handleError(error: any): Observable<never> {
    let errorMessage = 'Ocurrió un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      switch (error.status) {
        case 400:
          errorMessage = 'Datos de búsqueda inválidos';
          break;
        case 401:
          errorMessage = 'Sesión expirada. Por favor, inicie sesión nuevamente';
          break;
        case 403:
          errorMessage = 'No tiene permisos para acceder a esta información';
          break;
        case 404:
          errorMessage = 'Cliente no encontrado';
          break;
        case 500:
          errorMessage = 'Error interno del servidor';
          break;
        default:
          errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }

    console.error('ClientesCompletosService Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}