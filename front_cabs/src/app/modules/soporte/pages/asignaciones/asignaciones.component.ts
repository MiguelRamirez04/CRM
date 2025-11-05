import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'; // Importa ReactiveFormsModule
import { SoporteService } from '../../services/soporte.service'; // (Necesitarás crear este servicio)
import { VehiculoService } from '../../services/vehiculo.service'; // (Usamos el que ya creamos)
import { Vehiculo } from '../../../../core/models/vehiculo.interface'; // (Usamos la que ya creamos)

// --- Define las interfaces aquí mismo o impórtalas desde 'core/models' ---
// (Estas son versiones simplificadas de tu 'ejecucion-orden.interface.ts')
export enum TipoEjecucion {
  CAMPO = 'CAMPO',
  REMOTO = 'REMOTO',
}

export interface OrdenAsignada {
  id: number; // ID de la Orden de Trabajo
  clienteNombre: string;
  vehiculoPlacas?: string;
  tipoOrden: string;
  modalidad: string;
  estado: string; // ej. 'ASIGNADA', 'EN_CURSO'
  prioridad: number;
  // ... (cualquier otro campo que necesites mostrar en la lista)
  
  // Si ya tiene una ejecución EN CURSO, vendrá aquí
  ejecucionActiva?: {
    id: number;
    hrInicio: string;
    tipoEjecucion: TipoEjecucion;
    vehiculoId?: number;
  }
}

export interface EjecucionCreateDto {
  ordenId: number;
  tecnicoId: number; // Se obtendrá del usuario logueado
  tipoEjecucion: TipoEjecucion;
  hrInicio: string;
  vehiculoId?: number;
  kmInicial?: number;
  // ... (otros campos de 'remoto' si los necesitas)
  comentarios?: string;
}

export interface EjecucionUpdateDto {
  hrFin: string;
  kmFinal?: number;
  comentarios?: string;
}
// -----------------------------------------------------------------


@Component({
  selector: 'app-mis-asignaciones',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule], // Añade FormsModule y ReactiveFormsModule
  templateUrl: './asignaciones.component.html',
  styleUrls: ['./asignaciones.component.css']
})
export class MisAsignacionesComponent implements OnInit {
  // --- Inyección de Servicios ---
  private soporteService = inject(SoporteService); // (Deberás crear este servicio)
  private vehiculoService = inject(VehiculoService);
  private fb = inject(FormBuilder);

  // --- Signals de Estado ---
  ordenesAsignadas = signal<OrdenAsignada[]>([]);
  ordenSeleccionada = signal<OrdenAsignada | null>(null); // Para el modal de detalles
  ejecucionActiva = signal<OrdenAsignada | null>(null); // Para los modales de Iniciar/Finalizar
  
  cargando = signal(false);
  error = signal<string | null>(null);

  // --- Catálogos ---
  vehiculos = signal<Vehiculo[]>([]);
  
  // --- Estados de Modal ---
  mostrarModalDetalles = signal(false);
  mostrarModalIniciar = signal(false);
  mostrarModalFinalizar = signal(false);

  // --- Formularios ---
  formularioIniciar: FormGroup;
  formularioFinalizar: FormGroup;
  
  // Enums para el template
  readonly TipoEjecucion = TipoEjecucion;

  constructor() {
    // Formulario para INICIAR una ejecución
    this.formularioIniciar = this.fb.group({
      tipoEjecucion: [TipoEjecucion.CAMPO, Validators.required],
      vehiculoId: [null as number | null], // Opcional al inicio
      kmInicial: [null as number | null],  // Opcional al inicio
      comentarios: ['']
    });

    // Formulario para FINALIZAR una ejecución
    this.formularioFinalizar = this.fb.group({
      hrFin: [new Date().toISOString(), Validators.required],
      kmFinal: [null as number | null],
      comentarios: ['', Validators.required] // Comentarios de cierre son obligatorios
    });

    // Validación dinámica para el formulario de INICIAR
    this.formularioIniciar.get('tipoEjecucion')?.valueChanges.subscribe(tipo => {
      const vehiculoControl = this.formularioIniciar.get('vehiculoId');
      const kmInicialControl = this.formularioIniciar.get('kmInicial');

      if (tipo === TipoEjecucion.CAMPO) {
        vehiculoControl?.setValidators(Validators.required);
        kmInicialControl?.setValidators([Validators.required, Validators.min(0)]);
      } else {
        vehiculoControl?.clearValidators();
        kmInicialControl?.clearValidators();
      }
      vehiculoControl?.updateValueAndValidity();
      kmInicialControl?.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.cargarOrdenesAsignadas();
    this.cargarCatalogos();
  }

  cargarCatalogos(): void {
    this.vehiculoService.getVehiculos({ activo: 'true' }).subscribe({
      next: (data) => this.vehiculos.set(data),
      error: (err) => console.error('Error al cargar vehículos:', err)
    });
  }

  cargarOrdenesAsignadas(): void {
    this.cargando.set(true);
    this.error.set(null);
    // Este servicio debe ser creado y debe devolver solo las órdenes del técnico logueado
    this.soporteService.getMisOrdenes().subscribe({
      next: (data) => {
        this.ordenesAsignadas.set(data);
        this.cargando.set(false);
      },
      error: (err) => this.handleError('Error al cargar las asignaciones.', err),
    });
  }

  // --- Lógica de Modales ---

  abrirModalDetalles(orden: OrdenAsignada): void {
    this.ordenSeleccionada.set(orden);
    this.mostrarModalDetalles.set(true);
  }

  abrirModalIniciar(orden: OrdenAsignada): void {
    this.ejecucionActiva.set(orden);
    this.formularioIniciar.reset({
      tipoEjecucion: orden.modalidad === 'Presencial' ? TipoEjecucion.CAMPO : TipoEjecucion.REMOTO,
      vehiculoId: null,
      kmInicial: null,
      comentarios: ''
    });
    // Forzar re-validación inicial
    this.formularioIniciar.get('tipoEjecucion')?.updateValueAndValidity();
    this.mostrarModalIniciar.set(true);
  }

  abrirModalFinalizar(orden: OrdenAsignada): void {
    if (!orden.ejecucionActiva) return;
    
    this.ejecucionActiva.set(orden);
    this.formularioFinalizar.reset({
      hrFin: new Date().toISOString(),
      kmFinal: null,
      comentarios: ''
    });
    
    // Si fue en campo, hacer kmFinal requerido
    const kmFinalControl = this.formularioFinalizar.get('kmFinal');
    if (orden.ejecucionActiva.tipoEjecucion === TipoEjecucion.CAMPO) {
      kmFinalControl?.setValidators([Validators.required, Validators.min(0)]);
    } else {
      kmFinalControl?.clearValidators();
    }
    kmFinalControl?.updateValueAndValidity();
    
    this.mostrarModalFinalizar.set(true);
  }

  cerrarModal(): void {
    this.mostrarModalDetalles.set(false);
    this.mostrarModalIniciar.set(false);
    this.mostrarModalFinalizar.set(false);
    this.ordenSeleccionada.set(null);
    this.ejecucionActiva.set(null);
    this.error.set(null); // Limpia errores de modal
  }

  // --- Acciones de API ---

  iniciarEjecucion(): void {
    if (this.formularioIniciar.invalid) {
      this.formularioIniciar.markAllAsTouched();
      this.error.set('Por favor, completa los campos requeridos.');
      return;
    }
    if (!this.ejecucionActiva()) return;

    this.cargando.set(true);
    this.error.set(null);
    
    const formValue = this.formularioIniciar.value;
    const dto: EjecucionCreateDto = {
      ordenId: this.ejecucionActiva()!.id,
      tecnicoId: 0, // El backend debe tomar el ID del usuario autenticado
      tipoEjecucion: formValue.tipoEjecucion,
      hrInicio: new Date().toISOString(),
      vehiculoId: formValue.vehiculoId,
      kmInicial: formValue.kmInicial,
      comentarios: formValue.comentarios
    };

    // Este servicio debe ser creado
    this.soporteService.iniciarEjecucion(dto).subscribe({
      next: () => {
        this.cargando.set(false);
        this.cerrarModal();
        this.cargarOrdenesAsignadas(); // Recarga la lista
      },
      error: (err) => this.handleError('Error al iniciar la ejecución.', err, false),
    });
  }

  finalizarEjecucion(): void {
    if (this.formularioFinalizar.invalid) {
      this.formularioFinalizar.markAllAsTouched();
      this.error.set('Por favor, completa los campos requeridos.');
      return;
    }
    if (!this.ejecucionActiva() || !this.ejecucionActiva()!.ejecucionActiva) return;

    this.cargando.set(true);
    this.error.set(null);

    const formValue = this.formularioFinalizar.value;
    const dto: EjecucionUpdateDto = {
      hrFin: new Date(formValue.hrFin).toISOString(),
      kmFinal: formValue.kmFinal,
      comentarios: formValue.comentarios
    };
    
    const ejecucionId = this.ejecucionActiva()!.ejecucionActiva!.id;

    // Este servicio debe ser creado
    this.soporteService.finalizarEjecucion(ejecucionId, dto).subscribe({
      next: () => {
        this.cargando.set(false);
        this.cerrarModal();
        this.cargarOrdenesAsignadas(); // Recarga la lista
      },
      error: (err) => this.handleError('Error al finalizar la ejecución.', err, false),
    });
  }

  // Helper para errores de formulario
  campoInvalido(form: FormGroup, campo: string): boolean {
    const control = form.get(campo);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }
  
  private handleError(message: string, error: any, setGlobalError: boolean = true): void {
    const apiError = error?.error?.message || error?.error || error?.message || 'Error desconocido';
    const fullMessage = `${message} Detalles: ${apiError}`;
    
    if (setGlobalError || !this.mostrarModalIniciar() && !this.mostrarModalFinalizar()) {
      this.error.set(fullMessage);
    } else {
      // Muestra el error solo en el modal
      this.error.set(fullMessage);
    }
    
    this.cargando.set(false);
    console.error(message, error);
  }
}