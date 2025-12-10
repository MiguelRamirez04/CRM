import { Component, Input, Output, EventEmitter, OnInit, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../../atoms/icono/icono.component';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-ui-input-field',
  standalone: true,
  imports: [CommonModule, UiIconComponent],
  templateUrl: './input.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UiInputComponent),
      multi: true
    }
  ]
})
export class UiInputComponent implements OnInit, ControlValueAccessor {
  /* Inputs */
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() variant:
    | 'text' | 'code' | 'email' | 'password' | 'tel' | 'number' | 'search'
    | 'url' | 'file' | 'button' | 'reset' | 'submit' | 'radio'
    | 'checkbox' | 'range' | 'image' | 'select' | 'info'
    = 'text';
  @Input() codeLength: number = 6;
  @Input() textoError: string = '';
  @Input() options: { value: string; label: string }[] = [];
  @Input() labelObligatorio: boolean = false;
  @Input() nombreIcono: string = "user";
  
  // Nueva propiedad para controlar si el campo es editable
  @Input() editable: boolean = false;

  /* Value del input */
  @Input() value: any = '';
  editMode = false;
  originalValue: any = '';
  tempValue: any = '';

  /* Outputs opcionales */
  @Output() valueChange = new EventEmitter<string>();
  @Output() checkedChange = new EventEmitter<boolean>();

  /* ControlValueAccessor */
  onChange: any = () => {};
  onTouched: any = () => {};
  isDisabled = false;

  isFocused:boolean = false;

  // Nueva propiedad para determinar si el campo es solo lectura (info)
  get isInfoMode(): boolean {
    return this.variant === 'info';
  }

  writeValue(value: any): void {
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
  }

  /* */
  iconoVisible: boolean = true;
  codeDigits: string[] = [];

  ngOnInit() {
    if (this.variant === 'code') {
      this.codeDigits = Array(this.codeLength).fill('');
      if (this.value) {
        this.codeDigits = this.value.split('').slice(0, this.codeLength);
      }
    }
  }

  // Método para obtener el valor a mostrar en modo visualización
  getDisplayValue(): string {
    if (this.variant === 'select' && this.value) {
      const option = this.options.find(opt => opt.value === this.value);
      return option ? option.label : this.value;
    }
    if (this.variant === 'checkbox') {
      return this.value === 'true' ? 'Sí' : 'No';
    }
    if (this.variant === 'password') {
      return '••••••••';
    }
    if (this.variant === 'email' && this.value) {
      return this.value;
    }
    if (this.variant === 'tel' && this.value) {
      return this.value;
    }
    return this.value || '';
  }

  // Método para activar modo edición (no permitir para variant info)
  activarEdicion(): void {
    if (this.isInfoMode || !this.editable || this.isDisabled) return;
    
    this.originalValue = this.value;
    this.tempValue = this.value;
    this.editMode = true;
    
    // Enfocar el input después de un breve delay
    setTimeout(() => {
      const input = document.querySelector('input, select') as HTMLElement;
      if (input) input.focus();
    }, 50);
  }

  // Método para guardar cambios
  guardarCambios(): void {
    if (this.validate()) {
      this.value = this.tempValue;
      this.editMode = false;
      
      // Actualizar codeDigits si es variant code
      if (this.variant === 'code' && this.value) {
        this.codeDigits = this.value.split('').slice(0, this.codeLength);
      }
      
      // Emitir cambios
      this.onChange(this.value);
      this.valueChange.emit(this.value);
      this.onTouched();
    }
  }

  // Método para cancelar edición
  cancelarEdicion(): void {
    this.value = this.originalValue;
    this.editMode = false;
    this.textoError = '';
    this.textoErrores = [];
    this.onTouched();
  }

  // Método para desactivar edición (con validación)
  desactivarEdicion(): void {
    if (this.validate()) {
      this.guardarCambios();
    }
  }

  // Clases para modo visualización
  obtenerClasesVisualizacion(): string {
    const baseClasses = `
      min-h-[44px] px-3 py-2.5 rounded-md border
      bg-gray-50 text-gray-700 text-sm font-normal
      flex items-center w-full
      transition-colors duration-200
      hover:bg-gray-100
      border-gray-200
    `;
    
    // Si es info mode, el cursor es default, si no, pointer para indicar que es editable
    const cursorClass = this.isInfoMode ? 'cursor-default' : 'cursor-pointer';
    
    return `${baseClasses} ${cursorClass}`;
  }

  onTextChange(event: Event) {
    const input = event.target as HTMLInputElement;
    
    if (this.editMode) {
      // En modo edición, guardamos en tempValue
      this.tempValue = input.value;
    } else {
      // En modo normal, actualizamos directamente el value
      this.value = input.value;
      this.onChange(this.value);
      this.valueChange.emit(this.value);
    }
    
    this.validate();
  }

  onDigitChange(index: number, event: Event) {
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
    const input = event.target as HTMLInputElement;
    this.tempValue = input.checked ? 'true' : 'false';
    this.validate();
  }

  /* Validaciones */
  textoErrores: string[] = [];

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

  /* Estilos */
  obtenerClasesInput(): string {
    const baseInput = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-100
    `;

    const baseInputSelect = `
      rounded-md font-normal pl-3 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-100
    `;
    
    const baseInputSearch = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      text-[var(--color-texto-icono)]
      focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      bg-gray-100
    `;    

    // Si hay error, solo halo rojo en foco
    const error = this.textoError ? 'text-red-500 focus:ring-red-500 border-red-500' : '';

    // Si no está enfocado, mantenemos borde normal
    const borde = this.isFocused ? 'border border-transparent' : 'border border-[var(--color-borde)]';

    // Selecciona la base según el variant
    let base = '';
    switch (this.variant) {
      case 'select':
        base = baseInputSelect;
        break;
      case 'search':
        base = baseInputSearch;
        break;
      default:
        base = baseInput;
        break;
    }
    return `${base} ${error} ${borde}`;
  }

  /* Otros */
  get mostrarLabelRojo() { 
    return (this.labelObligatorio && !this.value) || !!this.textoError; 
  }

  dropdownAbierto = false;
  
  toggleDropdown() { 
    if (!this.isDisabled && !this.isInfoMode) {
      this.dropdownAbierto = !this.dropdownAbierto; 
    }
  }

  seleccionar(valor: string) {
    this.tempValue = valor;
    this.dropdownAbierto = false;
    this.validate();
  }

  onFocus() {
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
    if (this.variant !== 'select') {
      this.onTouched();
    }
  }
  
  getColorIcono(): string {
    if (this.textoError || this.textoErrores.length > 0) {
      return 'text-red-500';
    }
    if (this.isFocused) {
      return 'text-blue-500';
    }
    return 'text-zinc-500';
  }
}