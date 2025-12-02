import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../../atoms/icono/icono.component'; 

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, UiIconComponent], 
  template: `
    <div [ngClass]="{
      'bg-red-50 text-red-800 border-red-200': tipo === 'error',
      'bg-yellow-50 text-yellow-800 border-yellow-200': tipo === 'warning',
      'bg-blue-50 text-blue-800 border-blue-200': tipo === 'info',
      'bg-green-50 text-green-800 border-green-200': tipo === 'success'
    }" 
    class="flex items-start p-4 rounded-lg border">
      
      <!-- 🔥 USAR app-ui-icono EN LUGAR DE SVG -->
      <app-ui-icono 
        *ngIf="mostrarIcono"
        [name]="obtenerIcono()"
        [size]="'w-5 h-5'"
        [color]="'currentColor'"
        class="mr-2 mt-0.5 flex-shrink-0">
      </app-ui-icono>
      
      <span [ngClass]="{'ml-0': !mostrarIcono}">
        <ng-content></ng-content>
      </span>
    </div>
  `
})
export class AlertComponent {
  @Input() tipo: 'error' | 'warning' | 'info' | 'success' = 'info';
  @Input() mostrarIcono: boolean = true;

  /**
   *  Método para obtener el nombre del icono según el tipo
   */
  obtenerIcono(): string {
    switch (this.tipo) {
      case 'error':
        return 'x-circle'; // Icono de error
      case 'warning':
        return 'exclamation-triangle'; // Icono de advertencia
      case 'success':
        return 'check-circle'; // Icono de éxito
      case 'info':
      default:
        return 'information-circle'; // Icono de información
    }
  }
}