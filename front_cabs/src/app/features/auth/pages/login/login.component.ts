import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgbModule, RouterLink],
  template: `
    <div class="container-fluid vh-100">
      <div class="row h-100">
        <!-- Panel izquierdo con imagen/branding -->
        <div class="col-lg-6 d-none d-lg-block bg-primary bg-gradient position-relative">
          <div class="d-flex flex-column justify-content-center h-100 text-white px-5">
            <h2 class="display-4 fw-bold mb-4">Bienvenido a CRM CABS</h2>
            <p class="lead">Gestiona tu negocio de manera eficiente con nuestra plataforma integral.</p>
            <ul class="list-unstyled mt-4">
              <li class="mb-2"><i class="fas fa-check-circle me-2"></i>Administración completa</li>
              <li class="mb-2"><i class="fas fa-check-circle me-2"></i>Recepción optimizada</li>
              <li class="mb-2"><i class="fas fa-check-circle me-2"></i>Soporte 24/7</li>
            </ul>
          </div>
        </div>

        <!-- Panel derecho con formulario -->
        <div class="col-lg-6">
          <div class="d-flex flex-column justify-content-center h-100 px-4 px-lg-5">
            <div class="w-100" style="max-width: 400px; margin: 0 auto;">
              <!-- Logo móvil -->
              <div class="text-center mb-4 d-lg-none">
                <h3 class="text-primary">CRM Sistema</h3>
              </div>

              <h4 class="mb-4">Iniciar Sesión</h4>
              
              <!-- Alerts -->
              <div *ngIf="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                {{ errorMessage }}
                <button type="button" class="btn-close" (click)="errorMessage = null"></button>
              </div>

              <div *ngIf="successMessage" class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle me-2"></i>
                {{ successMessage }}
                <button type="button" class="btn-close" (click)="successMessage = null"></button>
              </div>

              <!-- Formulario -->
              <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" novalidate>
                <div class="mb-3">
                  <label for="email" class="form-label">Correo Electrónico</label>
                  <div class="input-group">
                    <span class="input-group-text"><i class="fas fa-envelope"></i></span>
                    <input 
                      type="email" 
                      class="form-control"
                      [class.is-invalid]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched"
                      id="email" 
                      formControlName="email"
                      placeholder="usuario@ejemplo.com"
                      autocomplete="email">
                  </div>
                  <div *ngIf="loginForm.get('email')?.invalid && loginForm.get('email')?.touched" class="invalid-feedback">
                    <small *ngIf="loginForm.get('email')?.errors?.['required']">
                      El correo es requerido
                    </small>
                    <small *ngIf="loginForm.get('email')?.errors?.['email']">
                      Formato de correo inválido
                    </small>
                  </div>
                </div>

                <div class="mb-3">
                  <label for="password" class="form-label">Contraseña</label>
                  <div class="input-group">
                    <span class="input-group-text"><i class="fas fa-lock"></i></span>
                    <input 
                      [type]="showPassword ? 'text' : 'password'" 
                      class="form-control"
                      [class.is-invalid]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched"
                      id="password" 
                      formControlName="password"
                      placeholder="Tu contraseña"
                      autocomplete="current-password">
                    <button 
                      type="button" 
                      class="btn btn-outline-secondary"
                      (click)="togglePassword()">
                      <i [class]="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                    </button>
                  </div>
                  <div *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched" class="invalid-feedback">
                    <small *ngIf="loginForm.get('password')?.errors?.['required']">
                      La contraseña es requerida
                    </small>
                    <small *ngIf="loginForm.get('password')?.errors?.['minlength']">
                      Mínimo 6 caracteres
                    </small>
                  </div>
                </div>

                <div class="mb-3 form-check">
                  <input type="checkbox" class="form-check-input" id="rememberMe" formControlName="rememberMe">
                  <label class="form-check-label" for="rememberMe">
                    Recordarme
                  </label>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primary w-100 mb-3"
                  [disabled]="loginForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  <i *ngIf="!isLoading" class="fas fa-sign-in-alt me-2"></i>
                  {{ isLoading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
                </button>
              </form>

              <!-- Enlaces adicionales -->
              <div class="text-center">
                <a routerLink="/auth/forgot-password" class="text-decoration-none">
                  ¿Olvidaste tu contraseña?
                </a>
              </div>

              <hr class="my-4">

              <div class="text-center">
                <span class="text-muted">¿No tienes cuenta?</span>
                <a routerLink="/auth/register" class="text-decoration-none ms-1">
                  Regístrate aquí
                </a>
              </div>

              <!-- Información de seguridad -->
              <div class="mt-4 p-3 bg-light rounded">
                <small class="text-muted">
                  <i class="fas fa-shield-alt me-2 text-success"></i>
                  Conexión segura con cookies HttpOnly y protección CSRF
                </small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .bg-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    }
    
    .input-group-text {
      background-color: #f8f9fa;
      border-right: none;
    }
    
    .form-control {
      border-left: none;
    }
    
    .form-control:focus {
      box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
      border-color: #86b7fe;
    }
    
    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border: none;
      padding: 12px;
      font-weight: 500;
    }
    
    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }
    
    @media (max-width: 991.98px) {
      .container-fluid {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      }
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
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
          
          // Redireccionar después de un breve delay
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
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