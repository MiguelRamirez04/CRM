import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table-header-cell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <th 
      class="celda-encabezado"
      [style.width]="ancho"
      [style.text-align]="alineacion">
      {{ encabezado }}
    </th>
  `,
  styles: [`
    .celda-encabezado {
      padding: 16px;
      font-size: 12px;
      color: var(--texto-body-secundario);
      text-transform: none;
      font-weight: 500;
      text-align: center;
    }
  `]
})
export class TableHeaderCellComponent {
  @Input() encabezado: string = '';
  @Input() ancho?: string;
  @Input() alineacion: 'left' | 'center' | 'right' = 'center';
}