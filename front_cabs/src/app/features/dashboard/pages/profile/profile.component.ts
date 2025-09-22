import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <div class="card shadow">
            <div class="card-header">
              <h4 class="mb-0">Mi Perfil</h4>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-4 text-center mb-4">
                  <div class="avatar-circle mx-auto mb-3">
                    <i class="fas fa-user fa-3x text-primary"></i>
                  </div>
                  <h5>{{ currentUser?.name }}</h5>
                  <p class="text-muted">{{ currentUser?.email }}</p>
                  <span class="badge bg-primary">{{ currentUser?.role }}</span>
                </div>
                <div class="col-md-8">
                  <h6>Información del Perfil</h6>
                  <div class="row">
                    <div class="col-sm-6 mb-3">
                      <label class="form-label fw-bold">Nombre:</label>
                      <p>{{ currentUser?.name }}</p>
                    </div>
                    <div class="col-sm-6 mb-3">
                      <label class="form-label fw-bold">Email:</label>
                      <p>{{ currentUser?.email }}</p>
                    </div>
                    <div class="col-sm-6 mb-3">
                      <label class="form-label fw-bold">Rol:</label>
                      <p>{{ currentUser?.role }}</p>
                    </div>
                    <div class="col-sm-6 mb-3">
                      <label class="form-label fw-bold">Estado:</label>
                      <span class="badge bg-success">Activo</span>
                    </div>
                  </div>

                  <div class="mt-4">
                    <h6>Permisos Asignados</h6>
                    <div class="mb-3">
                      <span *ngFor="let permission of currentUser?.permissions" class="badge bg-secondary me-2 mb-2">
                        {{ permission }}
                      </span>
                    </div>
                  </div>

                  <div class="mt-4">
                    <button class="btn btn-primary me-2" (click)="editProfile()">
                      <i class="fas fa-edit me-1"></i>
                      Editar Perfil
                    </button>
                    <button class="btn btn-outline-primary" (click)="changePassword()">
                      <i class="fas fa-key me-1"></i>
                      Cambiar Contraseña
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .avatar-circle {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      background-color: #f8f9fa;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 3px solid #e9ecef;
    }
  `]
})
export class ProfileComponent {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  currentUser = this.authService.getCurrentUser();

  editProfile(): void {
    // TODO: Implementar edición de perfil
    alert('Funcionalidad de edición próximamente...');
  }

  changePassword(): void {
    // TODO: Implementar cambio de contraseña
    alert('Funcionalidad de cambio de contraseña próximamente...');
  }
}