// =====================================================================================
// SERVICIO GASTOS VIATICOS - gasto-viatico.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Servicio Angular para consumir la API REST de Gastos de Viáticos del backend.
// Maneja todas las operaciones CRUD, filtros y paginación.
//
// ENDPOINTS DISPONIBLES:
// - GET    /api/GastoViaticos                - Obtener viáticos paginados (con filtros)
// - GET    /api/GastoViaticos/{id}           - Obtener viático por ID
// - POST   /api/GastoViaticos                - Crear nuevo viático
// - PUT    /api/GastoViaticos/{id}           - Actualizar viático
//
// AUTENTICACIÓN:
// - Todas las peticiones requieren JWT via HttpOnly cookies
// - Requiere rol "Soporte" en el backend
// - El interceptor secure-auth.interceptor.ts añade el token CSRF automáticamente
//
// PAGINACIÓN:
// - Por defecto: página 1, 10 resultados
// - Soporta filtros por: ordenId, fechaDesde, fechaHasta
//
// =====================================================================================

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  GastoViaticoCreateRequest,
  GastoViaticoUpdateRequest,
  GastoViaticoResponse,
  GastoViaticoPaginatedResponse,
  GastoViaticoFiltros,
  GastoViaticoListItem,
  GastoViaticoEstadisticas
} from '../models/gasto-viatico.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GastoViaticoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/GastoViaticos`;

  constructor() { }

  // =====================================================================================
  // MÉTODOS CRUD PRINCIPALES
  // =====================================================================================

  /**
   * Obtener viáticos con paginación y filtros opcionales
   * @param filtros - Objeto con criterios de filtrado y paginación
   * @returns Observable con respuesta paginada
   * 
   * @example
   * // Obtener primera página (10 resultados por defecto)
   * service.obtenerViaticos({}).subscribe(response => {
   *   console.log(response.items); // Array de viáticos
   *   console.log(response.totalPaginas); // Total de páginas
   * });
   * 
   * @example
   * // Filtrar por orden y rango de fechas
   * service.obtenerViaticos({
   *   ordenId: 123,
   *   fechaDesde: '2024-01-01',
   *   fechaHasta: '2024-12-31',
   *   pageNumber: 1,
   *   pageSize: 20
   * }).subscribe(...);
   */
  obtenerViaticos(filtros: GastoViaticoFiltros = {}): Observable<GastoViaticoPaginatedResponse> {
    let params = new HttpParams();

    // Aplicar filtros opcionales
    if (filtros.ordenId !== undefined && filtros.ordenId !== null) {
      params = params.set('ordenId', filtros.ordenId.toString());
    }

    if (filtros.fechaDesde) {
      const fecha = this.formatearFechaISO(filtros.fechaDesde);
      params = params.set('fechaDesde', fecha);
    }

    if (filtros.fechaHasta) {
      const fecha = this.formatearFechaISO(filtros.fechaHasta);
      params = params.set('fechaHasta', fecha);
    }

    // Paginación (valores por defecto si no se especifican)
    const pageNumber = filtros.pageNumber ?? 1;
    const pageSize = filtros.pageSize ?? 10;
    params = params.set('pageNumber', pageNumber.toString());
    params = params.set('pageSize', pageSize.toString());

    return this.http.get<GastoViaticoPaginatedResponse>(this.apiUrl, { params });
  }

  /**
   * Obtener un viático específico por ID
   * @param id - ID del viático
   * @returns Observable con el viático encontrado
   * 
   * @example
   * service.obtenerPorId(123).subscribe(viatico => {
   *   console.log(viatico.descripcion);
   *   console.log(viatico.montoTotal);
   * });
   */
  obtenerPorId(id: number): Observable<GastoViaticoResponse> {
    return this.http.get<GastoViaticoResponse>(`${this.apiUrl}/${id}`);
  }

  /**
   * Crear un nuevo gasto de viático
   * @param viatico - Datos del nuevo viático
   * @returns Observable con el viático creado (incluye ID generado)
   * 
   * @example
   * const nuevoViatico: GastoViaticoCreateRequest = {
   *   ordenId: 123,
   *   tieneFactura: true,
   *   descripcion: 'Viaje a Monterrey',
   *   proveedorNombre: 'Gasolinera Shell',
   *   fecha: new Date(),
   *   kmRecorridos: 450,
   *   gastos: 'COMBUSTIBLE, CASETAS',
   *   montoTotal: 1500.00,
   *   lugarDestino: 'Monterrey, NL'
   * };
   * service.crear(nuevoViatico).subscribe(creado => {
   *   console.log('Viático creado con ID:', creado.id);
   * });
   */
  crear(viatico: GastoViaticoCreateRequest): Observable<GastoViaticoResponse> {
    // Asegurar que la fecha esté en formato ISO
    const payload = {
      ...viatico,
      fecha: this.formatearFechaISO(viatico.fecha)
    };
    return this.http.post<GastoViaticoResponse>(this.apiUrl, payload);
  }

  /**
   * Actualizar un viático existente
   * @param id - ID del viático a actualizar
   * @param cambios - Datos actualizados (completos, no parciales)
   * @returns Observable vacío (204 No Content en caso de éxito)
   * 
   * @example
   * const actualizacion: GastoViaticoUpdateRequest = {
   *   tieneFactura: true,
   *   descripcion: 'Viaje actualizado',
   *   fecha: new Date(),
   *   gastos: 'COMBUSTIBLE, HOSPEDAJE',
   *   montoTotal: 2000.00
   * };
   * service.actualizar(123, actualizacion).subscribe(() => {
   *   console.log('Viático actualizado correctamente');
   * });
   */
  actualizar(id: number, cambios: GastoViaticoUpdateRequest): Observable<void> {
    // Asegurar que la fecha esté en formato ISO
    const payload = {
      ...cambios,
      fecha: this.formatearFechaISO(cambios.fecha)
    };
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  // =====================================================================================
  // MÉTODOS DE UTILIDAD Y HELPERS
  // =====================================================================================

  /**
   * Obtener viáticos de una orden específica
   * Método de conveniencia que usa obtenerViaticos con filtro
   * @param ordenId - ID de la orden de trabajo
   * @returns Observable con respuesta paginada de viáticos de esa orden
   */
  obtenerPorOrden(ordenId: number, pageNumber: number = 1, pageSize: number = 10): Observable<GastoViaticoPaginatedResponse> {
    return this.obtenerViaticos({ ordenId, pageNumber, pageSize });
  }

  /**
   * Obtener viáticos en un rango de fechas
   * Método de conveniencia para búsquedas por período
   * @param fechaDesde - Fecha inicial
   * @param fechaHasta - Fecha final
   * @returns Observable con respuesta paginada
   */
  obtenerPorRangoFechas(
    fechaDesde: string | Date,
    fechaHasta: string | Date,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<GastoViaticoPaginatedResponse> {
    return this.obtenerViaticos({ fechaDesde, fechaHasta, pageNumber, pageSize });
  }

  /**
   * Convertir respuesta paginada a lista simplificada
   * Útil para componentes que solo necesitan data básica
   * @param response - Respuesta paginada del backend
   * @returns Array de items simplificados para tablas
   */
  convertirAListaSimple(response: GastoViaticoPaginatedResponse): GastoViaticoListItem[] {
    return response.items.map(item => ({
      id: item.id,
      fecha: item.fecha,
      descripcion: item.descripcion,
      lugarDestino: item.lugarDestino,
      montoTotal: item.montoTotal,
      tieneFactura: item.tieneFactura,
      ordenId: item.ordenId
    }));
  }

  /**
   * Calcular estadísticas de un conjunto de viáticos
   * Útil para dashboards y reportes
   * @param viaticos - Array de viáticos
   * @returns Objeto con estadísticas calculadas
   */
  calcularEstadisticas(viaticos: GastoViaticoResponse[]): GastoViaticoEstadisticas {
    if (!viaticos || viaticos.length === 0) {
      return {
        totalGastos: 0,
        cantidadRegistros: 0,
        promedioGasto: 0,
        gastosConFactura: 0,
        gastosSinFactura: 0,
        totalKmRecorridos: 0
      };
    }

    const totalGastos = viaticos.reduce((sum, v) => sum + v.montoTotal, 0);
    const gastosConFactura = viaticos.filter(v => v.tieneFactura).length;
    const gastosSinFactura = viaticos.length - gastosConFactura;
    const totalKmRecorridos = viaticos.reduce((sum, v) => sum + (v.kmRecorridos ?? 0), 0);

    return {
      totalGastos,
      cantidadRegistros: viaticos.length,
      promedioGasto: totalGastos / viaticos.length,
      gastosConFactura,
      gastosSinFactura,
      totalKmRecorridos
    };
  }

  // =====================================================================================
  // VALIDACIONES Y FORMATEO
  // =====================================================================================

  /**
   * Formatear fecha a string ISO (YYYY-MM-DD)
   * Convierte Date o string a formato esperado por el backend
   * @param fecha - Fecha a formatear (Date o string)
   * @returns String en formato ISO (YYYY-MM-DD)
   */
  formatearFechaISO(fecha: string | Date): string {
    if (typeof fecha === 'string') {
      // Si ya es string, intentar parsearlo primero
      const parsed = new Date(fecha);
      if (!isNaN(parsed.getTime())) {
        return parsed.toISOString().split('T')[0];
      }
      return fecha; // Si no se puede parsear, devolver tal cual
    }
    // Si es Date, convertir a ISO
    return fecha.toISOString().split('T')[0];
  }

  /**
   * Formatear fecha para display en español
   * Convierte ISO date a formato legible (ej: "4 de noviembre, 2024")
   * @param fechaISO - String ISO date
   * @returns Fecha formateada en español
   */
  formatearFechaDisplay(fechaISO: string): string {
    const fecha = new Date(fechaISO);
    return fecha.toLocaleDateString('es-MX', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  /**
   * Formatear monto a moneda mexicana
   * @param monto - Cantidad a formatear
   * @returns String formateado (ej: "$1,500.00 MXN")
   */
  formatearMoneda(monto: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(monto);
  }

  /**
   * Validar monto positivo
   * @param monto - Cantidad a validar
   * @returns true si es mayor a 0
   */
  validarMonto(monto: number): boolean {
    return monto > 0;
  }

  /**
   * Validar kilómetros (debe ser no negativo)
   * @param km - Kilómetros a validar
   * @returns true si es >= 0
   */
  validarKilometros(km: number | null | undefined): boolean {
    if (km === null || km === undefined) return true; // Opcional
    return km >= 0;
  }

  /**
   * Validar que la fecha no sea futura
   * @param fecha - Fecha a validar
   * @returns true si la fecha es hoy o anterior
   */
  validarFechaNoFutura(fecha: string | Date): boolean {
    const fechaViatico = typeof fecha === 'string' ? new Date(fecha) : fecha;
    const hoy = new Date();
    hoy.setHours(23, 59, 59, 999); // Final del día
    return fechaViatico <= hoy;
  }

  /**
   * Obtener clase CSS según tiene factura o no
   * Útil para badges/chips en la UI
   * @param tieneFactura - Boolean indicando si tiene factura
   * @returns Clase CSS Tailwind
   */
  obtenerClaseFactura(tieneFactura: boolean): string {
    return tieneFactura
      ? 'bg-green-100 text-green-800 border border-green-300'
      : 'bg-gray-100 text-gray-800 border border-gray-300';
  }

  /**
   * Obtener label para factura
   * @param tieneFactura - Boolean indicando si tiene factura
   * @returns Texto descriptivo
   */
  obtenerLabelFactura(tieneFactura: boolean): string {
    return tieneFactura ? 'Con Factura' : 'Sin Factura';
  }

  /**
   * Parsear string de gastos en array
   * Convierte "COMBUSTIBLE, HOSPEDAJE, ALIMENTOS" en ["COMBUSTIBLE", "HOSPEDAJE", "ALIMENTOS"]
   * @param gastosStr - String de gastos separados por coma
   * @returns Array de conceptos
   */
  parsearGastos(gastosStr: string): string[] {
    return gastosStr
      .split(',')
      .map(g => g.trim())
      .filter(g => g.length > 0);
  }

  /**
   * Construir string de gastos desde array
   * Convierte ["COMBUSTIBLE", "HOSPEDAJE"] en "COMBUSTIBLE, HOSPEDAJE"
   * @param gastos - Array de conceptos
   * @returns String concatenado
   */
  construirStringGastos(gastos: string[]): string {
    return gastos
      .filter(g => g && g.trim().length > 0)
      .map(g => g.trim().toUpperCase())
      .join(', ');
  }

  /**
   * Validar formato de string de gastos
   * Debe tener máximo 200 caracteres (restricción de BD)
   * @param gastos - String a validar
   * @returns true si cumple restricción
   */
  validarFormatoGastos(gastos: string): boolean {
    return gastos.length > 0 && gastos.length <= 200;
  }
}
