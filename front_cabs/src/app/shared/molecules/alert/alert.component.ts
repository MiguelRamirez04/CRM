import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [ngClass]="{
      'bg-red-50 text-red-800 border-red-200': tipo === 'error',
      'bg-yellow-50 text-yellow-800 border-yellow-200': tipo === 'warning',
      'bg-blue-50 text-blue-800 border-blue-200': tipo === 'info',
      'bg-green-50 text-green-800 border-green-200': tipo === 'success'
    }" 
    class="flex items-start p-4 rounded-lg border">
      <svg class="w-5 h-5 mr-2 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
        <path *ngIf="tipo === 'error'" fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
        <path *ngIf="tipo !== 'error'" fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path>
      </svg>
      <span><ng-content></ng-content></span>
    </div>
  `
})
export class AlertComponent {
  @Input() tipo: 'error' | 'warning' | 'info' | 'success' = 'info';
}