import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-ui-boton',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './boton.component.html'
})
export class UiBotonComponent {
  // Propiedades de entrada
  @Input() variante: 'primario' | 'secundario' | 'terciario' = 'primario';
  @Input() texto: string = 'Botón';
  @Input() estaCargando: boolean = false;
  @Input() estaDeshabilitado: boolean = false;
  @Input() tipo: 'button' | 'submit' | 'reset' = 'button';
  @Input() anchoCompleto: boolean = true;
  @Input() mostrarIcono: boolean = false;
  @Input() iconoPersonalizado?: string;
  @Input() textoAlCargar?: string;
  @Input() clasesAdicionales?: string;

  // Evento de salida
  @Output() alClickear = new EventEmitter<Event>();

  constructor(private sanitizer: DomSanitizer) {}

  // Método para sanitizar el HTML del icono
  get iconoSanitizado(): SafeHtml | null {
    if (this.iconoPersonalizado) {
      return this.sanitizer.bypassSecurityTrustHtml(this.iconoPersonalizado);
    }
    return null;
  }

  // Método para manejar el click
  manejarClick(evento: Event): void {
    if (!this.estaDeshabilitado && !this.estaCargando) {
      this.alClickear.emit(evento);
    }
  }

  // Método para obtener las clases de Tailwind según la variante
  obtenerClasesTailwind(): string {
    // Clases base para todos los botones
    const clasesBase = `
      font-semibold py-3 px-6 rounded-lg shadow-md
      transition-all duration-300
      focus:outline-none focus:ring-3
      disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none
      flex justify-center items-center gap-2
    `;

    // Clases según el ancho
    const clasesAncho = this.anchoCompleto ? 'w-full' : '';

    // Clases según la variante
    let clasesVariante = '';
    switch (this.variante) {
      case 'primario':
        clasesVariante = `
          bg-gradient-to-r from-blue-600 to-blue-800 
          text-white
          hover:from-blue-700 hover:to-blue-900 
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-500/50
        `;
        break;
      
      case 'secundario':
        clasesVariante = `
          bg-[#35D9FD] 
          text-[#34428F]
          hover:bg-[#2dc4e8] 
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-[#35D9FD]/50
        `;
        break;
      
      case 'terciario':
        clasesVariante = `
          bg-white 
          text-[#333333]
          border-2 border-gray-300
          hover:bg-gray-50 hover:border-gray-400
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-gray-300/50
        `;
        break;
    }

    // Combinar todas las clases y agregar las adicionales si existen
    const todasLasClases = `
      ${clasesBase} 
      ${clasesAncho} 
      ${clasesVariante} 
      ${this.clasesAdicionales || ''}
    `.replace(/\s+/g, ' ').trim();

    return todasLasClases;
  }

  // Método para determinar si el botón debe estar deshabilitado
  estaInactivo(): boolean {
    return this.estaDeshabilitado || this.estaCargando;
  }

  // Método para obtener el texto a mostrar
  obtenerTexto(): string {
    if (this.estaCargando && this.textoAlCargar) {
      return this.textoAlCargar;
    }
    return this.texto;
  }
}