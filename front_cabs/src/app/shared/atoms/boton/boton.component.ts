import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-ui-boton',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './boton.component.html',
})
export class UiBotonComponent {
  // Propiedades de entrada básicas
  @Input() variante: string = 'primario';
  @Input() texto: string = 'Botón';
  @Input() estaCargando: boolean = false;
  @Input() estaDeshabilitado: boolean = false;
  @Input() tipo: 'button' | 'submit' | 'reset' = 'button';
  @Input() anchoCompleto: boolean = true;
  @Input() mostrarIcono: boolean = false;
  @Input() visualizarTexto: boolean = true;


  @Input() textoAlCargar?: string;
  @Input() clasesAdicionales?: string;

  // NUEVAS PROPIEDADES PARA PERSONALIZACIÓN DE COLORES
  @Input() colorBorde?: string;
  @Input() colorTexto?: string;
  @Input() colorIcono?: string;
  @Input() colorFondo?: string;
  @Input() colorHoverFondo?: string;
  @Input() colorHoverBorde?: string;
  @Input() anchoBorde?: string;

  // Evento de salida
  @Output() alClickear = new EventEmitter<Event>();
  iconoPersonalizado: any;

  constructor(private sanitizer: DomSanitizer) { }

  // Método para sanitizar el HTML del icono personalizado
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

  // Obtener estilos personalizados
  get estilosPersonalizados(): { [key: string]: string } {
    const estilos: { [key: string]: string } = {};

    if (this.colorBorde) {
      estilos['border-color'] = this.colorBorde;
    }
    if (this.colorTexto) {
      estilos['color'] = this.colorTexto;
    }
    if (this.colorFondo) {
      estilos['background'] = this.colorFondo;
      estilos['background-color'] = this.colorFondo;
    }
    if (this.anchoBorde) {
      estilos['border-width'] = this.anchoBorde;
    }

    return estilos;
  }

  // Obtener estilos para el icono
  get estilosIcono(): { [key: string]: string } {
    const estilos: { [key: string]: string } = {};

    if (this.colorIcono) {
      estilos['color'] = this.colorIcono;
      estilos['stroke'] = this.colorIcono;
    }

    return estilos;
  }

  // Obtener clases con hover personalizado
  get atributosHover(): string {
    if (this.colorHoverFondo || this.colorHoverBorde) {
      return 'hover-personalizado';
    }
    return '';
  }

  // Método para verificar si debe mostrar ícono predeterminado
  get debeMotrarIconoPredeterminadoVariante(): boolean {
    const variantesConIcono = ['ver', 'editar', 'eliminar', 'guardar', 'cancelar',
      'descargar', 'compartir', 'buscar', 'cerrar', 'info'];
    return variantesConIcono.includes(this.variante);
  }

  // Método para obtener las clases de Tailwind según la variante
  obtenerClasesTailwind(): string {
    // Clases base para todos los botones
    const clasesBase = `
      font-semibold py-3 px-6 rounded-lg

      transition-all duration-300
      focus:outline-none focus:ring-2 focus:ring-opacity-30
      disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none
      flex justify-center items-center gap-2
      cursor-pointer
    `;

    // Clases según el ancho
    const clasesAncho = this.anchoCompleto ? 'w-full' : '';

    // Si hay colores personalizados, usar clases neutras
    if (this.tieneColoresPersonalizados()) {
      const clasesVariante = `
        border-2 border-solid
        ${this.atributosHover}
      `;

      const todasLasClases = `
        ${clasesBase}
        ${clasesAncho}
        ${clasesVariante}
        ${this.clasesAdicionales || ''}
      `.replace(/\s+/g, ' ').trim();

      return todasLasClases;
    }

    // Clases según la variante (código original)
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
          bg-white text-blue-600
          border-2 border-blue-600
          hover:border-blue-700
          hover:bg-gray-50
          hover:shadow-xl
          hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-500/50
          rounded-lg px-6 py-3 font-semibold transition-all duration-300
        `;
        break;

      case 'terciario':
        clasesVariante = `
          text-blue-600 underline underline-offset-4 decoration-blue-600
        `;
        break;

      // BOTONES FUNCIONALES
      case 'ver':
        clasesVariante = `
          bg-gradient-to-r from-blue-500 to-green-500
          text-white
          hover:from-blue-600 hover:to-green-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-400/50
        `;
        break;

      case 'editar':
        clasesVariante = `
          bg-gradient-to-r from-yellow-500 to-yellow-600
          text-white
          hover:from-yellow-600 hover:to-yellow-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-yellow-400/50
        `;
        break;

      case 'eliminar':
        clasesVariante = `
          bg-gradient-to-r from-red-500 to-red-700
          text-white
          hover:from-red-600 hover:to-red-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-red-400/50
        `;
        break;

      case 'guardar':
        clasesVariante = `
          bg-gradient-to-r from-green-500 to-green-700
          text-white
          hover:from-green-600 hover:to-green-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-green-400/50
        `;
        break;

      case 'cancelar':
        clasesVariante = `
          bg-gradient-to-r from-gray-500 to-gray-600
          text-white
          hover:from-gray-600 hover:to-gray-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-gray-400/50
        `;
        break;

      case 'descargar':
        clasesVariante = `
          bg-gradient-to-r from-purple-500 to-purple-700
          text-white
          hover:from-purple-600 hover:to-purple-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-purple-400/50
        `;
        break;

      case 'compartir':
        clasesVariante = `
          bg-gradient-to-r from-teal-500 to-teal-600
          text-white
          hover:from-teal-600 hover:to-teal-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-teal-400/50
        `;
        break;

      case 'buscar':
        clasesVariante = `
          bg-gradient-to-r from-blue-600 to-slate-700
          text-white
          hover:from-blue-700 hover:to-slate-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-400/50
        `;
        break;

      case 'cerrar':
        clasesVariante = `
          bg-gradient-to-r from-slate-600 to-slate-700
          text-white
          hover:from-slate-700 hover:to-slate-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-slate-400/50
        `;
        break;

      case 'info':
        clasesVariante = `
          bg-gradient-to-r from-sky-500 to-blue-600
          text-white
          hover:from-sky-600 hover:to-blue-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-sky-400/50
        `;
        break;

      // VARIACIONES DE AZUL
      case 'azul-1':
        clasesVariante = `
          bg-gradient-to-r from-indigo-600 to-purple-600
          text-white
          hover:from-indigo-700 hover:to-purple-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-indigo-400/50
        `;
        break;

      case 'azul-2':
        clasesVariante = `
          bg-gradient-to-r from-blue-500 to-blue-700
          text-white
          hover:from-blue-600 hover:to-blue-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-400/50
        `;
        break;

      case 'azul-3':
        clasesVariante = `
          bg-gradient-to-r from-cyan-600 to-blue-600
          text-white
          hover:from-cyan-700 hover:to-blue-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-cyan-400/50
        `;
        break;

      case 'azul-4':
        clasesVariante = `
          bg-gradient-to-r from-blue-800 to-blue-900
          text-white
          hover:from-blue-900 hover:to-slate-900
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-600/50
        `;
        break;

      case 'azul-5':
        clasesVariante = `
          bg-gradient-to-r from-cyan-400 to-blue-500
          text-white
          hover:from-cyan-500 hover:to-blue-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-cyan-300/50
        `;
        break;

      case 'azul-6':
        clasesVariante = `
          bg-gradient-to-r from-sky-400 to-cyan-400
          text-white
          hover:from-sky-500 hover:to-cyan-500
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-sky-300/50
        `;
        break;

      case 'azul-7':
        clasesVariante = `
          bg-gradient-to-r from-emerald-400 to-cyan-400
          text-slate-800
          hover:from-emerald-500 hover:to-cyan-500
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-emerald-300/50
        `;
        break;

      case 'azul-8':
        clasesVariante = `
          bg-gradient-to-r from-indigo-500 to-pink-500
          text-white
          hover:from-indigo-600 hover:to-pink-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-indigo-400/50
        `;
        break;

      case 'azul-9':
        clasesVariante = `
          bg-gradient-to-r from-cyan-500 to-indigo-800
          text-white
          hover:from-cyan-600 hover:to-indigo-900
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-cyan-400/50
        `;
        break;

      case 'azul-10':
        clasesVariante = `
          bg-gradient-to-r from-cyan-200 to-pink-200
          text-slate-700
          hover:from-cyan-300 hover:to-pink-300
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-cyan-200/50
        `;
        break;

      // NUEVAS VARIACIONES DE AZUL DEL HTML
      case 'cielo-profundo':
        clasesVariante = `
          bg-gradient-to-bl from-sky-500 to-blue-600
          text-white
          hover:from-sky-600 hover:to-blue-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-sky-500/50
        `;
        break;

      case 'anil-suave':
        clasesVariante = `
          bg-gradient-to-r from-indigo-500 to-indigo-700
          text-white
          hover:from-indigo-600 hover:to-indigo-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-indigo-500/50
        `;
        break;

      case 'cian-vibrante':
        clasesVariante = `
          bg-gradient-to-tr from-cyan-400 to-blue-500
          text-white
          hover:from-cyan-500 hover:to-blue-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-cyan-400/50
        `;
        break;

      case 'mar-oscuro':
        clasesVariante = `
          bg-gradient-to-r from-blue-800 to-blue-950
          text-white
          hover:from-blue-900 hover:to-slate-950
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-800/50
        `;
        break;

      case 'aqua-degradado':
        clasesVariante = `
          bg-gradient-to-r from-teal-400 to-blue-500
          text-white
          hover:from-teal-500 hover:to-blue-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-teal-400/50
        `;
        break;

      case 'azul-frio':
        clasesVariante = `
          bg-gradient-to-br from-blue-400 to-indigo-500
          text-white
          hover:from-blue-500 hover:to-indigo-600
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-400/50
        `;
        break;

      case 'azul-electrico':
        clasesVariante = `
          bg-gradient-to-r from-sky-500 to-blue-700
          text-white
          hover:from-sky-600 hover:to-blue-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-sky-500/50
        `;
        break;

      case 'azul-metalico':
        clasesVariante = `
          bg-gradient-to-r from-slate-600 to-blue-700
          text-white
          hover:from-slate-700 hover:to-blue-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-slate-600/50
        `;
        break;

      case 'marino-uniforme':
        clasesVariante = `
          bg-blue-700
          text-white
          hover:bg-blue-800
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-700/50
        `;
        break;

      case 'fusion-morada':
        clasesVariante = `
          bg-gradient-to-r from-purple-600 to-blue-600
          text-white
          hover:from-purple-700 hover:to-blue-700
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-purple-600/50
        `;
        break;

      case 'borde-azul':
        clasesVariante = `
          bg-white
          text-blue-700
          border-2 border-blue-700
          hover:bg-blue-50
          hover:shadow-xl hover:scale-[1.02]
          active:scale-[0.98]
          focus:ring-blue-700/50
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

  // Verificar si tiene colores personalizados
  private tieneColoresPersonalizados(): boolean {
    return !!(
      this.colorBorde ||
      this.colorTexto ||
      this.colorIcono ||
      this.colorFondo ||
      this.colorHoverFondo ||
      this.colorHoverBorde
    );
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
