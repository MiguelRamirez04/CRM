import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { SecureAuthService, RegisterRequest } from '../../../../core/services/secure-auth.service';
import { RolUsuario } from '../../../../core/enums/rol-usuario.enum';
import { TipoTransmision } from '../../../../core/enums/tipo-transmision.enum';

// Validador para la coincidencia de contraseñas
export function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const pass = group.get('contrasena')?.value;
    const confirmPass = group.get('confirmarContrasena')?.value;
    return pass === confirmPass ? null : { mismatch: true };
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

  // Exponer enums a la plantilla para usarlos en el HTML
  RolUsuario = RolUsuario;
  TipoTransmision = TipoTransmision;
  roles = Object.values(RolUsuario);
  transmisiones = Object.values(TipoTransmision);

  constructor() {
    this.registerForm = this.fb.group(
      {
        nombre: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        apellido: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        telefono: ['', [Validators.pattern(/^\d{10}$/)]], // Opcional, 10 dígitos
        email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
        contrasena: ['', [Validators.required, Validators.minLength(8)]],
        confirmarContrasena: ['', Validators.required],
        rol: [RolUsuario.Recepcion, Validators.required], // Rol por defecto
        licenciaConducir: [''], // Opcional, ahora es string
        transmisionHabilitada: [TipoTransmision.Ninguna], // Opcional
      },
      {
        validators: [passwordMatchValidator],
      }
    );
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    // Construir payload incluyendo confirmarContrasena y normalizando strings
    const form = this.registerForm.value;
    const payload: RegisterRequest = {
      nombre: (form.nombre || '').trim(),
      apellido: (form.apellido || '').trim(),
      telefono: form.telefono ? parseInt(form.telefono, 10) : null,
      email: (form.email || '').trim(),
      contrasena: form.contrasena,
      confirmarContrasena: form.confirmarContrasena,
      rol: form.rol,
      licenciaConducir: form.licenciaConducir ? (form.licenciaConducir || '').trim() : null,
      transmisionHabilitada: form.transmisionHabilitada ?? TipoTransmision.Ninguna,
    };

    this.secureAuthService.register(payload).subscribe({
      next: () => {
        this.isLoading = false;
        // Redirigir al login con un mensaje de éxito
        this.router.navigate(['/auth/login'], {
          queryParams: { registered: true },
        });
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err;
      },
    });
  }
}