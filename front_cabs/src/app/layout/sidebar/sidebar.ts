import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BandejaPerfilComponent } from '../bandeja-perfil/bandeja-perfil.component';
import { SecureAuthService, User } from '../../../../src/app/core/services/secure-auth.service';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';

type NavItem = {
  label: string;
  icon: 'resumen' | 'registros' | 'reportes' | 'usuarios' | 'calendario' | 'notificaciones' | 'configuracion' | 'tutorial';
  link?: string;
  notify?: boolean;
};

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, BandejaPerfilComponent],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class SidebarComponent {
  @Input() activeLabel: string | null = null;

  isCollapsed = false;
  mostrarBandejaPerfil = false;

  user$: Observable<User | null>;

<<<<<<< HEAD
  // 👇 ============ SECCIÓN MODIFICADA ============ 👇
  mainNav: NavItem[] = [
    { label: 'Resumen', icon: 'resumen', link: '/dashboard' },
    // CORREGIDO: Apunta a la ruta del dashboard de recepción
    { label: 'Registros', icon: 'registros', link: '/recepcion' }, 
    // NUEVO: Enlace a la nueva bandeja que creamos
    { label: 'Bandeja', icon: 'reportes', link: '/recepcion/bandeja' }, 
    { label: 'Reportes', icon: 'reportes', link: '/dashboard/profile' },
    { label: 'Usuarios', icon: 'usuarios', link: '/dashboard/usuarios' },
    { label: 'Calendario', icon: 'calendario', link: '/dashboard/calendario', notify: true },
  ];
  // 👆 ============ FIN DE SECCIÓN MODIFICADA ============ 👆
=======
  mainNav: NavItem[] = [
    { label: 'Resumen', icon: 'resumen', link: '/dashboard' },
    { label: 'Registros', icon: 'registros', link: 'src/app/modules/recepcion/pages/dashboard' },
    { label: 'Reportes', icon: 'reportes', link: '/dashboard/profile' },
    { label: 'Usuarios', icon: 'usuarios', link: '/dashboard/usuarios' },
    { label: 'Calendario', icon: 'calendario', link: '/dashboard/calendario', notify: true},
  ];
>>>>>>> d708b7e (🎨 Modificando estilos del navbar)

  secondaryNav: NavItem[] = [
    { label: 'Notificaciones', icon: 'notificaciones', link: '/dashboard/notificaciones', notify: true },
    { label: 'Configuración', icon: 'configuracion', link: '/dashboard/configuracion' },
    { label: 'Tutorial', icon: 'tutorial', link: '/dashboard/tutorial' },
  ];

  getInitials(user: User | null): string {
    if (!user) return '';
    if (user.nombre && user.apellido) {
      return `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`.toUpperCase();
    }
    if (user.name) {
      const parts = user.name.split(' ');
      return parts.map(p => p.charAt(0)).join('').toUpperCase();
    }
    return '';
  }

constructor(
  private authService: SecureAuthService,
  private router: Router
) {
  this.user$ = this.authService.currentUser$;
}

<<<<<<< HEAD
  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }
  

  toggleBandejaPerfil(): void {
    this.mostrarBandejaPerfil = !this.mostrarBandejaPerfil;
  }

  isActive(link?: string): boolean {
    return !!link && this.router.url === link;
  }
=======
  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }
  

  toggleBandejaPerfil(): void {
    this.mostrarBandejaPerfil = !this.mostrarBandejaPerfil;
  }
  isActive(link?: string): boolean {
    return !!link && this.router.url === link;
  }
>>>>>>> d708b7e (🎨 Modificando estilos del navbar)
}
