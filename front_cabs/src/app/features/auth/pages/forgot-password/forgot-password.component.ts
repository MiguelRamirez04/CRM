import { Component, inject, OnInit, OnDestroy, ElementRef, ViewChildren, QueryList, AfterViewInit } from '@angular/core';
import { CommonModule, Location, DecimalPipe, NgClass } from '@angular/common'; // Asegúrate de importar DecimalPipe y NgClass
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms'; // Asegúrate de importar FormControl
import { Router, RouterModule } from '@angular/router';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';
import { interval, Subscription, timer } from 'rxjs';
import { takeWhile } from 'rxjs/operators';

// Enum para manejar los pasos
export enum RecoveryStep {
  RequestEmail,
  VerifyCode,
  ResetPassword
}

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    DecimalPipe, // Para el pipe 'number' del timer
    NgClass      // Para [ngClass]
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit, OnDestroy, AfterViewInit {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);
  private location = inject(Location);

  // --- FORMULARIOS ---
  emailForm: FormGroup;
  verificationForm: FormGroup; // Modificado
  resetForm: FormGroup;

  // Control del paso actual
  currentStep: RecoveryStep = RecoveryStep.RequestEmail;
  RecoveryStep = RecoveryStep;

  // Estado y mensajes
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Timer para reenvío de código
  resendTimer = 0;
  private timerSubscription: Subscription | null = null;

  // --- NUEVO: Referencias a los inputs del código ---
  @ViewChildren('digitInput') digitInputs!: QueryList<ElementRef<HTMLInputElement>>;
  // ---------------------------------------------

  constructor() {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    // --- VERIFICATION FORM MODIFICADO ---
    // Creamos 6 FormControls, uno para cada dígito
    this.verificationForm = this.fb.group({
      digit1: ['', [Validators.required, Validators.pattern(/^\d$/)]],
      digit2: ['', [Validators.required, Validators.pattern(/^\d$/)]],
      digit3: ['', [Validators.required, Validators.pattern(/^\d$/)]],
      digit4: ['', [Validators.required, Validators.pattern(/^\d$/)]],
      digit5: ['', [Validators.required, Validators.pattern(/^\d$/)]],
      digit6: ['', [Validators.required, Validators.pattern(/^\d$/)]]
    });
    // ------------------------------------

    this.resetForm = this.fb.group({
      contrasena: ['', [Validators.required, Validators.minLength(8)]],
      confirmarContrasena: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    // Cuando se muestre el paso de verificación (o si ya estaba), enfoca el primer input
    if (this.currentStep === RecoveryStep.VerifyCode) {
        this.focusFirstInput();
    }
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('contrasena');
    const confirmPassword = form.get('confirmarContrasena');
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    // Si llegamos aquí y confirmPassword tenía el error 'mismatch', lo quitamos
    // Cuidado: No sobrescribir otros errores como 'required'
    if (confirmPassword?.hasError('mismatch')) {
        const errors = confirmPassword.errors;
        if (errors) {
            delete errors['mismatch']; // Elimina solo el error mismatch
            if (Object.keys(errors).length === 0) {
                 confirmPassword.setErrors(null); // Si no quedan errores, limpia
            } else {
                 confirmPassword.setErrors(errors); // Si quedan otros, mantenlos
            }
        }
    }
    return null;
  }


  nextStep(): void {
    this.errorMessage = null; 
    
    switch (this.currentStep) {
      case RecoveryStep.RequestEmail:
        if (this.emailForm.invalid) { this.emailForm.markAllAsTouched(); this.errorMessage = 'Ingresa un correo válido.'; return; }
        console.log('Enviando solicitud para:', this.emailForm.value.email);
        this.isLoading = true;
        setTimeout(() => {
            this.isLoading = false;
            this.currentStep = RecoveryStep.VerifyCode;
            setTimeout(() => this.focusFirstInput(), 50); // Pequeña espera para asegurar que los inputs estén renderizados
            this.startTimer();
        }, 1500);
        break;

      case RecoveryStep.VerifyCode:
         if (this.verificationForm.invalid) {
             this.verificationForm.markAllAsTouched();
             this.errorMessage = 'Ingresa los 6 dígitos del código.';
             return;
         }
        const code = Object.values(this.verificationForm.value).join('');
        console.log('Verificando código:', code);
        this.isLoading = true;
        setTimeout(() => {
            this.isLoading = false;
            if (code === '123456') { // Simulación
                this.currentStep = RecoveryStep.ResetPassword;
                this.stopTimer();
            } else {
                this.errorMessage = 'Código incorrecto.';
                this.verificationForm.reset();
                this.focusFirstInput();
            }
        }, 1500);
        break;

      case RecoveryStep.ResetPassword:
         if (this.resetForm.invalid) { this.resetForm.markAllAsTouched(); this.errorMessage = 'Revisa los campos de contraseña.'; return; }
        console.log('Restableciendo contraseña...');
         this.isLoading = true;
        setTimeout(() => {
            this.isLoading = false;
            this.successMessage = 'Contraseña restablecida con éxito. Redirigiendo...';
            setTimeout(() => this.router.navigate(['/auth/login']), 2000);
        }, 1500);
        break;
    }
  }

  prevStep(): void {
    this.errorMessage = null; 
    if (this.currentStep > RecoveryStep.RequestEmail) {
      this.currentStep--;
    }
    if (this.currentStep === RecoveryStep.RequestEmail) {
        this.stopTimer(); 
    }
  }

  startTimer(duration: number = 60): void {
     this.stopTimer(); 
     this.resendTimer = duration;
     this.timerSubscription = timer(0, 1000) 
       .pipe(takeWhile(() => this.resendTimer > 0))
       .subscribe(() => {
         this.resendTimer--;
       });
  }
  
  stopTimer(): void {
      if (this.timerSubscription) {
          this.timerSubscription.unsubscribe();
          this.timerSubscription = null;
      }
  }

  goBack(): void {
    this.location.back();
  }

  // --- NUEVAS FUNCIONES PARA LOS INPUTS DEL CÓDIGO ---

  onKeyUp(event: KeyboardEvent, index: number): void {
    const inputElement = event.target as HTMLInputElement;
    const value = inputElement.value;

    // Solo permite dígitos
    if (!/^\d$/.test(value)) {
        inputElement.value = '';
        return;
    }

    // Si se ingresó un dígito y no es el último input, mueve el foco al siguiente
    if (value.length === 1 && index < 5) {
      this.focusNextInput(index);
    }
  }

  onKeyDown(event: KeyboardEvent, index: number): void {
    const inputElement = event.target as HTMLInputElement;

    // Si se presiona Backspace en un input vacío y no es el primero, mueve el foco al anterior
    if (event.key === 'Backspace' && inputElement.value === '' && index > 0) {
      this.focusPrevInput(index);
    } 
    // Si se presiona Flecha Izquierda y no es el primero, mueve foco al anterior
    else if (event.key === 'ArrowLeft' && index > 0) {
        event.preventDefault(); // Previene movimiento del cursor dentro del input
        this.focusPrevInput(index);
    }
    // Si se presiona Flecha Derecha y no es el último, mueve foco al siguiente
    else if (event.key === 'ArrowRight' && index < 5) {
        event.preventDefault(); // Previene movimiento del cursor dentro del input
        this.focusNextInput(index);
    }
  }

  onPaste(event: ClipboardEvent): void {
      event.preventDefault();
      const pastedData = event.clipboardData?.getData('text').trim().replace(/\s+/g, ''); // Quita espacios

      if (pastedData && /^\d{1,6}$/.test(pastedData)) { // Acepta de 1 a 6 dígitos
          const digits = pastedData.split('');
          const inputs = this.digitInputs.toArray();

          for (let i = 0; i < digits.length && i < inputs.length; i++) {
              const controlName = `digit${i + 1}`;
              this.verificationForm.get(controlName)?.setValue(digits[i]);
              if (i < inputs.length - 1) {
                  // Mueve el foco solo si no es el último input que llenamos con el paste
                  if (i < digits.length - 1) {
                       this.focusNextInput(i);
                  } else {
                       inputs[i + 1].nativeElement.focus(); // Enfoca el siguiente vacío
                  }
              } else {
                  inputs[i].nativeElement.focus(); // Si pegó hasta el último, lo enfoca
              }
          }
      } else {
           // Opcional: Mostrar error si lo pegado no son solo números
           this.errorMessage = "Solo puedes pegar números.";
           setTimeout(() => this.errorMessage = null, 2000);
      }
  }

  private focusFirstInput(): void {
    // Intenta enfocar el primer input después de un pequeño delay
    setTimeout(() => {
        if (this.digitInputs?.first) {
            this.digitInputs.first.nativeElement.focus();
            this.digitInputs.first.nativeElement.select(); // Selecciona el contenido si ya hay algo
        }
    }, 50); // Aumenta si sigue sin enfocar
  }

  private focusNextInput(index: number): void {
    if (index < 5 && this.digitInputs) {
      const inputs = this.digitInputs.toArray();
      inputs[index + 1].nativeElement.focus();
      inputs[index + 1].nativeElement.select(); // Selecciona para fácil reemplazo
    }
  }

  private focusPrevInput(index: number): void {
    if (index > 0 && this.digitInputs) {
      const inputs = this.digitInputs.toArray();
      inputs[index - 1].nativeElement.focus();
      inputs[index - 1].nativeElement.select(); // Selecciona para fácil reemplazo
    }
  }
}