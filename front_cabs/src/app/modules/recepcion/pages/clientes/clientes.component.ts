import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Gestión de Clientes</h2>
      <p>Administración de clientes del sistema.</p>
      <div class="alert alert-info">
        Gestión de clientes próximamente disponible.
      </div>
    </div>
  `
})
export class ClientesComponent {}