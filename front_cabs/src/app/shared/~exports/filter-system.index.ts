// =====================================================================================
// FILTER SYSTEM - Exportaciones centralizadas
// =====================================================================================
// UBICACIÓN: Este archivo debe estar en: app/shared/filter-system.index.ts
// =====================================================================================

// ATOMS
export { FilterCheckboxComponent } from '../atoms/filter-checkbox/filter-checkbox.component';
export { FilterInputComponent } from '../atoms/filter-input/filter-input.component';
export type { FilterInputType } from '../atoms/filter-input/filter-input.component';
export { FilterSelectComponent } from '../atoms/filter-select/filter-select.component';
export type { SelectOption } from '../atoms/filter-select/filter-select.component';

// MOLECULES
export { FilterCheckboxGroupComponent } from '../molecules/filter-checkbox-group/filter-checkbox-group.component';
export type { CheckboxOption } from '../molecules/filter-checkbox-group/filter-checkbox-group.component';

export { FilterFieldComponent } from '../molecules/filter-field/filter-field.component';
export type { FilterFieldType } from '../molecules/filter-field/filter-field.component';

// ORGANISMS
export { FilterPanelComponent } from '../organisms/filter-panel/filter-panel.component';
export type { 
  FilterCheckboxGroupConfig,
  FilterFieldConfig,
  FilterPanelConfig,
  FilterResult
} from '../organisms/filter-panel/filter-panel.component';