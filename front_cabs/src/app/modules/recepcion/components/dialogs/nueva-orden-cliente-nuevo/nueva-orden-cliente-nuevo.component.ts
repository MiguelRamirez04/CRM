import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { RecepcionService } from '../../../services/recepcion.service';
import { SecureAuthService } from '../../../../../core/services/secure-auth.service';
import { OrdenTrabajoRequest } from '../../../../../core/models/orden-trabajo.interface';
import { Modalidad, TipoOrden, EstadoOrden } from '../../../../../core/enums/estado-orden.enum';

@Component({
  selector: 'app-nueva-orden-cliente-nuevo',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './nueva-orden-cliente-nuevo.component.html',
  styleUrl: './nueva-orden-cliente-nuevo.component.css'
})
export class NuevaOrdenClienteNuevoComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NuevaOrdenClienteNuevoComponent>);
  private recepcionService = inject(RecepcionService);
  private authService = inject(SecureAuthService);

  ordenForm!: FormGroup;
  guardandoOrden = signal(false);
  errorMessage = signal<string | null>(null);

  // Enums para el template
  modalidades = Object.values(Modalidad);
  tiposOrden = Object.values(TipoOrden);
  estados = Object.values(EstadoOrden);

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.ordenForm = this.fb.group({
      // Datos del cliente nuevo
      nombreCliente: ['', [Validators.required, Validators.minLength(3)]],
      clienteTelefono: ['', [Validators.pattern(/^\d{10}$/)]],
      
      // Datos de la orden
      citaProgramadaInicio: ['', Validators.required],
      citaProgramadaFin: [''],
      modalidad: [Modalidad.PRESENCIAL, Validators.required],
      tipoOrden: [TipoOrden.ASESORIA, Validators.required],
      estado: [EstadoOrden.CAPTURADA, Validators.required],
      prioridad: [1, [Validators.required, Validators.min(1), Validators.max(5)]],
      notas: [''],
      ubicacionText: [''],
      requiereFactura: [false],
      costoEstimado: [null, Validators.min(0)]
    });
  }

  onSubmit(): void {
    if (this.ordenForm.invalid) {
      Object.keys(this.ordenForm.controls).forEach(key => {
        this.ordenForm.get(key)?.markAsTouched();
      });
      return;
    }

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser?.id) {
      this.errorMessage.set('No se pudo obtener el usuario actual');
      return;
    }

    const formValue = this.ordenForm.value;
    
    const ordenRequest: OrdenTrabajoRequest = {
      requestDto: {
        nuevoCliente: true,
        clienteId: undefined, // No se usa para cliente nuevo
        nombreCliente: formValue.nombreCliente,
        clienteTelefono: formValue.clienteTelefono ? parseInt(formValue.clienteTelefono) : undefined,
        citaProgramadaInicio: new Date(formValue.citaProgramadaInicio).toISOString(),
        citaProgramadaFin: formValue.citaProgramadaFin ? new Date(formValue.citaProgramadaFin).toISOString() : undefined,
        modalidad: formValue.modalidad,
        tipoOrden: formValue.tipoOrden,
        estado: formValue.estado,
        prioridad: formValue.prioridad,
        notas: formValue.notas || undefined,
        ubicacionText: formValue.ubicacionText || undefined,
        requiereFactura: formValue.requiereFactura,
        costoEstimado: formValue.costoEstimado || undefined,
        creadoPorUserId: currentUser.id
      }
    };

    this.guardandoOrden.set(true);
    this.errorMessage.set(null);

    this.recepcionService.crearOrden(ordenRequest).subscribe({
      next: (ordenCreada) => {
        console.log('Orden creada exitosamente:', ordenCreada);
        this.dialogRef.close(ordenCreada);
      },
      error: (error) => {
        console.error('Error al crear la orden:', error);
        this.errorMessage.set(error.error?.mensaje || 'Error al crear la orden');
        this.guardandoOrden.set(false);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
