import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-info-alert',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-info-alert.component.html',
  styleUrls: ['./form-info-alert.component.css']
})
export class FormInfoAlertComponent {
  @Input() message: string = '';
  @Input() type: 'info' | 'warning' | 'success' | 'error' = 'info';
  @Input() showIcon: boolean = true;
}