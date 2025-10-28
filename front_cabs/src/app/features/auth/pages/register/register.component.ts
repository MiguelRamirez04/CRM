import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { SecureAuthService, RegisterRequest } from '../../../../core/services/secure-auth.service';
import { RolUsuario } from '../../../../core/enums/rol-usuario.enum';
import { TipoTransmision } from '../../../../core/enums/tipo-transmision.enum';

// Validador para coincidencia de contraseñas
export function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const pass = group.get('contrasena')?.value;
  const confirmPass = group.get('confirmarContrasena')?.value;
  
  // Si no hay contraseña, no validar coincidencia
  if (!pass) return null;
  
  // Si hay contraseña pero no confirmación, marcar error
  if (pass && !confirmPass) return { required: true };
  
  return pass === confirmPass ? null : { mismatch: true };
}

//Validador personalizado para contraseña robusta
export function strongPasswordValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;

  const hasNumber = /[0-9]/.test(value);
  const hasUpperCase = /[A-Z]/.test(value);
  const hasLowerCase = /[a-z]/.test(value);

  const passwordValid = hasNumber && hasUpperCase && hasLowerCase && value.length >= 8;

  return !passwordValid
    ? {
        strongPassword: { hasNumber, hasUpperCase, hasLowerCase },
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
  roles: RolUsuario[] = Object.values(RolUsuario) as RolUsuario[];
  transmisiones: TipoTransmision[] = Object.values(TipoTransmision) as TipoTransmision[];

  mostrarVehiculo: boolean = false;

  //Propiedades para mostrar/ocultar contraseña y validaciones visuales
  showPassword = false;
  passwordChecks = {
    length: false,
    lowercase: false,
    uppercase: false,
    number: false,
  };
  allValid = false;
  rolAbierto = false;
  transmisionAbierto = false;
  formularioEnviado = false;

  constructor() {
    this.registerForm = this.fb.group(
      {
        nombre: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        apellido: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        telefono: ['', [Validators.pattern(/^\d{10}$/)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
        contrasena: ['', [Validators.required, Validators.minLength(8), strongPasswordValidator]],
        confirmarContrasena: ['', Validators.required],
        rol: ['', Validators.required],
        transmisionHabilitada: ['', Validators.required],
        activo: [false],
      },
      { validators: [passwordMatchValidator] }
    );

    // Suscripción para actualizar validaciones visuales
    this.registerForm.get('contrasena')?.valueChanges.subscribe(() => {
      this.checkPasswordStrength();
    });
  }

  //Mostrar / ocultar contraseña
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  //Validación visual de la contraseña
  checkPasswordStrength(): void {
    const value = this.registerForm.get('contrasena')?.value || '';
    this.passwordChecks.length = value.length >= 8;
    this.passwordChecks.lowercase = /[a-z]/.test(value);
    this.passwordChecks.uppercase = /[A-Z]/.test(value);
    this.passwordChecks.number = /\d/.test(value);
    this.allValid = Object.values(this.passwordChecks).every((v) => v);
  }

  //oggle dropdowns
  toggleRol(): void {
    this.rolAbierto = !this.rolAbierto;
    if (this.rolAbierto) this.transmisionAbierto = false;
  }

  toggleTransmision(): void {
    this.transmisionAbierto = !this.transmisionAbierto;
    if (this.transmisionAbierto) this.rolAbierto = false;
  }

  //Seleccionar valores
  seleccionarRol(rol: RolUsuario): void {
    this.registerForm.patchValue({ rol });
    this.rolAbierto = false;
  }

  seleccionarTransmision(transmision: TipoTransmision): void {
    this.registerForm.patchValue({ transmisionHabilitada: transmision });
    this.transmisionAbierto = false;
  }

  // Enviar formulario
  onSubmit(): void {
    this.formularioEnviado = true;

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

  //Verificar si confirmar contraseña debe mostrar error
  mostrarErrorConfirmarContrasena(): boolean {
    const contrasenaControl = this.registerForm.get('contrasena');
    const confirmarControl = this.registerForm.get('confirmarContrasena');
    
    return (
      (confirmarControl?.touched || this.formularioEnviado) &&
      (confirmarControl?.hasError('required') || this.registerForm.hasError('mismatch'))
    );
  }

  // Obtener mensaje de error para confirmar contraseña
  obtenerMensajeErrorConfirmar(): string {
    const contrasenaControl = this.registerForm.get('contrasena');
    const confirmarControl = this.registerForm.get('confirmarContrasena');
    
    if (!contrasenaControl?.value) {
      return 'Primero debe ingresar una contraseña';
    }
    
    if (confirmarControl?.hasError('required')) {
      return 'Este campo es obligatorio';
    }
    
    if (this.registerForm.hasError('mismatch')) {
      return 'Las contraseñas no coinciden';
    }
    
    return '';
  }

  // Verificar si el teléfono debe mostrar error
  mostrarErrorTelefono(): boolean {
    const telefonoControl = this.registerForm.get('telefono');
    
    return (
      (telefonoControl?.touched || this.formularioEnviado) &&
      !!telefonoControl?.invalid
    );
  }

  // Obtener mensaje de error para teléfono
  obtenerMensajeErrorTelefono(): string {
    const telefonoControl = this.registerForm.get('telefono');
    if (!telefonoControl) return '';
    
    if (telefonoControl?.hasError('required')) {
      return 'El teléfono es obligatorio';
    }
    
    if (telefonoControl?.hasError('pattern')) {
      return 'Debe contener exactamente 10 dígitos';
    }
    
    return '';
  }
}