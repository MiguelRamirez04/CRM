import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { SecureAuthService, User } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  currentUser = this.authService.getCurrentUser();

  user$: Observable<User | null>;

  showNewPassword: boolean | undefined;
  showConfirmPassword: boolean | undefined;


  constructor() {
    this.user$ = this.authService.currentUser$;
  }

  editProfile(): void {
    alert('Funcionalidad de edición próximamente...');
  }

  changePassword(): void {
    alert('Funcionalidad de cambio de contraseña próximamente...');
  }

  getInitials(user: User | null): string {
    if (!user) return 'U';
    if (user.nombre && user.apellido) {
      return `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`.toUpperCase();
    }
    if (user.nombreCompleto) {
      const parts = user.nombreCompleto.split(' ');
      if (parts.length >= 2) {
        return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
      }
      return parts[0].substring(0, 2).toUpperCase();
    }
    if (user.name) {
      const parts = user.name.split(' ');
      return parts.map(p => p.charAt(0)).join('').substring(0, 2).toUpperCase();
    }
    return 'U';
  }

  // 📱 Campo teléfono
  isEditingPhone = false;
  phoneNumber = '6183316693';
  phoneError = '';

  toggleEditPhone() {
    this.isEditingPhone = !this.isEditingPhone;
    if (this.isEditingPhone) {
      this.phoneError = '';
    }
  }

  onPhoneInput(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/[^0-9]/g, '').slice(0, 10);
    input.value = value;
    this.phoneNumber = value;
    this.validatePhone(value);
  }

  validatePhone(value: string) {
    if (value.length === 0) {
      this.phoneError = 'El número de teléfono es requerido';
    } else if (value.length < 10) {
      this.phoneError = `Faltan ${10 - value.length} dígitos. El número debe tener 10 dígitos`;
    } else if (!/^[0-9]{10}$/.test(value)) {
      this.phoneError = 'Solo se permiten números. Ingrese 10 dígitos';
    } else {
      this.phoneError = '';
    }
  }

  updatePhone() {
    this.validatePhone(this.phoneNumber);
    if (this.phoneError === '') {
      console.log('Actualizando teléfono:', this.phoneNumber);
      this.isEditingPhone = false;
    } else {
      console.log('Error en validación:', this.phoneError);
    }
  }

  onBadgeHover(event: MouseEvent, isHovering: boolean): void {
    const element = event.target as HTMLElement;
    if (isHovering) {
      element.style.transform = 'scale(1.05)';
      element.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)';
    } else {
      element.style.transform = 'scale(1)';
      element.style.boxShadow = '0 1px 2px rgba(0, 0, 0, 0.05)';
    }
  }

  showPassword = false;
  showCurrentPassword = false;
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  passwordError = '';


  // Alternar visibilidad de contraseña actual
  toggleCurrentPasswordVisibility(): void {
    this.showCurrentPassword = !this.showCurrentPassword;
  }

  // Alternar visibilidad de nueva contraseña
  toggleNewPasswordVisibility(): void {
    this.showNewPassword = !this.showNewPassword;
  }

  // Alternar visibilidad de confirmación de contraseña
  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  // Método para actualizar contraseña
  updatePassword(): void {
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      alert('Por favor, completa todos los campos.');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      alert('Las contraseñas nuevas no coinciden.');
      return;
    }

    if (this.newPassword.length < 6) {
      alert('La nueva contraseña debe tener al menos 6 caracteres.');
      return;
    }
    alert('✅ Contraseña actualizada correctamente.');
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }


}
