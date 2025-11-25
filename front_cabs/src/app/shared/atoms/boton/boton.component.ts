import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiIconComponent } from '../icono/icono.component';

@Component({
  selector: 'app-ui-boton',
  standalone: true,
  imports: [CommonModule, UiIconComponent],
  templateUrl: './boton.component.html'
})
export class UiBotonComponent {
  @Input() variante: 'primario' | 'secundario' | 'terciario' = 'primario';
  @Input() texto: string = 'Botón';
  @Input() estaCargando: boolean = false;
  @Input() estaDeshabilitado: boolean = false;
  @Input() tipo: 'button' | 'submit' | 'reset' = 'button';
  @Input() anchoCompleto: boolean = true;

  // Íconos
  @Input() nombreIcono: string = '';
  @Input() mostrarIcono: boolean = false;
  @Input() visualizarTexto: boolean = true;

  @Input() textoAlCargar?: string;
  @Input() clasesAdicionales?: string;

  @Output() alClickear = new EventEmitter<Event>();

  manejarClick(evento: Event): void {
    if (!this.estaDeshabilitado && !this.estaCargando) {
      this.alClickear.emit(evento);
    }
  }

  obtenerClasesTailwind(): string {
    const base = `
      font-semibold py-3 px-6 rounded-lg shadow-md
      transition-all duration-300
      focus:outline-none focus:ring-3
      disabled:opacity-50 disabled:cursor-not-allowed
      flex justify-center items-center gap-2
    `;

    const ancho = this.anchoCompleto ? 'w-full' : '';

    let variante = '';
    switch (this.variante) {
      case 'primario':
        variante = `
          bg-gradient-to-r from-blue-600 to-blue-800 text-white
          hover:from-blue-700 hover:to-blue-900 
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-500/50
        `;
        break;

      case 'secundario':
        variante = `
          bg-[#35D9FD] text-[#34428F]
          hover:bg-[#2dc4e8]
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-[#35D9FD]/50
        `;
        break;

      case 'terciario':
        variante = `
          bg-white text-[#333]
          border-2 border-gray-300
          hover:bg-gray-50 hover:border-gray-400
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-gray-300/50
        `;
        break;
    }

    return `
      ${base}
      ${ancho}
      ${variante}
      ${this.clasesAdicionales || ''}
    `.replace(/\s+/g, ' ').trim();
  }

  estaInactivo(): boolean {
    return this.estaDeshabilitado || this.estaCargando;
  }

  obtenerTexto(): string {
    return this.estaCargando && this.textoAlCargar ? this.textoAlCargar : this.texto;
  }
}
