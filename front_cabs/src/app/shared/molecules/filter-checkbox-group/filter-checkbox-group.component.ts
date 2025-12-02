// =====================================================================================
// MOLECULE: Filter Checkbox Group - Grupo de checkboxes con título
// =====================================================================================

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilterCheckboxComponent } from '../../atoms/filter-checkbox/filter-checkbox.component';

export interface CheckboxOption {
  valor: any;
  etiqueta: string;
  descripcion?: string;
  seleccionado?: boolean;
}

@Component({
  selector: 'app-filter-checkbox-group',
  standalone: true,
  imports: [CommonModule, FilterCheckboxComponent],
  templateUrl: './filter-checkbox-group.component.html',
  styleUrls: ['./filter-checkbox-group.component.css']
})
export class FilterCheckboxGroupComponent {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() titulo: string = '';
  @Input() options: CheckboxOption[] = [];
  @Input() selectedValues: any[] = [];
  
  @Output() selectionChange = new EventEmitter<any[]>();

  // =====================================================================================
  // MÉTODOS
  // =====================================================================================

  isChecked(valor: any): boolean {
    return this.selectedValues.includes(valor);
  }

  onCheckboxChange(option: CheckboxOption, checked: boolean): void {
    let newSelection: any[] = [];

    if (checked) {
      // Agregar si no existe
      if (!this.selectedValues.includes(option.valor)) {
        newSelection = [...this.selectedValues, option.valor];
      } else {
        newSelection = [...this.selectedValues];
      }
    } else {
      // Remover
      newSelection = this.selectedValues.filter(v => v !== option.valor);
    }

    this.selectionChange.emit(newSelection);
  }
}