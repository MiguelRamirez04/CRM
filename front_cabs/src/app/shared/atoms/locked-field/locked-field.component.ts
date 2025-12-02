import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-locked-field',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './locked-field.component.html',
  styleUrls: ['./locked-field.component.css']
})
export class LockedFieldComponent {
  @Input() label: string = '';
  @Input() value: string = '';
  @Input() helpText: string = 'Este campo no se puede modificar';
  @Input() required: boolean = false;
  @Input() showIcon: boolean = true;
}