import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../icono/icono.component';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule, UiIconComponent],
  template: `
    <span 
      [ngClass]="{
        'bg-green-100 text-green-800': tipo === 'success',
        'bg-gray-100 text-gray-800': tipo === 'default',
        'bg-blue-100 text-blue-800': tipo === 'info',
        'bg-yellow-100 text-yellow-800': tipo === 'warning',
        'bg-red-100 text-red-800': tipo === 'error'
      }" 
      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xl font-medium select-none"
    >
      <!-- Íconos según tipo -->
      @switch (tipo) {
        @case ('success') {
          <app-ui-icono
            name="check-circle"
            color="text-green-600"
            class="mr-1 inline-block align-middle"
          ></app-ui-icono>
        }
        @case ('error') {
          <app-ui-icono
            name="x-circle"
            color="text-red-600"
            class="mr-1 inline-block align-middle"
          ></app-ui-icono>
        }
        @case ('info') {
          <app-ui-icono
            name="information-circle"
            color="text-blue-600"
            class="mr-1 inline-block align-middle"
          ></app-ui-icono>
        }
        @case ('warning') {
          <app-ui-icono
            name="exclamation-triangle"
            color="text-yellow-600"
            class="mr-1 inline-block align-middle"
          ></app-ui-icono>
        }
        @case ('default') {
          <app-ui-icono
            name="exclamation-triangle"
            color="text-gray-600"
            class="mr-1 inline-block align-middle"
          ></app-ui-icono>
        }
      }
      <!-- Texto -->
      <span class="align-middle">{{ Texto }}</span>
    </span>
  `
})
export class BadgeComponent {
  @Input() tipo: 'success' | 'default' | 'info' | 'warning' | 'error' = 'default';
  @Input() Texto: string = "Texto";
}
