import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'; // Importa ReactiveFormsModule
import { Router, ActivatedRoute } from '@angular/router';
import { VehiculoService } from '../../../../core/services/vehiculo.service';
import { Vehiculo, VehiculoCreateDto, VehiculoUpdateDto } from '../../../../core/models/vehiculo.interface';
import { VehiculosDialogComponent } from './vehiculos-dialog/vehiculos-dialog.component';

@Component({
  selector: 'app-vehiculos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, VehiculosDialogComponent], // Usa ReactiveFormsModule
  templateUrl: './vehiculos.component.html',
  styleUrls: ['./vehiculos.component.css'] // Apunta al CSS
})
export class VehiculosComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private fb = inject(FormBuilder); // Inyecta FormBuilder

  // Signals para estado reactivo
  vehiculos = signal<Vehiculo[]>([]);
  vehiculoSeleccionado = signal<Vehiculo | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Filtros
  filtroTermino = signal<string>('');

  // Computed values para estadísticas
  vehiculosActivos = computed(() => this.vehiculos().filter(v => v.activo).length);

  // Modal states
  mostrarModal = signal(false);
  modoModal = signal<'crear' | 'editar'>('crear');
  mostrarPanelDetalles = signal(false);

  // Formulario Reactivo
  formularioVehiculo: FormGroup;

  constructor() {
    // Inicializa el formulario con los nuevos campos
    this.formularioVehiculo = this.fb.group({
      nombreVehiculo: ['', [Validators.required, Validators.maxLength(100)]],
      placas: ['', [Validators.required, Validators.maxLength(20)]],
      tipoVehiculo: ['Automóvil', [Validators.required, Validators.maxLength(50)]],
      transmision: ['Manual', [Validators.required]],
      esDeEmpresa: [true, Validators.required],
      activo: [true, Validators.required],
      kilometraje: [null],
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
      nombreVehiculo: '',
      placas: '',
      tipoVehiculo: 'Automóvil',
      transmision: 'Manual',
      esDeEmpresa: true,
      activo: true,
      kilometraje: null,
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

  cerrarModal(event?: string): void {
    this.mostrarModal.set(false);
    this.vehiculoSeleccionado.set(null);
    this.modoModal.set('crear');
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

    if (this.modoModal() === 'crear') {
      // Para crear, envía todos los campos
      const dto = this.formularioVehiculo.value as VehiculoCreateDto;
      this.vehiculoService.createVehiculo(dto).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cerrarModal();
          this.cargarVehiculos(); // Recarga la lista
        },
        error: (err) => this.handleError('Error al guardar el vehículo.', err, false),
      });
    } else {
      // Para editar, solo envía los campos permitidos: kilometraje, placas y activo
      const formValue = this.formularioVehiculo.value;
      const dto: VehiculoUpdateDto = {
        placas: formValue.placas,
        activo: formValue.activo,
        kilometraje: formValue.kilometraje
      };

      this.vehiculoService.updateVehiculo(this.vehiculoSeleccionado()!.id, dto).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cerrarModal();
          this.cargarVehiculos(); // Recarga la lista
        },
        error: (err) => this.handleError('Error al guardar el vehículo.', err, false),
      });
    }
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

  /**
   * Handlers para eventos del componente dialogs
   */
  onVehiculoCreado(): void {
    this.cargarVehiculos();
    this.mostrarModal.set(false);
    this.vehiculoSeleccionado.set(null);
    this.modoModal.set('crear');
  }

  onVehiculoActualizado(): void {
    this.cargarVehiculos();
    this.mostrarModal.set(false);
    this.vehiculoSeleccionado.set(null);
    this.modoModal.set('crear');
  }

  /**
   * Métodos para el panel lateral de detalles
   */
  seleccionarVehiculo(vehiculo: Vehiculo): void {
    this.vehiculoSeleccionado.set(vehiculo);
  }

  verDetalles(vehiculo: Vehiculo): void {
    // Obtener detalles completos del vehículo desde la API
    this.vehiculoService.getVehiculoById(vehiculo.id).subscribe({
      next: (vehiculoCompleto) => {
        this.vehiculoSeleccionado.set(vehiculoCompleto);
        this.mostrarPanelDetalles.set(true);
      },
      error: (err) => {
        console.error('Error al obtener detalles del vehículo:', err);
        this.error.set('Error al cargar los detalles del vehículo');
      }
    });
  }

  cerrarPanelDetalles(): void {
    this.mostrarPanelDetalles.set(false);
    this.vehiculoSeleccionado.set(null);
  }

  verHistorial(vehiculo: Vehiculo): void {
    this.router.navigate(['historial', vehiculo.id], { relativeTo: this.route });
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