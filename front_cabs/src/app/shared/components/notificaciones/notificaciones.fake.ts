// notificaciones.fake.ts
import { Notificacion, TipoNotificacion, PrioridadNotificacion } from '../../../core/models/notificacion.interface';

export const NOTIFICACIONES_FAKE: Notificacion[] = [
  {
    id: 1,
    tipo: TipoNotificacion.TAREA_DELEGADA,
    titulo: 'Nueva tarea asignada',
    mensaje: 'Juan Pérez te ha delegado la tarea "Dashboard Admin"',
    leida: false,
    fechaCreacion: new Date('2024-01-15T10:30:00').toISOString(),
    prioridad: PrioridadNotificacion.ALTA,
    accion: '/tareas/123'
  },
  {
    id: 2,
    tipo: TipoNotificacion.TAREA_FINALIZADA,
    titulo: 'Tarea completada',
    mensaje: 'María Gómez ha completado la tarea "Diseñar mockups"',
    leida: false,
    fechaCreacion: new Date('2024-01-15T09:15:00').toISOString(),
    prioridad: PrioridadNotificacion.MEDIA,
    accion: '/tareas/456'
  },
  {
    id: 3,
    tipo: TipoNotificacion.RECORDATORIO_PENDIENTE,
    titulo: 'Recordatorio reunión',
    mensaje: 'Reunión de equipo en 30 minutos: Sala de conferencias 3B',
    leida: true,
    fechaCreacion: new Date('2024-01-15T08:45:00').toISOString(),
    fechaLectura: new Date('2024-01-15T09:00:00').toISOString(),
    prioridad: PrioridadNotificacion.ALTA,
    accion: '/reuniones/789'
  },
  {
    id: 4,
    tipo: TipoNotificacion.SISTEMA,
    titulo: 'Actualización del sistema',
    mensaje: 'El sistema se actualizará mañana de 2:00 AM a 4:00 AM',
    leida: false,
    fechaCreacion: new Date('2024-01-14T16:20:00').toISOString(),
    prioridad: PrioridadNotificacion.BAJA,
    accion: '/configuracion/actualizaciones'
  },
  {
    id: 5,
    tipo: TipoNotificacion.TAREA_DELEGADA,
    titulo: 'Nuevo usuario registrado',
    mensaje: 'Carlos Rodríguez se ha registrado en la plataforma',
    leida: true,
    fechaCreacion: new Date('2024-01-14T14:10:00').toISOString(),
    fechaLectura: new Date('2024-01-14T15:30:00').toISOString(),
    prioridad: PrioridadNotificacion.MEDIA,
    accion: '/usuarios/101'
  },
  {
    id: 6,
    tipo: TipoNotificacion.SISTEMA,
    titulo: 'Reporte mensual listo',
    mensaje: 'El reporte de ventas de enero está disponible para revisión',
    leida: false,
    fechaCreacion: new Date('2024-01-14T11:05:00').toISOString(),
    prioridad: PrioridadNotificacion.MEDIA,
    accion: '/reportes/enero-2024'
  },
  {
    id: 7,
    tipo: TipoNotificacion.SISTEMA,
    titulo: 'Alerta de seguridad',
    mensaje: 'Se detectó un intento de acceso sospechoso a tu cuenta',
    datos: JSON.stringify({ ip: '192.168.1.100', fechaIntento: '2024-01-13T22:25:00' }),
    leida: true,
    fechaCreacion: new Date('2024-01-13T22:30:00').toISOString(),
    fechaLectura: new Date('2024-01-13T23:15:00').toISOString(),
    prioridad: PrioridadNotificacion.ALTA,
    accion: '/seguridad/alertas'
  },
  {
    id: 8,
    tipo: TipoNotificacion.SISTEMA,
    titulo: 'Felicitaciones',
    mensaje: '¡Has alcanzado 100 días consecutivos usando la plataforma!',
    datos: JSON.stringify({ diasConsecutivos: 100, logro: 'CENTENARIO' }),
    leida: false,
    fechaCreacion: new Date('2024-01-13T18:45:00').toISOString(),
    prioridad: PrioridadNotificacion.BAJA,
    accion: '/perfil/logros'
  },
  {
    id: 9,
    tipo: TipoNotificacion.RECORDATORIO_PENDIENTE,
    titulo: 'Vencimiento próximo',
    mensaje: 'La tarea "Revisión de código" vence en 2 días',
    leida: false,
    fechaCreacion: new Date('2024-01-12T14:20:00').toISOString(),
    prioridad: PrioridadNotificacion.MEDIA,
    accion: '/tareas/vencidas'
  },
  {
    id: 10,
    tipo: TipoNotificacion.TAREA_FINALIZADA,
    titulo: 'Proyecto completado',
    mensaje: 'El proyecto "Sistema de Notificaciones" ha sido finalizado',
    leida: true,
    fechaCreacion: new Date('2024-01-11T17:40:00').toISOString(),
    fechaLectura: new Date('2024-01-12T09:15:00').toISOString(),
    prioridad: PrioridadNotificacion.ALTA,
    accion: '/proyectos/completados'
  }
];