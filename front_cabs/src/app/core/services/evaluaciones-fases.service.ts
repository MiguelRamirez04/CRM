import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// --- Interfaces (DTOs) ---
export interface DtoEvaDetallesResponse {
  id: number;
  evaluacionId: number;
  fase: string; 
  descripcion?: string;
  sugerencias?: string;
  scoreFase?: number;
  evidenciasNota?: string;
  creadoEn: Date;
  lugar: string;
}

export interface EvaluacionFotoResponseDto {
  id: number;
  detalleId: number;
  documentoId: number;
  tipo?: string;
  descripcion?: string;
  creadoEn: Date;
  nombreArchivo?: string;
  mimeType?: string;
  tamanoBytes?: number;
  urlDescarga?: string;
}

export interface DtoEvaDetallesRequest {
  evaluacionId: number;
  fase: string;
  descripcion?: string;
  sugerencias?: string;
  scoreFase?: number;
  evidenciaNota?: string;
  lugar: string;
}

export interface EvaluacionFotoRequestDto {
  detalleId: number;
  archivo: File;
  tipo?: string;
  descripcion?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EvaluacionService {
  private readonly apiUrlDetalles = `${environment.apiUrl}/api/EvaluacionDetalles`;
  private readonly apiUrlFotos = `${environment.apiUrl}/api/FotosEvaluacion`;

  constructor(private http: HttpClient) {}

  // ========== DETALLES DE EVALUACIÓN ==========

  
//Obtiene un detalle de evaluación por su ID
   
  getDetalleById(id: number): Observable<DtoEvaDetallesResponse> {
    return this.http.get<DtoEvaDetallesResponse>(`${this.apiUrlDetalles}/${id}`);
  }

  
//Obtiene todos los detalles de una evaluación
   
  getDetallesByEvaluacionId(evaluacionId: number): Observable<DtoEvaDetallesResponse[]> {
    return this.http.get<DtoEvaDetallesResponse[]>(`${this.apiUrlDetalles}/por-evaluacion/${evaluacionId}`);
  }

  
//Crea un nuevo detalle de evaluación
   
  createDetalle(detalle: DtoEvaDetallesRequest): Observable<DtoEvaDetallesResponse> {
    return this.http.post<DtoEvaDetallesResponse>(this.apiUrlDetalles, detalle);
  }

  
//Actualiza un detalle existente
   
  updateDetalle(id: number, detalle: DtoEvaDetallesRequest): Observable<DtoEvaDetallesResponse> {
    return this.http.put<DtoEvaDetallesResponse>(`${this.apiUrlDetalles}/${id}`, detalle);
  }

  
//Elimina un detalle
   
  deleteDetalle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrlDetalles}/${id}`);
  }

  // ========== FOTOS DE EVALUACIÓN ==========

  
//Obtiene todas las fotos
   
  getAllFotos(): Observable<EvaluacionFotoResponseDto[]> {
    return this.http.get<EvaluacionFotoResponseDto[]>(this.apiUrlFotos);
  }

  
//Obtiene una foto por su ID
   
  getFotoById(id: number): Observable<EvaluacionFotoResponseDto> {
    return this.http.get<EvaluacionFotoResponseDto>(`${this.apiUrlFotos}/${id}`);
  }

  
//Sube una nueva foto
   
  uploadFoto(requestDto: EvaluacionFotoRequestDto): Observable<EvaluacionFotoResponseDto> {
    const formData = new FormData();
    formData.append('DetalleId', requestDto.detalleId.toString());
    formData.append('Archivo', requestDto.archivo);
    
    if (requestDto.tipo) {
      formData.append('Tipo', requestDto.tipo);
    }
    if (requestDto.descripcion) {
      formData.append('Descripcion', requestDto.descripcion);
    }

    return this.http.post<EvaluacionFotoResponseDto>(this.apiUrlFotos, formData);
  }

  
//Actualiza metadatos de una foto
   
  updateFotoMetadata(id: number, tipo?: string, descripcion?: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrlFotos}/${id}`, { tipo, descripcion });
  }

  
//Elimina una foto
   
  deleteFoto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrlFotos}/${id}`);
  }

  
//Obtiene la URL de descarga de una foto
   
  getFotoDownloadUrl(id: number): string {
    return `${this.apiUrlFotos}/${id}/download`;
  }

  
//Descarga una foto como Blob
   
  downloadFoto(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrlFotos}/${id}/download`, {
      responseType: 'blob'
    });
  }
}