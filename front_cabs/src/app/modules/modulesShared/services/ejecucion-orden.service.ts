import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, tap, throwError, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  EjecucionOrdenCreateDto,
  EjecucionOrdenUpdateDto,
  DelegateEjecucionDto,
  EjecucionOrdenResponse,
  EjecucionOrdenFilters,
  OrdenTrabajoSimple,
  TecnicoSimple,
  VehiculoSimple
} from '../interfaces/ejecucion-orden.interface';

/**
 * Servicio para gestionar las ejecuciones de órdenes de trabajo
 */
@Injectable({
  providedIn: 'root'
})
export class EjecucionOrdenService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/EjecucionOrden`;

  /**
   * Crea una nueva ejecución de orden
   * @param dto Datos de la ejecución a crear
   * @returns Observable con la ejecución creada
   */
  createEjecucion(dto: EjecucionOrdenCreateDto): Observable<EjecucionOrdenResponse> {
    return this.http.post<EjecucionOrdenResponse>(this.apiUrl, dto).pipe(
      tap(response => console.log('✅ Ejecución creada:', response)),
      catchError(error => {
        console.error('❌ Error al crear ejecución:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene una ejecución por su ID
   * @param id ID de la ejecución
   * @returns Observable con los datos de la ejecución
   */
  getEjecucionById(id: number): Observable<EjecucionOrdenResponse> {
    return this.http.get<EjecucionOrdenResponse>(`${this.apiUrl}/${id}`).pipe(
      tap(response => console.log('📋 Ejecución obtenida:', response)),
      catchError(error => {
        console.error('❌ Error al obtener ejecución:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Lista ejecuciones con filtros opcionales
   * @param filters Filtros de búsqueda
   * @returns Observable con la lista de ejecuciones
   */
  getEjecuciones(filters?: EjecucionOrdenFilters): Observable<EjecucionOrdenResponse[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.ordenId) params = params.set('ordenId', filters.ordenId.toString());
      if (filters.tecnicoId) params = params.set('tecnicoId', filters.tecnicoId.toString());
      if (filters.tipoEjecucion) params = params.set('tipoEjecucion', filters.tipoEjecucion);
      if (filters.fechaDesde) {
        const fecha = typeof filters.fechaDesde === 'string' 
          ? filters.fechaDesde 
          : filters.fechaDesde.toISOString();
        params = params.set('fechaDesde', fecha);
      }
      if (filters.fechaHasta) {
        const fecha = typeof filters.fechaHasta === 'string' 
          ? filters.fechaHasta 
          : filters.fechaHasta.toISOString();
        params = params.set('fechaHasta', fecha);
      }
    }

    return this.http.get<EjecucionOrdenResponse[]>(this.apiUrl, { params }).pipe(
      tap(response => console.log('📋 Ejecuciones obtenidas:', response.length)),
      catchError(error => {
        console.error('❌ Error al listar ejecuciones:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Actualiza una ejecución existente
   * @param id ID de la ejecución
   * @param dto Datos a actualizar
   * @returns Observable vacío (NoContent)
   */
  updateEjecucion(id: number, dto: EjecucionOrdenUpdateDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}`, dto).pipe(
      tap(() => console.log('✅ Ejecución actualizada:', id)),
      catchError(error => {
        console.error('❌ Error al actualizar ejecución:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Delega una ejecución a otro técnico
   * @param id ID de la ejecución
   * @param dto Datos de la delegación
   * @returns Observable vacío (NoContent)
   */
  delegateEjecucion(id: number, dto: DelegateEjecucionDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/delegate`, dto).pipe(
      tap(() => console.log('✅ Ejecución delegada:', id)),
      catchError(error => {
        console.error('❌ Error al delegar ejecución:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Finaliza una ejecución de tipo CAMPO
   * @param id ID de la ejecución
   * @param kmFinal Kilometraje final
   * @param comentarios Comentarios opcionales
   * @returns Observable vacío
   */
  finalizarEjecucionCampo(id: number, kmFinal: number, comentarios?: string): Observable<void> {
    const dto: EjecucionOrdenUpdateDto = {
      hrFin: new Date().toISOString(),
      kmFinal,
      comentarios
    };
    return this.updateEjecucion(id, dto);
  }

  /**
   * Finaliza una ejecución de tipo REMOTO
   * @param id ID de la ejecución
   * @param comentarios Comentarios opcionales
   * @returns Observable vacío
   */
  finalizarEjecucionRemoto(id: number, comentarios?: string): Observable<void> {
    const dto: EjecucionOrdenUpdateDto = {
      hrFin: new Date().toISOString(),
      comentarios
    };
    return this.updateEjecucion(id, dto);
  }

  /**
   * Calcula la duración de una ejecución en formato legible
   * @param ejecucion Datos de la ejecución
   * @returns String con la duración formateada (ej: "2h 30m")
   */
  calcularDuracion(ejecucion: EjecucionOrdenResponse): string {
    if (!ejecucion.hrFin) return 'En curso...';

    const inicio = new Date(ejecucion.hrInicio);
    const fin = new Date(ejecucion.hrFin);
    const diffMs = fin.getTime() - inicio.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    
    const hours = Math.floor(diffMinutes / 60);
    const minutes = diffMinutes % 60;

    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }

  /**
   * Obtiene el icono apropiado según el tipo de ejecución
   * @param tipo Tipo de ejecución
   * @returns Nombre del icono
   */
  getIconoTipo(tipo: string): string {
    return tipo === 'CAMPO' ? '🚗' : '💻';
  }

  /**
   * Obtiene el color apropiado según el tipo de ejecución
   * @param tipo Tipo de ejecución
   * @returns Clase CSS de color
   */
  getColorTipo(tipo: string): string {
    return tipo === 'CAMPO' ? 'text-blue-600' : 'text-green-600';
  }

  // ====================================
  // Métodos para catálogos (datos legibles)
  // ====================================

  /**
   * Obtiene la lista de órdenes de trabajo disponibles
   * @returns Observable con órdenes simplificadas (con nombre de cliente)
   */
  getOrdenesDisponibles(): Observable<OrdenTrabajoSimple[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/OrdenTrabajo`).pipe(
      map(ordenes => ordenes.map(orden => ({
        id: orden.id,
        clienteNombre: orden.clienteNombre || orden.cliente?.nombre || `Cliente #${orden.clienteId}`,
        vehiculoPlacas: orden.vehiculoPlacas || orden.vehiculo?.placas,
        descripcion: orden.descripcion,
        estado: orden.estado
      }))),
      tap(ordenes => console.log('📋 Órdenes disponibles:', ordenes.length)),
      catchError(error => {
        console.error('❌ Error al obtener órdenes:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la lista de técnicos disponibles
   * @returns Observable con técnicos simplificados
   */
  getTecnicosDisponibles(): Observable<TecnicoSimple[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/auth/usuarios`).pipe(
      map(usuarios => usuarios
        .filter(u => u.rol === 'SOPORTE' || u.rol === 'ADMINISTRACION')
        .map(user => ({
          id: user.id,
          nombre: user.nombre || '',
          apellido: user.apellido || '',
          nombreCompleto: `${user.nombre || ''} ${user.apellido || ''}`.trim() || user.correo,
          correo: user.correo
        }))),
      tap(tecnicos => console.log('👷 Técnicos disponibles:', tecnicos.length)),
      catchError(error => {
        console.error('❌ Error al obtener técnicos:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Obtiene la lista de vehículos disponibles
   * @returns Observable con vehículos simplificados
   */
  getVehiculosDisponibles(): Observable<VehiculoSimple[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/Vehiculo`).pipe(
      map(vehiculos => vehiculos.map(v => ({
        id: v.id,
        placas: v.placas,
        marca: v.marca,
        modelo: v.modelo,
        anio: v.anio,
        descripcion: `${v.placas} - ${v.marca || ''} ${v.modelo || ''} ${v.anio || ''}`.trim()
      }))),
      tap(vehiculos => console.log('🚗 Vehículos disponibles:', vehiculos.length)),
      catchError(error => {
        console.error('❌ Error al obtener vehículos:', error);
        return throwError(() => error);
      })
    );
  }
}
