// =====================================================================================
// DTO ESTADÍSTICAS RECEPCIÓN - EstadisticasRecepcionResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la estructura de respuesta para las estadísticas del dashboard de recepción.
// Incluye totales generales y desglose detallado por estados de órdenes.
//
// CUÁNDO USARLO:
// - Endpoint GET /api/Recepcion/estadisticas
// - Dashboard de recepción para mostrar métricas
// - Reportes y análisis de órdenes de trabajo
//
// CÓMO USARLO:
// var estadisticas = new EstadisticasRecepcionResponseDto { ... };
// return Ok(estadisticas);
//
// =====================================================================================

using back_cabs.CRM.enums;

namespace back_cabs.CRM.DTOs.Response
{
    /// <summary>
    /// DTO para la respuesta de estadísticas del dashboard de recepción
    /// </summary>
    public class EstadisticasRecepcionResponseDto
    {
        /// <summary>
        /// Total de órdenes en el sistema
        /// </summary>
        public int TotalOrdenes { get; set; }
        
        /// <summary>
        /// Órdenes que no están cerradas (estados activos)
        /// </summary>
        public int OrdenesActivas { get; set; }
        
        /// <summary>
        /// Órdenes completamente cerradas
        /// </summary>
        public int OrdenesCerradas { get; set; }
        
        /// <summary>
        /// Desglose detallado por cada estado
        /// </summary>
        public EstadisticasPorEstadoDto PorEstado { get; set; } = new();
        
        /// <summary>
        /// Estadísticas adicionales de flujo de trabajo
        /// </summary>
        public EstadisticasFlujoDto Flujo { get; set; } = new();
        
        /// <summary>
        /// Fecha y hora de generación de las estadísticas
        /// </summary>
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Desglose de órdenes por cada estado del enum
    /// </summary>
    public class EstadisticasPorEstadoDto
    {
        /// <summary>
        /// Órdenes recién creadas, pendientes de asignar
        /// </summary>
        public int Capturadas { get; set; }
        
        /// <summary>
        /// Órdenes asignadas a un técnico
        /// </summary>
        public int Asignadas { get; set; }
        
        /// <summary>
        /// Órdenes en curso de atención
        /// </summary>
        public int EnCurso { get; set; }
        
        /// <summary>
        /// Trabajo técnico completado
        /// </summary>
        public int Completadas { get; set; }
        
        /// <summary>
        /// Pendientes de facturación
        /// </summary>
        public int PorFacturar { get; set; }
        
        /// <summary>
        /// Facturadas
        /// </summary>
        public int Facturadas { get; set; }
        
        /// <summary>
        /// Cerradas completamente
        /// </summary>
        public int Cerradas { get; set; }
    }
    
    /// <summary>
    /// Estadísticas de flujo de trabajo y productividad
    /// </summary>
    public class EstadisticasFlujoDto
    {
        /// <summary>
        /// Órdenes pendientes (capturadas + asignadas)
        /// </summary>
        public int OrdenesPendientes { get; set; }
        
        /// <summary>
        /// Órdenes en proceso (en curso + completadas)
        /// </summary>
        public int OrdenesEnProceso { get; set; }
        
        /// <summary>
        /// Órdenes finalizadas (por facturar + facturadas + cerradas)
        /// </summary>
        public int OrdenesFinalizadas { get; set; }
        
        /// <summary>
        /// Porcentaje de órdenes completadas vs total activas
        /// </summary>
        public decimal PorcentajeCompletadas { get; set; }
        
        /// <summary>
        /// Porcentaje de órdenes facturadas vs total
        /// </summary>
        public decimal PorcentajeFacturadas { get; set; }
    }
}