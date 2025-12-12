// ============================================
// FORM SYSTEM - Exportaciones centralizadas
// ============================================

// ==================== INPUT EN UN SOLO ARCHIVO ====================
// Todos los inputs ahora están en un solo componente: UiInputComponent
// Selector: <app-ui-input-field>
// Usa la propiedad [variant] para especificar el tipo de input

export { UiInputComponent } from '../molecules/input/input.component';

// Exportación de tipos (con export type para isolatedModules)
export type { SelectOption, InputVariant, ToggleSize } from '../molecules/input/input.component';

// ==================== MOLECULES ====================
export * from '../molecules/form-row/form-row.component';
export * from '../molecules/form-section/form-section.component';
export * from '../molecules/form-info-alert/form-info-alert.component';
export * from '../molecules/score-display/score-display.component';
export * from '../molecules/loading-overlay/loading-overlay.component';

// ==================== ATOMS (UTILITIES) ====================
export * from '../atoms/loading-spinner/loading-spinner.component';

// ==================== ORGANISMS ====================
export * from '../organisms/form-panel/form-panel.component';