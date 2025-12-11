// =====================================================================================
// COMPONENTE HEADER - header.component.ts
// =====================================================================================

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, NgClass  } from '@angular/common';
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
import { UiDividerComponent } from "../../shared/atoms/linea/linea.component";

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificacionesComponent, ClickOutsideDirective, UiIconComponent, UitipografiaComponent, UiAvatarComponent, UiDividerComponent, NgClass],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  usuarioActual: User | null = null;
  mostrarMenuSignalR = false;
  mostrarMenuUsuario = false;
  
  // Estado del usuario en SignalR
  estadoUsuario: 'conectado' | 'descanso' | 'desconectado' = 'conectado';

  private subscriptions: Subscription = new Subscription();

  constructor(
    private router: Router,
    private authService: SecureAuthService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.usuarioActual = this.authService.getCurrentUser();

    this.subscriptions.add(
      this.signalRService.connectionState.subscribe(state => {
        // Solo actualizar a 'desconectado' si realmente se desconecta
        if (state === 'disconnected') {
          this.estadoUsuario = 'desconectado';
        }
      })
    );

    if (this.usuarioActual?.id) {
      this.iniciarSignalR();
    }
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  private async iniciarSignalR() {
    try {
      if (!this.usuarioActual?.id) return;
      const userId = Number(this.usuarioActual.id);
      if (isNaN(userId) || userId <= 0) return;

      await this.signalRService.startConnection(userId);
      this.estadoUsuario = 'conectado';
    } catch (error) {
      console.error('❌ Error conectando SignalR en header:', error);
      this.estadoUsuario = 'desconectado';
    }
  }

  toggleMenuUsuario() {
    this.mostrarMenuUsuario = !this.mostrarMenuUsuario;
  }

  toggleMenuSignalR(){
    this.mostrarMenuSignalR = !this.mostrarMenuSignalR;
  }

  async logout() {
    try {
      if (this.usuarioActual?.id) {
        await this.signalRService.stopConnection(this.usuarioActual.id);
      }
    } catch (error) {
      console.error('Error durante logout SignalR:', error);
    } finally {
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }

  onClickOutsideUsuario() {
    this.mostrarMenuUsuario = false;
  }
  
  onClicOutsideSignalR(){
    this.mostrarMenuSignalR = false;
  }

  // Cambiar estado de usuario (conectado/descanso)
  cambiarEstadoUsuario(estado: 'conectado' | 'descanso') {
    this.estadoUsuario = estado;
    console.log('📡 Estado de usuario cambiado a:', estado);
    
    // Aquí podrías llamar a SignalR para notificar el cambio de estado
    // Ejemplo: this.signalRService.notifyUserStatus(estado);
  }

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

  getUserRole(user: User | null): string {
    if (!user) return 'Sin rol';
    const capitalize = (text: string) =>
      text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();

    if (typeof user.rol === 'number' && user.rol !== null) {
      const rolTexto = this.rolesMap[user.rol] || 'Sin rol';
      return capitalize(rolTexto);
    }

    if (typeof user.role === 'string') {
      return capitalize(user.role);
    }

    return 'Sin rol';
  }
}