import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'; // Importa ReactiveFormsModule
import { VehiculoService } from '../../../soporte/services/vehiculo.service';
import { Vehiculo, VehiculoCreateDto, VehiculoUpdateDto } from '../../../../core/models/vehiculo.interface';

@Component({
  selector: 'app-vehiculos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Usa ReactiveFormsModule
  templateUrl: './vehiculos.component.html',
  styleUrls: ['./vehiculos.component.css'] // Apunta al CSS
})
export class VehiculosComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);
  private fb = inject(FormBuilder); // Inyecta FormBuilder

  // Signals para estado reactivo
  vehiculos = signal<Vehiculo[]>([]);
  vehiculoSeleccionado = signal<Vehiculo | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Filtros
  filtroTermino = signal<string>('');

  // Modal states
  mostrarModal = signal(false);
  modoModal = signal<'crear' | 'editar'>('crear');

  // Formulario Reactivo
  formularioVehiculo: FormGroup;

  constructor() {
    // Inicializa el formulario (coincide con tu modal y BD)
    this.formularioVehiculo = this.fb.group({
      placas: ['', [Validators.required, Validators.maxLength(20)]],
      tipoVehiculo: ['Automóvil', [Validators.required, Validators.maxLength(50)]],
      transmision: ['Manual', [Validators.required]],
      esDeEmpresa: [true, Validators.required],
      activo: [true, Validators.required],
      observaciones: ['']
    });
  }

  ngOnInit(): void {
    this.cargarVehiculos();
  }

  cargarVehiculos(): void {
    this.cargando.set(true);
    this.error.set(null);
    const filtros: { [key: string]: string } = {};
    if (this.filtroTermino()) {
      filtros['termino'] = this.filtroTermino(); // Tu API debe poder filtrar por 'termino'
    }

    this.vehiculoService.getVehiculos(filtros).subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.cargando.set(false);
      },
      error: (err) => this.handleError('Error al cargar los vehículos.', err),
    });
  }

  abrirModalCrear(): void {
    this.formularioVehiculo.reset({
      placas: '',
      tipoVehiculo: 'Automóvil',
      transmision: 'Manual',
      esDeEmpresa: true,
      activo: true,
      observaciones: ''
    });
    this.modoModal.set('crear');
    this.vehiculoSeleccionado.set(null);
    this.mostrarModal.set(true);
    this.error.set(null); // Limpia error al abrir
  }

  abrirModalEditar(vehiculo: Vehiculo): void {
    this.formularioVehiculo.patchValue(vehiculo); // Carga los datos en el form
    this.modoModal.set('editar');
    this.vehiculoSeleccionado.set(vehiculo);
    this.mostrarModal.set(true);
    this.error.set(null); // Limpia error al abrir
  }

  cerrarModal(): void {
    this.mostrarModal.set(false);
    this.error.set(null); // Limpia error al cerrar
  }

  guardarVehiculo(): void {
    if (this.formularioVehiculo.invalid) {
      this.formularioVehiculo.markAllAsTouched();
      this.error.set('Por favor, completa los campos requeridos.');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    const dto = this.formularioVehiculo.value as VehiculoCreateDto;

    const obs = this.modoModal() === 'crear'
      ? this.vehiculoService.createVehiculo(dto)
      : this.vehiculoService.updateVehiculo(this.vehiculoSeleccionado()!.id, dto as VehiculoUpdateDto);

    obs.subscribe({
      next: () => {
        this.cargando.set(false);
        this.cerrarModal();
        this.cargarVehiculos(); // Recarga la lista
      },
      // Muestra el error en el modal (setGlobalError = false)
      error: (err) => this.handleError('Error al guardar el vehículo.', err, false),
    });
  }

  onEliminar(vehiculo: Vehiculo): void {
    if (confirm(`¿Estás seguro de eliminar el vehículo ${vehiculo.placas}?`)) {
      this.cargando.set(true); // Activa el spinner principal
      this.vehiculoService.deleteVehiculo(vehiculo.id).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cargarVehiculos();
          if (this.vehiculoSeleccionado()?.id === vehiculo.id) {
            this.cerrarModal(); // Cierra el modal si se borró el que se estaba editando
          }
        },
        error: (err) => this.handleError('Error al eliminar el vehículo.', err),
      });
    }
  }

  // Helper para mostrar errores en el formulario
  campoInvalido(campo: string): boolean {
    const control = this.formularioVehiculo.get(campo);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  /**
   * Manejador de errores
   * @param message Mensaje amigable para el usuario
   * @param error Error original
   * @param setGlobalError Si es 'true', muestra el error en la página. Si es 'false', lo prepara para el modal.
   */
  private handleError(message: string, error: any, setGlobalError: boolean = true): void {
    const apiError = error?.error?.message || error?.error || error?.message || 'Error desconocido';
    const fullMessage = `${message} Detalles: ${apiError}`;
    
    // Si el modal está abierto, siempre muestra el error ahí.
    if (this.mostrarModal()) {
      this.error.set(fullMessage);
    } else {
      // Si el modal está cerrado, usa el parámetro global
      if (setGlobalError) {
        this.error.set(fullMessage);
      }
    }
    
    this.cargando.set(false);
    console.error(message, error);
  }
}