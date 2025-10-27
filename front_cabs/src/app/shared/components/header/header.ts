import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SecureAuthService } from '../../../core/services/secure-auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class HeaderComponent {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

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

  confirmarCerrarSesion(): void {
    if (confirm('¿Seguro de cerrar sesión?')) {
      this.cerrarSesion();
    }
  }

  private cerrarSesion(): void {
    // Limpia el token, cookies y datos de usuario
    this.authService.logout().subscribe({
      next: () => {
        console.log('Sesión cerrada exitosamente');
        this.router.navigate(['/auth/login'], { replaceUrl: true });
      },
      error: (error) => {
        console.error('Error al cerrar sesión:', error);
        // Aún en caso de error, forzar logout local
        this.authService.forceLogout();
      }
    });
  }
}
