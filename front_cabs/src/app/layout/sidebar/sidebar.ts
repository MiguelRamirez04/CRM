import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SecureAuthService, User } from '../../../../src/app/core/services/secure-auth.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BandejaPerfilComponent } from '../bandeja-perfil/bandeja-perfil.component';

type IconType = 
  | 'resumen' 
  | 'evaluaciones' 
  | 'reparaciones' 
  | 'ordenes' 
  | 'clientes' 
  | 'cotizaciones'
  | 'viaticos'
  | 'vehiculos'
  | 'calendario' 
  | 'notificaciones' 
  | 'configuracion' 
  | 'tutorial'
  | 'ayuda';

interface NavItem {
  label: string;
  icon: IconType;
  link?: string;
  notify?: boolean;
  roles?: string[]; // Roles que tienen acceso
  children?: NavItem[]; // Submenu items
  expanded?: boolean; // Estado de expansión del submenu
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule,BandejaPerfilComponent],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class Sidebar implements OnInit {
  @Input() activeLabel: string | null = null;

  isCollapsed = false;
  mostrarBandejaPerfil = false;

  user$: Observable<User | null>;
  currentUserRole$: Observable<string | null>;

  // Navegación completa con control de acceso por roles
  private readonly allNavItems: NavItem[] = [
    {
      label: 'Resumen',
      icon: 'resumen',
      link: '/administracion',
      roles: ['ADMINISTRACION']
    },
    {
      label: 'Evaluaciones',
      icon: 'evaluaciones',
      link: '/dashboard/evaluaciones',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE'],
      children: [
      /* {
          label: 'Detalles de Evaluación',
          icon: 'evaluaciones',
          link: '/evaluaciones/detalles',
          roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
        }*/
      ]
    },
    {
      label: 'Reparaciones',
      icon: 'reparaciones',
      link: '/modulesShared/reparaciones', 
      children: [
        {
          label: 'Detalles de Reparación',
          icon: 'reparaciones',
          link: '/reparaciones/detalles',
          roles: ['ADMINISTRACION', 'SOPORTE']
        }
      ]
    },
    {
      label: 'Órdenes de Trabajo',
      icon: 'ordenes',
      link: '/recepcion/ordenes-trabajo',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE'],
      children: [
        {
          label: 'Ejecuciones de Orden',
          icon: 'ordenes',
          link: '/recepcion/ordenes-trabajo/ejecuciones',
          roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
        }
      ]
    },
    {
      label: 'Clientes Legacy',
      icon: 'clientes',
      link: '/recepcion/clientes-completos',
      roles: ['ADMINISTRACION', 'RECEPCION']
    },
    {
      label: 'Cotizaciones',
      icon: 'cotizaciones',
      link: '/dashboard/cotizaciones',
      roles: ['ADMINISTRACION', 'RECEPCION']
    },
    {
      label: 'Viáticos',
      icon: 'viaticos',
      link: '/modulesShared/viaticos',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
    },
    {
      label: 'Vehículos',
      icon: 'vehiculos',
      link: '/modulesShared/vehiculos',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
    },
    {
      label: 'Calendario',
      icon: 'calendario',
      link: '/dashboard/calendario',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE'],
      notify: true
    }
  ];

  private readonly secondaryNavItems: NavItem[] = [
    {
      label: 'Notificaciones',
      icon: 'notificaciones',
      link: '/notificaciones',
      notify: true,
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
    },
    {
      label: 'Configuración',
      icon: 'configuracion',
      link: '/configuracion',
      roles: ['ADMINISTRACION']
    },
    {
      label: 'Tutorial',
      icon: 'tutorial',
      link: '/tutorial',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
    },
    {
      label: 'Ayuda',
      icon: 'ayuda',
      link: '/dashboard/centrodeayuda',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE']
    }
  ];

  mainNav: NavItem[] = [];
  secondaryNav: NavItem[] = [];

  constructor(
    private authService: SecureAuthService,
    private router: Router
  ) {
    this.user$ = this.authService.currentUser$;
    this.currentUserRole$ = this.user$.pipe(
      map(user => user?.role || null)
    );
  }

  ngOnInit(): void {
    // Filtrar navegación según el rol del usuario
    this.currentUserRole$.subscribe(role => {
      if (role) {
        this.mainNav = this.filterNavByRole(this.allNavItems, role);
        this.secondaryNav = this.filterNavByRole(this.secondaryNavItems, role);
      }
    });
  }

  /**
   * Filtra los items de navegación según el rol del usuario
   */
  private filterNavByRole(items: NavItem[], userRole: string): NavItem[] {
    return items
      .filter(item => !item.roles || item.roles.includes(userRole))
      .map(item => ({
        ...item,
        children: item.children
          ? this.filterNavByRole(item.children, userRole)
          : undefined
      }));
  }

  /**
   * Toggle submenu expansion
   */
  toggleSubmenu(item: NavItem, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (item.children && item.children.length > 0) {
      item.expanded = !item.expanded;
    }
  }

  /**
   * Verifica si un item tiene hijos
   */
  hasChildren(item: NavItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  /**
   * Verifica si la ruta actual coincide con el link
   */
  isActive(link?: string): boolean {
    if (!link) return false;
    return this.router.url === link;
  }


  /**
   * Verifica si algún hijo está activo
   */
  hasActiveChild(item: NavItem): boolean {
    if (!item.children) return false;
    return item.children.some(child => this.isActive(child.link));
  }

  /**
   * Obtiene las iniciales del usuario
   */
  getInitials(user: User | null): string {
    if (!user) return 'U';
    
    if (user.nombre && user.apellido) {
      return `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`.toUpperCase();
    }
    
    if (user.nombreCompleto) {
      const parts = user.nombreCompleto.split(' ');
      if (parts.length >= 2) {
        return `${parts[0].charAt(0)}${parts[1].charAt(0)}`.toUpperCase();
      }
      return parts[0].substring(0, 2).toUpperCase();
    }
    
    if (user.name) {
      const parts = user.name.split(' ');
      return parts.map(p => p.charAt(0)).join('').substring(0, 2).toUpperCase();
    }
    
    return 'U';
  }

  /**
   * Toggle sidebar collapsed state
   */
  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }

  /**
   * Toggle bandeja perfil
   */
  toggleBandejaPerfil(): void {
    this.mostrarBandejaPerfil = !this.mostrarBandejaPerfil;
  }
}
