import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { ClientesCompletosService } from '../../../services/clientes-completos.service';
import { RecepcionService } from '../../../services/recepcion.service';
import { SecureAuthService } from '../../../../../core/services/secure-auth.service';
import { ClienteCompleto } from '../../../../../core/models/cliente-completo.interface';
import { OrdenTrabajoRequest } from '../../../../../core/models/orden-trabajo.interface';
import { Modalidad, TipoOrden, EstadoOrden } from '../../../../../core/enums/estado-orden.enum';

@Component({
  selector: 'app-nueva-orden-cliente-legacy',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './nueva-orden-cliente-legacy.component.html',
  styleUrl: './nueva-orden-cliente-legacy.component.css'
})
export class NuevaOrdenClienteLegacyComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NuevaOrdenClienteLegacyComponent>);
  private clientesService = inject(ClientesCompletosService);
  private recepcionService = inject(RecepcionService);
  private authService = inject(SecureAuthService);

  ordenForm!: FormGroup;
  clientesEncontrados = signal<ClienteCompleto[]>([]);
  clienteSeleccionado = signal<ClienteCompleto | null>(null);
  buscandoClientes = signal(false);
  guardandoOrden = signal(false);
  errorMessage = signal<string | null>(null);

  // Enums para el template
  modalidades = Object.values(Modalidad);
  tiposOrden = Object.values(TipoOrden);
  estados = Object.values(EstadoOrden);

  ngOnInit(): void {
    this.initForm();
    this.setupClienteSearch();
  }

  private initForm(): void {
    this.ordenForm = this.fb.group({
      // Búsqueda de cliente
      busquedaCliente: ['', [Validators.required, Validators.minLength(3)]],
      clienteId: [null, Validators.required],
      
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

  private setupClienteSearch(): void {
    this.ordenForm.get('busquedaCliente')?.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < 3) {
          this.clientesEncontrados.set([]);
          return of([]);
        }
        
        this.buscandoClientes.set(true);
        this.errorMessage.set(null);
        
        return this.clientesService.buscarPorNombre(term, 1, 10).pipe(
          catchError(error => {
            console.error('Error buscando clientes:', error);
            this.errorMessage.set('Error al buscar clientes');
            this.buscandoClientes.set(false);
            return of({ data: [], paginaActual: 1, totalPaginas: 0, totalRegistros: 0, tieneSiguiente: false, tieneAnterior: false });
          })
        );
      })
    ).subscribe(response => {
      if (Array.isArray(response)) {
        this.clientesEncontrados.set(response);
      } else if (response && 'data' in response) {
        this.clientesEncontrados.set(response.data);
      }
      this.buscandoClientes.set(false);
    });
  }

  onClienteSeleccionado(cliente: ClienteCompleto): void {
    this.clienteSeleccionado.set(cliente);
    this.ordenForm.patchValue({
      clienteId: cliente.clienteId,
      busquedaCliente: cliente.nombreComercial || ''
    });
    this.clientesEncontrados.set([]); // Limpiar resultados después de seleccionar
  }

  displayCliente(cliente: ClienteCompleto): string {
    return cliente ? (cliente.nombreComercial || '') : '';
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
        nuevoCliente: false,
        clienteId: formValue.clienteId,
        nombreCliente: undefined, // No se usa para cliente legacy
        clienteTelefono: undefined, // No se usa para cliente legacy
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

  limpiarSeleccion(): void {
    this.clienteSeleccionado.set(null);
    this.ordenForm.patchValue({
      clienteId: null,
      busquedaCliente: ''
    });
  }
}
