import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Gestión de Roles</h2>
      <p>Administración de roles y permisos del sistema.</p>
      <div class="alert alert-info">
        Gestión de roles próximamente disponible.
      </div>
    </div>
  `
})
export class RolesComponent {}