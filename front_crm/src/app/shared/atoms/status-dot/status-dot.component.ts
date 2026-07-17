import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { UiIconComponent } from '../icono/icono.component';

export type StatusType =
  | 'completado'
  | 'atencion'
  | 'nuevo'
  | 'revision'
  | 'rechazado'
  | 'enviada'
  | 'aprobado'
  | 'venta-realizada'
  | 'personalizado'
  | 'neutral';

@Component({
  selector: 'app-status-dot',
  standalone: true,
  imports: [CommonModule, UiIconComponent],
  templateUrl: './status-dot.component.html',
  styleUrls: ['./status-dot.component.css'],
})
export class StatusDotComponent {
  @Input() texto: string = '';
  @Input() tipo: StatusType = 'personalizado';
  @Input() mostrarIcono: boolean = true;

  // Personalización (solo para tipo personalizado)
  @Input() colorTexto?: string;
  @Input() colorFondo?: string;
  @Input() svgPersonalizado?: string;

  svgSeguro?: SafeHtml;

  constructor(private sanitizer: DomSanitizer) {}

  ngOnChanges() {
    if (this.svgPersonalizado) {
      this.svgSeguro = this.sanitizer.bypassSecurityTrustHtml(this.svgPersonalizado);
    }
  }
}
