import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrdenTrabajo, OrdenTrabajoRequest, EstadoOrden, TipoOrden, Modalidad } from '../../../../core/models/orden-trabajo/orden-trabajo.interface';
import { ClienteSearchComponent } from '../cliente-search/cliente-search.component';

@Component({
  selector: 'app-orden-form',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ClienteSearchComponent],
  templateUrl: './orden-form.component.html',
  styleUrl: './orden-form.component.css'
})
export class OrdenFormComponent implements OnInit {
  onCancelar() {
    this.cancelar.emit();
  }
  @Input() ordenEditar: OrdenTrabajo | null = null;
  @Input() tipoCliente: 'nuevo' | 'existente' = 'nuevo';
  @Input() loading = false;

  @Output() guardar = new EventEmitter<OrdenTrabajoRequest | null>();
  @Output() cancelar = new EventEmitter<void>();
  @Output() buscarCliente = new EventEmitter<string>();

  form: FormGroup;

  // Enums para el template
  EstadoOrden = EstadoOrden;
  TipoOrden = TipoOrden;
  Modalidad = Modalidad;

  constructor(private fb: FormBuilder) {
    this.form = this.createForm();
  }

  ngOnInit() {
    this.form.get('tipoCliente')?.setValue(this.tipoCliente);
    this.onTipoClienteChange(this.tipoCliente);

    if (this.ordenEditar) {
      this.loadOrdenData();
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // Cliente
      tipoCliente: ['nuevo'],
      nombreCliente: ['', [Validators.required, Validators.minLength(2)]],
      clienteId: [null],

      // Orden
      tipoOrden: [TipoOrden.ASESORIA, Validators.required],
      modalidad: [Modalidad.PRESENCIAL, Validators.required],
      prioridad: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      estado: [EstadoOrden.CAPTURADA, Validators.required],

      // Fechas
      citaProgramadaInicio: ['', Validators.required],
      citaProgramadaFin: [''],

      // Ubicación y notas
      ubicacionText: [''],
      notas: [''],

      // Facturación
      requiereFactura: [false],
      facturaFolio: [''],
      costoEstimado: [0, Validators.min(0)],
      costoReal: [0, Validators.min(0)]
    });
  }

  private loadOrdenData() {
    if (!this.ordenEditar) return;

    this.tipoCliente = this.ordenEditar.nuevoCliente ? 'nuevo' : 'existente';

    this.form.patchValue({
      tipoCliente: this.tipoCliente,
      nombreCliente: this.ordenEditar.nombreCliente,
      clienteId: this.ordenEditar.clienteId,
      tipoOrden: this.ordenEditar.tipoOrden,
      modalidad: this.ordenEditar.modalidad,
      prioridad: this.ordenEditar.prioridad,
      estado: this.ordenEditar.estado,
      citaProgramadaInicio: this.formatDateForInput(this.ordenEditar.citaProgramadaInicio),
      citaProgramadaFin: this.ordenEditar.citaProgramadaFin ? this.formatDateForInput(this.ordenEditar.citaProgramadaFin) : '',
      ubicacionText: this.ordenEditar.ubicacionText,
      notas: this.ordenEditar.notas,
      requiereFactura: this.ordenEditar.requiereFactura,
      facturaFolio: this.ordenEditar.facturaFolio,
      costoEstimado: this.ordenEditar.costoEstimado,
      costoReal: this.ordenEditar.costoReal
    });
  }

  private formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16); // YYYY-MM-DDTHH:MM
  }

  onTipoClienteChange(tipo: 'nuevo' | 'existente') {
    this.tipoCliente = tipo;
    if (tipo === 'nuevo') {
      this.form.patchValue({ clienteId: null });
    } else {
      this.form.patchValue({ nombreCliente: '' });
    }
  }

  onClienteSeleccionado(cliente: any) {
    this.form.patchValue({
      clienteId: cliente.id,
      nombreCliente: cliente.nombre
    });
  }

  onSubmit() {
    if (this.form.valid) {
      const formValue = this.form.value;

      const ordenRequest: OrdenTrabajoRequest = {
        requestDto: {
          nuevoCliente: this.tipoCliente === 'nuevo',
          nombreCliente: formValue.nombreCliente,
          clienteId: formValue.clienteId,
          tipoOrden: formValue.tipoOrden,
          modalidad: formValue.modalidad,
          prioridad: formValue.prioridad,
          estado: formValue.estado,
          citaProgramadaInicio: new Date(formValue.citaProgramadaInicio).toISOString(),
          citaProgramadaFin: formValue.citaProgramadaFin ? new Date(formValue.citaProgramadaFin).toISOString() : undefined,
          ubicacionText: formValue.ubicacionText,
          notas: formValue.notas,
          requiereFactura: formValue.requiereFactura,
          facturaFolio: formValue.facturaFolio,
          costoEstimado: formValue.costoEstimado,
          costoReal: formValue.costoReal,
          creadoPorUserId: 3 // TODO: Obtener del auth service
        }
      };

      this.guardar.emit(ordenRequest);
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched() {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      control?.markAsTouched();
    });
  }

  onBusquedaCliente(termino: string) {
    this.buscarCliente.emit(termino);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.form.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return 'Este campo es requerido';
      if (field.errors['minlength']) return 'Mínimo 2 caracteres';
      if (field.errors['min']) return 'Valor mínimo inválido';
      if (field.errors['max']) return 'Valor máximo inválido';
    }
    return '';
  }
}
