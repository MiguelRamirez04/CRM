import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgbModule, RouterLink],
  template: `
    <div class="container-fluid vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="row justify-content-center w-100">
        <div class="col-lg-6 col-md-8">
          <div class="card border-0 shadow-lg">
            <div class="card-body text-center p-5">
              <h4 class="mb-4">Crear Cuenta</h4>

              <div *ngIf="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                {{ errorMessage }}
                <button type="button" class="btn-close" (click)="errorMessage = null"></button>
              </div>

              <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" novalidate>
                <div class="mb-3">
                  <label for="name" class="form-label">Nombre Completo</label>
                  <input 
                    type="text" 
                    class="form-control"
                    [class.is-invalid]="registerForm.get('name')?.invalid && registerForm.get('name')?.touched"
                    id="name" 
                    formControlName="name"
                    placeholder="Tu nombre completo">
                  <div *ngIf="registerForm.get('name')?.invalid && registerForm.get('name')?.touched" class="invalid-feedback">
                    El nombre es requerido
                  </div>
                </div>

                <div class="mb-3">
                  <label for="email" class="form-label">Correo Electrónico</label>
                  <input 
                    type="email" 
                    class="form-control"
                    [class.is-invalid]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched"
                    id="email" 
                    formControlName="email"
                    placeholder="usuario@ejemplo.com">
                  <div *ngIf="registerForm.get('email')?.invalid && registerForm.get('email')?.touched" class="invalid-feedback">
                    <small *ngIf="registerForm.get('email')?.errors?.['required']">
                      El correo es requerido
                    </small>
                    <small *ngIf="registerForm.get('email')?.errors?.['email']">
                      Formato de correo inválido
                    </small>
                  </div>
                </div>

                <div class="mb-3">
                  <label for="password" class="form-label">Contraseña</label>
                  <input 
                    type="password" 
                    class="form-control"
                    [class.is-invalid]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched"
                    id="password" 
                    formControlName="password"
                    placeholder="Tu contraseña">
                  <div *ngIf="registerForm.get('password')?.invalid && registerForm.get('password')?.touched" class="invalid-feedback">
                    <small *ngIf="registerForm.get('password')?.errors?.['required']">
                      La contraseña es requerida
                    </small>
                    <small *ngIf="registerForm.get('password')?.errors?.['minlength']">
                      Mínimo 6 caracteres
                    </small>
                  </div>
                </div>

                <div class="mb-3">
                  <label for="confirmPassword" class="form-label">Confirmar Contraseña</label>
                  <input 
                    type="password" 
                    class="form-control"
                    [class.is-invalid]="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched"
                    id="confirmPassword" 
                    formControlName="confirmPassword"
                    placeholder="Confirma tu contraseña">
                  <div *ngIf="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched" class="invalid-feedback">
                    Las contraseñas no coinciden
                  </div>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primary w-100 mb-3"
                  [disabled]="registerForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  {{ isLoading ? 'Creando cuenta...' : 'Crear Cuenta' }}
                </button>
              </form>

              <div class="text-center">
                <span class="text-muted">¿Ya tienes cuenta?</span>
                <a routerLink="/auth/login" class="text-decoration-none ms-1">
                  Inicia sesión aquí
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .bg-light {
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%) !important;
    }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  registerForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor() {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid && !this.isLoading) {
      this.isLoading = true;
      this.errorMessage = null;

      const registerData = {
        name: this.registerForm.get('name')?.value,
        email: this.registerForm.get('email')?.value,
        password: this.registerForm.get('password')?.value,
        confirmPassword: this.registerForm.get('confirmPassword')?.value
      };

      this.authService.register(registerData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Error al crear cuenta';
        }
      });
    } else {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
    }
  }
}