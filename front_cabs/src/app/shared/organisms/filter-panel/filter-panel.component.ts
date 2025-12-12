// =====================================================================================
// ORGANISM: Filter Panel - Panel lateral completo de filtros
// =====================================================================================

import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiBotonComponent } from '../../atoms/boton/boton.component';
import { StatusDotComponent } from '../../atoms/status-dot/status-dot.component';
import { FilterCheckboxGroupComponent, CheckboxOption } from '../../molecules/filter-checkbox-group/filter-checkbox-group.component';
import { FilterFieldComponent } from '../../molecules/filter-field/filter-field.component';
import { SelectOption } from '../../molecules/input/input.component';

// =====================================================================================
// INTERFACES
// =====================================================================================

export interface FilterCheckboxGroupConfig {
  id: string;
  titulo: string;
  opciones: CheckboxOption[];
}

export interface FilterFieldConfig {
  id: string;
  titulo: string;
  placeholder: string;
  tipo: 'date' | 'month' | 'year' | 'select' | 'text';
  opciones?: SelectOption[]; // Solo para tipo 'select'
  iconName?: string;
}

export interface FilterPanelConfig {
  titulo?: string;
  gruposCheckbox?: FilterCheckboxGroupConfig[];
  campos?: FilterFieldConfig[];
  mostrarBotonLimpiar?: boolean;
  textoBotonAplicar?: string;
  textoBotonLimpiar?: string;
  textoBotonCerrar?: string;
}

export interface FilterResult {
  checkboxes: Record<string, any[]>;
  campos: Record<string, any>;
}

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [
    CommonModule,
    UiBotonComponent,
    StatusDotComponent,
    FilterCheckboxGroupComponent,
    FilterFieldComponent
  ],
  templateUrl: './filter-panel.component.html',
  styleUrls: ['./filter-panel.component.css']
})
export class FilterPanelComponent implements OnInit {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================

  @Input() visible: boolean = false;
  @Input() configuracion: FilterPanelConfig = {};
  @Input() filtrosAplicados?: FilterResult;

  @Output() cerrar = new EventEmitter<void>();
  @Output() aplicarFiltros = new EventEmitter<FilterResult>();
  @Output() limpiarFiltros = new EventEmitter<void>();

  // =====================================================================================
  // PROPIEDADES
  // =====================================================================================

  // Estado temporal mientras el usuario selecciona
  estadoCheckboxes: Record<string, any[]> = {};
  estadoCampos: Record<string, any> = {};

  // Estado aplicado - los filtros que realmente están activos
  filtrosAplicadosActuales: FilterResult | null = null;

  config: Required<FilterPanelConfig> = {
    titulo: 'Filtros',
    gruposCheckbox: [],
    campos: [],
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

    // Restaurar filtros aplicados si existen
    if (this.filtrosAplicados && this.tieneFiltrosReales(this.filtrosAplicados)) {
      this.estadoCheckboxes = this.clonarProfundo(this.filtrosAplicados.checkboxes);
      this.estadoCampos = { ...this.filtrosAplicados.campos };
      this.filtrosAplicadosActuales = this.clonarProfundo(this.filtrosAplicados);
    }
  }

  // =====================================================================================
  // MÉTODOS PRINCIPALES
  // =====================================================================================

  private inicializarEstadosVacios(): void {
    // Inicializar checkboxes
    const checkboxesVacios: Record<string, any[]> = {};
    this.config.gruposCheckbox?.forEach(grupo => {
      checkboxesVacios[grupo.id] = [];
    });
    this.estadoCheckboxes = checkboxesVacios;

    // Inicializar campos
    const camposVacios: Record<string, any> = {};
    this.config.campos?.forEach(campo => {
      camposVacios[campo.id] = campo.tipo === 'select' ? null : '';
    });
    this.estadoCampos = camposVacios;
  }

  private inicializarEstados(): void {
    this.inicializarEstadosVacios();
    this.filtrosAplicadosActuales = null;
  }

  private clonarProfundo(obj: any): any {
    return JSON.parse(JSON.stringify(obj));
  }

  private tieneFiltrosReales(filtros: FilterResult): boolean {
    const hayCheckboxes = Object.values(filtros.checkboxes || {}).some(
      arr => Array.isArray(arr) && arr.length > 0
    );
    
    const hayCampos = Object.values(filtros.campos || {}).some(
      valor => valor !== null && valor !== '' && valor !== undefined
    );
    
    return hayCheckboxes || hayCampos;
  }

  // =====================================================================================
  // HANDLERS
  // =====================================================================================

  onCerrar(): void {
    this.cerrar.emit();
  }

  onAplicar(): void {
    const resultado: FilterResult = {
      checkboxes: this.clonarProfundo(this.estadoCheckboxes),
      campos: { ...this.estadoCampos }
    };

    this.filtrosAplicadosActuales = this.clonarProfundo(resultado);

    console.log('Filtros aplicados:', resultado);
    
    this.aplicarFiltros.emit(resultado);
    this.onCerrar();
  }

  onLimpiar(): void {
    this.inicializarEstados();

    console.log('Filtros limpiados');
    
    this.limpiarFiltros.emit();
  }

  onCheckboxGroupChange(grupoId: string, selectedValues: any[]): void {
    this.estadoCheckboxes[grupoId] = selectedValues;
    console.log(`Grupo ${grupoId}:`, selectedValues);
  }

  onFieldChange(campoId: string, value: any): void {
    this.estadoCampos[campoId] = value;
    console.log(`Campo ${campoId}:`, value);
  }

  getSelectedValues(grupoId: string): any[] {
    return this.estadoCheckboxes[grupoId] || [];
  }

  getFieldValue(campoId: string): any {
    return this.estadoCampos[campoId] || '';
  }

  // =====================================================================================
  // UTILIDADES
  // =====================================================================================

  hayFiltrosActivos(): boolean {
    if (!this.filtrosAplicadosActuales) {
      return false;
    }

    const hayCheckboxes = Object.values(this.filtrosAplicadosActuales.checkboxes || {}).some(
      arr => Array.isArray(arr) && arr.length > 0
    );
    
    const hayCampos = Object.values(this.filtrosAplicadosActuales.campos || {}).some(
      valor => valor !== null && valor !== '' && valor !== undefined
    );
    
    return hayCheckboxes || hayCampos;
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('panel-overlay')) {
      this.onCerrar();
    }
  }
}
