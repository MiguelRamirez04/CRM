import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col items-center justify-center py-12">
      <div class="border-4 border-gray-200 border-t-blue-600 rounded-full w-12 h-12 animate-spin"></div>
      <p *ngIf="mensaje" class="mt-4 text-gray-600">{{ mensaje }}</p>
    </div>
  `
})
export class LoadingSpinnerComponent {
  @Input() mensaje = 'Cargando...';
}