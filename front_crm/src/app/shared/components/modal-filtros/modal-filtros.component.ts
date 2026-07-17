// =====================================================================================
// COMPONENTE MODAL DE FILTROS REUTILIZABLE
// =====================================================================================

import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBotonComponent } from '../../atoms/boton/boton.component';
import { StatusDotComponent } from '../../atoms/status-dot/status-dot.component';

// =====================================================================================
// INTERFACES
// =====================================================================================

export interface GrupoFiltroCheckbox {
  id: string;
  titulo: string;
  opciones: OpcionCheckbox[];
}

export interface OpcionCheckbox {
  valor: any;
  etiqueta: string;
  descripcion?: string;
  seleccionado?: boolean;
}

export interface FiltroFecha {
  id: string;
  titulo: string;
  placeholder: string;
  tipo: 'date' | 'month' | 'year';
}

export interface FiltroSelect {
  id: string;
  titulo: string;
  placeholder: string;
  opciones: { valor: any; etiqueta: string }[];
}

export interface ConfiguracionModalFiltros {
  titulo?: string;
  gruposCheckbox?: GrupoFiltroCheckbox[];
  filtrosFecha?: FiltroFecha[];
  filtrosSelect?: FiltroSelect[];
  mostrarBotonLimpiar?: boolean;
  textoBotonAplicar?: string;
  textoBotonLimpiar?: string;
  textoBotonCerrar?: string;
}

export interface ResultadoFiltros {
  checkboxes: Record<string, any[]>;
  fechas: Record<string, string>;
  selects: Record<string, any>;
}

@Component({
  selector: 'app-modal-filtros',
  standalone: true,
  imports: [CommonModule, FormsModule, UiBotonComponent, StatusDotComponent],
  templateUrl: './modal-filtros.component.html',
  styleUrls: ['./modal-filtros.component.css']
})
export class ModalFiltrosComponent implements OnInit {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================

  @Input() visible: boolean = false;
  @Input() configuracion: ConfiguracionModalFiltros = {};
  @Input() filtrosAplicados?: ResultadoFiltros;

  @Output() cerrar = new EventEmitter<void>();
  @Output() aplicarFiltros = new EventEmitter<ResultadoFiltros>();
  @Output() limpiarFiltros = new EventEmitter<void>();

  // =====================================================================================
  // PROPIEDADES
  // =====================================================================================

  // Estado temporal mientras el usuario selecciona
  estadoCheckboxes: Record<string, any[]> = {};
  estadoFechas: Record<string, string> = {};
  estadoSelects: Record<string, any> = {};

  // Estado aplicado - los filtros que realmente están activos
  filtrosAplicadosActuales: ResultadoFiltros | null = null;

  config: Required<ConfiguracionModalFiltros> = {
    titulo: 'Filtros',
    gruposCheckbox: [],
    filtrosFecha: [],
    filtrosSelect: [],
    mostrarBotonLimpiar: true,
    textoBotonAplicar: 'Aplicar',
    textoBotonLimpiar: 'Limpiar',
    textoBotonCerrar: 'Cerrar'
  };

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    this.config = {
      ...this.config,
      ...this.configuracion
    };

    this.inicializarEstadosVacios();

    // Solo restaurar si hay filtros realmente aplicados desde el padre
    if (this.filtrosAplicados && this.tieneFiltrosReales(this.filtrosAplicados)) {
      this.estadoCheckboxes = this.clonarProfundo(this.filtrosAplicados.checkboxes);
      this.estadoFechas = { ...this.filtrosAplicados.fechas };
      this.estadoSelects = { ...this.filtrosAplicados.selects };
      this.filtrosAplicadosActuales = this.clonarProfundo(this.filtrosAplicados);
    }
  }

  // =====================================================================================
  // MÉTODOS PRINCIPALES
  // =====================================================================================

  /**
   * Inicializar estados vacíos
   */
  private inicializarEstadosVacios(): void {
    const checkboxesVacios: Record<string, any[]> = {};
    this.config.gruposCheckbox?.forEach(grupo => {
      checkboxesVacios[grupo.id] = [];
    });
    this.estadoCheckboxes = checkboxesVacios;

    const fechasVacias: Record<string, string> = {};
    this.config.filtrosFecha?.forEach(filtro => {
      fechasVacias[filtro.id] = '';
    });
    this.estadoFechas = fechasVacias;

    const selectsVacios: Record<string, any> = {};
    this.config.filtrosSelect?.forEach(filtro => {
      selectsVacios[filtro.id] = null;
    });
    this.estadoSelects = selectsVacios;
  }

  /**
   * Inicializar estados (usado en limpiar)
   */
  private inicializarEstados(): void {
    this.inicializarEstadosVacios();

    this.config.gruposCheckbox?.forEach(grupo => {
      grupo.opciones.forEach(opcion => {
        opcion.seleccionado = false;
      });
    });

    this.filtrosAplicadosActuales = null;
  }

  /**
   * Clonar profundamente un objeto
   */
  private clonarProfundo(obj: any): any {
    return JSON.parse(JSON.stringify(obj));
  }

  /**
   * Verificar si un objeto ResultadoFiltros tiene filtros reales
   */
  private tieneFiltrosReales(filtros: ResultadoFiltros): boolean {
    const hayCheckboxes = Object.values(filtros.checkboxes || {}).some(
      arr => Array.isArray(arr) && arr.length > 0
    );
    
    const hayFechas = Object.values(filtros.fechas || {}).some(
      fecha => fecha !== '' && fecha !== null && fecha !== undefined
    );
    
    const haySelects = Object.values(filtros.selects || {}).some(
      valor => valor !== null && valor !== '' && valor !== undefined
    );
    
    return hayCheckboxes || hayFechas || haySelects;
  }

  onCerrar(): void {
    this.cerrar.emit();
  }

  onAplicar(): void {
    const resultado: ResultadoFiltros = {
      checkboxes: this.clonarProfundo(this.estadoCheckboxes),
      fechas: { ...this.estadoFechas },
      selects: { ...this.estadoSelects }
    };

    this.filtrosAplicadosActuales = this.clonarProfundo(resultado);

    console.log('Filtros aplicados:', resultado);
    console.log('Estado aplicado actualizado:', this.filtrosAplicadosActuales);
    
    this.aplicarFiltros.emit(resultado);
    this.onCerrar();
  }

  onLimpiar(): void {
    this.inicializarEstados();

    console.log('Filtros limpiados');
    console.log('Estado aplicado reseteado:', this.filtrosAplicadosActuales);
    
    this.limpiarFiltros.emit();
  }

  // =====================================================================================
  // HANDLERS
  // =====================================================================================

  onCheckboxChange(grupoId: string, opcion: OpcionCheckbox, event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    const checked = checkbox.checked;

    if (!this.estadoCheckboxes[grupoId]) {
      this.estadoCheckboxes[grupoId] = [];
    }

    if (checked) {
      if (!this.estadoCheckboxes[grupoId].includes(opcion.valor)) {
        this.estadoCheckboxes[grupoId].push(opcion.valor);
      }
    } else {
      this.estadoCheckboxes[grupoId] = this.estadoCheckboxes[grupoId].filter(
        v => v !== opcion.valor
      );
    }

    opcion.seleccionado = checked;
    console.log(`Checkbox ${grupoId}:`, this.estadoCheckboxes[grupoId]);
  }

  isCheckboxChecked(grupoId: string, valor: any): boolean {
    return this.estadoCheckboxes[grupoId]?.includes(valor) || false;
  }

  onFechaChange(filtroId: string, event: Event): void {
    const input = event.target as HTMLInputElement;
    this.estadoFechas[filtroId] = input.value;
    console.log(`Fecha ${filtroId}:`, input.value);
  }

  onSelectChange(filtroId: string, event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.estadoSelects[filtroId] = select.value;
    console.log(`Select ${filtroId}:`, select.value);
  }

  // =====================================================================================
  // UTILIDADES
  // =====================================================================================

  /**
   * Verifica los filtros realmente aplicados
   */
  hayFiltrosActivos(): boolean {
    if (!this.filtrosAplicadosActuales) {
      console.log('No hay filtros aplicados (null)');
      return false;
    }

    const hayCheckboxes = Object.values(this.filtrosAplicadosActuales.checkboxes || {}).some(
      arr => Array.isArray(arr) && arr.length > 0
    );
    
    const hayFechas = Object.values(this.filtrosAplicadosActuales.fechas || {}).some(
      fecha => fecha !== '' && fecha !== null && fecha !== undefined
    );
    
    const haySelects = Object.values(this.filtrosAplicadosActuales.selects || {}).some(
      valor => valor !== null && valor !== '' && valor !== undefined
    );
    
    const resultado = hayCheckboxes || hayFechas || haySelects;
    
    console.log('Verificando filtros activos:', {
      hayCheckboxes,
      hayFechas,
      haySelects,
      resultado,
      filtrosAplicadosActuales: this.filtrosAplicadosActuales
    });
    
    return resultado;
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onCerrar();
    }
  }
}