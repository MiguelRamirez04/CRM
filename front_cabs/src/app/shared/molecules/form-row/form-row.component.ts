import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-row',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-row.component.html',
  styleUrls: ['./form-row.component.css']
})
export class FormRowComponent {
  @Input() columns: 1 | 2 | 3 = 2;
  @Input() gap: 'small' | 'medium' | 'large' = 'medium';
  @Input() alignItems: 'start' | 'center' | 'end' = 'start';

  get gridClass(): string {
    return `grid-${this.columns}col gap-${this.gap} align-${this.alignItems}`;
  }
}