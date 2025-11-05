// ==================== DTOs para EVALUACIÓN ====================

//DTO para crear/actualizar una evaluación
 
export interface EvaluacionRequestDto {
  ordenId: number;
  ejecucionId?: number | null;
  cLienteId?: number | null;  // Nota: el backend usa "CLienteId" (con L mayúscula)
  evaluadorId: number;
  objetivo?: string;
  comentariosGenerales?: string;
  scoreCalidadTotal?: number;
  requiereSeguimiento: boolean;
  seguimientoNotas?: string;
}

//DTO de respuesta de evaluación

export interface EvaluacionResponseDto {
  id: number;
  ordenId: number;
  ejecucionId?: number;
  clienteId?: number;
  evaluadorId: number;
  objetivo?: string;
  comentariosGenerales?: string;
  scoreCalidadTotal?: number;
  requiereSeguimiento: boolean;
  seguimientoNotas?: string;
  creadoEn: Date;
}

// ==================== DTOs para DETALLES DE EVALUACIÓN ====================

//DTO para crear/actualizar un detalle de evaluación (fase)

export interface EvaluacionDetalleRequestDto {
  evaluacionId: number;
  fase: string;  // "ANTES" o "DESPUES"
  lugar: string;
  descripcion?: string;
  sugerencias?: string;
  scoreFase?: number;
  evidenciaNota?: string;
}

// DTO de respuesta de detalle de evaluación
 
export interface EvaluacionDetalleResponseDto {
  id: number;
  evaluacionId: number;
  fase: string;
  lugar: string;
  descripcion?: string;
  sugerencias?: string;
  scoreFase?: number;
  evidenciasNota?: string;  
  creadoEn: Date;
}

// ==================== DTOs para FOTOS DE EVALUACIÓN ====================


//DTO para subir una foto de evaluación

export interface EvaluacionFotoRequestDto {
  detalleId: number;
  archivo: File;
  tipo?: string;
  descripcion?: string;
}


//DTO de respuesta de foto de evaluación

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

// ==================== DTOs COMPUESTOS ====================


//DTO que representa los datos de una fase (ANTES o DESPUÉS)
 
export interface FaseEvaluacionDto {
  detalleId?: number;  // Si existe, se actualiza; si no, se crea
  lugar: string;
  fechaCreacion: string;
  scoreFase: number;
  descripcion: string;
  sugerencias: string;
  notaGeneral: string;
  fotos: FotoFaseDto[];
}


//DTO para una foto dentro de una fase

export interface FotoFaseDto {
  id?: number;  // Si existe, es una foto ya guardada
  tipo: string;
  fecha: string;
  descripcion: string;
  archivo?: File;  // Solo se envía para fotos nuevas
  preview?: string;  // Preview local para mostrar en UI
}


//DTO completo que agrupa toda la evaluación
 
export interface EvaluacionCompletaDto {
  evaluacionId?: number;  // Si existe, se actualiza; si no, se crea
  evaluacion: EvaluacionRequestDto;
  faseAntes?: FaseEvaluacionDto;
  faseDespues?: FaseEvaluacionDto;
}

// ==================== INTERFACES DEL FRONTEND ====================


//Interface para el formulario de información general

export interface FormularioInfoGeneral {
  ordenTrabajoId: string;
  ejecucionId: string;
  clienteId: string;
  evaluadorId: string;
  objetivo: string;
  comentariosGenerales: string;
  scoreCalidad: number;
  requiereSeguimiento: boolean;
  notasSeguimiento: string;
}


//Interface para foto en el componente

export interface Foto {
  id?: string;  // ID temporal local o ID de BD
  tipo: string;
  fecha: string;
  descripcion: string;
  archivo?: File;
  preview?: string;
  fotoIdBD?: number;  // ID real en la base de datos
}

// ==================== MAPPERS ====================


//Convierte el formulario de InfoGeneral a EvaluacionRequestDto
 
export function mapFormularioToEvaluacionDto(formulario: FormularioInfoGeneral): EvaluacionRequestDto {
  return {
    ordenId: parseInt(formulario.ordenTrabajoId),
    ejecucionId: formulario.ejecucionId ? parseInt(formulario.ejecucionId) : null,
    cLienteId: formulario.clienteId ? parseInt(formulario.clienteId) : null,
    evaluadorId: parseInt(formulario.evaluadorId),
    objetivo: formulario.objetivo || '',
    comentariosGenerales: formulario.comentariosGenerales || '',
    scoreCalidadTotal: formulario.scoreCalidad || 0,
    requiereSeguimiento: formulario.requiereSeguimiento,
    seguimientoNotas: formulario.notasSeguimiento || ''
  };
}


//Convierte los datos de una fase del componente a FaseEvaluacionDto

export function mapComponenteFaseToDto(
  lugar: string,
  fechaCreacion: string,
  scoreFase: number,
  descripcion: string,
  sugerencias: string,
  notaGeneral: string,
  fotos: Foto[],
  detalleId?: number
): FaseEvaluacionDto {
  return {
    detalleId: detalleId,
    lugar: lugar,
    fechaCreacion: fechaCreacion,
    scoreFase: scoreFase,
    descripcion: descripcion,
    sugerencias: sugerencias,
    notaGeneral: notaGeneral,
    fotos: fotos.map(foto => ({
      id: foto.fotoIdBD,
      tipo: foto.tipo,
      fecha: foto.fecha,
      descripcion: foto.descripcion,
      archivo: foto.archivo,
      preview: foto.preview
    }))
  };
}