import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
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