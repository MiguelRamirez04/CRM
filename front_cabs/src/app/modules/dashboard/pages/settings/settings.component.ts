import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card shadow">
            <div class="card-header">
              <h4 class="mb-0">Configuración del Sistema</h4>
            </div>
            <div class="card-body">
              <div class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                Configuración del sistema próximamente disponible.
              </div>

              <div class="row">
                <div class="col-md-6">
                  <h6>Preferencias de Usuario</h6>
                  <div class="form-check mb-3">
                    <input class="form-check-input" type="checkbox" id="darkMode">
                    <label class="form-check-label" for="darkMode">
                      Modo oscuro
                    </label>
                  </div>
                  <div class="form-check mb-3">
                    <input class="form-check-input" type="checkbox" id="notifications">
                    <label class="form-check-label" for="notifications">
                      Notificaciones push
                    </label>
                  </div>
                </div>
                <div class="col-md-6">
                  <h6>Configuración Regional</h6>
                  <div class="mb-3">
                    <label class="form-label">Idioma</label>
                    <select class="form-select">
                      <option value="es">Español</option>
                      <option value="en">English</option>
                    </select>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Zona Horaria</label>
                    <select class="form-select">
                      <option value="America/Buenos_Aires">Argentina (GMT-3)</option>
                      <option value="Europe/Madrid">España (GMT+1)</option>
                    </select>
                  </div>
                </div>
              </div>

              <div class="mt-4">
                <button class="btn btn-primary me-2">
                  <i class="fas fa-save me-1"></i>
                  Guardar Cambios
                </button>
                <button class="btn btn-outline-secondary">
                  <i class="fas fa-undo me-1"></i>
                  Restaurar Valores
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class SettingsComponent {
  private router = inject(Router);
}