import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface ConfiguracionBuscador {
  placeholderBusqueda?: string;
  mostrarBotonFiltro?: boolean;
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
  // Configuración del componente
  @Input() configuracion: ConfiguracionBuscador = {
    placeholderBusqueda: 'Buscar...',
    mostrarBotonFiltro: true,
    tiempoEsperaBusqueda: 300
  };

  // Valor actual del campo de busqueda
  @Input() valorBusqueda: string = '';

  // Eventos de salida
  @Output() cambioBusqueda = new EventEmitter<string>();
  @Output() clicFiltro = new EventEmitter<void>();

  private temporizadorBusqueda: any;

  // Método que se ejecuta al escribir en el buscador
  alEscribir(valor: string): void {
    if (this.temporizadorBusqueda) {
      clearTimeout(this.temporizadorBusqueda);
    }

    this.temporizadorBusqueda = setTimeout(() => {
      this.cambioBusqueda.emit(valor);
    }, this.configuracion.tiempoEsperaBusqueda || 300);
  }

  // Clic en botón (Filtro)
  alClicFiltro(): void {
    this.clicFiltro.emit();
  }

  // Limpia el temporizador al destruir el componente
  ngOnDestroy(): void {
    if (this.temporizadorBusqueda) {
      clearTimeout(this.temporizadorBusqueda);
    }
  }
}