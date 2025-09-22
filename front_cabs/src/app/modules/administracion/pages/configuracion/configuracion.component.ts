import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Configuración del Sistema</h2>
      <p>Configuración general del módulo de administración.</p>
      <div class="alert alert-info">
        Configuración próximamente disponible.
      </div>
    </div>
  `
})
export class ConfiguracionComponent {}