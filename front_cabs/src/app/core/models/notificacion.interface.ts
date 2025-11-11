// =====================================================================================
// INTERFAZ NOTIFICACIÓN - notificacion.interface.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la estructura de datos para las notificaciones del sistema.
// Se utiliza tanto para comunicación con la API como para SignalR.
//
// PROPIEDADES:
// - id: Identificador único
// - tipo: Categoría de notificación
// - titulo: Título descriptivo
// - mensaje: Contenido detallado
// - leida: Estado de lectura
// - fechaCreacion: Momento de creación
// - prioridad: Nivel de importancia
// - accion: URL o acción sugerida
//
// =====================================================================================

export interface Notificacion {
  id: number;
  tipo: string;
  titulo: string;
  mensaje: string;
  datos?: string;
  leida: boolean;
  fechaCreacion: string;
  fechaLectura?: string;
  prioridad: string;
  accion?: string;
}

export interface NotificacionResponseDto {
  id: number;
  tipo: string;
  titulo: string;
  mensaje: string;
  datos?: string;
  leida: boolean;
  fechaCreacion: string;
  fechaLectura?: string;
  prioridad: string;
  accion?: string;
}

// Tipos de notificación disponibles
export enum TipoNotificacion {
  TAREA_DELEGADA = 'TAREA_DELEGADA',
  TAREA_FINALIZADA = 'TAREA_FINALIZADA',
  RECORDATORIO_PENDIENTE = 'RECORDATORIO_PENDIENTE',
  SISTEMA = 'SISTEMA'
}

// Niveles de prioridad
export enum PrioridadNotificacion {
  BAJA = 'BAJA',
  MEDIA = 'MEDIA',
  ALTA = 'ALTA'
}