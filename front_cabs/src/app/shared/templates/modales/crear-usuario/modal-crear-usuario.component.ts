import { Component, forwardRef  } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';
import { UiHeaderModalComponent } from '../../../molecules/headerModal/header-modal.component'; 
import { UiInputComponent } from '../../../molecules/input/input.component';
import { UiBotonComponent } from '../../../atoms/boton/boton.component';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { SecureAuthService, RegisterRequest } from '../../../../core/services/secure-auth.service';
import { RolUsuario } from '../../../../core/enums/rol-usuario.enum';
import { TipoTransmision } from '../../../../core/enums/tipo-transmision.enum';
import { UiDividerComponent } from '../../../atoms/linea/linea.component';

@Component({
  selector: 'app-crear-usuario',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,   
    UiHeaderModalComponent,
    UiInputComponent,
    UiBotonComponent,
    UiDividerComponent
  ],
  templateUrl: './modal-crear-usuario.component.html',
})
export class ModalCrearUsuario {
  form: FormGroup;

  roles = [
    { value: RolUsuario.Administrador, label: 'Administrador' },
    { value: RolUsuario.Soporte, label: 'Soporte' },
    { value: RolUsuario.Recepcion, label: 'Recepción' }
  ];

  trasmision = [
    { value: TipoTransmision.Ambas, label: 'Ambos' },
    { value: TipoTransmision.Automatico, label: 'Automático' },
    { value: TipoTransmision.Manual, label: 'Estándar' }
  ];

  visualizarInputTrasmision = false;
  // Estado del label o input 
  estadoLabel= false;  

  constructor(
    public dialogRef: MatDialogRef<ModalCrearUsuario>,
    private fb: FormBuilder,
    private registerService: SecureAuthService
  ) {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      apellido: ['', Validators.required],
      telefono: [null],
      email: ['', [Validators.required, Validators.email]],
      contrasena: ['', [Validators.required, Validators.minLength(8)]],
      confirmarContrasena: ['', Validators.required],
      rol: ['', Validators.required],
      transmisionHabilitada: [null],
      activo: [true]
    }, { validators: this.passwordMatchValidator });
  }

  estadoCheckot(checked: boolean): void {
    this.visualizarInputTrasmision = checked;
    if (!checked) {
      this.form.patchValue({ transmisionHabilitada: null });
    }
  }

  cerrar(): void {
    this.dialogRef.close();
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: RegisterRequest = {
      ...this.form.value,
      rol: this.form.value.rol as RolUsuario,
      transmisionHabilitada: this.form.value.transmisionHabilitada as TipoTransmision | null
    };

    this.registerService.register(request).subscribe({
      next: (resp) => {
        console.log('✅ Usuario registrado:', resp);
        this.cerrar();
      },
      error: (err) => {
        console.error('❌ Error al registrar:', err);
      }
    });
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const contrasena = group.get('contrasena')?.value;
    const confirmar = group.get('confirmarContrasena')?.value;
    return contrasena === confirmar ? null : { passwordMismatch: true };
  }
}


