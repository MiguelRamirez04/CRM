import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="container-fluid vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="row justify-content-center w-100">
        <div class="col-lg-6 col-md-8">
          <div class="card border-0 shadow-lg">
            <div class="card-body text-center p-5">
              <h4 class="mb-4">Restablecer Contraseña</h4>
              <p class="text-muted mb-4">
                Ingresa tu nueva contraseña.
              </p>

              <div *ngIf="successMessage" class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle me-2"></i>
                {{ successMessage }}
                <button type="button" class="btn-close" (click)="successMessage = null"></button>
              </div>

              <div *ngIf="errorMessage" class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                {{ errorMessage }}
                <button type="button" class="btn-close" (click)="errorMessage = null"></button>
              </div>

              <form [formGroup]="resetForm" (ngSubmit)="onSubmit()" novalidate>
                <div class="mb-3">
                  <label for="password" class="form-label">Nueva Contraseña</label>
                  <input 
                    type="password" 
                    class="form-control"
                    [class.is-invalid]="resetForm.get('password')?.invalid && resetForm.get('password')?.touched"
                    id="password" 
                    formControlName="password"
                    placeholder="Tu nueva contraseña">
                  <div *ngIf="resetForm.get('password')?.invalid && resetForm.get('password')?.touched" class="invalid-feedback">
                    <small *ngIf="resetForm.get('password')?.errors?.['required']">
                      La contraseña es requerida
                    </small>
                    <small *ngIf="resetForm.get('password')?.errors?.['minlength']">
                      Mínimo 6 caracteres
                    </small>
                  </div>
                </div>

                <div class="mb-3">
                  <label for="confirmPassword" class="form-label">Confirmar Nueva Contraseña</label>
                  <input 
                    type="password" 
                    class="form-control"
                    [class.is-invalid]="resetForm.get('confirmPassword')?.invalid && resetForm.get('confirmPassword')?.touched"
                    id="confirmPassword" 
                    formControlName="confirmPassword"
                    placeholder="Confirma tu nueva contraseña">
                  <div *ngIf="resetForm.get('confirmPassword')?.invalid && resetForm.get('confirmPassword')?.touched" class="invalid-feedback">
                    Las contraseñas no coinciden
                  </div>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primary w-100 mb-3"
                  [disabled]="resetForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  {{ isLoading ? 'Restableciendo...' : 'Restablecer Contraseña' }}
                </button>
              </form>

              <div class="text-center">
                <a routerLink="/auth/login" class="text-decoration-none">
                  ← Volver al inicio de sesión
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
export class ResetPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  resetForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  token: string = '';

  constructor() {
    this.resetForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    // Obtener token de la URL
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || '';
    });
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
    if (this.resetForm.valid && !this.isLoading && this.token) {
      this.isLoading = true;
      this.errorMessage = null;
      this.successMessage = null;

      const newPassword = this.resetForm.get('password')?.value;

      this.authService.resetPassword(this.token, newPassword).subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Contraseña restablecida exitosamente. Redirigiendo al login...';
          
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 2000);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Error al restablecer la contraseña';
        }
      });
    } else {
      if (!this.token) {
        this.errorMessage = 'Token de restablecimiento inválido o expirado';
      }
      Object.keys(this.resetForm.controls).forEach(key => {
        this.resetForm.get(key)?.markAsTouched();
      });
    }
  }
}