import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header del Dashboard -->
      <div class="bg-white shadow-sm">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div class="flex justify-between items-center">
            <div>
              <h1 class="text-3xl font-bold text-gray-900">Dashboard</h1>
              <p class="text-gray-600 mt-1">Bienvenido al sistema CRM</p>
            </div>
            <div class="flex items-center space-x-4">
              <button class="inline-flex items-center px-4 py-2 border border-blue-300 text-sm font-medium rounded-md text-blue-700 bg-white hover:bg-blue-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200" (click)="refreshData()">
                <i class="fas fa-sync-alt mr-2"></i>
                Actualizar
              </button>
              <button class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors duration-200" (click)="logout()">
                <i class="fas fa-sign-out-alt mr-2"></i>
                Cerrar Sesión
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- Cards de Estadísticas -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div class="bg-white rounded-lg shadow-md border-l-4 border-blue-500 p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-bold text-blue-500 uppercase tracking-wide mb-2">Total Usuarios</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.totalUsers }}</p>
              </div>
              <div class="bg-blue-100 rounded-full p-3">
                <i class="fas fa-users text-2xl text-blue-500"></i>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-md border-l-4 border-green-500 p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-bold text-green-500 uppercase tracking-wide mb-2">Tickets Activos</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.activeTickets }}</p>
              </div>
              <div class="bg-green-100 rounded-full p-3">
                <i class="fas fa-ticket-alt text-2xl text-green-500"></i>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-md border-l-4 border-cyan-500 p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-bold text-cyan-500 uppercase tracking-wide mb-2">Pedidos Pendientes</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.pendingOrders }}</p>
              </div>
              <div class="bg-cyan-100 rounded-full p-3">
                <i class="fas fa-shopping-cart text-2xl text-cyan-500"></i>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-md border-l-4 border-yellow-500 p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-xs font-bold text-yellow-500 uppercase tracking-wide mb-2">Alertas del Sistema</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.systemAlerts }}</p>
              </div>
              <div class="bg-yellow-100 rounded-full p-3">
                <i class="fas fa-exclamation-triangle text-2xl text-yellow-500"></i>
              </div>
            </div>
          </div>
          </div>
        </div>

        <!-- Panel de Acceso Rápido -->
        <div class="bg-white rounded-lg shadow-md mb-8">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-semibold text-gray-900">Acceso Rápido</h3>
          </div>
          <div class="p-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <a routerLink="/administracion" class="flex flex-col items-center justify-center p-6 border-2 border-blue-200 rounded-lg text-blue-600 hover:bg-blue-50 hover:border-blue-300 transition-all duration-200 transform hover:scale-105">
                <i class="fas fa-users text-3xl mb-3"></i>
                <span class="font-medium">Administración</span>
              </a>
              <a routerLink="/recepcion" class="flex flex-col items-center justify-center p-6 border-2 border-green-200 rounded-lg text-green-600 hover:bg-green-50 hover:border-green-300 transition-all duration-200 transform hover:scale-105">
                <i class="fas fa-inbox text-3xl mb-3"></i>
                <span class="font-medium">Recepción</span>
              </a>
              <a routerLink="/soporte" class="flex flex-col items-center justify-center p-6 border-2 border-cyan-200 rounded-lg text-cyan-600 hover:bg-cyan-50 hover:border-cyan-300 transition-all duration-200 transform hover:scale-105">
                <i class="fas fa-headset text-3xl mb-3"></i>
                <span class="font-medium">Soporte</span>
              </a>
              <a routerLink="/dashboard/profile" class="flex flex-col items-center justify-center p-6 border-2 border-gray-200 rounded-lg text-gray-600 hover:bg-gray-50 hover:border-gray-300 transition-all duration-200 transform hover:scale-105">
                <i class="fas fa-user text-3xl mb-3"></i>
                <span class="font-medium">Mi Perfil</span>
              </a>
            </div>
          </div>
        </div>

        <!-- Información del Usuario Actual -->
        <div class="bg-white rounded-lg shadow-md">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-semibold text-gray-900">Información de Sesión</h3>
          </div>
          <div class="p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="space-y-3">
                <p class="text-sm text-gray-600"><span class="font-medium text-gray-900">Usuario:</span> {{ currentUser?.name }}</p>
                <p class="text-sm text-gray-600"><span class="font-medium text-gray-900">Email:</span> {{ currentUser?.email }}</p>
                <p class="text-sm text-gray-600"><span class="font-medium text-gray-900">Rol:</span> {{ currentUser?.role }}</p>
              </div>
              <div class="space-y-3">
                <div>
                  <p class="text-sm font-medium text-gray-900 mb-2">Permisos:</p>
                  <div class="flex flex-wrap gap-2 mb-4">
                    <span *ngFor="let permission of currentUser?.permissions" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {{ permission }}
                    </span>
                  </div>
                </div>
                <button class="inline-flex items-center px-4 py-2 border border-red-300 rounded-md shadow-sm text-sm font-medium text-red-700 bg-white hover:bg-red-50 hover:border-red-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors duration-200" (click)="logout()">
                  <i class="fas fa-sign-out-alt mr-2"></i>
                  Cerrar Sesión
                </button>
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