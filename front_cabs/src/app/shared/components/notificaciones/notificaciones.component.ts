// =====================================================================================
// COMPONENTE NOTIFICACIONES - notificaciones.component.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Componente que muestra las notificaciones del sistema en un dropdown.
// Integra SignalR para recibir notificaciones en tiempo real.
//
// FUNCIONALIDADES:
// - Dropdown con lista de notificaciones
// - Badge con contador de no leídas
// - Marcar como leída al hacer clic
// - Notificaciones en tiempo real vía SignalR
// - Estados de conexión SignalR
//
// =====================================================================================

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificacionesService } from '../../../core/services/notificaciones.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { Notificacion } from '../../../core/models/notificacion.interface';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';

@Component({
  selector: 'app-notificaciones',
  standalone: true,
  imports: [CommonModule, ClickOutsideDirective],
  templateUrl: './notificaciones.component.html',
  styleUrls: ['./notificaciones.component.css']
})
export class NotificacionesComponent implements OnInit, OnDestroy {
  notificaciones: Notificacion[] = [];
  mostrarDropdown = false;
  notificacionesNoLeidas = 0;
  connectionState: 'connecting' | 'connected' | 'disconnected' | 'reconnecting' = 'disconnected';

  private subscriptions: Subscription = new Subscription();

  constructor(
    private notificacionesService: NotificacionesService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    // Suscribirse a cambios en notificaciones
    this.subscriptions.add(
      this.notificacionesService.notificaciones.subscribe(notificaciones => {
        this.notificaciones = notificaciones;
        this.notificacionesNoLeidas = notificaciones.filter(n => !n.leida).length;
      })
    );

    // Suscribirse al estado de conexión SignalR
    this.subscriptions.add(
      this.signalRService.connectionState.subscribe(state => {
        this.connectionState = state;
      })
    );

    // Cargar notificaciones iniciales
    this.cargarNotificaciones();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  toggleDropdown() {
    this.mostrarDropdown = !this.mostrarDropdown;
    if (this.mostrarDropdown) {
      this.cargarNotificaciones();
    }
  }

  cargarNotificaciones() {
    this.notificacionesService.getNotificaciones().subscribe({
      next: (notificaciones) => {
        console.log('📋 Notificaciones cargadas:', notificaciones.length);
      },
      error: (error) => {
        console.error('❌ Error al cargar notificaciones:', error);
      }
    });
  }

  marcarComoLeida(notificacion: Notificacion, event: Event) {
    event.stopPropagation(); // Evitar que se cierre el dropdown

    if (!notificacion.leida) {
      this.notificacionesService.marcarComoLeida(notificacion.id).subscribe({
        next: () => {
          console.log('✅ Notificación marcada como leída:', notificacion.id);
        },
        error: (error) => {
          console.error('❌ Error al marcar notificación como leída:', error);
        }
      });
    }
  }

  getPrioridadClass(prioridad: string): string {
    switch (prioridad) {
      case 'ALTA':
        return 'border-l-4 border-red-500 bg-red-50';
      case 'MEDIA':
        return 'border-l-4 border-yellow-500 bg-yellow-50';
      case 'BAJA':
        return 'border-l-4 border-green-500 bg-green-50';
      default:
        return 'border-l-4 border-gray-500 bg-gray-50';
    }
  }

  getConnectionStatusClass(): string {
    const classes = {
      'connected': 'text-green-600',
      'connecting': 'text-yellow-600',
      'reconnecting': 'text-orange-600',
      'disconnected': 'text-red-600'
    };
    return classes[this.connectionState] || 'text-gray-600';
  }

  getConnectionStatusText(): string {
    switch (this.connectionState) {
      case 'connected':
        return '🟢 Conectado';
      case 'connecting':
        return '🟡 Conectando...';
      case 'reconnecting':
        return '🟠 Reconectando...';
      case 'disconnected':
        return '🔴 Desconectado';
      default:
        return '⚪ Desconocido';
    }
  }
}
