import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../../atoms/icono/icono.component'; 

@Component({
  selector: 'app-detail-section',
  standalone: true,
  imports: [CommonModule, UiIconComponent], 
  template: `
    <div class="bg-gray-50 rounded-lg p-4 mb-4">
      <h3 class="text-sm font-semibold text-gray-700 mb-3 flex items-center">
        <!--  USAR app-ui-icono EN LUGAR DE SVG -->
        <app-ui-icono 
          *ngIf="icono" 
          [name]="icono" 
          [size]="'w-5 h-5'" 
          [color]="colorIcono"
          class="mr-2">
        </app-ui-icono>
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