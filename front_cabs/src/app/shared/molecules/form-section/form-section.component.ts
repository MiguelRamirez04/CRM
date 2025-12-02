import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-section.component.html',
  styleUrls: ['./form-section.component.css']
})
export class FormSectionComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() showDivider: boolean = true;
  @Input() spacing: 'small' | 'medium' | 'large' = 'medium';
}