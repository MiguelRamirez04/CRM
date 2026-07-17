import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-panel.component.html',
  styleUrls: ['./form-panel.component.css']
})
export class FormPanelComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() showHeader: boolean = true;
  @Input() showFooter: boolean = true;
  @Input() loading: boolean = false;
  @Input() loadingMessage: string = 'Guardando...';
  
  @Input() showPrimaryButton: boolean = true;
  @Input() primaryButtonText: string = 'Guardar';
  @Input() primaryButtonDisabled: boolean = false;
  
  @Input() showSecondaryButton: boolean = true;
  @Input() secondaryButtonText: string = 'Cancelar';
  @Input() secondaryButtonDisabled: boolean = false;

  @Output() primaryClick = new EventEmitter<void>();
  @Output() secondaryClick = new EventEmitter<void>();

  onPrimaryClick(): void {
    if (!this.primaryButtonDisabled && !this.loading) {
      this.primaryClick.emit();
    }
  }

  onSecondaryClick(): void {
    if (!this.secondaryButtonDisabled && !this.loading) {
      this.secondaryClick.emit();
    }
  }
}