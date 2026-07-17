import { Component, EventEmitter, Input, Output, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Vehiculo, RegistrarSalidaDto, RegistrarEntradaDto } from '../../../../../../core/models/vehiculo.interface';
import { SecureAuthService, User } from '../../../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-accion-vehiculo-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './accion-vehiculo-modal.component.html',
  styleUrl: './accion-vehiculo-modal.component.css'
})
export class AccionVehiculoModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);

  @Input() modo: 'salida' | 'entrada' = 'salida';
  @Input() vehiculo: Vehiculo | null = null;
  @Input() visible = false;

  @Output() cerrar = new EventEmitter<void>();
  @Output() guardarSalida = new EventEmitter<RegistrarSalidaDto>();
  @Output() guardarEntrada = new EventEmitter<RegistrarEntradaDto>();

  form!: FormGroup;
  usuarios = signal<User[]>([]);
  cargandoUsuarios = signal(false);
  usosActivos: any[] = []; // Si necesitamos validar algo más

  ngOnInit(): void {
    this.iniciliazarFormulario();
    if (this.modo === 'salida') {
      this.cargarUsuarios();
    }
  }

  private iniciliazarFormulario(): void {
    if (this.modo === 'salida') {
      this.form = this.fb.group({
        usuarioId: [null, [Validators.required]],
        motivoUso: ['', [Validators.required, Validators.maxLength(250)]],
        kilometrajeInicial: [{ value: this.vehiculo?.kilometraje || 0, disabled: true }, [Validators.required]]
      });
    } else {
      this.form = this.fb.group({
        kilometrajeFinal: [this.vehiculo?.kilometraje || 0, [Validators.required, Validators.min(this.vehiculo?.kilometraje || 0)]],
        observaciones: ['', [Validators.maxLength(500)]],
        estado: ['COMPLETADO']
      });
    }
  }

  private cargarUsuarios(): void {
    this.cargandoUsuarios.set(true);
    // Asumiendo que getUsuarios devuelve un objeto con { data: User[] }
    this.authService.getUsuarios(false).subscribe({
      next: (resp) => {
        this.usuarios.set(resp.data); // Ajusta si la estructura es diferente
        this.cargandoUsuarios.set(false);
      },
      error: (err) => {
        console.error('Error cargando usuarios', err);
        this.cargandoUsuarios.set(false);
      }
    });
  }

  onCancelar(): void {
    this.cerrar.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.modo === 'salida') {
      const formValue = this.form.getRawValue();
      const dto: RegistrarSalidaDto = {
        usuarioId: +formValue.usuarioId,
        motivoUso: formValue.motivoUso,
        kilometrajeInicial: formValue.kilometrajeInicial
      };
      this.guardarSalida.emit(dto);
    } else {
      const formValue = this.form.value;

      // Validación extra: Km final >= Km inicial
      if (formValue.kilometrajeFinal < (this.vehiculo?.kilometraje || 0)) {
        alert('El kilometraje final no puede ser menor al actual.');
        return;
      }

      const dto: RegistrarEntradaDto = {
        kilometrajeFinal: formValue.kilometrajeFinal,
        observaciones: formValue.observaciones,
        estado: formValue.estado
      };
      this.guardarEntrada.emit(dto);
    }
  }

  campoEsInvalido(campo: string): boolean {
    const control = this.form.get(campo);
    return !!(control?.invalid && (control.dirty || control.touched));
  }
}
