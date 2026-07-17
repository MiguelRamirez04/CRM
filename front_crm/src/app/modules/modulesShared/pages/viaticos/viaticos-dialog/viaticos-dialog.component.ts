import { Component, inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GastoViaticoService } from '../../../../../core/services/gasto-viatico.service';
import {
  GastoViaticoCreateRequest,
  GastoViaticoResponse,
  GastoViaticoUpdateRequest
} from '../../../../../core/models/gasto-viatico.interface';

@Component({
  selector: 'app-viaticos-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './viaticos-dialog.component.html',
  styleUrl: './viaticos-dialog.component.css'
})
export class ViaticosDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private viaticoService = inject(GastoViaticoService);

  @Input() viaticoEditar: GastoViaticoResponse | null = null;
  @Input() mostrar = false;
  @Output() cerrar = new EventEmitter<void>();
  @Output() guardadoExitoso = new EventEmitter<GastoViaticoResponse>();

  viaticoForm: FormGroup;
  guardando = false;
  submitted = false;
  mensajeError = '';

  constructor() {
    this.viaticoForm = this.fb.group({
      tieneFactura: [false, Validators.required],
      descripcion: ['', Validators.maxLength(500)],
      proveedorNombre: ['', Validators.maxLength(255)],
      fecha: ['', Validators.required],
      kmRecorridos: [null, [Validators.min(0), Validators.max(999999)]],
      gastos: ['', [Validators.required, Validators.maxLength(200)]],
      montoTotal: [0, [Validators.required, Validators.min(0.01)]],
      lugarDestino: ['', Validators.maxLength(255)]
    });
  }

  ngOnInit(): void {
    // Actualizar campos cuando cambia tieneFactura
    this.viaticoForm.get('tieneFactura')?.valueChanges.subscribe(tieneFactura => {
      const proveedorControl = this.viaticoForm.get('proveedorNombre');
      if (tieneFactura) {
        proveedorControl?.setValidators([Validators.required, Validators.maxLength(255)]);
      } else {
        proveedorControl?.clearValidators();
        proveedorControl?.setValue('');
      }
      proveedorControl?.updateValueAndValidity();
    });

    // Si hay viático para editar, cargar datos
    if (this.viaticoEditar) {
      this.cargarDatos();
    }
  }

  ngOnChanges(): void {
    if (this.viaticoEditar) {
      this.cargarDatos();
    } else {
      this.resetForm();
    }
  }

  private cargarDatos(): void {
    if (!this.viaticoEditar) return;
    
    this.viaticoForm.patchValue({
      tieneFactura: this.viaticoEditar.tieneFactura,
      descripcion: this.viaticoEditar.descripcion,
      proveedorNombre: this.viaticoEditar.proveedorNombre,
      fecha: this.viaticoEditar.fecha.split('T')[0], // Convertir a YYYY-MM-DD
      kmRecorridos: this.viaticoEditar.kmRecorridos,
      gastos: this.viaticoEditar.gastos,
      montoTotal: this.viaticoEditar.montoTotal,
      lugarDestino: this.viaticoEditar.lugarDestino
    });
  }

  private resetForm(): void {
    this.viaticoForm.reset({
      tieneFactura: false,
      fecha: new Date().toISOString().split('T')[0],
      kmRecorridos: null,
      montoTotal: 0
    });
    this.submitted = false;
    this.mensajeError = '';
  }

  onCerrar(): void {
    this.resetForm();
    this.cerrar.emit();
  }

  onGuardar(): void {
    this.submitted = true;
    this.mensajeError = '';

    if (this.viaticoForm.invalid) {
      this.mensajeError = 'Por favor, complete todos los campos requeridos correctamente.';
      return;
    }

    this.guardando = true;

    if (this.viaticoEditar) {
      this.actualizarViatico();
    } else {
      this.crearViatico();
    }
  }

  private crearViatico(): void {
    const request: GastoViaticoCreateRequest = {
      ordenId: null, // Siempre null, no se usa
      tieneFactura: this.viaticoForm.value.tieneFactura,
      descripcion: this.viaticoForm.value.descripcion || null,
      proveedorNombre: this.viaticoForm.value.proveedorNombre || null,
      fecha: this.viaticoForm.value.fecha + 'T00:00:00', // Agregar hora para formato ISO
      kmRecorridos: this.viaticoForm.value.kmRecorridos || null,
      gastos: this.viaticoForm.value.gastos || '',
      montoTotal: this.viaticoForm.value.montoTotal,
      lugarDestino: this.viaticoForm.value.lugarDestino || null
    };

    this.viaticoService.crear(request).subscribe({
      next: (creado) => {
        this.guardando = false;
        this.guardadoExitoso.emit(creado);
        this.resetForm();
      },
      error: (err) => {
        console.error('Error al crear viático:', err);
        this.mensajeError = err.error?.message || 'Error al crear el viático. Por favor, intente nuevamente.';
        this.guardando = false;
      }
    });
  }

  private actualizarViatico(): void {
    if (!this.viaticoEditar) return;

    const request: GastoViaticoUpdateRequest = {
      ordenId: null, // Siempre null, no se usa
      tieneFactura: this.viaticoForm.value.tieneFactura,
      descripcion: this.viaticoForm.value.descripcion || null,
      proveedorNombre: this.viaticoForm.value.proveedorNombre || null,
      fecha: this.viaticoForm.value.fecha + 'T00:00:00', // Agregar hora para formato ISO
      kmRecorridos: this.viaticoForm.value.kmRecorridos || null,
      gastos: this.viaticoForm.value.gastos || '',
      montoTotal: this.viaticoForm.value.montoTotal,
      lugarDestino: this.viaticoForm.value.lugarDestino || null
    };

    this.viaticoService.actualizar(this.viaticoEditar.id, request).subscribe({
      next: () => {
        this.guardando = false;
        // Emitir el viático editado con los datos actualizados
        const viaticoActualizado: GastoViaticoResponse = {
          id: this.viaticoEditar!.id,
          ordenId: request.ordenId,
          tieneFactura: request.tieneFactura,
          descripcion: request.descripcion,
          proveedorNombre: request.proveedorNombre,
          fecha: typeof request.fecha === 'string' ? request.fecha : request.fecha.toISOString(),
          kmRecorridos: request.kmRecorridos,
          gastos: request.gastos,
          montoTotal: request.montoTotal,
          lugarDestino: request.lugarDestino
        };
        this.guardadoExitoso.emit(viaticoActualizado);
        this.resetForm();
      },
      error: (err) => {
        console.error('Error al actualizar viático:', err);
        this.mensajeError = err.error?.message || 'Error al actualizar el viático. Por favor, intente nuevamente.';
        this.guardando = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.viaticoForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched || this.submitted));
  }

  getFieldError(fieldName: string): string {
    const field = this.viaticoForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) return 'Este campo es requerido';
    if (field.errors['min']) return `Valor mínimo: ${field.errors['min'].min}`;
    if (field.errors['max']) return `Valor máximo: ${field.errors['max'].max}`;
    if (field.errors['maxlength']) return `Máximo ${field.errors['maxlength'].requiredLength} caracteres`;
    
    return 'Campo inválido';
  }

  get tituloDialog(): string {
    return this.viaticoEditar ? 'Editar Viático' : 'Nuevo Viático';
  }

  get textoBoton(): string {
    return this.guardando ? 'Guardando...' : (this.viaticoEditar ? 'Actualizar' : 'Guardar');
  }
}
