import { Component, Input, Output, EventEmitter, OnInit, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../../atoms/icono/icono.component';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export type InputVariant =
  | 'text' | 'code' | 'email' | 'password' | 'tel' | 'number' | 'search'
  | 'url' | 'file' | 'button' | 'reset' | 'submit' | 'radio'
  | 'checkbox' | 'range' | 'image' | 'select' | 'info'
  | 'date' | 'month' | 'year' | 'textarea' | 'toggle' | 'locked'
  | 'filter-input' | 'filter-select' | 'filter-checkbox';

export type ToggleSize = 'small' | 'medium' | 'large';

@Component({
  selector: 'app-ui-input-field',
  standalone: true,
  imports: [CommonModule, UiIconComponent, FormsModule],
  templateUrl: './input.component.html',
  styleUrls: ['./input.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UiInputComponent),
      multi: true
    }
  ]
})
export class UiInputComponent implements OnInit, ControlValueAccessor {
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() variant: InputVariant = 'text';
  @Input() textoError: string = '';
  @Input() labelObligatorio: boolean = false;
  @Input() nombreIcono: string = 'user';
  @Input() editable: boolean = false;
  @Input() disabled: boolean = false;
  @Input() readonly: boolean = false;
  @Input() value: any = '';
  @Input() helpText: string = '';
  @Input() codeLength: number = 6;
  @Input() options: SelectOption[] = [];
  @Input() loading: boolean = false;
  @Input() loadingText: string = 'Cargando...';
  @Input() emptyText: string = 'No hay opciones disponibles';
  @Input() rows: number = 4;
  @Input() maxLength?: number;
  @Input() showCharCount: boolean = false;
  @Input() toggleSize: ToggleSize = 'medium';
  @Input() checkboxDescription?: string;
  @Input() checked: boolean = false;

  @Output() valueChange = new EventEmitter<any>();
  @Output() checkedChange = new EventEmitter<boolean>();
  @Output() selectionChange = new EventEmitter<SelectOption | null>();
  @Output() toggle = new EventEmitter<boolean>();

  editMode = false;
  originalValue: any = '';
  tempValue: any = '';
  isFocused: boolean = false;
  iconoVisible: boolean = true;
  codeDigits: string[] = [];
  dropdownAbierto = false;
  textoErrores: string[] = [];

  onChange: any = () => {};
  onTouched: any = () => {};
  isDisabled = false;

  writeValue(value: any): void {
    if (this.variant === 'toggle') {
      this.value = !!value;
      return;
    }
    
    if (this.variant === 'filter-checkbox') {
      this.checked = !!value;
      this.value = value;
      return;
    }
    
    if (this.variant === 'checkbox') {
      if (typeof value === 'boolean') {
        this.value = value ? 'true' : 'false';
      } else if (value === 'true' || value === true) {
        this.value = 'true';
      } else {
        this.value = 'false';
      }
      this.checked = this.value === 'true';
      return;
    }
    
    this.value = value ?? '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
    this.disabled = isDisabled;
  }

  ngOnInit() {
    if (this.variant === 'code') {
      this.codeDigits = Array(this.codeLength).fill('');
      if (this.value) {
        this.codeDigits = this.value.split('').slice(0, this.codeLength);
      }
    }
    
    if (this.variant === 'checkbox' && this.value) {
      this.checked = this.value === 'true';
    }
  }

  get isInfoMode(): boolean {
    return this.variant === 'info';
  }

  get isLockedMode(): boolean {
    return this.variant === 'locked';
  }

  get isFilterVariant(): boolean {
    return this.variant === 'filter-input' || this.variant === 'filter-select' || this.variant === 'filter-checkbox';
  }

  get hasIcon(): boolean {
    return !!this.nombreIcono || this.variant === 'date' || this.variant === 'month' || this.variant === 'year';
  }

  get defaultIcon(): string {
    if (this.nombreIcono) return this.nombreIcono;
    
    switch (this.variant) {
      case 'date':
      case 'month':
      case 'year':
        return 'calendar';
      case 'email':
        return 'envelope';
      case 'tel':
        return 'phone';
      case 'search':
      case 'filter-input':
        return 'magnifying-glass';
      case 'password':
        return 'lock-closed';
      case 'url':
        return 'globe-alt';
      default:
        return this.nombreIcono;
    }
  }

  get placeholderText(): string {
    if (this.loading) return this.loadingText;
    if (this.options.length === 0 && (this.variant === 'select' || this.variant === 'filter-select')) return this.emptyText;
    return this.placeholder || 'Seleccione una opción';
  }

  get charCount(): string {
    if (!this.showCharCount || !this.maxLength) return '';
    const currentLength = (this.value || '').length;
    return `${currentLength}/${this.maxLength}`;
  }

  get toggleSizeClass(): string {
    return `toggle-${this.toggleSize}`;
  }

  get mostrarLabelRojo() { 
    return (this.labelObligatorio && !this.value) || !!this.textoError; 
  }

  getDisplayValue(): string {
    if ((this.variant === 'select' || this.variant === 'filter-select') && this.value) {
      const option = this.options.find(opt => opt.value?.toString() === this.value?.toString());
      return option ? option.label : this.value;
    }
    
    if (this.variant === 'checkbox') {
      return this.value === 'true' ? 'Sí' : 'No';
    }
    
    if (this.variant === 'filter-checkbox') {
      return this.checked ? 'Sí' : 'No';
    }
    
    if (this.variant === 'password') {
      return '••••••••';
    }
    
    if (this.variant === 'toggle') {
      return this.value ? 'Activado' : 'Desactivado';
    }
    
    return this.value || '';
  }

  activarEdicion(): void {
    if (this.isInfoMode || this.isLockedMode || !this.editable || this.isDisabled || this.disabled) return;
    
    this.originalValue = this.value;
    this.tempValue = this.value;
    this.editMode = true;
    
    setTimeout(() => {
      const input = document.querySelector('input, select, textarea') as HTMLElement;
      if (input) input.focus();
    }, 50);
  }

  guardarCambios(): void {
    if (this.validate()) {
      this.value = this.tempValue;
      this.editMode = false;
      
      if (this.variant === 'code' && this.value) {
        this.codeDigits = this.value.split('').slice(0, this.codeLength);
      }
      
      this.onChange(this.value);
      this.valueChange.emit(this.value);
      this.onTouched();
    }
  }

  cancelarEdicion(): void {
    this.value = this.originalValue;
    this.editMode = false;
    this.textoError = '';
    this.textoErrores = [];
    this.onTouched();
  }

  desactivarEdicion(): void {
    if (this.validate()) {
      this.guardarCambios();
    }
  }

  onTextChange(event: Event) {
    if (this.disabled) return;
    
    const input = event.target as HTMLInputElement;
    
    if (this.editMode) {
      this.tempValue = input.value;
    } else {
      this.value = input.value;
      this.onChange(this.value);
      this.valueChange.emit(this.value);
    }
    
    this.validate();
  }

  onTextareaChange(value: string): void {
    if (this.disabled) return;
    
    if (this.editMode) {
      this.tempValue = value;
    } else {
      this.value = value;
      this.onChange(value);
      this.valueChange.emit(value);
    }
  }

  onDigitChange(index: number, event: Event) {
    if (this.disabled) return;
    
    const input = event.target as HTMLInputElement;
    const digit = input.value.slice(0, 1);

    this.codeDigits[index] = digit;
    const code = this.codeDigits.join('');

    this.tempValue = code;
    this.validate();

    const next = input.nextElementSibling as HTMLInputElement;
    if (digit && next) {
      next.focus();
    }
  }

  onCheckboxChange(event: Event) {
    if (this.disabled) return;
    
    const input = event.target as HTMLInputElement;
    const isChecked = input.checked;
    
    if (this.variant === 'filter-checkbox') {
      this.checked = isChecked;
      this.onChange(isChecked);
      this.checkedChange.emit(isChecked);
      this.onTouched();
      this.validate();
      return;
    }
    
    if (this.variant === 'checkbox') {
      const stringValue = isChecked ? 'true' : 'false';
      
      if (this.editMode) {
        this.tempValue = stringValue;
      } else {
        this.value = stringValue;
        this.checked = isChecked;
        this.onChange(stringValue);
        this.valueChange.emit(stringValue);
      }
      
      this.onTouched();
      this.validate();
    }
  }

  onSelectChange(event: Event): void {
    if (this.disabled) return;
    
    const select = event.target as HTMLSelectElement;
    const value = select.value || null;
    
    if (this.editMode) {
      this.tempValue = value;
    } else {
      this.value = value;
      this.onChange(value);
      this.valueChange.emit(value);
      
      const selectedOption = this.options.find(opt => opt.value?.toString() === value);
      this.selectionChange.emit(selectedOption || null);
    }
    
    this.onTouched();
    this.validate();
  }

  onToggle(): void {
    if (this.disabled) return;
    
    this.value = !this.value;
    this.onChange(this.value);
    this.valueChange.emit(this.value);
    this.toggle.emit(this.value);
    this.onTouched();
  }

  toggleDropdown() { 
    if (!this.isDisabled && !this.disabled && !this.isInfoMode && !this.isLockedMode) {
      this.dropdownAbierto = !this.dropdownAbierto; 
    }
  }

  seleccionar(valor: string) {
    if (this.disabled) return;
    
    this.tempValue = valor;
    this.dropdownAbierto = false;
    
    if (!this.editMode) {
      this.value = valor;
      this.onChange(this.value);
      this.valueChange.emit(this.value);
      
      const selectedOption = this.options.find(opt => opt.value?.toString() === valor);
      this.selectionChange.emit(selectedOption || null);
    }
    
    this.validate();
  }

  onFocus() {
    if (this.disabled) return;
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
    if (this.variant !== 'select' && this.variant !== 'filter-select') {
      this.onTouched();
    }
  }

  setError(msg: string) { 
    this.textoError = msg; 
  }
  
  clearError() { 
    this.textoError = ''; 
  }

  validate(): boolean {
    this.textoErrores = [];
    const valueToValidate = this.editMode ? this.tempValue : this.value;

    if (this.labelObligatorio && !valueToValidate) {
      this.setError(`El ${this.label.toLowerCase()} es obligatorio`);
      return false;
    }

    if (this.variant === 'email' && valueToValidate && !/\S+@\S+\.\S+/.test(valueToValidate)) {
      this.setError('Correo inválido');
      return false;
    }

    if (this.variant === 'tel' && valueToValidate) {
      if (/[a-zA-Z]/.test(valueToValidate)) {
        this.setError('Teléfono inválido, contiene letras');
        return false;
      }
      if (/[^0-9\s+\-()]/.test(valueToValidate)) {
        this.setError('Teléfono inválido, contiene símbolos no permitidos');
        return false;
      }
    }

    if (this.variant === 'password' && valueToValidate) {
      if (!/[A-Z]/.test(valueToValidate)) {
        this.textoErrores.push('La contraseña debe tener una mayúscula');
      }
      if (valueToValidate.length < 8) {
        this.textoErrores.push('Debe tener mínimo 8 caracteres');
      }
      if (!/[!@#$%^&*(),.?":{}|<>]/.test(valueToValidate)) {
        this.textoErrores.push('La contraseña debe tener un símbolo');
      }
    }

    this.clearError();
    return this.textoErrores.length === 0;
  }

  obtenerClasesVisualizacion(): string {
    const baseClasses = `
      min-h-[44px] px-3 py-2.5 rounded-md border
      bg-gray-50 text-gray-700 text-sm font-normal
      flex items-center w-full
      transition-colors duration-200
      hover:bg-gray-100
      border-gray-200
    `;
    
    const cursorClass = this.disabled ? 'cursor-not-allowed opacity-70' : 
                       this.isInfoMode || this.isLockedMode ? 'cursor-default' : 'cursor-pointer';
    
    return `${baseClasses} ${cursorClass}`;
  }

  obtenerClasesInput(): string {
    const baseInput = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-50
    `;

    const baseInputSelect = `
      rounded-md font-normal pl-3 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-50
    `;
    
    const baseInputSearch = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-50
    `;    

    const error = this.textoError ? 'text-red-500 focus:ring-red-500 border-red-500' : '';
    const borde = this.isFocused ? 'border border-transparent' : 'border border-[var(--color-borde)]';

    let base = '';
    switch (this.variant) {
      case 'select':
      case 'filter-select':
        base = baseInputSelect;
        break;
      case 'search':
      case 'filter-input':
        base = baseInputSearch;
        break;
      default:
        base = baseInput;
        break;
    }
    
    const disabledClass = this.disabled ? 'opacity-50 cursor-not-allowed' : '';
    
    return `${base} ${error} ${borde} ${disabledClass}`;
  }

  getColorIcono(): string {
    if (this.disabled) return 'text-gray-400';
    if (this.textoError || this.textoErrores.length > 0) {
      return 'text-red-500';
    }
    if (this.isFocused) {
      return 'text-blue-500';
    }
    return 'text-zinc-500';
  }
}