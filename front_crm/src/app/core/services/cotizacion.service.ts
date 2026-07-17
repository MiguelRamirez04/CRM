
// =====================================================================================
// SERVICIO COTIZACIONES - cotizacion.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Cotizaciones del backend.
// Maneja todas las operaciones CRUD y filtros de búsqueda.
//
// ENDPOINTS DISPONIBLES:
// - GET    /api/Cotizaciones              - Obtener todas las cotizaciones
// - GET    /api/Cotizaciones/{id}         - Obtener cotización por ID
// - GET    /api/Cotizaciones/orden/{id}   - Obtener cotizaciones por OrdenId
// - GET    /api/Cotizaciones/estado/{est} - Filtrar por estado
// - GET    /api/Cotizaciones/cliente?q=   - Buscar por nombre cliente
// - POST   /api/Cotizaciones              - Crear nueva cotización
// - PUT    /api/Cotizaciones/{id}         - Actualizar cotización
// - DELETE /api/Cotizaciones/{id}         - Eliminar cotización
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
  CotizacionCreateRequest, 
  CotizacionUpdateRequest, 
  CotizacionResponse,
  CotizacionFiltros 
} from '../models/cotizacion.interface';
import { EstadoCotizacion } from '../enums/estado-cotizacion.enum';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CotizacionService {
  actualizarEstado(id: any, VENCIDA: EstadoCotizacion) {
    throw new Error('Method not implemented.');
  }
  obtenerCotizacionesActivas() {
    throw new Error('Method not implemented.');
  }
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/Cotizaciones`;
  

  constructor() { }

  /**
   * Obtener todas las cotizaciones
   * @returns Observable con array de cotizaciones
   */
  obtenerTodas(): Observable<CotizacionResponse[]> {
    return this.http.get<CotizacionResponse[]>(this.apiUrl);
  }

  /**
   * Obtener cotización por ID
   * @param id - ID de la cotización
   * @returns Observable con la cotización
   */
  obtenerPorId(id: number): Observable<CotizacionResponse> {
    return this.http.get<CotizacionResponse>(`${this.apiUrl}/${id}`);
  }

  /**
   * Obtener cotizaciones por ID de orden de trabajo
   * @param ordenId - ID de la orden de trabajo
   * @returns Observable con array de cotizaciones asociadas
   */
  obtenerPorOrden(ordenId: number): Observable<CotizacionResponse[]> {
    return this.http.get<CotizacionResponse[]>(`${this.apiUrl}/orden/${ordenId}`);
  }

  /**
   * Filtrar cotizaciones por estado
   * @param estado - Estado de la cotización (NUEVA, APROBADA, etc.)
   * @returns Observable con array de cotizaciones filtradas
   */
  filtrarPorEstado(estado: EstadoCotizacion): Observable<CotizacionResponse[]> {
    return this.http.get<CotizacionResponse[]>(`${this.apiUrl}/estado/${estado}`);
  }

  /**
   * Buscar cotizaciones por nombre de cliente
   * @param nombreCliente - Texto a buscar en nombre de cliente (búsqueda parcial)
   * @returns Observable con array de cotizaciones que coinciden
   */
  buscarPorCliente(nombreCliente: string): Observable<CotizacionResponse[]> {
    const params = new HttpParams().set('q', nombreCliente);
    return this.http.get<CotizacionResponse[]>(`${this.apiUrl}/cliente`, { params });
  }

  /**
   * Crear nueva cotización
   * @param cotizacion - Datos de la nueva cotización
   * @returns Observable con la cotización creada (incluye ID generado)
   */
  crear(cotizacion: CotizacionCreateRequest): Observable<CotizacionResponse> {
    return this.http.post<CotizacionResponse>(this.apiUrl, cotizacion);
  }

  /**
   * Actualizar cotización existente
   * @param id - ID de la cotización a actualizar
   * @param cambios - Datos actualizados (puede ser parcial o completo)
   * @returns Observable con la cotización actualizada
   */
  actualizar(id: number, cambios: Partial<CotizacionUpdateRequest>): Observable<CotizacionResponse> {
    return this.http.put<CotizacionResponse>(`${this.apiUrl}/${id}`, cambios);
  }

  /**
   * Eliminar cotización
   * @param id - ID de la cotización a eliminar
   * @returns Observable con confirmación
   */
  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Filtrar cotizaciones con múltiples criterios
   * Útil para tablas con filtros combinados
   * @param filtros - Objeto con criterios de filtrado opcionales
   * @returns Observable con array de cotizaciones filtradas
   */
  filtrarConCriterios(filtros: CotizacionFiltros): Observable<CotizacionResponse[]> {
    // Si se especifica ordenId, usar ese endpoint específico
    if (filtros.ordenId) {
      return this.obtenerPorOrden(filtros.ordenId);
    }
    
    // Si se especifica estado, usar endpoint de estado
    if (filtros.estado) {
      return this.filtrarPorEstado(filtros.estado);
    }
    
    // Si se especifica cliente, buscar por cliente
    if (filtros.cliente) {
      return this.buscarPorCliente(filtros.cliente);
    }
    
    // Si no hay filtros, obtener todas
    return this.obtenerTodas();
  }

  /**
   * Calcular total final manualmente (útil para preview antes de guardar)
   * @param subtotal - Subtotal de la cotización
   * @param impuestosTotal - Total de impuestos
   * @param descuento - Descuento aplicado (opcional)
   * @returns Total final calculado
   */
  calcularTotalFinal(subtotal: number, impuestosTotal: number, descuento?: number | null): number {
    const total = subtotal + impuestosTotal;
    const desc = descuento ?? 0;
    return total - desc;
  }

  /**
   * Validar RFC mexicano (formato básico)
   * @param rfc - RFC a validar
   * @returns true si cumple formato básico
   */
  validarRFC(rfc: string): boolean {
    const rfcRegex = /^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$/;
    return rfcRegex.test(rfc);
  }

  /**
   * Validar correo electrónico (formato estándar)
   * @param correo - Correo a validar
   * @returns true si cumple formato de email
   */
  validarCorreo(correo: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(correo);
  }

  /**
   * Validar teléfono (5 a 15 dígitos)
   * @param telefono - Número a validar
   * @returns true si tiene entre 5 y 15 dígitos
   */
  validarTelefono(telefono: number): boolean {
    const telefonoStr = telefono.toString();
    const longitud = telefonoStr.length;
    return longitud >= 5 && longitud <= 15;
  }

  /**
   * Formatear teléfono para display (opcional)
   * Convierte 6178907616 a 617-890-7616
   * @param telefono - Número a formatear
   * @returns Teléfono formateado
   */
  formatearTelefono(telefono: number): string {
    const str = telefono.toString();
    if (str.length === 10) {
      return `${str.slice(0, 3)}-${str.slice(3, 6)}-${str.slice(6)}`;
    }
    return str; // Si no es 10 dígitos, devolver sin formato
  }

  /**
   * Obtener label en español para estado
   * @param estado - Estado de la cotización
   * @returns Texto formateado en español
   */
  obtenerLabelEstado(estado: EstadoCotizacion): string {
    const labels: Record<EstadoCotizacion, string> = {
      [EstadoCotizacion.NUEVA]: 'Nueva',
      [EstadoCotizacion.APROBADA]: 'Aprobada',
      [EstadoCotizacion.RECHAZADA]: 'Rechazada',
      [EstadoCotizacion.VENCIDA]: 'Vencida',
      [EstadoCotizacion.CANCELADA]: 'Cancelada'
    };
    return labels[estado] || estado;
  }

  /**
   * Obtener clase CSS según estado (útil para badges/chips)
   * @param estado - Estado de la cotización
   * @returns Clase CSS según convención del proyecto
   */
  obtenerClaseEstado(estado: EstadoCotizacion): string {
    const clases: Record<EstadoCotizacion, string> = {
      [EstadoCotizacion.NUEVA]: 'bg-blue-500 text-white',
      [EstadoCotizacion.APROBADA]: 'bg-green-500 text-white',
      [EstadoCotizacion.RECHAZADA]: 'bg-red-500 text-white',
      [EstadoCotizacion.VENCIDA]: 'bg-orange-500 text-white',
      [EstadoCotizacion.CANCELADA]: 'bg-gray-500 text-white'
    };
    return clases[estado] || 'bg-gray-300 text-black';
  }
}
