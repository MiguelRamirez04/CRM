import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterLink, NgbModule],
  template: `
    <div class="container-fluid vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="row justify-content-center w-100">
        <div class="col-lg-6 col-md-8">
          <div class="card border-0 shadow-lg">
            <div class="card-body text-center p-5">
              <!-- Icono de acceso denegado -->
              <div class="mb-4">
                <i class="fas fa-shield-alt text-danger" style="font-size: 4rem;"></i>
              </div>

              <!-- Título principal -->
              <h1 class="display-4 text-danger mb-3">Acceso Denegado</h1>
              
              <!-- Mensaje explicativo -->
              <p class="lead text-muted mb-4">
                No tienes los permisos necesarios para acceder a esta página.
              </p>

              <!-- Información adicional -->
              <div class="alert alert-warning" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Permisos requeridos:</strong> Esta sección requiere permisos específicos que tu cuenta actual no posee.
              </div>

              <!-- Acciones disponibles -->
              <div class="d-grid gap-2 d-md-block">
                <a routerLink="/dashboard" class="btn btn-primary btn-lg me-md-2">
                  <i class="fas fa-home me-2"></i>
                  Ir al Dashboard
                </a>
                
                <button class="btn btn-outline-secondary btn-lg" (click)="goBack()">
                  <i class="fas fa-arrow-left me-2"></i>
                  Volver Atrás
                </button>
              </div>

              <!-- Información de contacto -->
              <div class="mt-5 pt-4 border-top">
                <h6 class="text-muted mb-3">¿Necesitas acceso a esta función?</h6>
                <p class="small text-muted mb-3">
                  Contacta a tu administrador para solicitar los permisos necesarios.
                </p>
                
                <div class="d-flex justify-content-center gap-3">
                  <button class="btn btn-sm btn-outline-info" (click)="contactAdmin()">
                    <i class="fas fa-envelope me-1"></i>
                    Contactar Admin
                  </button>
                  
                  <button class="btn btn-sm btn-outline-success" (click)="requestAccess()">
                    <i class="fas fa-key me-1"></i>
                    Solicitar Acceso
                  </button>
                </div>
              </div>

              <!-- Información del usuario actual -->
              <div class="mt-4 p-3 bg-light rounded">
                <small class="text-muted">
                  <i class="fas fa-user me-2"></i>
                  Usuario actual: <strong>{{ getCurrentUserEmail() }}</strong>
                  <br>
                  <i class="fas fa-shield-alt me-2"></i>
                  Rol: <strong>{{ getCurrentUserRole() }}</strong>
                </small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      transition: transform 0.3s ease;
    }
    
    .card:hover {
      transform: translateY(-2px);
    }
    
    .btn {
      transition: all 0.3s ease;
    }
    
    .btn:hover {
      transform: translateY(-1px);
    }
    
    .fas {
      transition: transform 0.3s ease;
    }
    
    .btn:hover .fas {
      transform: scale(1.1);
    }
    
    .bg-light {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%) !important;
    }
  `]
})
export class UnauthorizedComponent {
  
  goBack(): void {
    window.history.back();
  }

  contactAdmin(): void {
    // TODO: Implementar modal o navegación a página de contacto
    alert('Funcionalidad de contacto próximamente...');
  }

  requestAccess(): void {
    // TODO: Implementar formulario de solicitud de acceso
    alert('Formulario de solicitud próximamente...');
  }

  getCurrentUserEmail(): string {
    // TODO: Obtener del servicio de autenticación
    return 'usuario@ejemplo.com';
  }

  getCurrentUserRole(): string {
    // TODO: Obtener del servicio de autenticación
    return 'Usuario';
  }
}