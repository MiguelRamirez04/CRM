import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

export type StatusType =
  | 'sin-seguimiento'
  | 'requiere-seguimiento'
  | 'nuevo'
  | 'revision'
  | 'rechazado'
  | 'enviada'
  | 'aprobado'
  | 'venta-realizada'
  | 'personalizado';

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

  // Personalización (solo para tipo personalizado)
  @Input() colorTexto?: string;
  @Input() colorFondo?: string;
  @Input() svgPersonalizado?: string;

  svgSeguro?: SafeHtml; // ✅ Aquí se guardará el SVG "limpio"

  constructor(private sanitizer: DomSanitizer) {}

  ngOnChanges() {
    if (this.svgPersonalizado) {
      // Limpia y autoriza el SVG para ser renderizado
      this.svgSeguro = this.sanitizer.bypassSecurityTrustHtml(this.svgPersonalizado);
    }
  }
}
