// =====================================================================================
// FILTER SYSTEM - Exportaciones centralizadas
// =====================================================================================

// INPUT CONSOLIDADO - Todos los inputs de filtro ahora están en molecules/input
// Selector: <app-ui-input-field>
// Usa la propiedad [variant] para especificar el tipo

export { UiInputComponent } from '../molecules/input/input.component';

// Exportación de tipos (con export type para isolatedModules)
export type { SelectOption, InputVariant } from '../molecules/input/input.component';

// Tipo compatible con código legacy
export type FilterInputType = 'text' | 'date' | 'month' | 'year' | 'number' | 'email';

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