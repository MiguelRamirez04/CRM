import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-form-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './form-select.component.html',
  styleUrls: ['./form-select.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormSelectComponent),
      multi: true
    }
  ]
})
export class FormSelectComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() placeholder: string = 'Seleccione una opción';
  @Input() options: SelectOption[] = [];
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() helpText: string = '';
  @Input() errorMessage: string = '';
  @Input() loading: boolean = false;
  @Input() loadingText: string = 'Cargando...';
  @Input() emptyText: string = 'No hay opciones disponibles';
  
  @Output() valueChange = new EventEmitter<string>();
  @Output() selectionChange = new EventEmitter<SelectOption | null>();

  value: string = '';

  onChange: any = () => {};
  onTouched: any = () => {};

  writeValue(value: any): void {
    this.value = value || '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onSelectChange(value: string): void {
    this.value = value;
    this.onChange(value);
    this.valueChange.emit(value);
    
    const selectedOption = this.options.find(opt => opt.value.toString() === value);
    this.selectionChange.emit(selectedOption || null);
  }

  onBlur(): void {
    this.onTouched();
  }

  get placeholderText(): string {
    if (this.loading) return this.loadingText;
    if (this.options.length === 0) return this.emptyText;
    return this.placeholder;
  }

  get isDisabled(): boolean {
    return this.disabled || this.loading;
  }
}