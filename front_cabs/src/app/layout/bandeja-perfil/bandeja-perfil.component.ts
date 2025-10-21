import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { SecureAuthService, User } from '../../../../src/app/core/services/secure-auth.service';

@Component({
  selector: 'app-bandeja-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bandeja-perfil.html',
  styleUrls: ['./bandeja-perfil.css']
})
export class BandejaPerfilComponent {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  user$: Observable<User | null> = this.authService.currentUser$;
  isCollapsed = false;

  getInitials(user: User | null): string {
    if (!user) return '';
    if (user.nombre && user.apellido) {
      return `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`.toUpperCase();
    }
    if (user.name) {
      const parts: string[] = user.name.split(' ');
      return parts.map((p: string) => p.charAt(0)).join('').toUpperCase();
    }
    return '';
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        console.log('Logout exitoso');
      },
      error: (error) => {
        console.error('Error durante logout:', error);
      }
    });
  }

  verPerfil(): void {
    this.router.navigate(['/dashboard/profile']);
  }

  abrirConfiguracion(): void {
    this.router.navigate(['/dashboard/settings']);
  }
}
