import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdenTrabajo } from '../../../../core/models/orden-trabajo.interface';

@Component({
  selector: 'app-orden-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orden-list.component.html',
  styleUrl: './orden-list.component.css'
})
export class OrdenListComponent {
  @Input() ordenes: OrdenTrabajo[] = [];
  @Input() loading = false;

  @Output() verDetalles = new EventEmitter<number>();
  @Output() editar = new EventEmitter<number>();
  @Output() crearEjecucion = new EventEmitter<OrdenTrabajo>();

  onVerDetalles(id: number) {
    this.verDetalles.emit(id);
  }

  onEditar(id: number) {
    this.editar.emit(id);
  }

  onCrearEjecucion(orden: OrdenTrabajo) {
    this.crearEjecucion.emit(orden);
  }

  trackByOrdenId(index: number, orden: any): number {
    return orden.id;
  }
}
