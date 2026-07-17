
import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table-cell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [style.text-align]="alineacion">
      <!-- Si hay template personalizado, usarlo -->
      <ng-container *ngIf="plantilla; else valorSimple">
        <ng-container *ngTemplateOutlet="plantilla; context: { 
          item: dato,           
          dato: valor,          
          $implicit: dato       
        }">
        </ng-container>
      </ng-container>

      <!-- Si no hay template, mostrar valor directo -->
      <ng-template #valorSimple>
        {{ valor }}
      </ng-template>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `]
})
export class TableCellComponent {
  @Input() valor: any;
  @Input() dato: any;  // El item completo
  @Input() plantilla?: TemplateRef<any>;
  @Input() alineacion: 'left' | 'center' | 'right' = 'center';
}