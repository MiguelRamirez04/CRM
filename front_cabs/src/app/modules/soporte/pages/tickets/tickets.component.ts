import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Gestión de Tickets</h2>
      <p>Administración de tickets de soporte.</p>
      <div class="alert alert-info">
        Gestión de tickets próximamente disponible.
      </div>
    </div>
  `
})
export class TicketsComponent {}