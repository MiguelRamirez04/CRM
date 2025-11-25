import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [ngClass]="{
      'bg-green-100 text-green-800': tipo === 'success',
      'bg-gray-100 text-gray-800': tipo === 'default',
      'bg-blue-100 text-blue-800': tipo === 'info',
      'bg-yellow-100 text-yellow-800': tipo === 'warning',
      'bg-red-100 text-red-800': tipo === 'error'
    }" 
    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium">
      <svg *ngIf="mostrarIcono && tipo === 'success'" class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
      </svg>
      <ng-content></ng-content>
    </span>
  `
})
export class BadgeComponent {
  @Input() tipo: 'success' | 'default' | 'info' | 'warning' | 'error' = 'default';
  @Input() mostrarIcono = false;
}