import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router'; // 👈 1. IMPORTAR ROUTER
import { ReparacionService } from '../../../../../core/services/reparacion.service';
import { ReparacionComponente, ReparacionComponenteDto } from '../../../../../core/models/reparacion-componente.interface';

@Component({
  selector: 'app-reparacion-componentes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reparacion-componentes.component.html'
})
export class ReparacionComponentesComponent implements OnInit {
  private readonly reparacionService = inject(ReparacionService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute); // 👈 2. Para leer el ID de la URL
  private router = inject(Router);        // 👈 3. Para navegar atrás

  // Ya no es fijo, inicia en 0 y se llena desde la URL
  reparacionIdPadre = 0; 

  componentes = signal<ReparacionComponente[]>([]);
  compSeleccionado = signal<ReparacionComponente | null>(null);
  cargando = signal(false);
  error = signal<string | null>(null);
  
  mostrarModal = signal(false);
  modoModal = signal<'crear' | 'editar'>('crear');

  formComponente: FormGroup;

  constructor() {
    this.formComponente = this.fb.group({
      reparacionId: [0, [Validators.required]], // Se actualiza en ngOnInit
      componente: ['', [Validators.required]],
      cantidad: [1, [Validators.required, Validators.min(1)]],
      proveedor: [''],
      garantiaMeses: [0],
      costoUnitarioCompra: [0, [Validators.required]],
      costoUnitarioPublico: [0, [Validators.required]],
      notas: ['']
    });
  }

  ngOnInit(): void {
    // 👇 4. LEER EL ID DE LA REPARACIÓN DESDE LA URL
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.reparacionIdPadre = +id; // El '+' convierte string a número
        this.cargarComponentes();
      } else {
        // Si no hay ID, regresamos a la lista principal por seguridad
        this.volver();
      }
    });
  }

  cargarComponentes(): void {
    this.cargando.set(true);
    // Usamos el ID dinámico capturado
    this.reparacionService.getComponentes({ reparacionId: this.reparacionIdPadre }).subscribe({
      next: (data) => {
        this.componentes.set(data);
        this.cargando.set(false);
      },
      error: (err) => this.handleError('Error al cargar componentes', err)
    });
  }

  abrirModalCrear(): void {
    this.formComponente.reset({
      reparacionId: this.reparacionIdPadre, // 👈 Usamos el ID dinámico
      componente: '',
      cantidad: 1,
      proveedor: '',
      garantiaMeses: 0,
      costoUnitarioCompra: 0,
      costoUnitarioPublico: 0,
      notas: ''
    });
    this.modoModal.set('crear');
    this.compSeleccionado.set(null);
    this.mostrarModal.set(true);
  }

  abrirModalEditar(comp: ReparacionComponente): void {
    this.formComponente.patchValue(comp);
    this.modoModal.set('editar');
    this.compSeleccionado.set(comp);
    this.mostrarModal.set(true);
  }

  guardarComponente(): void {
    if (this.formComponente.invalid) {
      this.formComponente.markAllAsTouched();
      return;
    }

    this.cargando.set(true);
    // Aseguramos que el ID padre siempre vaya correcto
    this.formComponente.patchValue({ reparacionId: this.reparacionIdPadre });
    
    const dto = this.formComponente.value as ReparacionComponenteDto;

    const obs = this.modoModal() === 'crear'
      ? this.reparacionService.createComponente(dto)
      : this.reparacionService.updateComponente(this.compSeleccionado()!.id, dto);

    obs.subscribe({
      next: () => {
        this.cargando.set(false);
        this.mostrarModal.set(false);
        this.cargarComponentes();
      },
      error: (err) => this.handleError('Error al guardar componente', err)
    });
  }

  // 👇 5. FUNCIÓN PARA EL BOTÓN "VOLVER"
  volver(): void {
    this.router.navigate(['/modulesShared/reparaciones']);
  }

  private handleError(msg: string, error: any): void {
    this.cargando.set(false);
    let detalle = error?.error?.message || error?.message || 'Error desconocido';
    if (error?.error?.errors) {
        detalle = Object.keys(error.error.errors).map(k => `${k}: ${error.error.errors[k]}`).join(' | ');
    }
    this.error.set(`${msg}: ${detalle}`);
    setTimeout(() => this.error.set(null), 5000); 
  }
}