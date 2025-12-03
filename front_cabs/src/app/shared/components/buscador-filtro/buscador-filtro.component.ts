import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface ConfiguracionBuscador {
  placeholderBusqueda?: string;
  mostrarBotonFiltro?: boolean;
  mostrarBotonNuevo?: boolean;
  textoBotonNuevo?: string;
  tiempoEsperaBusqueda?: number;
}

@Component({
  selector: 'app-buscador-filtro',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './buscador-filtro.component.html',
  styleUrls: ['./buscador-filtro.component.css']
})
export class BuscadorFiltroComponent {
  @Input() configuracion: ConfiguracionBuscador = {
    placeholderBusqueda: 'Buscar...',
    mostrarBotonFiltro: true,
    mostrarBotonNuevo: true,
    textoBotonNuevo: 'Nuevo',
    tiempoEsperaBusqueda: 300
  };

  @Input() valorBusqueda: string = '';

  @Output() cambioBusqueda = new EventEmitter<string>();
  @Output() clicFiltro = new EventEmitter<void>();
  @Output() clicNuevo = new EventEmitter<void>();

  private temporizadorBusqueda: any;

  // SVG del ícono de agregar para el botón
  iconoAgregar = `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
         stroke-width="1.5" stroke="currentColor" class="w-5 h-5">
      <path stroke-linecap="round" stroke-linejoin="round"
        d="M12 9v6m3-3H9m12 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
    </svg>
  `;

  alEscribir(valor: string): void {
    if (this.temporizadorBusqueda) {
      clearTimeout(this.temporizadorBusqueda);
    }

    this.temporizadorBusqueda = setTimeout(() => {
      this.cambioBusqueda.emit(valor);
    }, this.configuracion.tiempoEsperaBusqueda || 300);
  }

  alClicFiltro(): void {
    this.clicFiltro.emit();
  }

  alClicNuevo(): void {
    this.clicNuevo.emit();
  }

  ngOnDestroy(): void {
    if (this.temporizadorBusqueda) {
      clearTimeout(this.temporizadorBusqueda);
    }
  }
}