import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';

import { RecepcionService } from '../../../../../core/services/recepcion.service';
import { SecureAuthService } from '../../../../../core/services/secure-auth.service';
import { OrdenTrabajo } from '../../../../../core/models/orden-trabajo.interface';
import { Modalidad, TipoOrden, EstadoOrden } from '../../../../../core/enums/estado-orden.enum';

@Component({
  selector: 'app-editar-orden',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './editar-orden.component.html',
  styleUrl: './editar-orden.component.css'
})
export class EditarOrdenComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<EditarOrdenComponent>);
  private recepcionService = inject(RecepcionService);
  private authService = inject(SecureAuthService);
  private data = inject<OrdenTrabajo>(MAT_DIALOG_DATA);

  ordenForm!: FormGroup;
  guardandoOrden = signal(false);
  errorMessage = signal<string | null>(null);

  modalidades = Object.values(Modalidad);
  tiposOrden = Object.values(TipoOrden);
  estados = Object.values(EstadoOrden);

  ordenActual = signal<OrdenTrabajo>(this.data);

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    const orden = this.data;
    
    this.ordenForm = this.fb.group({
      citaProgramadaInicio: [this.formatDateTimeLocal(orden.citaProgramadaInicio), Validators.required],
      citaProgramadaFin: [this.formatDateTimeLocal(orden.citaProgramadaFin)],
      modalidad: [orden.modalidad || Modalidad.PRESENCIAL, Validators.required],
      tipoOrden: [orden.tipoOrden || TipoOrden.ASESORIA, Validators.required],
      estado: [orden.estado || EstadoOrden.CAPTURADA, Validators.required],
      prioridad: [orden.prioridad || 1, [Validators.required, Validators.min(1), Validators.max(5)]],
      notas: [orden.notas || ''],
      ubicacionText: [orden.ubicacionText || ''],
      requiereFactura: [orden.requiereFactura || false],
      costoEstimado: [orden.costoEstimado, Validators.min(0)]
    });
  }

  private formatDateTimeLocal(dateString: string | null | undefined): string {
    if (!dateString) return '';
    
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '';
      
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      
      return `${year}-${month}-${day}T${hours}:${minutes}`;
    } catch {
      return '';
    }
  }

  onSubmit(): void {
    if (this.ordenForm.invalid) {
      this.ordenForm.markAllAsTouched();
      this.errorMessage.set('Por favor complete todos los campos requeridos');
      return;
    }

    this.guardandoOrden.set(true);
    this.errorMessage.set(null);

    const formValue = this.ordenForm.value;

    const updateData = {
      citaProgramadaInicio: formValue.citaProgramadaInicio ? new Date(formValue.citaProgramadaInicio).toISOString() : undefined,
      citaProgramadaFin: formValue.citaProgramadaFin ? new Date(formValue.citaProgramadaFin).toISOString() : undefined,
      modalidad: formValue.modalidad,
      tipoOrden: formValue.tipoOrden,
      estado: formValue.estado,
      prioridad: formValue.prioridad,
      notas: formValue.notas,
      ubicacionText: formValue.ubicacionText,
      requiereFactura: formValue.requiereFactura,
      costoEstimado: formValue.costoEstimado ? Number(formValue.costoEstimado) : undefined
    };

    this.recepcionService.actualizarOrden(this.data.id, updateData).subscribe({
      next: (response) => {
        console.log('Orden actualizada exitosamente:', response);
        this.guardandoOrden.set(false);
        this.dialogRef.close(response);
      },
      error: (error) => {
        console.error('Error al actualizar orden:', error);
        this.errorMessage.set(error.error?.mensaje || 'Error al actualizar la orden');
        this.guardandoOrden.set(false);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.ordenForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.ordenForm.get(fieldName);
    if (field?.hasError('required')) return 'Este campo es requerido';
    if (field?.hasError('min')) return `Valor mínimo: ${field.errors?.['min'].min}`;
    if (field?.hasError('max')) return `Valor máximo: ${field.errors?.['max'].max}`;
    return '';
  }
}
