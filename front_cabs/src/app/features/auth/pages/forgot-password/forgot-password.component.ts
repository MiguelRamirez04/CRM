// src/app/features/auth/pages/forgot-password/forgot-password.component.ts

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

// Define las fases del flujo
enum RecoveryStep {
  RequestEmail = 1,
  VerifyCode = 2,
  ResetPassword = 3,
}

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], 
  // Usa las URL que ya tienes para el Reset/Forgot
  templateUrl: './forgot-password.component.html', 
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);

  // Exponemos las fases para usarlas en el HTML
  RecoveryStep = RecoveryStep;
  
  // CRÍTICO: Controla qué panel se está mostrando
  currentStep: RecoveryStep = RecoveryStep.RequestEmail;

  // Formularios para cada fase
  emailForm: FormGroup;
  verificationForm: FormGroup;
  resetForm: FormGroup;

  // Temporizador de reenvío (solo visual por ahora)
  resendTimer: number = 20; // 0:20
  timerSubscription: any; // Usaremos setInterval simple para la demo visual

  constructor() {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    this.verificationForm = this.fb.group({
      // Campo que combina los 6 inputs visuales en uno solo para validación
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]], 
    });
    
    this.resetForm = this.fb.group({
      contrasena: ['', [Validators.required, Validators.minLength(6)]],
      confirmarContrasena: ['', [Validators.required]],
    });
    
    // Agregamos un validador para que las contraseñas coincidan
    // this.resetForm.get('confirmarContrasena')?.addValidators(
      //Validators.compose([
        //Validators.required,
        //(control: any) => {
          //const pass = this.resetForm.get('contrasena')?.value;
          //return pass === control.value ? null : { mismatch: true };
        //},
      //])
    //);
  }
  
  ngOnInit(): void {
      // Si el paso inicial es la verificación (por si viene de un link de correo), 
      // podrías iniciar el temporizador aquí.
  }

  // Lógica de navegación entre pasos
  nextStep(): void {
    if (this.currentStep === RecoveryStep.RequestEmail && this.emailForm.valid) {
      // Lógica de backend: Enviar correo de verificación (a futuro)
      this.currentStep = RecoveryStep.VerifyCode;
      this.startTimer(); // Inicia el temporizador al ir a verificación
      return;
    } 
    
    if (this.currentStep === RecoveryStep.VerifyCode && this.verificationForm.valid) {
      // Lógica de backend: Verificar código (a futuro)
      this.currentStep = RecoveryStep.ResetPassword;
      if (this.timerSubscription) {
          clearInterval(this.timerSubscription);
      }
      return;
    }
    
    if (this.currentStep === RecoveryStep.ResetPassword && this.resetForm.valid) {
        // Lógica de backend: Enviar nueva contraseña (a futuro)
        // Por ahora, solo simula un retorno al login:
        alert('Contraseña restablecida con éxito (Simulación)');
        this.router.navigate(['/auth/login']);
        return;
    }
    
    // Marcar campos como tocados si la validación falla
    this.markFormGroupTouched(
      this.currentStep === RecoveryStep.RequestEmail ? this.emailForm : 
      this.currentStep === RecoveryStep.VerifyCode ? this.verificationForm : 
      this.resetForm
    );
  }

  // Vuelve al paso anterior
  prevStep(): void {
    if (this.currentStep === RecoveryStep.VerifyCode) {
      this.currentStep = RecoveryStep.RequestEmail;
      if (this.timerSubscription) {
          clearInterval(this.timerSubscription);
      }
    } else if (this.currentStep === RecoveryStep.ResetPassword) {
      this.currentStep = RecoveryStep.VerifyCode;
      this.startTimer(); // Reinicia el temporizador si regresa
    }
  }

  // Simulación de temporizador de reenvío
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
  
  // Función de utilidad para marcar todos los campos como tocados
  markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if ((control as FormGroup).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }
}