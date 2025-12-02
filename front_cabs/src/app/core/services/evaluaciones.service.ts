import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, forkJoin, of, throwError } from 'rxjs';
import { map, catchError, retry, delay, tap, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  EvaluacionResponse,
  EvaluacionCreateRequest,
  EvaluacionUpdateRequest,
  EvaluacionDetalleResponse,
  EvaluacionDetalleRequest,
  FotoEvaluacionResponse,
  FotoEvaluacionUploadRequest,
  EvaluacionCompletaDTO,
  DatosFase
} from '../models/evaluaciones.interface';

// =====================================================================================
// INTERFACES PARA CATÁLOGOS (CLIENTES ELIMINADO)
// =====================================================================================

/** Orden de trabajo para el select */
export interface OrdenTrabajoSelect {
  id: number;
  folio?: string;
  nombreCliente: string;
  estado: string;
  displayText: string;
}

/** Ejecución para el select */
export interface EjecucionSelect {
  id: number;
  ordenId: number;
  tecnicoNombre: string;
  fechaInicio: string;
  displayText: string;
}

/** Usuario para el select (Evaluadores) */
export interface UsuarioSelect {
  id: number;
  nombreCompleto: string;
  rol: string;
  displayText: string;
}

/** Respuesta paginada genérica */
interface PaginatedResponse<T> {
  items: T[];
  pagina: number;
  resultadosPorPagina: number;
  totalItems: number;
  totalPaginas: number;
}

@Injectable({
  providedIn: 'root'
})
export class EvaluacionService {
  // URLs del API
  private readonly API = `${environment.apiUrl}/api`;
  public readonly API_FOTOS = `${this.API}/FotosEvaluacion`;
  private readonly API_FILES = `${this.API}/Files`;

  constructor(private http: HttpClient) { }

  // =====================================================================================
  // CATÁLOGOS PARA FORMULARIOS (CLIENTES ELIMINADO)
  // =====================================================================================

  /**
   * Obtener órdenes de trabajo
   * GET /api/Recepcion
   */
  obtenerOrdenesTrabajo(estado?: string): Observable<OrdenTrabajoSelect[]> {
    let params = new HttpParams();
    if (estado) {
      params = params.set('estado', estado);
    }

    return this.http.get<any[]>(`${this.API}/Recepcion`, { params }).pipe(
      map(ordenes => ordenes.map((orden: any) => ({
        id: orden.id,
        folio: orden.folio || `OT-${orden.id}`,
        nombreCliente: orden.nombreCliente || 'Sin nombre',
        estado: orden.estado || 'Desconocido',
        displayText: `${orden.folio || 'OT-' + orden.id} - ${orden.nombreCliente || 'Sin nombre'} [${orden.estado || 'N/A'}]`
      }))),
      catchError(error => {
        console.error('Error al obtener órdenes:', error);
        return of([]);
      })
    );
  }

  /**
   * Obtener ejecuciones
   * GET /api/EjecucionOrden
   */
  obtenerEjecuciones(ordenId?: number): Observable<EjecucionSelect[]> {
    let params = new HttpParams();
    if (ordenId) {
      params = params.set('ordenId', ordenId.toString());
    }

    return this.http.get<any[]>(`${this.API}/EjecucionOrden`, { params }).pipe(
      map(ejecuciones => ejecuciones.map((ej: any) => ({
        id: ej.id,
        ordenId: ej.ordenId,
        tecnicoNombre: ej.tecnicoNombre || 'Sin técnico',
        fechaInicio: ej.hrInicio,
        displayText: `Ejecución #${ej.id} - ${ej.tecnicoNombre || 'Sin técnico'} (${this.formatearFecha(ej.hrInicio)})`
      }))),
      catchError(error => {
        console.error('Error al obtener ejecuciones:', error);
        return of([]);
      })
    );
  }

  /**
   * Obtener usuario actual
   * GET /api/Auth/me - Retorna string JSON que debe parsearse
   */
  obtenerUsuarioActual(): Observable<UsuarioSelect> {
    return this.http.get(`${this.API}/Auth/me`, {
      responseType: 'text'
    }).pipe(
      map(response => {
        try {
          const data = typeof response === 'string' ? JSON.parse(response) : response;
          const user = data?.user || data;

          return {
            id: user?.id ?? 0,
            nombreCompleto: user?.nombreCompleto ?? user?.name ?? 'Usuario',
            rol: user?.rol ?? user?.role ?? 'Desconocido',
            displayText: `${user?.nombreCompleto ?? user?.name ?? 'Usuario'} (${user?.rol ?? user?.role ?? 'Rol'})`
          } as UsuarioSelect;
        } catch (error) {
          console.error('Error al parsear usuario actual:', error);
          return {
            id: 0,
            nombreCompleto: 'Usuario',
            rol: 'Desconocido',
            displayText: 'Usuario (Desconocido)'
          } as UsuarioSelect;
        }
      }),
      catchError(error => {
        console.error('Error al obtener usuario actual:', error);
        return of({
          id: 0,
          nombreCompleto: 'Usuario',
          rol: 'Desconocido',
          displayText: 'Usuario (Desconocido)'
        } as UsuarioSelect);
      })
    );
  }

  /**
   * Cargar todos los catálogos (SIN CLIENTES)
   */
  cargarCatalogosFormulario(ordenId?: number): Observable<{
    ordenes: OrdenTrabajoSelect[];
    ejecuciones: EjecucionSelect[];
    usuarioActual: UsuarioSelect;
  }> {
    return forkJoin({
      ordenes: this.obtenerOrdenesTrabajo(),
      ejecuciones: ordenId ? this.obtenerEjecuciones(ordenId) : of([]),
      usuarioActual: this.obtenerUsuarioActual()
    }).pipe(
      catchError(error => {
        console.error('Error al cargar catálogos:', error);
        return of({
          ordenes: [],
          ejecuciones: [],
          usuarioActual: {
            id: 0,
            nombreCompleto: 'Usuario',
            rol: 'Desconocido',
            displayText: 'Usuario (Desconocido)'
          }
        });
      })
    );
  }

  // =====================================================================================
  // EVALUACIONES - CRUD COMPLETO
  // =====================================================================================

  obtenerTodas(): Observable<EvaluacionResponse[]> {
    return this.http.get<EvaluacionResponse[]>(`${this.API}/Evaluacion`).pipe(
      catchError(this.handleError)
    );
  }

  obtenerPorId(id: number): Observable<EvaluacionResponse> {
    return this.http.get<EvaluacionResponse>(`${this.API}/Evaluacion/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  crear(evaluacion: EvaluacionCreateRequest): Observable<EvaluacionResponse> {
    return this.http.post<EvaluacionResponse>(`${this.API}/Evaluacion`, evaluacion).pipe(
      catchError(this.handleError)
    );
  }

  actualizar(id: number, cambios: Partial<EvaluacionUpdateRequest>): Observable<void> {
    return this.http.put<void>(`${this.API}/Evaluacion/${id}`, cambios).pipe(
      catchError(this.handleError)
    );
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API}/Evaluacion/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // =====================================================================================
  // DETALLES DE EVALUACIÓN
  // =====================================================================================

  obtenerDetallePorId(id: number): Observable<EvaluacionDetalleResponse> {
    return this.http.get<EvaluacionDetalleResponse>(`${this.API}/EvaluacionDetalles/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  obtenerDetallesPorEvaluacion(evaluacionId: number): Observable<EvaluacionDetalleResponse[]> {
    return this.http.get<EvaluacionDetalleResponse[]>(
      `${this.API}/EvaluacionDetalles/por-evaluacion/${evaluacionId}`
    ).pipe(
      catchError(error => {
        console.error('Error al obtener detalles:', error);
        return of([]);
      })
    );
  }

  crearDetalle(detalle: EvaluacionDetalleRequest): Observable<EvaluacionDetalleResponse> {
    return this.http.post<EvaluacionDetalleResponse>(`${this.API}/EvaluacionDetalles`, detalle).pipe(
      catchError(this.handleError)
    );
  }

  actualizarDetalle(
    id: number,
    cambios: Partial<EvaluacionDetalleRequest>
  ): Observable<EvaluacionDetalleResponse> {
    return this.http.put<EvaluacionDetalleResponse>(`${this.API}/EvaluacionDetalles/${id}`, cambios).pipe(
      catchError(this.handleError)
    );
  }

  eliminarDetalle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API}/EvaluacionDetalles/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // =====================================================================================
  // FOTOS DE EVALUACIÓN
  // =====================================================================================

  obtenerTodasFotos(): Observable<FotoEvaluacionResponse[]> {
    return this.http.get<FotoEvaluacionResponse[]>(this.API_FOTOS).pipe(
      catchError(error => {
        console.error('Error al obtener fotos:', error);
        return of([]);
      })
    );
  }

  obtenerFotoPorId(id: number): Observable<FotoEvaluacionResponse> {
    return this.http.get<FotoEvaluacionResponse>(`${this.API_FOTOS}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  subirFoto(fotoData: FotoEvaluacionUploadRequest): Observable<FotoEvaluacionResponse> {
    console.log('Preparando subida de foto:', {
      detalleId: fotoData.detalleId,
      archivo: fotoData.archivo.name,
      tipo: fotoData.tipo,
      size: `${(fotoData.archivo.size / 1024).toFixed(2)} KB`
    });

    if (!this.esArchivoImagen(fotoData.archivo)) {
      const error = new Error('El archivo no es una imagen válida');
      console.error('', error.message);
      return throwError(() => error);
    }

    if (!this.validarTamañoArchivo(fotoData.archivo, 5)) {
      const error = new Error('El archivo excede el tamaño máximo de 5MB');
      console.error('', error.message);
      return throwError(() => error);
    }

    const formData = new FormData();
    formData.append('DetalleId', fotoData.detalleId.toString());
    formData.append('Archivo', fotoData.archivo, fotoData.archivo.name);

    if (fotoData.tipo) {
      formData.append('Tipo', fotoData.tipo);
    }

    if (fotoData.descripcion) {
      formData.append('Descripcion', fotoData.descripcion);
    }

    return this.http.post<FotoEvaluacionResponse>(this.API_FOTOS, formData).pipe(
      tap(response => {
        console.log(' Foto subida exitosamente:', response);
      }),
      catchError(error => {
        console.error(' Error al subir foto:', error);
        return this.handleError(error);
      })
    );
  }

  descargarFoto(id: number): Observable<Blob> {
    return this.http.get(`${this.API_FOTOS}/${id}/download`, {
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    );
  }

  eliminarFoto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_FOTOS}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  obtenerUrlFoto(id: number): string {
    return `${this.API_FOTOS}/${id}/download`;
  }

  // =====================================================================================
  // OPERACIONES COMPLEJAS
  // =====================================================================================

  async guardarEvaluacionCompleta(dto: EvaluacionCompletaDTO): Promise<EvaluacionResponse> {
    try {
      let evaluacion: EvaluacionResponse;

      if (dto.evaluacionId) {
        console.log(' Actualizando evaluación existente:', dto.evaluacionId);
        await this.actualizar(dto.evaluacionId, dto.evaluacion).toPromise();
        await this.sleep(800);
        const evaluacionActualizada = await this.obtenerPorId(dto.evaluacionId).toPromise();
        if (!evaluacionActualizada) {
          throw new Error('No se pudo obtener la evaluación actualizada');
        }
        evaluacion = evaluacionActualizada;
      } else {
        console.log(' Creando nueva evaluación');
        const nuevaEvaluacion = await this.crear(dto.evaluacion).toPromise();
        if (!nuevaEvaluacion) {
          throw new Error('No se pudo crear la evaluación');
        }
        evaluacion = nuevaEvaluacion;
      }

      console.log(' Evaluación guardada:', evaluacion.id);

      if (dto.faseAntes) {
        await this.guardarFase(evaluacion.id, 'ANTES', dto.faseAntes);
      }

      if (dto.faseDespues) {
        await this.guardarFase(evaluacion.id, 'DESPUES', dto.faseDespues);
      }

      return evaluacion;

    } catch (error) {
      console.error(' Error al guardar evaluación completa:', error);
      throw error;
    }
  }

  private async guardarFase(
    evaluacionId: number,
    fase: 'ANTES' | 'DESPUES',
    datosFase: DatosFase
  ): Promise<void> {
    try {
      const detalleRequest: EvaluacionDetalleRequest = {
        evaluacionId: evaluacionId,
        fase: fase,
        lugar: datosFase.lugar,
        descripcion: datosFase.descripcion || null,
        sugerencias: datosFase.sugerencias || null,
        scoreFase: datosFase.scoreFase || null,
        evidenciaNota: datosFase.notaGeneral || null
      };

      let detalle: EvaluacionDetalleResponse;

      if (datosFase.detalleId) {
        console.log(` Actualizando detalle ${fase}:`, datosFase.detalleId);
        detalle = await this.actualizarDetalle(datosFase.detalleId, detalleRequest).toPromise() as EvaluacionDetalleResponse;
      } else {
        console.log(` Creando detalle ${fase}`);
        detalle = await this.crearDetalle(detalleRequest).toPromise() as EvaluacionDetalleResponse;
      }

      console.log(` Detalle ${fase} guardado:`, detalle.id);
      await this.sleep(500);

      const fotosNuevas = datosFase.fotos.filter(f => f.archivo && !f.fotoIdBD);

      if (fotosNuevas.length > 0) {
        console.log(` Subiendo ${fotosNuevas.length} fotos nuevas para ${fase}`);

        for (let i = 0; i < fotosNuevas.length; i++) {
          const foto = fotosNuevas[i];

          try {
            console.log(`[${i + 1}/${fotosNuevas.length}] Subiendo: ${foto.descripcion || foto.archivo?.name}`);

            const fotoRequest: FotoEvaluacionUploadRequest = {
              detalleId: detalle.id,
              archivo: foto.archivo!,
              tipo: foto.tipo,
              descripcion: foto.descripcion
            };

            const resultado = await this.subirFoto(fotoRequest).toPromise();

            console.log(` [${i + 1}/${fotosNuevas.length}] Foto subida:`, resultado?.id);

            if (i < fotosNuevas.length - 1) {
              console.log(' Esperando 300ms antes de la siguiente foto...');
              await this.sleep(300);
            }

          } catch (error) {
            console.error(` Error al subir foto ${i + 1}:`, error);
            console.warn(' Continuando con las siguientes fotos...');
          }
        }

        console.log(` Proceso de fotos de ${fase} completado`);
      }

    } catch (error) {
      console.error(` Error al guardar fase ${fase}:`, error);
      throw error;
    }
  }

  // =====================================================================================
  // CARGAR EVALUACIÓN COMPLETA CON EJECUCIONES
  // =====================================================================================

  cargarEvaluacionCompleta(id: number): Observable<{
    evaluacion: EvaluacionResponse;
    detalleAntes?: EvaluacionDetalleResponse;
    detalleDespues?: EvaluacionDetalleResponse;
    fotosAntes: FotoEvaluacionResponse[];
    fotosDespues: FotoEvaluacionResponse[];
    ejecuciones: EjecucionSelect[];
  }> {
    return forkJoin({
      evaluacion: this.obtenerPorId(id),
      detalles: this.obtenerDetallesPorEvaluacion(id),
      todasFotos: this.obtenerTodasFotos()
    }).pipe(
      map(({ evaluacion, detalles, todasFotos }) => {
        const detalleAntes = detalles.find(d => d.fase === 'ANTES');
        const detalleDespues = detalles.find(d => d.fase === 'DESPUES');

        const fotosAntes = detalleAntes
          ? todasFotos.filter(f => f.detalleId === detalleAntes.id)
          : [];

        const fotosDespues = detalleDespues
          ? todasFotos.filter(f => f.detalleId === detalleDespues.id)
          : [];

        return {
          evaluacion,
          detalleAntes,
          detalleDespues,
          fotosAntes,
          fotosDespues
        };
      }),
      switchMap(datosBasicos => {
        console.log(' Cargando ejecuciones para orden:', datosBasicos.evaluacion.ordenId);
        
        return this.obtenerEjecuciones(datosBasicos.evaluacion.ordenId).pipe(
          map(ejecuciones => {
            console.log(' Ejecuciones cargadas:', ejecuciones.length);
            
            return {
              ...datosBasicos,
              ejecuciones: ejecuciones
            };
          }),
          catchError(error => {
            console.error(' Error al cargar ejecuciones, continuando sin ellas:', error);
            return of({
              ...datosBasicos,
              ejecuciones: []
            });
          })
        );
      }),
      catchError(error => {
        console.error(' Error al cargar evaluación completa:', error);
        throw error;
      })
    );
  }

  // =====================================================================================
  // MÉTODOS HELPER
  // =====================================================================================

  obtenerClaseScore(score: number | null | undefined): string {
    if (score === null || score === undefined) {
      return 'bg-gray-300';
    }

    if (score >= 90) {
      return 'bg-green-500';
    }

    if (score >= 80) {
      return 'bg-blue-500';
    }

    if (score >= 60) {
      return 'bg-yellow-500';
    }

    return 'bg-red-500';
  }

  obtenerTextoSeguimiento(requiereSeguimiento: boolean): string {
    return requiereSeguimiento ? 'Requiere seguimiento' : 'Completado';
  }

  obtenerClaseSeguimiento(requiereSeguimiento: boolean): string {
    return requiereSeguimiento
      ? 'bg-orange-500 text-white'
      : 'bg-green-500 text-white';
  }

  obtenerLabelFase(fase: string): string {
    const labels: Record<string, string> = {
      'ANTES': 'Antes',
      'DESPUES': 'Después',
      'antes': 'Antes',
      'despues': 'Después'
    };
    return labels[fase] || fase;
  }

  obtenerClaseFase(fase: string): string {
    const clases: Record<string, string> = {
      'ANTES': 'bg-blue-500 text-white',
      'DESPUES': 'bg-purple-500 text-white',
      'antes': 'bg-blue-500 text-white',
      'despues': 'bg-purple-500 text-white'
    };
    return clases[fase] || 'bg-gray-300 text-black';
  }

  // =====================================================================================
  // UTILIDADES
  // =====================================================================================

  formatearFecha(fechaISO: string): string {
    try {
      const fecha = new Date(fechaISO);
      if (isNaN(fecha.getTime())) {
        return 'Fecha inválida';
      }

      const dia = fecha.getDate().toString().padStart(2, '0');
      const mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
      const anio = fecha.getFullYear();
      const hora = fecha.getHours().toString().padStart(2, '0');
      const minutos = fecha.getMinutes().toString().padStart(2, '0');

      return `${dia}/${mes}/${anio} ${hora}:${minutos}`;
    } catch (error) {
      console.error('Error al formatear fecha:', error);
      return 'Fecha inválida';
    }
  }

  validarIds(ordenId?: string, evaluadorId?: string): boolean {
    if (!ordenId || !evaluadorId) return false;
    const ordenNum = parseInt(ordenId);
    const evaluadorNum = parseInt(evaluadorId);
    return !isNaN(ordenNum) && !isNaN(evaluadorNum) && ordenNum > 0 && evaluadorNum > 0;
  }

  calcularScorePromedio(detalles: EvaluacionDetalleResponse[]): number {
    const scoresValidos = detalles
      .map((d: EvaluacionDetalleResponse) => d.scoreFase)
      .filter((score): score is number => score !== null && score !== undefined);

    if (scoresValidos.length === 0) return 0;

    const suma = scoresValidos.reduce((acc, score) => acc + score, 0);
    return Math.round(suma / scoresValidos.length);
  }

  esArchivoImagen(file: File): boolean {
    const tiposPermitidos = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    return tiposPermitidos.includes(file.type);
  }

  validarTamañoArchivo(file: File, maxMB: number = 5): boolean {
    const maxBytes = maxMB * 1024 * 1024;
    return file.size <= maxBytes;
  }

  validarScore(score: number): boolean {
    return score >= 0 && score <= 100;
  }

  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Error desconocido';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      switch (error.status) {
        case 400:
          errorMessage = 'Solicitud inválida. Verifique los datos enviados.';
          break;
        case 401:
          errorMessage = 'No autorizado. Inicie sesión nuevamente.';
          break;
        case 403:
          errorMessage = 'No tiene permisos para esta operación.';
          break;
        case 404:
          errorMessage = 'Recurso no encontrado.';
          break;
        case 500:
          errorMessage = 'Error interno del servidor. Intente más tarde.';
          break;
        default:
          errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }

    console.error('Error HTTP:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }
}