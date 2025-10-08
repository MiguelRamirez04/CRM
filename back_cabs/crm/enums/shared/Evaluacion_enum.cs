// =====================================================================================
// ENUM EvalUACION - Evaluacion_enum.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los estados posibles para una orden de trabajo del módulo del trabajo.
// Garantiza consistencia en los estados y provee métodos de extensión útiles.
//
// CUÁNDO USARLO:
// - Validación de estados en lo que son las evaluaciones del trabajo realizado
// - Conversión entre string de BD y enum en el modelo
// - Control de flujo del trabajo
//
// =====================================================================================
///Creacion de los sig: 
/// enum ejecucion orden

using System.ComponentModel;

namespace back_cabs.CRM.enums
{

    ///<summary>
    /// Prioridad de las evaluaciones 
    ///<summary>

    public enum Evaluacion_enum
    {
        [Description("Antes")]
        ANTES = 1,
        [Description("Despues")]
        DESPUES = 2,
        
    }
    
}