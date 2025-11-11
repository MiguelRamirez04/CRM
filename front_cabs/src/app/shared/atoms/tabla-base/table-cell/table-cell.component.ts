import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table-cell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <td 
      class="celda-dato"
      [style.text-align]="alineacion">
      
      <!-- Si tiene plantilla personalizada -->
      <ng-container *ngIf="plantilla; else contenidoSimple">
        <ng-container 
          *ngTemplateOutlet="plantilla; context: { $implicit: dato, dato: valor }">
        </ng-container>
      </ng-container>

      <!-- Contenido simple sin plantilla -->
      <ng-template #contenidoSimple>
        {{ valor }}
      </ng-template>
    </td>
  `,
  styles: [`
    .celda-dato {
      padding: 20px 16px;
      font-size: 14px;
      color: var(--texto-body);
      font-weight: 500;
    }
  `]
})
export class TableCellComponent {
  @Input() valor: any;
  @Input() dato: any;
  @Input() plantilla?: TemplateRef<any>;
  @Input() alineacion: 'left' | 'center' | 'right' = 'center';
}