import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';
import { HeaderComponent } from '../../../../shared/components/header/header';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, HeaderComponent],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class DashboardComponent {
  private authService = inject(SecureAuthService);

  currentUser = this.authService.getCurrentUser();

  get nombreUsuario(): string {
    if (this.currentUser?.nombreCompleto) {
      return this.currentUser.nombreCompleto;
    }
    if (this.currentUser?.nombre && this.currentUser?.apellido) {
      return `${this.currentUser.nombre} ${this.currentUser.apellido}`;
    }
    if (this.currentUser?.name) {
      return this.currentUser.name;
    }
    return 'Usuario';
  }

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
