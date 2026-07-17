// =====================================================================================
// ENUM EVALUACION - evaluacion.enum.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define las fases posibles de una evaluación (ANTES/DESPUES).
// Corresponde al enum Evaluacion_enum del backend.
//
// CUÁNDO USARLO:
// - Validación de fases en formularios de evaluación
// - Filtros de búsqueda por fase
// - Badges/chips de estado visual
//
// =====================================================================================

/**
 * Fases de evaluación disponibles
 * Sincronizado con back_crm.CRM.enums.Evaluacion_enum
 */
export enum FaseEvaluacion {
  ANTES = 'ANTES',
  DESPUES = 'DESPUES'
}

/**
 * Labels en español para las fases
 */
export const FASE_EVALUACION_LABELS: Record<FaseEvaluacion, string> = {
  [FaseEvaluacion.ANTES]: 'Antes',
  [FaseEvaluacion.DESPUES]: 'Después'
};

/**
 * Clases CSS para badges según fase
 */
export const FASE_EVALUACION_CLASSES: Record<FaseEvaluacion, string> = {
  [FaseEvaluacion.ANTES]: 'bg-blue-500 text-white',
  [FaseEvaluacion.DESPUES]: 'bg-green-500 text-white'
};