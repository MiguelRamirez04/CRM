// =====================================================================================
// COMPONENTE HEADER - header.component.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE COMPONENTE?
// Header principal de la aplicación con notificaciones y menú de usuario.
// Se integra en el layout principal del dashboard.
//
// FUNCIONALIDADES:
// - Barra de navegación superior
// - Componente de notificaciones
// - Menú de usuario
// - Información del usuario actual
//
// =====================================================================================

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificacionesComponent } from '../../shared/components/notificaciones/notificaciones.component';
import { ClickOutsideDirective } from '../../shared/directives/click-outside.directive';
import { SignalRService } from '../../core/services/signalr.service';
import { SecureAuthService } from '../../core/services/secure-auth.service';
import { UiIconComponent } from "../../shared/atoms/icono/icono.component";
import { UitipografiaComponent } from "../../shared/atoms/tipografia/tipografia.component";
import { UiAvatarComponent } from "../../shared/atoms/avatar/avatar.component";
import { User } from '../../core/services/secure-auth.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificacionesComponent, ClickOutsideDirective, UiIconComponent, UitipografiaComponent, UiAvatarComponent,],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  usuarioActual: User | null = null;
  mostrarMenuUsuario = false;
  signalRConectado = false;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private router: Router,
    private authService: SecureAuthService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    // Obtener información del usuario actual
    this.usuarioActual = this.authService.getCurrentUser();

    // Suscribirse al estado de SignalR
    this.subscriptions.add(
      this.signalRService.connectionState.subscribe(state => {
        this.signalRConectado = state === 'connected';
      })
    );

    // Iniciar conexión SignalR si hay usuario
    if (this.usuarioActual?.id) {
      this.iniciarSignalR();
    }
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  private async iniciarSignalR() {
    try {
      console.log('🔍 Usuario actual en header:', this.usuarioActual);
      console.log('🔍 Tipo de usuarioActual.id:', typeof this.usuarioActual?.id);
      console.log('🔍 Valor de usuarioActual.id:', this.usuarioActual?.id);

      if (!this.usuarioActual?.id) {
        console.error('❌ No hay usuario ID válido para SignalR');
        return;
      }

      // Asegurar que el ID sea un número válido y positivo
      const userId = Number(this.usuarioActual.id);
      if (isNaN(userId) || userId <= 0) {
        console.error('❌ El usuario ID no es un número válido o es <= 0:', this.usuarioActual.id, 'parsed:', userId);
        return;
      }

      console.log('📡 Iniciando SignalR con userId:', userId, 'tipo:', typeof userId);
      await this.signalRService.startConnection(userId);
      console.log('✅ SignalR conectado en header');
    } catch (error) {
      console.error('❌ Error conectando SignalR en header:', error);
    }
  }

  toggleMenuUsuario() {
    this.mostrarMenuUsuario = !this.mostrarMenuUsuario;
  }

  async logout() {
    try {
      // Detener SignalR antes de logout
      if (this.usuarioActual?.id) {
        await this.signalRService.stopConnection(this.usuarioActual.id);
      }

      // Limpiar notificaciones
      // this.notificacionesService.clearNotificaciones();

      // Realizar logout
      this.authService.logout();
      this.router.navigate(['/login']);
    } catch (error) {
      console.error('Error durante logout:', error);
      // Forzar logout aunque haya error
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }

  // Método para cerrar menú cuando se hace clic fuera
  onClickOutside() {
    this.mostrarMenuUsuario = false;
  }

  /* 
  * Metodo para leer el nombre 
  */
  getFullName(user: User | null): string {
    if (!user) return 'Usuario';
    if (user.nombre && user.apellido) return `${user.nombre} ${user.apellido}`;
    if (user.nombreCompleto) return user.nombreCompleto;
    if (user.name) return user.name;
    return 'Usuario';
  }  


  private rolesMap: Record<number, string> = {
  1: 'Administración',
  2: 'Soporte',
  3: 'Recepción'
};
  /*
  * Metodo para leer el rol 
  */
  getUserRole(user: User | null): string {
    if (!user) return 'Sin rol';

    const capitalize = (text: string) =>
      text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();

    // Caso: rol numérico
    if (typeof user.rol === 'number' && user.rol !== null) {
      const rolTexto = this.rolesMap[user.rol] || 'Sin rol';
      return capitalize(rolTexto);
    }

    // Caso: rol como string
    if (typeof user.role === 'string') {
      return capitalize(user.role);
    }

    return 'Sin rol';
  }
}
