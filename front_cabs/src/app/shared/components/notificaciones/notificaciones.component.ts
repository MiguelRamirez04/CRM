// ============================================================
// COMPONENTE DE NOTIFICACIONES - notificaciones.component.ts
// ============================================================
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificacionesService } from '../../../core/services/notificaciones.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { Notificacion } from '../../../core/models/notificacion.interface';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';
import { UiIconComponent } from '../../atoms/icono/icono.component';
import { UitipografiaComponent } from "../../~exports/detail-view.index";
import { NOTIFICACIONES_FAKE } from './notificaciones.fake';

@Component({
  selector: 'app-notificaciones',
  standalone: true,
  imports: [CommonModule, ClickOutsideDirective, UiIconComponent, UitipografiaComponent],
  templateUrl: './notificaciones.component.html'
})
export class NotificacionesComponent implements OnInit, OnDestroy {
  // Estado del componente
  notificaciones: Notificacion[] = [];
  mostrarDropdown = false;
  notificacionesNoLeidas = 0;  
  // Modo prueba para desarrollo
  private modoPrueba = true; // Cambia a false cuando tengas backend
  private subscriptions = new Subscription();

  constructor(
    private notificacionesService: NotificacionesService,
    private signalRService: SignalRService
  ) {}

  // INICIALIZACIÓN
  ngOnInit() {
    if (this.modoPrueba) {
      // Usar datos falsos para desarrollo
      this.notificaciones = NOTIFICACIONES_FAKE;
      this.actualizarContador();
      console.log('🔔 Modo prueba activado - Usando datos falsos');
    } else {
      // Modo producción con backend real
      this.suscribirNotificaciones();
      this.cargarNotificaciones();
    }
  }

  // LIMPIEZA
  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  // SUSCRIPCIONES
  private suscribirNotificaciones() {
    this.subscriptions.add(
      this.notificacionesService.notificaciones.subscribe(notificaciones => {
        this.notificaciones = notificaciones;
        this.actualizarContador();
      })
    );
  }


  private actualizarContador() {
    this.notificacionesNoLeidas = this.notificaciones.filter(n => !n.leida).length;
  }

  // UI - DROPDOWN
  toggleDropdown() {
    this.mostrarDropdown = !this.mostrarDropdown;
    if (this.mostrarDropdown) {
      console.log('📂 Dropdown abierto -', this.notificacionesNoLeidas, 'no leídas');
    }
  }

  // DATOS
  cargarNotificaciones() {
    if (this.modoPrueba) return; // No cargar en modo prueba
    
    this.notificacionesService.getNotificaciones().subscribe({
      error: (error) => console.error('Error cargando notificaciones:', error)
    });
  }

  // ACCIONES
  marcarComoLeida(notificacion: Notificacion) {
    if (notificacion.leida) return;
    
    if (this.modoPrueba) {
      // Simular en modo prueba
      notificacion.leida = true;
      this.actualizarContador();
      console.log('✅ Notificación marcada como leída:', notificacion.titulo);
    } else {
      this.notificacionesService.marcarComoLeida(notificacion.id).subscribe({
        error: (error) => console.error('Error marcando como leída:', error)
      });
    }
  }

  ejecutarAccion(notificacion: Notificacion) {
    console.log('🎯 Acción ejecutada:', notificacion.accion);
    
    // Simular navegación (cambiar según tu router)
    if (notificacion.accion) {
      alert(`Navegando a: ${notificacion.accion}`);
      // this.router.navigate([notificacion.accion]);
    }
    
    // Opcional: cerrar dropdown después de acción
    // this.mostrarDropdown = false;
  }
}