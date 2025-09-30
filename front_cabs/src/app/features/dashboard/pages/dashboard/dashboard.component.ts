import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, NgbModule],
  template: `
    <div class="container-fluid">
      <!-- Header del Dashboard -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <h1 class="h3 mb-0">Dashboard</h1>
              <p class="text-muted">Bienvenido al sistema CRM</p>
            </div>
            <div class="d-flex gap-2">
              <button class="btn btn-outline-primary" (click)="refreshData()">
                <i class="fas fa-sync-alt"></i> Actualizar
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Cards de Estadísticas -->
      <div class="row mb-4">
        <div class="col-xl-3 col-md-6 mb-4">
          <div class="card border-left-primary shadow h-100 py-2">
            <div class="card-body">
              <div class="row no-gutters align-items-center">
                <div class="col mr-2">
                  <div class="text-xs font-weight-bold text-primary text-uppercase mb-1">
                    Total Usuarios
                  </div>
                  <div class="h5 mb-0 font-weight-bold text-gray-800">{{ stats.totalUsers }}</div>
                </div>
                <div class="col-auto">
                  <i class="fas fa-users fa-2x text-primary"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-3 col-md-6 mb-4">
          <div class="card border-left-success shadow h-100 py-2">
            <div class="card-body">
              <div class="row no-gutters align-items-center">
                <div class="col mr-2">
                  <div class="text-xs font-weight-bold text-success text-uppercase mb-1">
                    Tickets Activos
                  </div>
                  <div class="h5 mb-0 font-weight-bold text-gray-800">{{ stats.activeTickets }}</div>
                </div>
                <div class="col-auto">
                  <i class="fas fa-ticket-alt fa-2x text-success"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-3 col-md-6 mb-4">
          <div class="card border-left-info shadow h-100 py-2">
            <div class="card-body">
              <div class="row no-gutters align-items-center">
                <div class="col mr-2">
                  <div class="text-xs font-weight-bold text-info text-uppercase mb-1">
                    Pedidos Pendientes
                  </div>
                  <div class="h5 mb-0 font-weight-bold text-gray-800">{{ stats.pendingOrders }}</div>
                </div>
                <div class="col-auto">
                  <i class="fas fa-shopping-cart fa-2x text-info"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-3 col-md-6 mb-4">
          <div class="card border-left-warning shadow h-100 py-2">
            <div class="card-body">
              <div class="row no-gutters align-items-center">
                <div class="col mr-2">
                  <div class="text-xs font-weight-bold text-warning text-uppercase mb-1">
                    Alertas del Sistema
                  </div>
                  <div class="h5 mb-0 font-weight-bold text-gray-800">{{ stats.systemAlerts }}</div>
                </div>
                <div class="col-auto">
                  <i class="fas fa-exclamation-triangle fa-2x text-warning"></i>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Panel de Acceso Rápido -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card shadow">
            <div class="card-header py-3">
              <h6 class="m-0 font-weight-bold text-primary">Acceso Rápido</h6>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-3 mb-3">
                  <a routerLink="/administracion" class="btn btn-outline-primary btn-block p-3">
                    <i class="fas fa-users fa-2x mb-2"></i><br>
                    Administración
                  </a>
                </div>
                <div class="col-md-3 mb-3">
                  <a routerLink="/recepcion" class="btn btn-outline-success btn-block p-3">
                    <i class="fas fa-inbox fa-2x mb-2"></i><br>
                    Recepción
                  </a>
                </div>
                <div class="col-md-3 mb-3">
                  <a routerLink="/soporte" class="btn btn-outline-info btn-block p-3">
                    <i class="fas fa-headset fa-2x mb-2"></i><br>
                    Soporte
                  </a>
                </div>
                <div class="col-md-3 mb-3">
                  <a routerLink="/dashboard/profile" class="btn btn-outline-secondary btn-block p-3">
                    <i class="fas fa-user fa-2x mb-2"></i><br>
                    Mi Perfil
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Información del Usuario Actual -->
      <div class="row">
        <div class="col-12">
          <div class="card shadow">
            <div class="card-header py-3">
              <h6 class="m-0 font-weight-bold text-primary">Información de Sesión</h6>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-6">
                  <p><strong>Usuario:</strong> {{ currentUser?.name }}</p>
                  <p><strong>Email:</strong> {{ currentUser?.email }}</p>
                  <p><strong>Rol:</strong> {{ currentUser?.role }}</p>
                </div>
                <div class="col-md-6">
                  <p><strong>Permisos:</strong></p>
                  <div class="mb-2">
                    <span *ngFor="let permission of currentUser?.permissions" class="badge badge-primary me-1">
                      {{ permission }}
                    </span>
                  </div>
                  <button class="btn btn-sm btn-outline-danger" (click)="logout()">
                    <i class="fas fa-sign-out-alt me-1"></i>
                    Cerrar Sesión
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .border-left-primary {
      border-left: 0.25rem solid #4e73df !important;
    }
    .border-left-success {
      border-left: 0.25rem solid #1cc88a !important;
    }
    .border-left-info {
      border-left: 0.25rem solid #36b9cc !important;
    }
    .border-left-warning {
      border-left: 0.25rem solid #f6c23e !important;
    }
    .text-primary {
      color: #5a5c69 !important;
    }
    .text-gray-800 {
      color: #5a5c69 !important;
    }
    .btn-block {
      display: block;
      width: 100%;
    }
    .btn-outline-primary:hover, .btn-outline-success:hover, .btn-outline-info:hover, .btn-outline-secondary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }
  `]
})
export class DashboardComponent {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  currentUser = this.authService.getCurrentUser();

  // Datos de ejemplo - en producción vendrían de la API
  stats = {
    totalUsers: 1250,
    activeTickets: 23,
    pendingOrders: 15,
    systemAlerts: 3
  };

  refreshData(): void {
    // TODO: Implementar refresh de datos desde API
    console.log('Refrescando datos del dashboard...');
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.router.navigate(['/auth/login']);
      }
    });
  }
}