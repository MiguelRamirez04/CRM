import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingSpinnerComponent } from '../../atoms/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  template: `
    <div 
      *ngIf="visible" 
      class="loading-overlay"
      [class.loading-overlay--fixed]="fixed"
      [class.loading-overlay--absolute]="!fixed">
      <div class="loading-overlay__content">
        <app-loading-spinner [mensaje]="mensaje"></app-loading-spinner>
      </div>
    </div>
  `,
  styles: [`
    .loading-overlay {
      inset: 0;
      background-color: rgba(255, 255, 255, 0.85);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      backdrop-filter: blur(2px);
    }

    .loading-overlay--fixed {
      position: fixed;
    }

    .loading-overlay--absolute {
      position: absolute;
    }

    .loading-overlay__content {
      background: white;
      padding: 2rem;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    }
  `]
})
export class LoadingOverlayComponent {
  /** Controla la visibilidad del overlay */
  @Input() visible = false;

  /** Mensaje a mostrar debajo del spinner */
  @Input() mensaje = 'Cargando...';

  /** Si es true usa position:fixed (pantalla completa), si es false usa absolute (relativo al contenedor) */
  @Input() fixed = false;
}