import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router'; // 👈 1. IMPORTAR ROUTER
import { ReparacionService } from '../../../../core/services/reparacion.service';
import { Reparacion, ReparacionDto } from '../../../../core/models/reparacion.interface';

@Component({
  selector: 'app-reparaciones',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reparaciones.component.html',
  styleUrls: ['./reparaciones.component.css']
})
export class ReparacionesComponent implements OnInit {
  private readonly reparacionService = inject(ReparacionService);
  private fb = inject(FormBuilder);
  private router = inject(Router); // 👈 2. INYECTAR ROUTER

  // Signals para estado reactivo
  reparaciones = signal<Reparacion[]>([]);
  reparacionSeleccionada = signal<Reparacion | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);

  // Filtros
  filtroTermino = signal<string>('');

  // Modal states
  mostrarModal = signal(false);
  modoModal = signal<'crear' | 'editar'>('crear');

  // Formulario Reactivo
  formularioReparacion: FormGroup;

  constructor() {
    // Inicializa el formulario
    this.formularioReparacion = this.fb.group({
      ordenId: [8, [Validators.required]], 
      tecnicoId: [4], 
      dispositivoTipo: ['', [Validators.required]],
      marca: ['', [Validators.required]],
      modelo: ['', [Validators.required]],
      accesoriosRecibidos: [''],
      descripcionFalla: ['', [Validators.required]],
      diagnostico: [''],
      solucionAplicada: [''],
      resultado: [''],
      causaIrreparable: [''],
      respaldoDatosAutorizado: [false],
      garantiaDias: [30], 
      tipoEntrega: ['RECOGE_CLIENTE', [Validators.required]], 
      ubicacionAlmacenamiento: [''],
      notas: [''],
      costoManoObra: [0],
      costoRefaccionesCompra: [0],
      costoRefaccionesPublico: [0],
      costoTotalPublico: [0] 
    });
  }

  ngOnInit(): void {
    this.cargarReparaciones();
  }

  cargarReparaciones(): void {
    this.cargando.set(true);
    this.error.set(null);
    const filtros: { [key: string]: string } = {};
    
    if (this.filtroTermino()) {
      filtros['termino'] = this.filtroTermino();
    }

    this.reparacionService.getReparaciones(filtros).subscribe({
      next: (data) => {
        this.reparaciones.set(data);
        this.cargando.set(false);
      },
      error: (err) => this.handleError('Error al cargar las reparaciones.', err),
    });
  }

  // 👇 3. NUEVO MÉTODO PARA NAVEGAR A PIEZAS
  irAComponentes(reparacion: Reparacion): void {
    // Navega a: /modulesShared/reparaciones/{ID}/componentes
    this.router.navigate(['/modulesShared/reparaciones', reparacion.id, 'componentes']);
  }

  abrirModalCrear(): void {
    this.formularioReparacion.reset({
      ordenId: 8, 
      tecnicoId: 4, 
      dispositivoTipo: '',
      marca: '',
      modelo: '',
      respaldoDatosAutorizado: false,
      garantiaDias: 30,
      tipoEntrega: 'RECOGE_CLIENTE', 
      resultado: 'Pendiente', 
      costoManoObra: 0,
      costoRefaccionesCompra: 0,
      costoRefaccionesPublico: 0,
      costoTotalPublico: 0
    });
    this.modoModal.set('crear');
    this.reparacionSeleccionada.set(null);
    this.mostrarModal.set(true);
    this.error.set(null);
  }

  abrirModalEditar(reparacion: Reparacion): void {
    this.formularioReparacion.patchValue(reparacion);
    this.modoModal.set('editar');
    this.reparacionSeleccionada.set(reparacion);
    this.mostrarModal.set(true);
    this.error.set(null);
  }

  cerrarModal(): void {
    this.mostrarModal.set(false);
    this.error.set(null);
  }

  guardarReparacion(): void {
    if (this.formularioReparacion.invalid) {
      this.formularioReparacion.markAllAsTouched();
      this.error.set('Por favor, completa los campos requeridos.');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    const dto = this.formularioReparacion.value as ReparacionDto;

    const obs = this.modoModal() === 'crear'
      ? this.reparacionService.createReparacion(dto)
      : this.reparacionService.updateReparacion(this.reparacionSeleccionada()!.id, dto);

    obs.subscribe({
      next: () => {
        this.cargando.set(false);
        this.cerrarModal();
        this.cargarReparaciones(); 
      },
      error: (err) => this.handleError('Error al guardar la reparación.', err, false),
    });
  }

  onEliminar(reparacion: Reparacion): void {
    if (confirm(`¿Estás seguro de eliminar la reparación del ${reparacion.dispositivoTipo} ${reparacion.marca}?`)) {
      this.cargando.set(true);
      this.reparacionService.deleteReparacion(reparacion.id).subscribe({
        next: () => {
          this.cargando.set(false);
          this.cargarReparaciones();
          if (this.reparacionSeleccionada()?.id === reparacion.id) {
            this.cerrarModal();
          }
        },
        error: (err) => this.handleError('Error al eliminar la reparación.', err),
      });
    }
  }

  campoInvalido(campo: string): boolean {
    const control = this.formularioReparacion.get(campo);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  private handleError(message: string, error: any, setGlobalError: boolean = true): void {
    let apiError = 'Error desconocido';

    if (error?.error?.errors) {
      const validationErrors = error.error.errors;
      const errorMessages = Object.keys(validationErrors).map(key => {
        return `${key}: ${validationErrors[key].join(', ')}`;
      });
      apiError = errorMessages.join(' | ');
    } 
    else if (error?.error?.message) {
      apiError = error.error.message;
    } 
    else if (typeof error?.error === 'string') {
      apiError = error.error;
    }
    else if (error?.message) {
      apiError = error.message;
    }

    const fullMessage = `${message} Detalles: ${apiError}`;
    
    if (this.mostrarModal()) {
      this.error.set(fullMessage);
    } else {
      if (setGlobalError) {
        this.error.set(fullMessage);
      }
    }
    
    this.cargando.set(false);
    console.error(message, error);
  }
}