import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-detail-field',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [ngClass]="{'flex justify-between': layout === 'horizontal', 'space-y-1': layout === 'vertical'}">
      <span class="text-sm text-gray-600" [ngClass]="{'block mb-1': layout === 'vertical'}">{{ label }}</span>
      <span class="text-sm font-medium text-gray-900"><ng-content></ng-content></span>
    </div>
  `
})
export class DetailFieldComponent {
  @Input() label = '';
  @Input() layout: 'horizontal' | 'vertical' = 'horizontal';
}