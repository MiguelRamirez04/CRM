import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ConfiguracionPaginacion {
  elementosPorPagina?: number;
  paginasVisiblesMaximas?: number;
  textoAnterior?: string;
  textoSiguiente?: string;
  textoMostrandoRegistros?: string;
  textoDeRegistros?: string;
  mostrarInfoRegistros?: boolean;
  mostrarBotonesPagina?: boolean;
}

@Component({
  selector: 'app-paginacion',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './paginacion.component.html',
  styleUrls: ['./paginacion.component.css']
})
export class PaginacionComponent implements OnChanges {
  // Entradas requeridas
  @Input() totalElementos: number = 0;
  @Input() paginaActual: number = 1;
  
  // Configuración opcional
  @Input() configuracion: ConfiguracionPaginacion = {};
  
  // Salidas de eventos
  @Output() cambioPagina = new EventEmitter<number>();
  @Output() cambioRangoPagina = new EventEmitter<{ inicio: number; fin: number }>();
  
  // Propiedades internas
  elementosPorPagina: number = 10;
  paginasVisiblesMaximas: number = 6;
  textoAnterior: string = 'Atrás';
  textoSiguiente: string = 'Siguiente';
  textoMostrandoRegistros: string = 'Visualizando';
  textoDeRegistros: string = 'de';
  mostrarInfoRegistros: boolean = true;
  mostrarBotonesPagina: boolean = true;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['configuracion'] && this.configuracion) {
      this.aplicarConfiguracion();
    }
    
    // Validar que la página actual no exceda el total de páginas
    if (this.paginaActual > this.totalPaginas && this.totalPaginas > 0) {
      this.irAPagina(this.totalPaginas);
    }
  }

  private aplicarConfiguracion(): void {
    this.elementosPorPagina = this.configuracion.elementosPorPagina || 10;
    this.paginasVisiblesMaximas = this.configuracion.paginasVisiblesMaximas || 6;
    this.textoAnterior = this.configuracion.textoAnterior || 'Atrás';
    this.textoSiguiente = this.configuracion.textoSiguiente || 'Siguiente';
    this.textoMostrandoRegistros = this.configuracion.textoMostrandoRegistros || 'Visualizando';
    this.textoDeRegistros = this.configuracion.textoDeRegistros || 'de';
    this.mostrarInfoRegistros = this.configuracion.mostrarInfoRegistros !== false;
    this.mostrarBotonesPagina = this.configuracion.mostrarBotonesPagina !== false;
  }

  get totalPaginas(): number {
    return Math.ceil(this.totalElementos / this.elementosPorPagina);
  }

  get elementoInicio(): number {
    return this.totalElementos === 0 ? 0 : (this.paginaActual - 1) * this.elementosPorPagina + 1;
  }

  get elementoFin(): number {
    return Math.min(this.paginaActual * this.elementosPorPagina, this.totalElementos);
  }

  get paginasVisibles(): (number | string)[] {
    const paginas: (number | string)[] = [];
    
    if (this.totalPaginas <= this.paginasVisiblesMaximas) {
      // Mostrar todas las páginas si son pocas
      for (let i = 1; i <= this.totalPaginas; i++) {
        paginas.push(i);
      }
    } else {
      // Lógica para mostrar páginas con puntos suspensivos
      if (this.paginaActual <= 3) {
        // Usuario está al inicio
        for (let i = 1; i <= 4; i++) {
          paginas.push(i);
        }
        paginas.push('...');
        paginas.push(this.totalPaginas);
      } else if (this.paginaActual >= this.totalPaginas - 2) {
        // Usuario está al final
        paginas.push(1);
        paginas.push('...');
        for (let i = this.totalPaginas - 3; i <= this.totalPaginas; i++) {
          paginas.push(i);
        }
      } else {
        // Usuario está en el medio
        paginas.push(1);
        paginas.push('...');
        paginas.push(this.paginaActual - 1);
        paginas.push(this.paginaActual);
        paginas.push(this.paginaActual + 1);
        paginas.push('...');
        paginas.push(this.totalPaginas);
      }
    }
    
    return paginas;
  }

  irAPagina(pagina: number | string): void {
    if (typeof pagina === 'string') return;
    
    if (pagina >= 1 && pagina <= this.totalPaginas && pagina !== this.paginaActual) {
      this.paginaActual = pagina;
      this.cambioPagina.emit(this.paginaActual);
      this.emitirRangoPagina();
    }
  }

  paginaAnterior(): void {
    if (this.paginaActual > 1) {
      this.irAPagina(this.paginaActual - 1);
    }
  }

  paginaSiguiente(): void {
    if (this.paginaActual < this.totalPaginas) {
      this.irAPagina(this.paginaActual + 1);
    }
  }

  private emitirRangoPagina(): void {
    this.cambioRangoPagina.emit({
      inicio: (this.paginaActual - 1) * this.elementosPorPagina,
      fin: this.paginaActual * this.elementosPorPagina
    });
  }

  esPuntosSuspensivos(pagina: number | string): boolean {
    return pagina === '...';
  }

  get habilitarBotonAnterior(): boolean {
    return this.paginaActual > 1;
  }

  get habilitarBotonSiguiente(): boolean {
    return this.paginaActual < this.totalPaginas;
  }
}