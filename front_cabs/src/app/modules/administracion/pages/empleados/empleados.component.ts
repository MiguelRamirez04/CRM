import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Gestión de Empleados</h2>
      <p>Administración de empleados del sistema.</p>
      <div class="alert alert-info">
        Gestión de empleados próximamente disponible.
      </div>
    </div>
  `
})
export class EmpleadosComponent {}