// =====================================================================================
// MOLECULE: Filter Field - Campo de filtro con título (fecha o select)
// =====================================================================================

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FilterInputComponent, FilterInputType } from '../../atoms/filter-input/filter-input.component';
import { FilterSelectComponent, SelectOption } from '../../atoms/filter-select/filter-select.component';

export type FilterFieldType = 'date' | 'month' | 'year' | 'select' | 'text';

@Component({
  selector: 'app-filter-field',
  standalone: true,
  imports: [CommonModule, FilterInputComponent, FilterSelectComponent],
  templateUrl: './filter-field.component.html',
  styleUrls: ['./filter-field.component.css']
})
export class FilterFieldComponent {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() titulo: string = '';
  @Input() type: FilterFieldType = 'text';
  @Input() placeholder: string = '';
  @Input() value: any = '';
  @Input() options: SelectOption[] = []; // Solo para tipo 'select'
  @Input() iconName?: string;
  
  @Output() valueChange = new EventEmitter<any>();

  // =====================================================================================
  // GETTERS
  // =====================================================================================

  get isSelect(): boolean {
    return this.type === 'select';
  }

  get isInput(): boolean {
    return !this.isSelect;
  }

  get inputType(): FilterInputType {
    if (this.type === 'select') return 'text';
    return this.type as FilterInputType;
  }

  // =====================================================================================
  // MÉTODOS
  // =====================================================================================

  onValueChange(newValue: any): void {
    this.valueChange.emit(newValue);
  }
}