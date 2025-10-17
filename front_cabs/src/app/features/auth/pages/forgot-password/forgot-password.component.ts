// src/app/features/auth/pages/forgot-password/forgot-password.component.ts

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { interval, Subscription } from 'rxjs'; 
import { startWith } from 'rxjs/operators';

// Define las fases del flujo
enum RecoveryStep {
  RequestEmail = 1,
  VerifyCode = 2,
  ResetPassword = 3,
}

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink], 
  templateUrl: './forgot-password.component.html', 
  styleUrls: ['./forgot-password.component.css'] 
})
export class ForgotPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);

  RecoveryStep = RecoveryStep;
  currentStep: RecoveryStep = RecoveryStep.RequestEmail;

  emailForm: FormGroup;
  verificationForm: FormGroup;
  resetForm: FormGroup;

  resendTimer: number = 20;
  timerSubscription: any; 

  constructor() {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    this.verificationForm = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]], 
    });
    
    this.resetForm = this.fb.group({
      contrasena: ['', [Validators.required, Validators.minLength(8)]],
      confirmarContrasena: ['', [Validators.required]],
    });
    
    // Validador para la coincidencia de contraseñas
    this.resetForm.get('confirmarContrasena')?.addValidators(
      (control: AbstractControl) => {
        const pass = this.resetForm.get('contrasena')?.value;
        return pass === control.value ? null : { mismatch: true };
      }
    );
  }
  
  ngOnInit(): void {
      if (this.currentStep === RecoveryStep.VerifyCode) {
          this.startTimer();
      }
  }

  /**
   * Maneja la navegación y la validación entre pasos.
   */
  nextStep(): void {
    if (this.currentStep === RecoveryStep.RequestEmail) {
        if (this.emailForm.invalid) {
            this.markFormGroupTouched(this.emailForm); // Uso correcto con 'this'
            return;
        }
        // ÉXITO DEL PASO 1 (A futuro, después del llamado al backend)
        this.currentStep = RecoveryStep.VerifyCode;
        this.startTimer();
        return;
    } 
    
    if (this.currentStep === RecoveryStep.VerifyCode) {
        if (this.verificationForm.invalid) {
            this.markFormGroupTouched(this.verificationForm); // Uso correcto con 'this'
            return;
        }
        // ÉXITO DEL PASO 2 (A futuro, después del llamado al backend)
        this.currentStep = RecoveryStep.ResetPassword;
        if (this.timerSubscription) {
            clearInterval(this.timerSubscription);
        }
        return;
    }
    
    if (this.currentStep === RecoveryStep.ResetPassword) {
        if (this.resetForm.invalid) {
            this.markFormGroupTouched(this.resetForm); // Uso correcto con 'this'
            return;
        }
        // ÉXITO DEL PASO 3 (A futuro, después del llamado al backend)
        alert('Contraseña restablecida con éxito (Simulación)');
        this.router.navigate(['/auth/login']);
        return;
    }
  }

  /**
   * Vuelve al paso anterior.
   */
  prevStep(): void {
    if (this.currentStep === RecoveryStep.VerifyCode) {
      this.currentStep = RecoveryStep.RequestEmail;
      if (this.timerSubscription) {
          clearInterval(this.timerSubscription);
      }
    } else if (this.currentStep === RecoveryStep.ResetPassword) {
      this.currentStep = RecoveryStep.VerifyCode;
      this.startTimer();
    }
  }

  /**
   * Simula el inicio del temporizador de reenvío de código.
   */
  startTimer(): void {
      this.resendTimer = 20;
      if (this.timerSubscription) {
          clearInterval(this.timerSubscription);
      }
      this.timerSubscription = setInterval(() => {
          if (this.resendTimer > 0) {
              this.resendTimer--;
          } else {
              clearInterval(this.timerSubscription);
          }
      }, 1000);
  }
  
  /**
   * MÉTODO DE CLASE: Marca recursivamente todos los controles de un FormGroup como tocados.
   */
  markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      // Verificación para FormControl o FormGroup/FormArray anidado
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Métodos visuales (no funcionales para el flujo, pero necesarios para compilar)
  getImageUrl(slide: number): string {
    // Implementación de getImageUrl
    return '';
  }
  togglePassword(): void {
    // Implementación de togglePassword
  }
}