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
    | 'checkbox' | 'range' | 'image' | 'select'
    = 'text';
  @Input() codeLength: number = 6;
  @Input() textoError: string = '';
  @Input() options: { value: string; label: string }[] = [];
  @Input() labelObligatorio: boolean = false;
  @Input() nombreIcono: string = "user";

  /*  Value del input  */
  @Input() value: any = '';
  editMode = false;

  /*  Outputs opcionales  */
  @Output() valueChange = new EventEmitter<string>();
  @Output() checkedChange = new EventEmitter<boolean>();

  /*  ControlValueAccessor  */
  onChange: any = () => {};
  onTouched: any = () => {};
  isDisabled = false;

  isFocused:boolean = false;

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

  /*   */
  iconoVisible: boolean = true;
  codeDigits: string[] = [];

  ngOnInit() {
    if (this.variant === 'code') {
      this.codeDigits = Array(this.codeLength).fill('');
    }
  }

  onTextChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.value = input.value;

    this.onChange(this.value); 
    this.valueChange.emit(this.value);

    this.validate();
  }

  onDigitChange(index: number, event: Event) {
    const input = event.target as HTMLInputElement;
    const digit = input.value.slice(0, 1);

    this.codeDigits[index] = digit;
    const code = this.codeDigits.join('');

    this.value = code;
    this.onChange(code);
    this.valueChange.emit(code);

    const next = input.nextElementSibling as HTMLInputElement;
    if (digit && next) next.focus();
  }

  onCheckboxChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.value = input.checked;
    this.onChange(this.value);
    this.checkedChange.emit(input.checked);
  }

  /*  Validaciones */
  textoErrores: string[] = [];

  setError(msg: string) { this.textoError = msg; }
  clearError() { this.textoError = ''; }

  validate(): boolean {
    this.textoErrores = [];

    if (this.labelObligatorio && !this.value) {
      this.setError(`El ${this.label.toLowerCase()} es obligatorio`);
      return false;
    }

    if (this.variant === 'email' && this.value && !/\S+@\S+\.\S+/.test(this.value)) {
      this.setError('Correo inválido');
      return false;
    }

    if (this.variant === 'tel' && this.value) {
      if (/[a-zA-Z]/.test(this.value)) {
        this.setError('Teléfono inválido, contiene letras');
        return false;
      }
      if (/[^0-9\s]/.test(this.value)) {
        this.setError('Teléfono inválido, contiene símbolos');
        return false;
      }
    }

    if (this.variant === 'password' && this.value) {
      if (!/[A-Z]/.test(this.value)) {
        this.textoErrores.push('La contraseña debe tener una mayúscula');
      }
      if (this.value.length < 8) {
        this.textoErrores.push('Debe tener mínimo 8 caracteres');
      }
      if (!/[!@#$%^&*(),.?":{}|<>]/.test(this.value)) {
        this.textoErrores.push('La contraseña debe tener un símbolo');
      }
    }

    this.clearError();
    return true;
  }

  /*  Estilos  */
  obtenerClasesInput(): string {
    const baseInput = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      bg-[var(--color-background-input)] text-[var(--color-texto-icono)]
      focus:ring-blue-500
    `;

    const baseInputSelect = `
      rounded-md font-normal pl-3 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      bg-[var(--color-background-input)] text-[var(--color-texto-icono)]
      focus:ring-blue-500
    `;
    const baseInputSearch = `
      rounded-md font-normal pl-8 pr-3 py-3 text-sm 
      focus:outline-none focus:ring-1 
      w-full
      bg-[var(--color-background-input)] text-[var(--color-texto-icono)]
      focus:ring-blue-500
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



  /*  Otros  */

  get mostrarLabelRojo() { return (this.labelObligatorio && !this.value) || !!this.textoError; }

  dropdownAbierto = false;
  toggleDropdown() { this.dropdownAbierto = !this.dropdownAbierto; }

  seleccionar(valor: string) {
    this.value = valor;
    this.onChange(valor);
    this.valueChange.emit(valor);
    this.dropdownAbierto = false;
    this.validate();
  }

  onFocus() {
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
  }
  getColorIcono(): string {
    if (this.textoError || this.textoErrores.length > 0) {
      return 'text-red-500';
    }
    if (this.isFocused) {
      return 'text-blue-500 ';
    }
    return 'text-zinc-500';
  }

  
  // Metodo para cambiar el estado del input del 
  actualizarLaber(){
    this.editMode = true;
  }
}
