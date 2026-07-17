// =====================================================================================
// ENUM ESTADO ORDEN - EstadoOrden.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los estados posibles para una orden de trabajo del módulo de Recepción.
// Garantiza consistencia en los estados y provee métodos de extensión útiles.
//
// CUÁNDO USARLO:
// - Validación de estados en órdenes de trabajo
// - Conversión entre string de BD y enum en el modelo
// - Control de flujo de trabajo de órdenes
//
// CÓMO USARLO:
// string estadoBd = EstadoOrden.CAPTURADA.ToDbValue();
// EstadoOrden estado = EstadoOrdenExtensions.FromDbValue("CAPTURADA");
//
// =====================================================================================

using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Estados posibles para una orden de trabajo
    /// </summary>
    public enum EstadoOrden
    {
        /// <summary>
        /// Orden recién creada, pendiente de asignar
        /// </summary>
        [Description("Orden capturada, pendiente de asignación")]
        CAPTURADA = 1,
        
        /// <summary>
        /// Orden asignada a un técnico o responsable
        /// </summary>
        [Description("Orden asignada a un técnico")]
        ASIGNADA = 2,
        
        /// <summary>
        /// Orden actualmente en ejecución
        /// </summary>
        [Description("Orden en curso de atención")]
        EN_CURSO = 3,
        
        /// <summary>
        /// Trabajo completado, pendiente de administrativo
        /// </summary>
        [Description("Trabajo técnico completado")]
        COMPLETADA = 4,
        
        /// <summary>
        /// Orden completada, pendiente de facturación
        /// </summary>
        [Description("Pendiente de facturación")]
        POR_FACTURAR = 5,
        
        /// <summary>
        /// Orden facturada
        /// </summary>
        [Description("Facturada")]
        FACTURADA = 6,
        
        /// <summary>
        /// Orden cerrada completamente
        /// </summary>
        [Description("Cerrada")]
        CERRADA = 7
    }
    
    /// <summary>
    /// Extensiones para el enum EstadoOrden
    /// </summary>
    public static class EstadoOrdenExtensions
    {
        /// <summary>
        /// Convierte el enum a string para almacenar en base de datos
        /// </summary>
        public static string ToDbValue(this EstadoOrden estado) => estado.ToString();
        
        /// <summary>
        /// Obtiene la descripción del estado
        /// </summary>
        public static string GetDescription(this EstadoOrden estado)
        {
            var field = estado.GetType().GetField(estado.ToString());
            
            if (field == null) return estado.ToString();
            
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            
            return attribute == null ? estado.ToString() : attribute.Description;
        }
        
        /// <summary>
        /// Convierte string de base de datos a enum
        /// </summary>
        public static EstadoOrden FromDbValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return EstadoOrden.CAPTURADA;
                
            return Enum.TryParse<EstadoOrden>(value, out var result)
                ? result
                : EstadoOrden.CAPTURADA;
        }
        
        /// <summary>
        /// Verifica si el estado permite edición de datos principales
        /// </summary>
        public static bool PermiteEdicion(this EstadoOrden estado)
        {
            return estado == EstadoOrden.CAPTURADA || 
                   estado == EstadoOrden.ASIGNADA;
        }
        
        /// <summary>
        /// Verifica si el estado es un estado final
        /// </summary>
        public static bool EsEstadoFinal(this EstadoOrden estado)
        {
            return estado == EstadoOrden.FACTURADA || 
                   estado == EstadoOrden.CERRADA;
        }
    }
}
