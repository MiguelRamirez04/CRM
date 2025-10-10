import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdenTrabajo } from '../../../../core/models/orden-trabajo.interface';

@Component({
  selector: 'app-orden-list',
  imports: [CommonModule],
  templateUrl: './orden-list.component.html',
  styleUrl: './orden-list.component.css'
})
export class OrdenListComponent {
  @Input() ordenes: OrdenTrabajo[] = [];
  @Input() loading = false;

  @Output() verDetalles = new EventEmitter<number>();
  @Output() editar = new EventEmitter<number>();
  @Output() eliminar = new EventEmitter<number>();

  onVerDetalles(id: number) {
    this.verDetalles.emit(id);
  }

  onEditar(id: number) {
    this.editar.emit(id);
  }

  onEliminar(id: number) {
    this.eliminar.emit(id);
  }

  trackByOrdenId(index: number, orden: any): number {
    return orden.id;
  }
}
