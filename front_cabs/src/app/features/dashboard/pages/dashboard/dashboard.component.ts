import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ],
  template: `
    <div class=" flex min-h-screen bg-gray-50">


      <!-- Contenido principal -->
      <main class="flex-1 p-6">
        <h1 class="text-xl font-semibold text-gray-800">Bienvenido, {{ currentUser?.name }}</h1>
        <!-- Aquí puedes renderizar tus stats o widgets -->
      </main>
    </div>
  `
})
export class DashboardComponent {
  private authService = inject(SecureAuthService);


  currentUser = this.authService.getCurrentUser();


  refreshData(): void {
    console.log('Refrescando datos del dashboard...');
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => console.log('Logout exitoso'),
      error: (error) => console.error('Error durante logout:', error)
    });
  }
}
