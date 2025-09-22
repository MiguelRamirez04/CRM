import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-recepcion-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Dashboard de Recepción</h2>
      <p>Panel principal del módulo de recepción.</p>
      <div class="alert alert-info">
        Funcionalidad de recepción próximamente disponible.
      </div>
    </div>
  `
})
export class RecepcionDashboardComponent {}