import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, catchError, of, finalize } from 'rxjs';
import { SecureAuthService, User } from '../../../../core/services/secure-auth.service';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
import { UiAvatarComponent } from '../../../../shared/atoms/avatar/avatar.component';
import { UitipografiaComponent, UiBotonComponent } from '../../../../shared/~exports/detail-view.index';
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

  // Estados de la UI
  isLoading = true;
  errorMessage = '';

  // Variables para contraseñas
  showNewPassword = false;
  showConfirmPassword = false;
  showCurrentPassword = false;
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  // Variables para teléfono
  isEditingPhone = false;
  phoneNumber = '';
  phoneError = '';

  constructor() {
    this.user$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.loadUserData();
    
    // Suscribirse a cambios en el usuario
    this.user$.subscribe(user => {
      if (user && user !== this.currentUser) {
        this.currentUser = user;
      }
    });
  }

  /**
   * Carga los datos del usuario usando el método getMe()
   */
  loadUserData(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.getMe().pipe(
      catchError(error => {
        console.error('Error al cargar usuario:', error);
        
        if (error.status === 401 || error.status === 403) {
          this.errorMessage = 'No estás autenticado. Redirigiendo al login...';
          setTimeout(() => this.router.navigate(['/auth/login']), 2000);
        } else if (error.status === 0) {
          this.errorMessage = 'No se pudo conectar con el servidor. Verifica tu conexión.';
        } else {
          this.errorMessage = `Error del servidor: ${error.status}`;
        }
        
        return of(null);
      }),
      finalize(() => {
        this.isLoading = false;
      })
    ).subscribe({
      next: (userData) => {
        if (userData) {
          this.currentUser = userData;
          
          // Inicializar teléfono si existe
          if (this.currentUser.telefono) {
            this.phoneNumber = this.currentUser.telefono.toString();
          }
          
          // Actualizar el BehaviorSubject del servicio
          this.authService['currentUserSubject'].next(this.currentUser);
          
          // Guardar en localStorage
          localStorage.setItem('current_user', JSON.stringify(this.currentUser));
        } else {
          this.errorMessage = 'No se pudieron obtener los datos del usuario';
          
          // Intentar obtener el usuario del caché
          const cachedUser = localStorage.getItem('current_user');
          if (cachedUser) {
            try {
              this.currentUser = JSON.parse(cachedUser);
            } catch (e) {
              console.error('Error parseando usuario en caché:', e);
            }
          }
        }
      }
    });
  }

  // 🔹 Métodos para teléfono
  toggleEditPhone(): void {
    this.isEditingPhone = !this.isEditingPhone;
    if (this.isEditingPhone) {
      this.phoneError = '';
      if (this.currentUser?.telefono) {
        this.phoneNumber = this.currentUser.telefono.toString();
      } else if (this.telefono) {
        this.phoneNumber = this.telefono;
      }
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
      if (this.currentUser) {
        this.currentUser = {
          ...this.currentUser,
          telefono: parseInt(this.phoneNumber)
        };
        this.authService['currentUserSubject'].next(this.currentUser);
      }
      this.isEditingPhone = false;
      alert('Teléfono actualizado correctamente (cambios locales)');
    } else {
      alert(`Error: ${this.phoneError}`);
    }
  }

  // Métodos para contraseña
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

    alert('✅ Contraseña actualizada correctamente (simulación).');
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }

  // 🔹 Método para recargar datos
  reloadData(): void {
    this.loadUserData();
  }

  // 🔹 Getters optimizados para usar los datos de getMe()
  get nombre(): string {
    return this.currentUser?.nombre || '';
  }

  get apellido(): string {
    return this.currentUser?.apellido || '';
  }

  get email(): string {
    return this.currentUser?.email || '';
  }

  get telefono(): string {
    const tel = this.currentUser?.telefono;
    if (tel === null || tel === undefined) return '';
    return tel.toString();
  }

  get rol(): string {
    const rol = this.currentUser?.rol;
    
    if (!rol && rol !== 0) return 'Usuario';
    
    let rolTexto = '';
    
    // Primero obtener el texto del rol
    if (typeof rol === 'string') {
      const rolesMap: { [key: string]: string } = {
        'ADMINISTRACION': 'Administración',
        'SOPORTE': 'Soporte',
        'RECEPCION': 'Recepción',
        'ADMIN': 'Administrador',
        'USER': 'Usuario',
        'USUARIO': 'Usuario'
      };
      
      rolTexto = rolesMap[rol.toUpperCase()] || rol;
    } else if (typeof rol === 'number') {
      const roles: { [key: number]: string } = {
        1: 'Administrador',
        2: 'Usuario',
        3: 'Gestor',
        4: 'Supervisor'
      };
      rolTexto = roles[rol] || `Rol ${rol}`;
    } else if (rol === null || rol === undefined) {
      return 'Usuario';
    } else {
      rolTexto = String(rol);
    }
    
    // Aplicar formato: primera letra mayúscula, resto minúsculas
    if (rolTexto && rolTexto.length > 0) {
      return rolTexto.charAt(0).toUpperCase() + rolTexto.slice(1).toLowerCase();
    }
    
    return 'Usuario';
  }

  get transmisionHabilitada(): string {
    const transmision = (this.currentUser as any)?.transmisionHabilitada;
    
    if (transmision === null || transmision === undefined || transmision === '') {
      return 'No especificado';
    }
    
    return String(transmision);
  }

  // En tu componente, agrega este getter:
  get estaActivo(): boolean {
    const activo = (this.currentUser as any)?.activo;
    
    if (typeof activo === 'boolean') {
      return activo;
    }
    
    if (typeof activo === 'string') {
      const lowerActivo = activo.toLowerCase();
      return lowerActivo === 'true' || lowerActivo === 'activo' || lowerActivo === '1';
    }
    
    if (typeof activo === 'number') {
      return activo === 1 || activo > 0;
    }
    
    return false; // Por defecto
  }

  get fullName(): string {
    if (!this.currentUser) return '';
    
    if (this.currentUser.nombreCompleto) {
      return this.currentUser.nombreCompleto;
    }
    
    const nombre = this.nombre;
    const apellido = this.apellido;
    
    if (nombre && apellido) {
      return `${nombre} ${apellido}`;
    }
    
    return nombre || apellido || '';
  }
}