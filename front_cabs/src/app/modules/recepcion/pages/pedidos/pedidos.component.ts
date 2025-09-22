import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pedidos',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Gestión de Pedidos</h2>
      <p>Administración de pedidos del sistema.</p>
      <div class="alert alert-info">
        Gestión de pedidos próximamente disponible.
      </div>
    </div>
  `
})
export class PedidosComponent {}