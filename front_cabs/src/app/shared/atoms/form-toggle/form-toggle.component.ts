import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
  selector: 'app-form-toggle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './form-toggle.component.html',
  styleUrls: ['./form-toggle.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormToggleComponent),
      multi: true
    }
  ]
})
export class FormToggleComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() disabled: boolean = false;
  @Input() helpText: string = '';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  
  @Output() valueChange = new EventEmitter<boolean>();
  @Output() toggle = new EventEmitter<boolean>();

  value: boolean = false;

  onChange: any = () => {};
  onTouched: any = () => {};

  writeValue(value: any): void {
    this.value = !!value;
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

  onToggle(): void {
    if (this.disabled) return;
    
    this.value = !this.value;
    this.onChange(this.value);
    this.valueChange.emit(this.value);
    this.toggle.emit(this.value);
    this.onTouched();
  }

  get toggleSizeClass(): string {
    return `toggle-${this.size}`;
  }
}