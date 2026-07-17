
// ==================== EVALUACIÓN PRINCIPAL ====================

/**
 * Respuesta de evaluación desde el backend
 * EXACTAMENTE como viene del API (con typos incluidos)
 * 
 * Endpoint: GET /api/Evaluacion/{id}
 */
export interface EvaluacionResponse {
  id: number;
  ordenId: number;
  ejecucionId?: number | null;          //  Corregido (sin typo)
  clienteId?: number | null;            //  Corregido (sin typo)
  evaluadorId: number;                  //  Corregido (sin typo)
  objetivo?: string | null;
  comentariosGenerales?: string | null;
  scoreCalidadTotal?: number | null;
  requiereSeguimiento: boolean;
  seguimientoNotas?: string | null;
  creadoEn: string;
}

// Interfaz extendida para el listado (incluye el nombre del evaluador)
export interface EvaluacionResponseExtendida extends EvaluacionResponse {
  evaluadorNombre?: string;
}

/**
 * Request para crear evaluación (sin typos, campos normalizados)
 * 
 * Endpoint: POST /api/Evaluacion
 * 
 * IMPORTANTE: Los nombres de campos en el request NO tienen typos
 */
export interface EvaluacionCreateRequest {
  ordenId: number;
  ejecucionId?: number | null;          //  Sin typo en request
  cLienteId?: number | null;            // Mantiene mayúscula L
  evaluadorId: number;                  //  Sin typo en request
  objetivo?: string | null;
  comentariosGenerales?: string | null;
  scoreCalidadTotal?: number | null;
  requiereSeguimiento?: boolean;
  seguimientoNotas?: string | null;
}

/**
 * Request para actualizar evaluación (todos los campos opcionales)
 * 
 * Endpoint: PUT /api/Evaluacion/{id}
 * Response: 200 OK (sin body)
 * 
 * IMPORTANTE: PUT no devuelve el objeto actualizado, solo 200 OK
 */
export interface EvaluacionUpdateRequest {
  ordenId?: number;
  ejecucionId?: number | null;
  cLienteId?: number | null;
  evaluadorId?: number;
  objetivo?: string | null;
  comentariosGenerales?: string | null;
  scoreCalidadTotal?: number | null;
  requiereSeguimiento?: boolean;
  seguimientoNotas?: string | null;
}

// ==================== DETALLES (FASES) ====================

/**
 * Respuesta de detalle de evaluación (fase)
 * 
 * Endpoint: GET /api/EvaluacionDetalles/{id}
 */
export interface EvaluacionDetalleResponse {
  id: number;
  evaluacionId: number;
  fase: string;                         // "ANTES" o "DESPUES"
  descripcion?: string | null;
  sugerencias?: string | null;
  scoreFase?: number | null;            // 0-100
  evidenciasNota?: string | null;       // Con "s" en response
  creadoEn: string;                     // ISO 8601
  lugar: string;                        // Campo agregado en API
}

//  Agregar con export
export interface UsuarioResponseDto {
  id: number;
  nombre: string;
  apellido: string;
  nombreCompleto?: string;
  email: string;
  rol: string;
  activo: boolean;
  creadoEn: string;
}
/**
 * Request para crear/actualizar detalle
 * 
 * Endpoints:
 * - POST /api/EvaluacionDetalles
 * - PUT /api/EvaluacionDetalles/{id}
 * 
 * IMPORTANTE: En request es "evidenciaNota" (sin "s")
 */
export interface EvaluacionDetalleRequest {
  evaluacionId: number;
  fase: string;                         // "ANTES" o "DESPUES"
  descripcion?: string | null;
  sugerencias?: string | null;
  scoreFase?: number | null;
  evidenciaNota?: string | null;        // Sin "s" en request
  lugar: string;
}

// ==================== FOTOS ====================

/**
 * Respuesta de foto de evaluación
 * 
 * Endpoint: GET /api/FotosEvaluacion/{id}
 */
export interface FotoEvaluacionResponse {
  id: number;
  detalleId: number;
  documentoId: number;
  tipo?: string | null;
  descripcion?: string | null;
  creadoEn: string;                     // ISO 8601
  nombreArchivo?: string;
  mimeType?: string;
  tamanoBytes?: number;
  urlDescarga?: string;                 // URL completa para descargar
}

/**
 * Request para subir foto (usar con FormData)
 * 
 * Endpoint: POST /api/FotosEvaluacion
 * Content-Type: multipart/form-data
 * 
 * Ejemplo de uso:
 * ```typescript
 * const formData = new FormData();
 * formData.append('DetalleId', detalleId.toString());
 * formData.append('Archivo', file);
 * formData.append('Tipo', 'ANTES');
 * formData.append('Descripcion', 'Foto descriptiva');
 * ```
 */
export interface FotoEvaluacionUploadRequest {
  detalleId: number;
  archivo: File;
  tipo?: string;
  descripcion?: string;
}

// ==================== INTERFACES PARA EL FRONTEND ====================

/**
 * Datos del formulario de información general
 * (Usado en el componente de info general)
 */
export interface FormularioInfoGeneral {
  ordenTrabajoId: string;               // String para select
  ejecucionId: string;                  // String para select (opcional)
  clienteId: string;                    // String para select (opcional)
  evaluadorId: string;                  // String para select
  objetivo: string;
  comentariosGenerales: string;
  scoreCalidad: number;                 // 0-100
  requiereSeguimiento: boolean;
  notasSeguimiento: string;
}

/**
 * Datos de una fase (ANTES o DESPUÉS) en el componente
 */
export interface DatosFase {
  detalleId?: number;                   // Si existe, es UPDATE
  lugar: string;                        // "Oficina", "Casa", etc.
  fechaCreacion: string;                // Formato: "YYYY-MM-DD"
  scoreFase: number;                    // 0-100
  descripcion: string;
  sugerencias: string;
  notaGeneral: string;
  fotos: FotoLocal[];
}

/**
 * Foto manejada en el componente (antes de subir)
 */
export interface FotoLocal {
  id?: string;                          // ID temporal local (UUID)
  fotoIdBD?: number;                    // ID real en BD (si ya está guardada)
  tipo: string;                         // "General", "Detalle", etc.
  fecha: string;                        // Formato: "YYYY-MM-DD"
  descripcion: string;
  archivo?: File;                       // Solo para fotos nuevas (no subidas)
  preview?: string;                     // Data URL para preview (createObjectURL)
}

// ==================== DTO COMPLETO PARA GUARDAR ====================

/**
 * DTO que agrupa toda la evaluación para guardar
 * (Usado en el servicio para guardar completo)
 */
export interface EvaluacionCompletaDTO {
  evaluacionId?: number;                // Si existe, es UPDATE
  evaluacion: EvaluacionCreateRequest;  // Datos de info general
  faseAntes?: DatosFase;                // Datos de fase ANTES (opcional)
  faseDespues?: DatosFase;              // Datos de fase DESPUÉS (opcional)
}

// ==================== MAPPERS ====================

/**
 * Convierte FormularioInfoGeneral (UI) a EvaluacionCreateRequest (API)
 * 
 * Uso:
 * ```typescript
 * const formData: FormularioInfoGeneral = { ... };
 * const apiRequest = mapFormularioToRequest(formData);
 * // Enviar apiRequest al backend
 * ```
 */
export function mapFormularioToRequest(form: FormularioInfoGeneral): EvaluacionCreateRequest {
  return {
    ordenId: parseInt(form.ordenTrabajoId),
    ejecucionId: form.ejecucionId ? parseInt(form.ejecucionId) : null,
    cLienteId: form.clienteId ? parseInt(form.clienteId) : null,
    evaluadorId: parseInt(form.evaluadorId),
    objetivo: form.objetivo || null,
    comentariosGenerales: form.comentariosGenerales || null,
    scoreCalidadTotal: form.scoreCalidad || null,
    requiereSeguimiento: form.requiereSeguimiento,
    seguimientoNotas: form.notasSeguimiento || null
  };
}

/**
 * Convierte EvaluacionResponse (API) a FormularioInfoGeneral (UI)
 * 
 * Uso:
 * ```typescript
 * const apiResponse: EvaluacionResponse = await api.obtenerPorId(id);
 * const formData = mapResponseToFormulario(apiResponse);
 * // Cargar formData en el formulario
 * ```
 */
export function mapResponseToFormulario(response: EvaluacionResponse): FormularioInfoGeneral {
  return {
    ordenTrabajoId: response.ordenId.toString(),
    ejecucionId: response.ejecucionId?.toString() || '',      
    clienteId: response.clienteId?.toString() || '',          
    evaluadorId: response.evaluadorId.toString(),             
    objetivo: response.objetivo || '',
    comentariosGenerales: response.comentariosGenerales || '',
    scoreCalidad: response.scoreCalidadTotal || 0,
    requiereSeguimiento: response.requiereSeguimiento,
    notasSeguimiento: response.seguimientoNotas || ''
  };
}

/**
 * Convierte DatosFase (UI) a EvaluacionDetalleRequest (API)
 * 
 * Uso:
 * ```typescript
 * const datosFase: DatosFase = { ... };
 * const apiRequest = mapDatosFaseToRequest(datosFase, evaluacionId);
 * // Enviar apiRequest al backend
 * ```
 */
export function mapDatosFaseToRequest(
  datosFase: DatosFase,
  evaluacionId: number
): EvaluacionDetalleRequest {
  return {
    evaluacionId: evaluacionId,
    fase: datosFase.lugar.includes('ANTES') ? 'ANTES' : 'DESPUES',
    lugar: datosFase.lugar,
    descripcion: datosFase.descripcion || null,
    sugerencias: datosFase.sugerencias || null,
    scoreFase: datosFase.scoreFase || null,
    // Mapear notaGeneral (UI) a evidenciaNota (API)
    evidenciaNota: datosFase.notaGeneral || null
  };
}

/**
 * Convierte EvaluacionDetalleResponse (API) a DatosFase (UI)
 * 
 * Uso:
 * ```typescript
 * const apiResponse: EvaluacionDetalleResponse = await api.obtenerDetalle(id);
 * const fotos: FotoEvaluacionResponse[] = await api.obtenerFotos(id);
 * const datosFase = mapDetalleResponseToDatos(apiResponse, fotos);
 * // Cargar datosFase en el formulario
 * ```
 */
export function mapDetalleResponseToDatos(
  response: EvaluacionDetalleResponse,
  fotos: FotoEvaluacionResponse[]
): DatosFase {
  return {
    detalleId: response.id,
    lugar: response.lugar,
    fechaCreacion: response.creadoEn.split('T')[0], // Extraer solo fecha
    scoreFase: response.scoreFase || 0,
    descripcion: response.descripcion || '',
    sugerencias: response.sugerencias || '',
    // Mapear evidenciasNota (API con "s") a notaGeneral (UI)
    notaGeneral: response.evidenciasNota || '',
    fotos: fotos.map((f: FotoEvaluacionResponse) => ({
      id: f.id.toString(),
      fotoIdBD: f.id,
      tipo: f.tipo || 'General',
      fecha: f.creadoEn.split('T')[0],
      descripcion: f.descripcion || '',
      preview: f.urlDescarga
    }))
  };
}

/**
 * Convierte FotoLocal (UI) a FormData para subir
 * 
 * Uso:
 * ```typescript
 * const fotoLocal: FotoLocal = { ... };
 * const formData = mapFotoLocalToFormData(fotoLocal, detalleId);
 * // Enviar formData al backend
 * ```
 */
export function mapFotoLocalToFormData(
  fotoLocal: FotoLocal,
  detalleId: number
): FormData {
  const formData = new FormData();
  formData.append('DetalleId', detalleId.toString());
  
  if (fotoLocal.archivo) {
    formData.append('Archivo', fotoLocal.archivo);
  }
  
  if (fotoLocal.tipo) {
    formData.append('Tipo', fotoLocal.tipo);
  }
  
  if (fotoLocal.descripcion) {
    formData.append('Descripcion', fotoLocal.descripcion);
  }
  
  return formData;
}

// ==================== VALIDADORES ====================

/**
 * Validar que un score esté en el rango correcto
 */
export function validarScore(score: number): boolean {
  return score >= 0 && score <= 100;
}

/**
 * Validar que una fase tenga los datos mínimos requeridos
 */
export function validarDatosFase(datosFase: DatosFase): {
  valido: boolean;
  errores: string[];
} {
  const errores: string[] = [];
  
  if (!datosFase.lugar || datosFase.lugar.trim() === '') {
    errores.push('El lugar es requerido');
  }
  
  if (datosFase.scoreFase !== undefined && !validarScore(datosFase.scoreFase)) {
    errores.push('El score debe estar entre 0 y 100');
  }
  
  return {
    valido: errores.length === 0,
    errores
  };
}

/**
 * Validar que una foto sea válida para subir
 */
export function validarFotoLocal(foto: FotoLocal): {
  valido: boolean;
  errores: string[];
} {
  const errores: string[] = [];
  
  if (!foto.archivo && !foto.fotoIdBD) {
    errores.push('La foto debe tener un archivo o un ID de BD');
  }
  
  if (foto.archivo) {
    const tiposPermitidos = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!tiposPermitidos.includes(foto.archivo.type)) {
      errores.push('Tipo de archivo no permitido. Use JPEG, PNG o WebP');
    }
    
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (foto.archivo.size > maxSize) {
      errores.push('El archivo excede el tamaño máximo de 5MB');
    }
  }
  
  return {
    valido: errores.length === 0,
    errores
  };
}

