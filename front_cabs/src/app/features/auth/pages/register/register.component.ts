import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { SecureAuthService, RegisterRequest } from '../../../../core/services/secure-auth.service';
import { RolUsuario } from '../../../../core/enums/rol-usuario.enum';
import { TipoTransmision } from '../../../../core/enums/tipo-transmision.enum';

// ✅ Validador para coincidencia de contraseñas
export function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const pass = group.get('contrasena')?.value;
  const confirmPass = group.get('confirmarContrasena')?.value;
  return pass === confirmPass ? null : { mismatch: true };
}

// ✅ Validador personalizado para contraseña robusta
export function strongPasswordValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;

  const hasNumber = /[0-9]/.test(value);
  const hasUpperCase = /[A-Z]/.test(value);
  const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);

  const passwordValid = hasNumber && hasUpperCase && hasSpecialChar;

  return !passwordValid
    ? {
        strongPassword: { hasNumber, hasUpperCase, hasSpecialChar },
      }
    : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private secureAuthService = inject(SecureAuthService);

  registerForm: FormGroup;
  errorMessage: string | null = null;
  isLoading = false;

  // Enums
  RolUsuario = RolUsuario;
  TipoTransmision = TipoTransmision;
  roles = Object.values(RolUsuario);
  transmisiones = Object.values(TipoTransmision);

  mostrarVehiculo: any;

  // 🧩 Propiedades para mostrar/ocultar contraseña y validaciones visuales
  showPassword = false;
  passwordChecks = {
    length: false,
    lowercase: false,
    uppercase: false,
    number: false,
  };
  allValid = false;

  constructor() {
    this.registerForm = this.fb.group(
      {
        nombre: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        apellido: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        telefono: ['', [Validators.pattern(/^\d{10}$/)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
        contrasena: ['', [Validators.required, Validators.minLength(8), strongPasswordValidator]],
        confirmarContrasena: ['', Validators.required],
        rol: [RolUsuario.Recepcion, Validators.required],
        transmisionHabilitada: [TipoTransmision.Ninguna],
        activo: [false],
      },
      { validators: [passwordMatchValidator] }
    );

    // Suscripción para actualizar validaciones visuales
    this.registerForm.get('contrasena')?.valueChanges.subscribe(() => {
      this.checkPasswordStrength();
    });
  }

  // 👁️ Mostrar / ocultar contraseña
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
  

  // 🧠 Validación visual de la contraseña
  checkPasswordStrength(): void {
    const value = this.registerForm.get('contrasena')?.value || '';
    this.passwordChecks.length = value.length >= 8;
    this.passwordChecks.lowercase = /[a-z]/.test(value);
    this.passwordChecks.uppercase = /[A-Z]/.test(value);
    this.passwordChecks.number = /\d/.test(value);
    this.allValid = Object.values(this.passwordChecks).every((v) => v);
  }

  // 🚀 Enviar formulario
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const form = this.registerForm.value;
    const payload: RegisterRequest = {
      nombre: (form.nombre || '').trim(),
      apellido: (form.apellido || '').trim(),
      telefono: form.telefono ? parseInt(form.telefono, 10) : null,
      email: (form.email || '').trim(),
      contrasena: form.contrasena,
      confirmarContrasena: form.confirmarContrasena,
      rol: form.rol,
      transmisionHabilitada: form.transmisionHabilitada ?? TipoTransmision.Ninguna,
      activo: !!form.activo,
    };

    this.secureAuthService.register(payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/auth/login'], { queryParams: { registered: true } });
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err;
      },
    });
  }
}
