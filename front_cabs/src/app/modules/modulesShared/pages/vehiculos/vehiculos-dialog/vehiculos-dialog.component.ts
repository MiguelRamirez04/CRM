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
    tipoVehiculo: '',
    transmision: '',
    esDeEmpresa: true,
    placas: '',
    kilometraje: 0,
    activo: true,
    observaciones: ''
  });

  formularioEditar = signal<Partial<VehiculoUpdateDto>>({
    kilometraje: 0,
    placas: '',
    observaciones: ''
  });

  ngOnInit(): void {
    // Si hay vehiculo seleccionado, pre-llenar formulario de edición
    if (this.vehiculoSeleccionado) {
      this.formularioEditar.set({
        kilometraje: this.vehiculoSeleccionado.kilometraje,
        placas: this.vehiculoSeleccionado.placas || '',
        observaciones: this.vehiculoSeleccionado.observaciones || ''
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
      tipoVehiculo: '',
      transmision: '',
      esDeEmpresa: true,
      placas: '',
      kilometraje: 0,
      activo: true,
      observaciones: ''
    });
    this.formularioEditar.set({
      kilometraje: 0,
      placas: '',
      observaciones: ''
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
        tipoVehiculo: formData.tipoVehiculo?.trim() || '',
        transmision: formData.transmision?.trim() || '',
        esDeEmpresa: formData.esDeEmpresa ?? true,
        placas: formData.placas.trim(),
        kilometraje: formData.kilometraje ?? 0,
        activo: formData.activo ?? true,
        observaciones: formData.observaciones?.trim() || ''
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
   * Actualiza un vehiculo existente (kilometraje obligatorio, placas y observaciones opcionales)
   */
  async actualizarVehiculo(): Promise<void> {
    if (!this.vehiculoSeleccionado) return;

    const formData = this.formularioEditar();

    // Validación - kilometraje es obligatorio y debe ser >= 0
    if (formData.kilometraje === null || formData.kilometraje === undefined || formData.kilometraje < 0) {
      this.error.set('El kilometraje es obligatorio y no puede ser negativo');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO con campos permitidos
      const dto: VehiculoUpdateDto = {
        kilometraje: formData.kilometraje,
        placas: formData.placas?.trim() || undefined,
        observaciones: formData.observaciones?.trim() || undefined
      };

      // Hacer PUT
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
