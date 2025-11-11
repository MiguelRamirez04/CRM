// =====================================================================================
// DTO TAREA PENDIENTE - TareaPendienteDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el DTO para representar tareas pendientes que pueden ser tomadas por técnicos.
// Una tarea pendiente es una orden de trabajo que no tiene ejecución activa asignada.
//
// CUÁNDO USARLO:
// - Mostrar lista de tareas disponibles para técnicos SOPORTE
// - Panel de tareas pendientes en el frontend
//
// =====================================================================================

using System;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO que representa una tarea pendiente disponible para técnicos
    /// </summary>
    public class TareaPendienteDto
    {
        /// <summary>
        /// ID de la orden de trabajo
        /// </summary>
        public int OrdenId { get; set; }

        /// <summary>
        /// Nombre del cliente (nuevo o legacy)
        /// </summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>
        /// Placas del vehículo (si aplica)
        /// </summary>
        public string? VehiculoPlacas { get; set; }

        /// <summary>
        /// Descripción de la orden/tarea
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Nivel de prioridad (BAJA, MEDIA, ALTA, URGENTE)
        /// </summary>
        public string Prioridad { get; set; } = "MEDIA";

        /// <summary>
        /// Fecha de creación de la orden
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Tiempo transcurrido desde la creación (formateado)
        /// </summary>
        public string TiempoEspera { get; set; } = string.Empty;

        /// <summary>
        /// Usuario que creó/asignó la tarea
        /// </summary>
        public string? AsignadaPor { get; set; }

        /// <summary>
        /// Tipo de orden (Asesoria, Instalacion, Mantenimiento, etc.)
        /// </summary>
        public string? TipoOrden { get; set; }

        /// <summary>
        /// Modalidad (Presencial, Virtual, Remoto)
        /// </summary>
        public string? Modalidad { get; set; }

        /// <summary>
        /// Ubicación del servicio
        /// </summary>
        public string? Ubicacion { get; set; }
    }
}