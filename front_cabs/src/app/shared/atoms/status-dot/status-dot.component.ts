import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

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
  imports: [CommonModule],
  templateUrl: './status-dot.component.html',
  styleUrls: ['./status-dot.component.css'],
})
export class StatusDotComponent {
  @Input() texto: string = '';
  @Input() tipo: StatusType = 'personalizado';
  @Input() icono?: TemplateRef<any>;
  @Input() mostrarIcono: boolean = true; // propiedad para controlar visibilidad

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