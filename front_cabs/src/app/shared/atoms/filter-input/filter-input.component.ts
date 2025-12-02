// =====================================================================================
// ATOM: Filter Input - Input estilizado con icono para filtros
// =====================================================================================

import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { UiIconComponent } from '../icono/icono.component';

export type FilterInputType = 'text' | 'date' | 'month' | 'year' | 'number' | 'email';

@Component({
  selector: 'app-filter-input',
  standalone: true,
  imports: [CommonModule, FormsModule, UiIconComponent],
  templateUrl: './filter-input.component.html',
  styleUrls: ['./filter-input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FilterInputComponent),
      multi: true
    }
  ]
})
export class FilterInputComponent implements ControlValueAccessor {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() type: FilterInputType = 'text';
  @Input() placeholder: string = '';
  @Input() iconName?: string;
  @Input() disabled: boolean = false;
  @Input() value: string = '';  // ← Agregado como @Input
  
  @Output() valueChange = new EventEmitter<string>();

  // =====================================================================================
  // PROPIEDADES
  // =====================================================================================

  // =====================================================================================
  // CONTROL VALUE ACCESSOR
  // =====================================================================================
  
  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // =====================================================================================
  // MÉTODOS
  // =====================================================================================

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    
    this.onChange(this.value);
    this.onTouched();
    this.valueChange.emit(this.value);
  }

  get hasIcon(): boolean {
    return !!this.iconName || this.type === 'date' || this.type === 'month' || this.type === 'year';
  }

  get defaultIcon(): string {
    if (this.iconName) return this.iconName;
    
    switch (this.type) {
      case 'date':
      case 'month':
      case 'year':
        return 'calendar';
      default:
        return '';
    }
  }
}