// =====================================================================================
// ENUM EvalUACION - Evaluacion_enum.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los estados posibles para una orden de trabajo del módulo del trabajo.
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