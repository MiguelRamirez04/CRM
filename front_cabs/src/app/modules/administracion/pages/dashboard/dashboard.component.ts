import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-administracion-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Dashboard de Administración</h2>
      <p>Panel principal del módulo de administración.</p>
      <div class="alert alert-info">
        Funcionalidad de administración próximamente disponible.
      </div>
    </div>
  `
})
export class AdministracionDashboardComponent {}