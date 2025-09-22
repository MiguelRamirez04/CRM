import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-soporte-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Dashboard de Soporte</h2>
      <p>Panel principal del módulo de soporte.</p>
      <div class="alert alert-info">
        Funcionalidad de soporte próximamente disponible.
      </div>
    </div>
  `
})
export class SoporteDashboardComponent {}