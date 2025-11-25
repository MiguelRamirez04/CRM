import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrdenTrabajoRequest, EstadoOrden, TipoOrden, Modalidad, ClienteLegacy } from '../../../../core/models/orden-trabajo.interface';
import { Cliente } from '../../../../core/models/cliente.interface';
import { ClienteSearchComponent } from '../cliente-search/cliente-search.component';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-orden-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ClienteSearchComponent],
  templateUrl: './orden-form.component.html',
  styleUrl: './orden-form.component.css'
})
export class OrdenFormComponent implements OnInit {
  @Input() loading = false;
  @Output() guardar = new EventEmitter<OrdenTrabajoRequest>();
  @Output() cancelar = new EventEmitter<void>();

  form: FormGroup;
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);

  // Enums para el template
  EstadoOrden = Object.values(EstadoOrden);
  TipoOrden = Object.values(TipoOrden);
  Modalidad = Object.values(Modalidad);

  constructor() {
    this.form = this.createForm();
  }

  ngOnInit() {
    this.setupFormBasedOnTipoCliente();
    this.setCurrentUser();
  }

  private setCurrentUser(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      console.log('Usuario actual:', currentUser);
      this.form.patchValue({
        creadoPorUserId: currentUser.id
      });
    } else {
      console.warn('No se encontró usuario logueado');
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      nuevoCliente: [true, Validators.required],
      clienteId: [null],
      nombreCliente: ['', Validators.required],
      clienteTelefono: ['', [Validators.pattern(/^\d{10}$/)]],
      tipoOrden: [TipoOrden.SERVICIO, Validators.required],
      modalidad: [Modalidad.PRESENCIAL, Validators.required],
      prioridad: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      estado: [EstadoOrden.CAPTURADA, Validators.required],
      citaProgramadaInicio: ['', Validators.required],
      citaProgramadaFin: [''],
      ubicacionText: [''],
      notas: [''],
      cotizaciones: [''],
      estadoFacturado: ['NO_FACTURADO'],
      RequiereFactura: [false],
      FacturaFolio: [0],
      costoEstimado: [0, [Validators.min(0)]],
      costoReal: [0, [Validators.min(0)]],
      creadoPorUserId: [null]
    });
  }

  setupFormBasedOnTipoCliente() {
    const nuevoCliente = this.form.get('nuevoCliente')?.value;
    const nombreClienteControl = this.form.get('nombreCliente');
    const clienteIdControl = this.form.get('clienteId');
    const clienteTelefonoControl = this.form.get('clienteTelefono');

    if (nuevoCliente) {
      nombreClienteControl?.setValidators([Validators.required, Validators.minLength(2)]);
      clienteIdControl?.clearValidators();
      clienteIdControl?.setValue(null);
      // Teléfono opcional para cliente nuevo
      clienteTelefonoControl?.setValidators([Validators.pattern(/^\d{10}$/)]);
    } else {
      nombreClienteControl?.clearValidators();
      clienteIdControl?.setValidators(Validators.required);
      nombreClienteControl?.setValue('');
      clienteTelefonoControl?.clearValidators();
      clienteTelefonoControl?.setValue('');
    }
    nombreClienteControl?.updateValueAndValidity();
    clienteIdControl?.updateValueAndValidity();
    clienteTelefonoControl?.updateValueAndValidity();
  }

  onTipoClienteChange() {
    this.setupFormBasedOnTipoCliente();
  }

  onClienteSeleccionado(cliente: Cliente) {
    this.form.patchValue({
      clienteId: cliente.id,
      nombreCliente: cliente.nombre // Guardar nombre para visualización
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.markFormGroupTouched();
      return;
    }

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) {
      console.error('No hay usuario logueado. No se puede crear la orden.');
      alert('Error: No hay usuario logueado. Por favor, inicie sesión nuevamente.');
      return;
    }

    const formValue = this.form.value;
    const ordenRequest: OrdenTrabajoRequest = {
      requestDto: {
        notas: formValue.notas || '',
        citaProgramadaInicio: formValue.citaProgramadaInicio ? new Date(formValue.citaProgramadaInicio).toISOString() : new Date().toISOString(),
        citaProgramadaFin: formValue.citaProgramadaFin ? new Date(formValue.citaProgramadaFin).toISOString() : undefined,
        modalidad: formValue.modalidad,
        tipoOrden: formValue.tipoOrden,
        cotizaciones: formValue.cotizaciones || '',
        nuevoCliente: formValue.nuevoCliente,
        nombreCliente: formValue.nuevoCliente ? formValue.nombreCliente : '',
        clienteId: formValue.nuevoCliente ? 0 : (formValue.clienteId || 0),
        clienteTelefono: formValue.nuevoCliente && formValue.clienteTelefono ? Number(formValue.clienteTelefono) : undefined,
        prioridad: formValue.prioridad,
        estado: formValue.estado,
        ubicacionText: formValue.ubicacionText || '',
        estadoFacturado: formValue.estadoFacturado || 'NO_FACTURADO',
        requiereFactura: formValue.RequiereFactura,
        facturaFolio: formValue.FacturaFolio || 0,
        costoReal: formValue.costoReal || 0,
        costoEstimado: formValue.costoEstimado || 0,
        creadoPorUserId: formValue.creadoPorUserId || currentUser.id
      }
    };
    this.guardar.emit(ordenRequest);
  }

  onCancelar() {
    this.cancelar.emit();
  }

  private markFormGroupTouched() {
    this.form.markAllAsTouched();
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.form.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    if (field.errors['required']) return 'Este campo es requerido.';
    if (field.errors['minlength']) return `Mínimo ${field.errors['minlength'].requiredLength} caracteres.`;
    if (field.errors['min']) return `El valor mínimo es ${field.errors['min'].min}.`;
    if (field.errors['max']) return `El valor máximo es ${field.errors['max'].max}.`;
    if (field.errors['pattern'] && fieldName === 'clienteTelefono') {
      return 'Teléfono inválido. Debe ser exactamente 10 dígitos numéricos.';
    }
    return 'Campo inválido.';
  }
}
