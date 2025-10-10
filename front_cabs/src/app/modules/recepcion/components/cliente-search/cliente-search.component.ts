import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClienteLegacy } from '../../../../core/models/orden-trabajo/orden-trabajo.interface';

@Component({
  selector: 'app-cliente-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './cliente-search.component.html',
  styleUrl: './cliente-search.component.css'
})
export class ClienteSearchComponent implements OnInit {
  @Input() placeholder = 'Buscar cliente por nombre o RFC...';
  @Input() disabled = false;
  @Input() resultados: ClienteLegacy[] = [];
  @Input() loading = false;

  @Output() clienteSeleccionado = new EventEmitter<ClienteLegacy>();
  @Output() busquedaCambiada = new EventEmitter<string>();

  terminoBusqueda = '';
  mostrarResultados = false;

  ngOnInit() {
    // Inicialización si es necesaria
  }

  onInputChange() {
    if (this.terminoBusqueda.length >= 2) {
      this.busquedaCambiada.emit(this.terminoBusqueda);
      this.mostrarResultados = true;
    } else {
      this.resultados = [];
      this.mostrarResultados = false;
    }
  }

  onClienteSeleccionado(cliente: ClienteLegacy) {
    this.clienteSeleccionado.emit(cliente);
    this.terminoBusqueda = cliente.nombre;
    this.mostrarResultados = false;
  }

  onFocus() {
    if (this.resultados.length > 0) {
      this.mostrarResultados = true;
    }
  }

  onBlur() {
    // Delay para permitir clic en resultados
    setTimeout(() => {
      this.mostrarResultados = false;
    }, 200);
  }

  limpiarBusqueda() {
    this.terminoBusqueda = '';
    this.resultados = [];
    this.mostrarResultados = false;
  }
}
