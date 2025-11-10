import { Component, Input, Output, EventEmitter, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Vehiculo,
  VehiculoCreateDto,
  VehiculoUpdateDto
} from '../../../../../core/models/vehiculo.interface';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';
import { SecureAuthService } from '../../../../../core/services/secure-auth.service';
import { NotificationService } from '../../../../../core/services/notification.service';

@Component({
  selector: 'app-vehiculos-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vehiculos-dialog.component.html',
  styleUrl: './vehiculos-dialog.component.css'
})
export class VehiculosDialogComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);
  private readonly authService = inject(SecureAuthService);
  private readonly notificationService = inject(NotificationService);

  // Inputs para controlar visibilidad de modales
  @Input() mostrarModalCrear = false;
  @Input() mostrarModalEditar = false;

  // Input para vehiculo a editar
  @Input() vehiculoSeleccionado: Vehiculo | null = null;

  // Outputs para eventos
  @Output() cerrarModal = new EventEmitter<string>();
  @Output() vehiculoCreado = new EventEmitter<void>();
  @Output() vehiculoActualizado = new EventEmitter<void>();

  // Estados
  cargando = signal(false);
  error = signal<string | null>(null);
  exito = signal<string | null>(null);

  // Formularios como signals
  formularioCrear = signal<Partial<VehiculoCreateDto>>({
    nombreVehiculo: '',
    tipoVehiculo: null,
    transmision: null,
    esDeEmpresa: true,
    placas: '',
    kilometraje: null,
    activo: true,
    observaciones: null
  });

  formularioEditar = signal<Partial<VehiculoUpdateDto>>({
    kilometraje: undefined,
    placas: '',
    activo: true
  });

  ngOnInit(): void {
    // Si hay vehiculo seleccionado, pre-llenar formulario de edición
    if (this.vehiculoSeleccionado) {
      this.formularioEditar.set({
        kilometraje: this.vehiculoSeleccionado.kilometraje || undefined,
        placas: this.vehiculoSeleccionado.placas,
        activo: this.vehiculoSeleccionado.activo
      });
    }
  }

  /**
   * Cierra el modal actual
   */
  cerrar(): void {
    this.cerrarModal.emit('cerrar');
    this.resetFormularios();
  }

  /**
   * Resetea los formularios y estados
   */
  private resetFormularios(): void {
    this.formularioCrear.set({
      nombreVehiculo: '',
      tipoVehiculo: null,
      transmision: null,
      esDeEmpresa: true,
      placas: '',
      kilometraje: null,
      activo: true,
      observaciones: null
    });
    this.formularioEditar.set({
      kilometraje: null,
      placas: '',
      activo: true
    });
    this.error.set(null);
    this.exito.set(null);
  }

  /**
   * Crea un nuevo vehiculo
   */
  async crearVehiculo(): Promise<void> {
    const formData = this.formularioCrear();

    // Validación
    if (!formData.nombreVehiculo?.trim()) {
      this.error.set('El nombre del vehículo es requerido');
      return;
    }

    if (!formData.placas?.trim()) {
      this.error.set('Las placas son requeridas');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO
      const dto: VehiculoCreateDto = {
        nombreVehiculo: formData.nombreVehiculo.trim(),
        tipoVehiculo: formData.tipoVehiculo || null,
        transmision: formData.transmision || null,
        esDeEmpresa: formData.esDeEmpresa ?? true,
        placas: formData.placas.trim(),
        kilometraje: formData.kilometraje ?? null,
        activo: formData.activo ?? true,
        observaciones: formData.observaciones || null
      };

      // Hacer POST
      await this.vehiculoService.createVehiculo(dto).toPromise();

      this.exito.set('✅ Vehículo creado exitosamente');
      this.notificationService.success('Vehículo creado exitosamente');
      setTimeout(() => {
        this.vehiculoCreado.emit();
        this.cerrar();
      }, 1500);

    } catch (error: any) {
      console.error('Error creando vehículo:', error);
      const mensaje = error.error?.message || 'Error al crear el vehículo';
      this.error.set(mensaje);
      this.notificationService.error(mensaje);
    } finally {
      this.cargando.set(false);
    }
  }

  /**
   * Actualiza un vehiculo existente (solo campos permitidos)
   */
  async actualizarVehiculo(): Promise<void> {
    if (!this.vehiculoSeleccionado) return;

    const formData = this.formularioEditar();

    // Validación - solo permitir actualizar kilometraje y activo
    // Las placas están deshabilitadas y se mantienen igual
    if (formData.kilometraje !== null && formData.kilometraje !== undefined && formData.kilometraje < 0) {
      this.error.set('El kilometraje no puede ser negativo');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO con solo campos permitidos (kilometraje y activo)
      // Las placas se mantienen igual ya que están deshabilitadas
      const dto: VehiculoUpdateDto = {
        kilometraje: formData.kilometraje,
        placas: this.vehiculoSeleccionado.placas, // Mantener placas originales
        activo: formData.activo ?? true
      };

      // Hacer PUT/PATCH
      await this.vehiculoService.updateVehiculo(this.vehiculoSeleccionado.id, dto).toPromise();

      this.exito.set('✅ Vehículo actualizado exitosamente');
      this.notificationService.success('Vehículo actualizado exitosamente');
      setTimeout(() => {
        this.vehiculoActualizado.emit();
        this.cerrar();
      }, 1500);

    } catch (error: any) {
      console.error('Error actualizando vehículo:', error);
      const mensaje = error.error?.message || 'Error al actualizar el vehículo';
      this.error.set(mensaje);
      this.notificationService.error(mensaje);
    } finally {
      this.cargando.set(false);
    }
  }
}
