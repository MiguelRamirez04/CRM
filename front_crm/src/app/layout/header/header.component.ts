import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ClickOutsideDirective } from '../../shared/directives/click-outside.directive';
import { SecureAuthService } from '../../core/services/secure-auth.service';
import { UiIconComponent } from "../../shared/atoms/icono/icono.component";
import { UitipografiaComponent } from "../../shared/atoms/tipografia/tipografia.component";
import { UiAvatarComponent } from "../../shared/atoms/avatar/avatar.component";
import { User } from '../../core/services/secure-auth.service';
import { RouterModule } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { toSignal } from '@angular/core/rxjs-interop';


@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ClickOutsideDirective, UiIconComponent, UitipografiaComponent, UiAvatarComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  usuarioActual: User | null = null;
  mostrarMenuSignalR = false;
  mostrarMenuUsuario = false;
  isDarkMode = false;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private router: Router,
    private authService: SecureAuthService,
    private themeService: ThemeService
  ) { 
    this.isDarkMode = this.themeService.darkMode();
  }

  ngOnInit() {
    this.usuarioActual = this.authService.getCurrentUser();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  toggleMenuUsuario() {
    this.mostrarMenuUsuario = !this.mostrarMenuUsuario;
  }

  toggleMenuSignalR() {
    this.mostrarMenuSignalR = !this.mostrarMenuSignalR;
  }

  toggleTheme() {
    this.themeService.toggle();
    this.isDarkMode = this.themeService.darkMode();
  }

  async logout() {
    try {
      this.authService.logout();
      this.router.navigate(['/login']);
    } catch (error) {
      console.error('Error durante logout:', error);
    }
  }

  onClickOutsideUsuario() {
    this.mostrarMenuUsuario = false;
  }

  onClicOutsideSignalR() {
    this.mostrarMenuSignalR = false;
  }

  getFullName(user: User | null): string {
    if (!user) return 'Usuario';
    if (user.nombre && user.apellido) return `${user.nombre} ${user.apellido} `;
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