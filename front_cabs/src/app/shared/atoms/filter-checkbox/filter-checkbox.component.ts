// =====================================================================================
// ATOM: Filter Checkbox - Checkbox estilizado para filtros
// =====================================================================================

import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-filter-checkbox',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './filter-checkbox.component.html',
  styleUrls: ['./filter-checkbox.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FilterCheckboxComponent),
      multi: true
    }
  ]
})
export class FilterCheckboxComponent implements ControlValueAccessor {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() label: string = '';
  @Input() description?: string;
  @Input() value: any;
  @Input() checked: boolean = false;
  @Input() disabled: boolean = false;
  
  @Output() checkedChange = new EventEmitter<boolean>();

  // =====================================================================================
  // CONTROL VALUE ACCESSOR
  // =====================================================================================
  
  private onChange: (value: boolean) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: boolean): void {
    this.checked = value;
  }

  registerOnChange(fn: (value: boolean) => void): void {
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

  onCheckboxChange(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    this.checked = checkbox.checked;
    
    this.onChange(this.checked);
    this.onTouched();
    this.checkedChange.emit(this.checked);
  }
}