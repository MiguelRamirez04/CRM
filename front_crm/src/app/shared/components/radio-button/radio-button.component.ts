import { Component, Input, Output, EventEmitter  } from '@angular/core';

@Component({
  selector: 'app-radio-button',
  imports: [],
  templateUrl: './radio-button.component.html',
  styleUrl: './radio-button.component.css'
})
export class RadiobuttonComponent {
  @Input() label: string = '';
  @Input() name: string = '';
  @Input() checked: boolean = false;
  @Input() disabled: boolean = false;
  @Output() change = new EventEmitter<void>();

  onSelect(): void {
    if (!this.disabled && !this.checked) {
      this.checked = true;
      this.change.emit();
    }
  }
}
