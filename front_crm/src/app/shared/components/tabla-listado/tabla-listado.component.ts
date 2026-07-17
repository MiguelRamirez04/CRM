import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ConfiguracionColumna<T = any> {
  encabezado: string;
  campo?: keyof T;
  plantilla?: TemplateRef<any>;
  ancho?: string;
  alineacion?: 'left' | 'center' | 'right';
}

export interface AccionTabla<T = any> {
  etiqueta: string;
  icono?: TemplateRef<any>;
  clase?: string;
  accion: (item: T) => void;
  mostrar?: (item: T) => boolean;
}

@Component({
  selector: 'app-tabla-listado',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tabla-listado.component.html',
  styleUrls: ['./tabla-listado.component.css']
})
export class TablaListadoComponent<T = any> {
  @Input() datos: T[] = [];
  @Input() columnas: ConfiguracionColumna<T>[] = [];
  @Input() acciones: AccionTabla<T>[] = [];
  @Input() cargando: boolean = false;
  @Input() mensajeSinDatos: string = 'No se encontraron registros';
  @Input() mensajeCargando: string = 'Cargando datos...';
  @Input() mostrarIndice: boolean = false;
  @Input() clasePersonalizada: string = '';

  /* Termino de búsqueda */
  @Input() terminoBusqueda: string = '';

  @Output() filaClick = new EventEmitter<T>();
  @Output() filaDobleClick = new EventEmitter<T>();

  // Get para filtrar

  get datosFiltrados(): T[] {
    if (!this.terminoBusqueda) return this.datos;

    const lower = this.terminoBusqueda.toLowerCase();
    return this.datos.filter(item =>
      this.columnas.some(col => {
        if (!col.campo) return false;
        const valor = item[col.campo];
        return valor && valor.toString().toLowerCase().includes(lower);
      })
    );
  }  

  obtenerValorCelda(item: T, campo: keyof T): any {
    return item[campo];
  }

  alManejarFilaClick(item: T): void {
    this.filaClick.emit(item);
  }

  alManejarFilaDobleClick(item: T): void {
    this.filaDobleClick.emit(item);
  }

  ejecutarAccion(accion: AccionTabla<T>, item: T, evento: Event): void {
    evento.stopPropagation();
    accion.accion(item);
  }

  mostrarAccion(accion: AccionTabla<T>, item: T): boolean {
    return accion.mostrar ? accion.mostrar(item) : true;
  }

  obtenerAlineacion(columna: ConfiguracionColumna<T>): string {
    return columna.alineacion || 'center';
  }
}