import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-detail-section',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-gray-50 rounded-lg p-4 mb-4">
      <h3 class="text-sm font-semibold text-gray-700 mb-3 flex items-center">
        <svg *ngIf="icono" class="w-5 h-5 mr-2" [ngClass]="colorIcono" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" [attr.d]="icono"></path>
        </svg>
        {{ titulo }}
      </h3>
      <ng-content></ng-content>
    </div>
  `
})
export class DetailSectionComponent {
  @Input() titulo = '';
  @Input() icono?: string;
  @Input() colorIcono = 'text-blue-600';
}