import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  EvaluacionRequestDto, 
  EvaluacionResponseDto,
  EvaluacionDetalleRequestDto,
  EvaluacionDetalleResponseDto,
  EvaluacionFotoRequestDto,
  EvaluacionFotoResponseDto,
  EvaluacionCompletaDto
} from '../models/evaluacion.dto';
import { environment } from '../../../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EvaluacionService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // ==================== EVALUACIONES ====================
  

//Obtiene todas las evaluaciones
   
  getAllEvaluaciones(): Observable<EvaluacionResponseDto[]> {
    return this.http.get<EvaluacionResponseDto[]>(`${this.apiUrl}/Evaluacion`);
  }


//Obtiene una evaluación por ID
   
  getEvaluacionById(id: number): Observable<EvaluacionResponseDto> {
    return this.http.get<EvaluacionResponseDto>(`${this.apiUrl}/Evaluacion/${id}`);
  }


//Crea una nueva evaluación (solo info general)
   
  createEvaluacion(evaluacion: EvaluacionRequestDto): Observable<EvaluacionResponseDto> {
    return this.http.post<EvaluacionResponseDto>(`${this.apiUrl}/Evaluacion`, evaluacion);
  }


//Actualiza una evaluación existente
   
  updateEvaluacion(id: number, evaluacion: EvaluacionRequestDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Evaluacion/${id}`, evaluacion);
  }


//Elimina una evaluación
   
  deleteEvaluacion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Evaluacion/${id}`);
  }

  // ==================== DETALLES ====================


//Obtiene los detalles de una evaluación
   
  getDetallesByEvaluacionId(evaluacionId: number): Observable<EvaluacionDetalleResponseDto[]> {
    return this.http.get<EvaluacionDetalleResponseDto[]>(
      `${this.apiUrl}/EvaluacionDetalles/por-evaluacion/${evaluacionId}`
    );
  }


//Obtiene un detalle específico por ID
   
  getDetalleById(id: number): Observable<EvaluacionDetalleResponseDto> {
    return this.http.get<EvaluacionDetalleResponseDto>(`${this.apiUrl}/EvaluacionDetalles/${id}`);
  }


//Crea un nuevo detalle de evaluación (fase)
   
  createDetalle(detalle: EvaluacionDetalleRequestDto): Observable<EvaluacionDetalleResponseDto> {
    return this.http.post<EvaluacionDetalleResponseDto>(`${this.apiUrl}/EvaluacionDetalles`, detalle);
  }


//Actualiza un detalle existente
   
  updateDetalle(id: number, detalle: EvaluacionDetalleRequestDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/EvaluacionDetalles/${id}`, detalle);
  }


//Elimina un detalle
   
  deleteDetalle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/EvaluacionDetalles/${id}`);
  }

  // ==================== FOTOS ====================


//Obtiene todas las fotos
   
  getAllFotos(): Observable<EvaluacionFotoResponseDto[]> {
    return this.http.get<EvaluacionFotoResponseDto[]>(`${this.apiUrl}/FotosEvaluacion`);
  }


//Obtiene una foto por ID
   
  getFotoById(id: number): Observable<EvaluacionFotoResponseDto> {
    return this.http.get<EvaluacionFotoResponseDto>(`${this.apiUrl}/FotosEvaluacion/${id}`);
  }


//Sube una nueva foto de evaluación
   
  uploadFoto(foto: EvaluacionFotoRequestDto): Observable<EvaluacionFotoResponseDto> {
    const formData = new FormData();
    formData.append('DetalleId', foto.detalleId.toString());
    formData.append('Archivo', foto.archivo);
    
    if (foto.tipo) {
      formData.append('Tipo', foto.tipo);
    }
    
    if (foto.descripcion) {
      formData.append('Descripcion', foto.descripcion);
    }

    return this.http.post<EvaluacionFotoResponseDto>(`${this.apiUrl}/FotosEvaluacion`, formData);
  }


//Actualiza metadatos de una foto
   
  updateFotoMetadata(id: number, tipo?: string, descripcion?: string): Observable<void> {
    const body = {
      tipo: tipo,
      descripcion: descripcion
    };
    return this.http.put<void>(`${this.apiUrl}/FotosEvaluacion/${id}`, body);
  }


//Elimina una foto
   
  deleteFoto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/FotosEvaluacion/${id}`);
  }


//Obtiene la URL para descargar una foto
   
  getFotoDownloadUrl(fotoId: number): string {
    return `${this.apiUrl}/FotosEvaluacion/${fotoId}/download`;
  }

  // ==================== OPERACIONES COMPLEJAS ====================


//Guarda una evaluación completa con sus detalles y fotos
   //Este método maneja la transacción completa:
   //1. Crea/actualiza la evaluación principal
   //2. Crea/actualiza los detalles de cada fase
   //3. Sube las fotos asociadas

  async guardarEvaluacionCompleta(evaluacionCompleta: EvaluacionCompletaDto): Promise<EvaluacionResponseDto> {
    try {
      // 1. Guardar evaluación principal
      let evaluacionResponse: EvaluacionResponseDto;
      
      if (evaluacionCompleta.evaluacionId) {
        // Actualizar evaluación existente
        await this.updateEvaluacion(evaluacionCompleta.evaluacionId, evaluacionCompleta.evaluacion).toPromise();
        evaluacionResponse = await this.getEvaluacionById(evaluacionCompleta.evaluacionId).toPromise() as EvaluacionResponseDto;
      } else {
        // Crear nueva evaluación
        evaluacionResponse = await this.createEvaluacion(evaluacionCompleta.evaluacion).toPromise() as EvaluacionResponseDto;
      }

      // 2. Guardar fase ANTES (si existe)
      if (evaluacionCompleta.faseAntes) {
        const detalleAntes = await this.guardarDetalleFase(
          evaluacionResponse.id,
          'ANTES',
          evaluacionCompleta.faseAntes
        );

        // Subir fotos de fase ANTES
        if (evaluacionCompleta.faseAntes.fotos && evaluacionCompleta.faseAntes.fotos.length > 0) {
          await this.subirFotosDeFase(detalleAntes.id, evaluacionCompleta.faseAntes.fotos);
        }
      }

      // 3. Guardar fase DESPUÉS (si existe)
      if (evaluacionCompleta.faseDespues) {
        const detalleDespues = await this.guardarDetalleFase(
          evaluacionResponse.id,
          'DESPUES',
          evaluacionCompleta.faseDespues
        );

        // Subir fotos de fase DESPUÉS
        if (evaluacionCompleta.faseDespues.fotos && evaluacionCompleta.faseDespues.fotos.length > 0) {
          await this.subirFotosDeFase(detalleDespues.id, evaluacionCompleta.faseDespues.fotos);
        }
      }

      return evaluacionResponse;
    } catch (error) {
      console.error('Error al guardar evaluación completa:', error);
      throw error;
    }
  }


//Guarda o actualiza un detalle de fase
   
  private async guardarDetalleFase(
    evaluacionId: number,
    fase: 'ANTES' | 'DESPUES',
    datosFase: any
  ): Promise<EvaluacionDetalleResponseDto> {
    const detalleDto: EvaluacionDetalleRequestDto = {
      evaluacionId: evaluacionId,
      fase: fase,
      lugar: datosFase.lugar || '',
      descripcion: datosFase.descripcion || '',
      sugerencias: datosFase.sugerencias || '',
      scoreFase: datosFase.scoreFase || 0,
      evidenciaNota: datosFase.notaGeneral || ''
    };

    if (datosFase.detalleId) {
      // Actualizar detalle existente
      await this.updateDetalle(datosFase.detalleId, detalleDto).toPromise();
      return await this.getDetalleById(datosFase.detalleId).toPromise() as EvaluacionDetalleResponseDto;
    } else {
      // Crear nuevo detalle
      return await this.createDetalle(detalleDto).toPromise() as EvaluacionDetalleResponseDto;
    }
  }


//Sube múltiples fotos para una fase
   
  private async subirFotosDeFase(detalleId: number, fotos: any[]): Promise<void> {
    const uploadPromises = fotos
      .filter(foto => foto.archivo && !foto.id) // Solo fotos nuevas que tienen archivo
      .map(foto => {
        const fotoDto: EvaluacionFotoRequestDto = {
          detalleId: detalleId,
          archivo: foto.archivo,
          tipo: foto.tipo || 'General',
          descripcion: foto.descripcion || ''
        };
        return this.uploadFoto(fotoDto).toPromise();
      });

    await Promise.all(uploadPromises);
  }
}