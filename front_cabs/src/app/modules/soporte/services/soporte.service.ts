import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../../../environments/environment';

// --- Interfaces (simplificadas para este componente) ---
// (Puedes moverlas a core/models si lo prefieres)

export enum TipoEjecucion {
  CAMPO = 'CAMPO',
  REMOTO = 'REMOTO',
}

export interface OrdenAsignada {
  id: number; // ID de la Orden de Trabajo
  clienteNombre: string;
  vehiculoPlacas?: string;
  tipoOrden: string;
  modalidad: string; // ej. 'Presencial' o 'Remoto'
  estado: string;    // ej. 'ASIGNADA', 'EN_CURSO'
  prioridad: number;
  
  // Si ya tiene una ejecución EN CURSO, vendrá aquí
  ejecucionActiva?: {
    id: number; // ID de la Ejecución
    hrInicio: string;
    tipoEjecucion: TipoEjecucion;
    vehiculoId?: number;
  }
}

export interface EjecucionCreateDto {
  ordenId: number;
  tecnicoId: number; // El backend debe tomar esto del usuario autenticado
  tipoEjecucion: TipoEjecucion;
  hrInicio: string;
  vehiculoId?: number | null;
  kmInicial?: number | null;
  comentarios?: string;
}

// Nota: Tu API (imagen) espera un PATCH para actualizar, 
// por lo que este DTO solo debería contener los campos que se actualizan.
export interface EjecucionUpdateDto {
  hrFin: string;
  kmFinal?: number | null;
  comentarios?: string;
  // Añade aquí cualquier otro campo que tu API espere en el PATCH
}
// ----------------------------------------------------

@Injectable({
  providedIn: 'root'
})
export class SoporteService {
  private http = inject(HttpClient);
  
  // --- CORRECCIÓN ---
  // La URL base debe apuntar al controlador de EjecucionOrden, no a Soporte
  private baseUrl = `${environment.apiUrl}/api/EjecucionOrden`; 

  /**
   * Obtiene solo las órdenes asignadas al técnico autenticado
   */
  getMisOrdenes(): Observable<OrdenAsignada[]> {
    // --- CORRECCIÓN ---
    // Esta es la llamada 'GET /api/EjecucionOrden' que soluciona tu 404
    return this.http.get<OrdenAsignada[]>(this.baseUrl);
  }

  /**
   * Inicia una nueva ejecución para una orden
   */
  iniciarEjecucion(dto: EjecucionCreateDto): Observable<any> {
    // --- CORRECCIÓN ---
    // Esta es la llamada 'POST /api/EjecucionOrden'
    return this.http.post(this.baseUrl, dto);
  }

  /**
   * Finaliza una ejecución existente
   */
  finalizarEjecucion(ejecucionId: number, dto: EjecucionUpdateDto): Observable<any> {
    // --- CORRECCIÓN ---
    // Tu API espera un 'PATCH /api/EjecucionOrden/{id}', no un PUT con /finalizar
    return this.http.patch(`${this.baseUrl}/${ejecucionId}`, dto);
  }

  /**
   * Obtiene los detalles completos de una orden (ejecución)
   */
  getOrdenDetalle(id: number): Observable<OrdenAsignada> {
     // --- CORRECCIÓN ---
     // Esto apunta a 'GET /api/EjecucionOrden/{id}'
     return this.http.get<OrdenAsignada>(`${this.baseUrl}/${id}`);
  }
}