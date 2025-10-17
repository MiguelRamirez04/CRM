import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,  RouterLink],
  template: `
    <div class="container-fluid vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="row justify-content-center w-100">
        <div class="col-lg-6 col-md-8">
          <div class="card border-0 shadow-lg">
            <div class="card-body text-center p-5">
              <h4 class="mb-4">Recuperar Contraseña</h4>
              <p class="text-muted mb-4">
                Ingresa tu correo electrónico y te enviaremos un enlace para restablecer tu contraseña.
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

              <form [formGroup]="forgotForm" (ngSubmit)="onSubmit()" novalidate>
                <div class="mb-3">
                  <label for="email" class="form-label">Correo Electrónico</label>
                  <input 
                    type="email" 
                    class="form-control"
                    [class.is-invalid]="forgotForm.get('email')?.invalid && forgotForm.get('email')?.touched"
                    id="email" 
                    formControlName="email"
                    placeholder="usuario@ejemplo.com">
                  <div *ngIf="forgotForm.get('email')?.invalid && forgotForm.get('email')?.touched" class="invalid-feedback">
                    <small *ngIf="forgotForm.get('email')?.errors?.['required']">
                      El correo es requerido
                    </small>
                    <small *ngIf="forgotForm.get('email')?.errors?.['email']">
                      Formato de correo inválido
                    </small>
                  </div>
                </div>

                <button 
                  type="submit" 
                  class="btn btn-primary w-100 mb-3"
                  [disabled]="forgotForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  {{ isLoading ? 'Enviando...' : 'Enviar Enlace de Recuperación' }}
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
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  forgotForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor() {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotForm.valid && !this.isLoading) {
      this.isLoading = true;
      this.errorMessage = null;
      this.successMessage = null;

      const email = this.forgotForm.get('email')?.value;

      this.authService.requestPasswordReset(email).subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Se ha enviado un enlace de recuperación a tu correo electrónico.';
          this.forgotForm.reset();
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Error al enviar el enlace de recuperación';
        }
      });
    } else {
      this.forgotForm.get('email')?.markAsTouched();
    }
  }
}