// =====================================================================================
// ATOM: Filter Select - Select estilizado para filtros
// =====================================================================================

import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { UiIconComponent } from '../icono/icono.component';

export interface SelectOption {
  valor: any;
  etiqueta: string;
}

@Component({
  selector: 'app-filter-select',
  standalone: true,
  imports: [CommonModule, FormsModule, UiIconComponent],
  templateUrl: './filter-select.component.html',
  styleUrls: ['./filter-select.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FilterSelectComponent),
      multi: true
    }
  ]
})
export class FilterSelectComponent implements ControlValueAccessor {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() options: SelectOption[] = [];
  @Input() placeholder: string = 'Seleccione una opción';
  @Input() disabled: boolean = false;
  @Input() value: any = null;  // ← Agregado como @Input
  
  @Output() valueChange = new EventEmitter<any>();

  // =====================================================================================
  // PROPIEDADES
  // =====================================================================================

  // =====================================================================================
  // CONTROL VALUE ACCESSOR
  // =====================================================================================
  
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: (value: any) => void): void {
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

  onSelectChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.value = select.value || null;
    
    this.onChange(this.value);
    this.onTouched();
    this.valueChange.emit(this.value);
  }
}