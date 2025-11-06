import { Component, Input, Output, EventEmitter, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  EjecucionOrdenResponse,
  EjecucionOrdenCreateDto,
  EjecucionOrdenUpdateDto,
  DelegateEjecucionDto,
  TipoEjecucion,
  OrdenTrabajoSimple,
  TecnicoSimple,
  VehiculoSimple
} from '../../../../../core/models/ejecucion-orden.interface';
import { EjecucionOrdenService } from '../../../../../core/services/ejecucion-orden.service';
import { SecureAuthService } from '../../../../../core/services/secure-auth.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../../environments/environment';

interface UsuarioSoporte {
  id: number;
  nombre: string;
  apellido: string;
  nombreCompleto: string;
  email: string;
  activo: boolean;
}

@Component({
  selector: 'app-ejecuciones-orden-dialogs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ejecuciones-orden-dialogs.component.html',
  styleUrl: './ejecuciones-orden-dialogs.component.css'
})
export class EjecucionesOrdenDialogsComponent implements OnInit {
  private readonly ejecucionService = inject(EjecucionOrdenService);
  private readonly authService = inject(SecureAuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly http = inject(HttpClient);

  // Inputs para controlar visibilidad de modales
  @Input() mostrarModalCrear = false;
  @Input() mostrarModalFinalizar = false;
  @Input() mostrarModalDelegar = false;

  // Inputs para datos necesarios
  @Input() ejecucionSeleccionada: EjecucionOrdenResponse | null = null;
  @Input() ordenes: OrdenTrabajoSimple[] = [];
  @Input() tecnicos: TecnicoSimple[] = [];
  @Input() vehiculos: VehiculoSimple[] = [];
  @Input() ordenIdPrefill: number = 0; // Para pre-llenar cuando viene de orden

  // Outputs para eventos
  @Output() cerrarModal = new EventEmitter<string>();
  @Output() ejecucionCreada = new EventEmitter<void>();
  @Output() ejecucionFinalizada = new EventEmitter<void>();
  @Output() ejecucionDelegada = new EventEmitter<void>();

  // Estados
  cargando = signal(false);
  error = signal<string | null>(null);
  exito = signal<string | null>(null);

  // Formularios como signals
  formularioCrear = signal<Partial<EjecucionOrdenCreateDto>>({
    ordenId: 0,
    tecnicoId: 0,
    tipoEjecucion: TipoEjecucion.CAMPO,
    hrInicio: new Date().toISOString(),
    comentarios: ''
  });

  formularioFinalizar = signal<Partial<EjecucionOrdenUpdateDto>>({
    hrFin: new Date().toISOString(),
    kmFinal: undefined,
    comentarios: ''
  });

  formularioDelegacion = signal<Partial<DelegateEjecucionDto>>({
    nuevoTecnicoId: 0,
    motivo: ''
  });

  // Lista de usuarios SOPORTE para delegación
  usuariosSoporte = signal<UsuarioSoporte[]>([]);

  // Exponer enums para template
  TipoEjecucion = TipoEjecucion;

  ngOnInit(): void {
    // Pre-llenar ordenId si viene de ordenes
    if (this.ordenIdPrefill > 0) {
      this.formularioCrear.update(f => ({ ...f, ordenId: this.ordenIdPrefill }));
    }

    // Cargar usuarios SOPORTE para delegación
    this.cargarUsuariosSoporte();
  }

  /**
   * Carga usuarios con rol SOPORTE desde la nueva API
   */
  private cargarUsuariosSoporte(): void {
    this.http.get<{ success: boolean; data: UsuarioSoporte[] }>(`${environment.apiUrl}/api/auth/usuarios/soporte`)
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.usuariosSoporte.set(response.data);
            console.log('✅ Usuarios SOPORTE cargados:', response.data.length);
          }
        },
        error: (err) => {
          console.error('❌ Error cargando usuarios SOPORTE:', err);
        }
      });
  }

  /**
   * Cambia el tipo de ejecución y limpia campos condicionales
   */
  cambiarTipoEjecucion(tipo: TipoEjecucion): void {
    this.formularioCrear.update(f => ({
      ...f,
      tipoEjecucion: tipo,
      // Limpiar campos de CAMPO si cambia a REMOTO
      vehiculoId: tipo === TipoEjecucion.CAMPO ? f.vehiculoId : undefined,
      kmInicial: tipo === TipoEjecucion.CAMPO ? f.kmInicial : undefined,
      // Limpiar campos de REMOTO si cambia a CAMPO
      herramientas: tipo === TipoEjecucion.REMOTO ? f.herramientas : undefined,
      codigoSesion: tipo === TipoEjecucion.REMOTO ? f.codigoSesion : undefined,
      contrasenaSesion: tipo === TipoEjecucion.REMOTO ? f.contrasenaSesion : undefined
    }));
  }

  /**
   * Crea una nueva ejecución
   */
  async crearEjecucion(): Promise<void> {
    const formData = this.formularioCrear();

    // Validación
    if (!formData.ordenId || !formData.tecnicoId) {
      this.error.set('Complete los campos requeridos: Orden y Técnico');
      return;
    }

    if (!formData.hrInicio) {
      this.error.set('La fecha y hora de inicio es requerida');
      return;
    }

    // Validar que hrInicio no sea fecha futura
    const fechaInicio = new Date(formData.hrInicio);
    const ahora = new Date();
    if (fechaInicio > ahora) {
      this.error.set('La fecha y hora de inicio no puede ser futura');
      return;
    }

    if (formData.tipoEjecucion === TipoEjecucion.CAMPO && !formData.vehiculoId) {
      this.error.set('Para ejecución en CAMPO, seleccione un vehículo');
      return;
    }

    if (formData.tipoEjecucion === TipoEjecucion.REMOTO && !formData.herramientas?.trim()) {
      this.error.set('Para ejecución REMOTO, especifique las herramientas/software utilizado');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO
      const dto: EjecucionOrdenCreateDto = {
        ordenId: formData.ordenId!,
        tecnicoId: formData.tecnicoId!,
        tipoEjecucion: formData.tipoEjecucion!,
        hrInicio: formData.hrInicio!,
        comentarios: formData.comentarios || undefined,
        vehiculoId: formData.tipoEjecucion === TipoEjecucion.CAMPO ? formData.vehiculoId : undefined,
        kmInicial: formData.tipoEjecucion === TipoEjecucion.CAMPO ? formData.kmInicial : undefined,
        herramientas: formData.tipoEjecucion === TipoEjecucion.REMOTO ? formData.herramientas : undefined,
        codigoSesion: formData.tipoEjecucion === TipoEjecucion.REMOTO ? formData.codigoSesion : undefined,
        contrasenaSesion: formData.tipoEjecucion === TipoEjecucion.REMOTO ? formData.contrasenaSesion : undefined
      };

      // Hacer POST
      await this.ejecucionService.createEjecucion(dto).toPromise();

      this.exito.set('✅ Ejecución creada exitosamente');
      this.notificationService.success('Ejecución creada exitosamente');
      setTimeout(() => {
        this.ejecucionCreada.emit();
        this.cerrar();
      }, 1500);
    } catch (err: any) {
      console.error('❌ Error creando ejecución:', err);
      const errorMessage = err.error?.message || 'Error al crear la ejecución';
      this.error.set(errorMessage);
      this.notificationService.error(errorMessage);
    } finally {
      this.cargando.set(false);
    }
  }

  /**
   * Finaliza una ejecución
   */
  async finalizarEjecucion(): Promise<void> {
    if (!this.ejecucionSeleccionada) return;

    const formData = this.formularioFinalizar();

    // Validación
    if (!formData.hrFin) {
      this.error.set('Complete la fecha/hora de finalización');
      return;
    }

    if (this.ejecucionSeleccionada.tipoEjecucion === TipoEjecucion.CAMPO && !formData.kmFinal) {
      this.error.set('Para ejecución en CAMPO, ingrese el kilometraje final');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO
      const dto: EjecucionOrdenUpdateDto = {
        hrFin: formData.hrFin!,
        kmFinal: this.ejecucionSeleccionada.tipoEjecucion === TipoEjecucion.CAMPO ? formData.kmFinal : undefined,
        comentarios: formData.comentarios || undefined
      };

      // Hacer PUT
      await this.ejecucionService.updateEjecucion(this.ejecucionSeleccionada.id, dto).toPromise();

      this.exito.set('✅ Ejecución finalizada exitosamente');
      this.notificationService.success('Ejecución finalizada exitosamente');
      setTimeout(() => {
        this.ejecucionFinalizada.emit();
        this.cerrar();
      }, 1500);
    } catch (err: any) {
      console.error('❌ Error finalizando ejecución:', err);
      const errorMessage = err.error?.message || 'Error al finalizar la ejecución';
      this.error.set(errorMessage);
      this.notificationService.error(errorMessage);
    } finally {
      this.cargando.set(false);
    }
  }

  /**
   * Delega una ejecución
   */
  async delegarEjecucion(): Promise<void> {
    if (!this.ejecucionSeleccionada) return;

    const formData = this.formularioDelegacion();

    // Validación
    if (!formData.nuevoTecnicoId || !formData.motivo) {
      this.error.set('Complete todos los campos: Nuevo técnico y Motivo');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.exito.set(null);

    try {
      // Obtener token CSRF
      await this.authService.obtenerCsrfToken().toPromise();

      // Construir DTO
      const dto: DelegateEjecucionDto = {
        nuevoTecnicoId: formData.nuevoTecnicoId!,
        motivo: formData.motivo!
      };

      // Hacer PATCH
      await this.ejecucionService.delegateEjecucion(this.ejecucionSeleccionada.id, dto).toPromise();

      this.exito.set('✅ Ejecución delegada exitosamente');
      this.notificationService.success('Ejecución delegada exitosamente');
      setTimeout(() => {
        this.ejecucionDelegada.emit();
        this.cerrar();
      }, 1500);
    } catch (err: any) {
      console.error('❌ Error delegando ejecución:', err);
      const errorMessage = err.error?.message || 'Error al delegar la ejecución';
      this.error.set(errorMessage);
      this.notificationService.error(errorMessage);
    } finally {
      this.cargando.set(false);
    }
  }

  /**
   * Cierra el modal actual
   */
  cerrar(): void {
    let modalType = '';
    if (this.mostrarModalCrear) modalType = 'crear';
    else if (this.mostrarModalFinalizar) modalType = 'finalizar';
    else if (this.mostrarModalDelegar) modalType = 'delegar';
    
    this.cerrarModal.emit(modalType);
    this.limpiarEstados();
  }

  /**
   * Limpia estados y formularios
   */
  private limpiarEstados(): void {
    this.error.set(null);
    this.exito.set(null);
    this.cargando.set(false);
  }
}
