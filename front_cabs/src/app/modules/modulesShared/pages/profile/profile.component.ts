import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { SecureAuthService, User } from '../../../../core/services/secure-auth.service';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
import { UiAvatarComponent } from '../../../../shared/atoms/avatar/avatar.component';
import { UitipografiaComponent, UiBotonComponent, BadgeComponent } from '../../../../shared/~exports/detail-view.index';
import { UiDividerComponent } from "../../../../shared/atoms/linea/linea.component";
import { UiInputComponent } from '../../../../shared/molecules/input/input.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    UiHeaderComponent,
    UiAvatarComponent,
    UitipografiaComponent,
    UiDividerComponent,
    UiInputComponent,
    UiBotonComponent,
  ],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  currentUser: User | null = null;
  user$: Observable<User | null>;

  showNewPassword = false;
  showConfirmPassword = false;

  constructor() {
    this.user$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Inicializar usuario actual
    this.currentUser = this.authService.getCurrentUser();
    console.log('Usuario cargado en Profile:', this.currentUser);
    
  }

  editProfile(): void {
    alert('Funcionalidad de edición próximamente...');
  }

  changePassword(): void {
    alert('Funcionalidad de cambio de contraseña próximamente...');
  }

  // 📱 Campo teléfono
  isEditingPhone = false;
  phoneNumber = '';
  phoneError = '';

  toggleEditPhone(): void {
    this.isEditingPhone = !this.isEditingPhone;
    if (this.isEditingPhone) {
      this.phoneError = '';
    }
  }

  onPhoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/[^0-9]/g, '').slice(0, 10);
    input.value = value;
    this.phoneNumber = value;
    this.validatePhone(value);
  }

  validatePhone(value: string): void {
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

  updatePhone(): void {
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

  toggleCurrentPasswordVisibility(): void {
    this.showCurrentPassword = !this.showCurrentPassword;
  }

  toggleNewPasswordVisibility(): void {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

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

  // 🔹 Getters para usar en inputs y tipografías
  get nombre(): string {
    return this.currentUser?.name ?? '';
  }

  get apellido(): string {
    return this.currentUser?.apellido ?? '';
  }

  get email(): string {
    return this.currentUser?.email ?? '';
  }

  get telefono(): string {
    return this.currentUser?.telefono?.toString() ?? '';
  }

  get rol(): string {
        return this.currentUser?.role ?? '';

  }

  get fullName(): string {
    if (!this.currentUser) return '';
    if (this.currentUser.nombreCompleto) return this.currentUser.nombreCompleto;
    return `${this.currentUser.nombre ?? ''} ${this.currentUser.apellido ?? ''}`.trim();
  }

}
