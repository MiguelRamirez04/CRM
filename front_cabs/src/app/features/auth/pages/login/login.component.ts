import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';
import { RolUsuario } from '../../../../core/enums/rol-usuario.enum';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50 flex">
      <!-- Panel izquierdo con branding -->
      <div class="hidden lg:flex w-1/2 bg-gradient-to-br from-[#667eea] to-[#764ba2] items-center justify-center p-12 text-white relative">
        <div class="text-center">
          <h1 class="text-5xl font-bold mb-4">Bienvenido a CRM CABS</h1>
          <p class="text-xl opacity-90 mb-8">Gestiona tu negocio de manera eficiente con nuestra plataforma integral.</p>
          <div class="space-y-4 text-left mx-auto max-w-md">
            <div class="flex items-center bg-white/20 p-4 rounded-lg">
              <i class="fas fa-check-circle text-2xl mr-4"></i>
              <span class="text-lg">Administración completa</span>
            </div>
            <div class="flex items-center bg-white/20 p-4 rounded-lg">
              <i class="fas fa-check-circle text-2xl mr-4"></i>
              <span class="text-lg">Recepción optimizada</span>
            </div>
            <div class="flex items-center bg-white/20 p-4 rounded-lg">
              <i class="fas fa-check-circle text-2xl mr-4"></i>
              <span class="text-lg">Soporte 24/7</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Panel derecho con formulario -->
      <div class="w-full lg:w-1/2 flex items-center justify-center p-6 sm:p-12">
        <div class="w-full max-w-md">
          <div class="text-center mb-8 lg:hidden">
            <h2 class="text-3xl font-bold text-[#667eea]">CRM CABS</h2>
          </div>
          
          <h3 class="text-3xl font-bold text-gray-800 mb-2 text-center">Iniciar Sesión</h3>
          <p class="text-gray-600 mb-8 text-center">Ingresa tus credenciales para acceder a tu cuenta.</p>

          <!-- Alerts -->
          <div *ngIf="errorMessage" class="bg-[#dc3545]/10 border-l-4 border-[#dc3545] text-[#dc3545] p-4 mb-6 rounded-md" role="alert">
            <div class="flex">
              <div class="py-1"><i class="fas fa-exclamation-triangle mr-3"></i></div>
              <div>
                <p class="font-bold">Error</p>
                <p class="text-sm">{{ errorMessage }}</p>
              </div>
              <button type="button" class="ml-auto -mx-1.5 -my-1.5" (click)="errorMessage = null">
                <i class="fas fa-times"></i>
              </button>
            </div>
          </div>

          <div *ngIf="successMessage" class="bg-[#28a745]/10 border-l-4 border-[#28a745] text-[#28a745] p-4 mb-6 rounded-md" role="alert">
            <p>{{ successMessage }}</p>
          </div>

          <!-- Formulario -->
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" novalidate class="space-y-6">
            <div>
              <label for="email" class="block text-sm font-medium text-gray-700">Correo Electrónico</label>
              <div class="mt-1 relative rounded-md shadow-sm">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <i class="fas fa-envelope text-gray-400"></i>
                </div>
                <input 
                  type="email" 
                  id="email" 
                  formControlName="email"
                  placeholder="usuario@ejemplo.com"
                  autocomplete="email"
                  class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-[#667eea] focus:border-[#667eea] sm:text-sm"
                  [ngClass]="{'border-[#dc3545]': loginForm.get('email')?.invalid && loginForm.get('email')?.touched}">
              </div>
              <div *ngIf="loginForm.get('email')?.invalid && loginForm.get('email')?.touched" class="mt-2 text-sm text-[#dc3545]">
                <span *ngIf="loginForm.get('email')?.errors?.['required']">El correo es requerido.</span>
                <span *ngIf="loginForm.get('email')?.errors?.['email']">Formato de correo inválido.</span>
              </div>
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700">Contraseña</label>
              <div class="mt-1 relative rounded-md shadow-sm">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <i class="fas fa-lock text-gray-400"></i>
                </div>
                <input 
                  [type]="showPassword ? 'text' : 'password'" 
                  id="password" 
                  formControlName="password"
                  placeholder="Tu contraseña"
                  autocomplete="current-password"
                  class="block w-full pl-10 pr-10 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-[#667eea] focus:border-[#667eea] sm:text-sm"
                  [ngClass]="{'border-[#dc3545]': loginForm.get('password')?.invalid && loginForm.get('password')?.touched}">
                <div class="absolute inset-y-0 right-0 pr-3 flex items-center">
                  <button 
                    type="button" 
                    (click)="togglePassword()"
                    class="text-gray-400 hover:text-gray-500">
                    <i [class]="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                  </button>
                </div>
              </div>
              <div *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched" class="mt-2 text-sm text-[#dc3545]">
                <span *ngIf="loginForm.get('password')?.errors?.['required']">La contraseña es requerida.</span>
                <span *ngIf="loginForm.get('password')?.errors?.['minlength']">Mínimo 6 caracteres.</span>
              </div>
            </div>

            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <input id="rememberMe" formControlName="rememberMe" type="checkbox" class="h-4 w-4 text-[#667eea] border-gray-300 rounded focus:ring-[#667eea]">
                <label for="rememberMe" class="ml-2 block text-sm text-gray-900">Recordarme</label>
              </div>
              <div class="text-sm">
                <a routerLink="/auth/forgot-password" class="font-medium text-[#667eea] hover:text-[#764ba2]">
                  ¿Olvidaste tu contraseña?
                </a>
              </div>
            </div>

            <div>
              <button 
                type="submit" 
                class="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-[#667eea] to-[#764ba2] hover:from-[#764ba2] hover:to-[#667eea] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#667eea] disabled:opacity-50 transition-all duration-300"
                [disabled]="loginForm.invalid || isLoading">
                <span *ngIf="isLoading" class="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
                   <svg class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                </span>
                <i *ngIf="!isLoading" class="fas fa-sign-in-alt mr-2"></i>
                {{ isLoading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
              </button>
            </div>
          </form>

          <div class="mt-6 relative">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-gray-300"></div>
            </div>
            <div class="relative flex justify-center text-sm">
              <span class="px-2 bg-gray-50 text-gray-500">¿No tienes cuenta?</span>
            </div>
          </div>

          <div class="mt-6">
            <a routerLink="/auth/register" class="w-full flex justify-center py-3 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary">
              Regístrate aquí
            </a>
          </div>
          
          <div class="mt-8 text-center">
            <p class="text-xs text-gray-500">
              <i class="fas fa-shield-alt text-success"></i>
              Conexión segura y protegida.
            </p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  returnUrl: string = '/dashboard';

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Obtener returnUrl de los query params
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    
    // Mostrar mensaje si viene de logout
    const message = this.route.snapshot.queryParams['message'];
    if (message) {
      this.successMessage = message;
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid && !this.isLoading) {
      this.isLoading = true;
      this.errorMessage = null;

      const loginData = {
        email: this.loginForm.get('email')?.value,
        password: this.loginForm.get('password')?.value
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.successMessage = 'Inicio de sesión exitoso';
          
          // Determinar URL de destino basado en el rol si no hay returnUrl específico
          let targetUrl = this.returnUrl;
          if (this.returnUrl === '/dashboard') {
            const userRole = response.user.role || response.user.rol;
            if (userRole === 'Recepcion' || userRole === RolUsuario.Recepcion) {
              targetUrl = '/recepcion/dashboard';
            }
          }
          
          // Usar el método del servicio para manejar redirección
          setTimeout(() => {
            this.authService.handleLoginSuccess(targetUrl);
          }, 1000);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Error al iniciar sesión';
          
          // Limpiar contraseña en caso de error
          this.loginForm.patchValue({ password: '' });
        }
      });
    } else {
      // Marcar todos los campos como touched para mostrar errores
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}