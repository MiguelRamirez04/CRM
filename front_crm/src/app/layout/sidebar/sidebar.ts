import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SecureAuthService, User } from '../../../../src/app/core/services/secure-auth.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UiIconComponent } from "../../shared/atoms/icono/icono.component";

type IconType =
  | 'resumen'
  | 'evaluaciones'
  | 'reparaciones'
  | 'ordenes'
  | 'cotizaciones'
  | 'viaticos'
  | 'vehiculos'
  | 'calendario'
  | 'notificaciones'
  | 'configuracion'
  | 'tutorial'
  | 'usuarios'
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
  imports: [CommonModule, RouterModule, UiIconComponent],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class Sidebar implements OnInit {
  @Input() activeLabel: string | null = null;

  isCollapsed = false;
  mostrarBandejaPerfil = false;

  user$!: Observable<User | null>;
  currentUserRole$!: Observable<string | null>;

  // Navegación completa con control de acceso por roles
  private readonly allNavItems: NavItem[] = [
    {
      label: 'Resumen',
      icon: 'resumen',
      link: '/administracion/panel-control',
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
      label: 'Legacy',
      icon: 'resumen',
      children: [
        {
          label: 'Catálogos Base',
          icon: 'ordenes',
          link: '/legacy/catalogos-base',
          roles: ['ADMINISTRACION']
        },
        {
          label: 'Config. Documentos',
          icon: 'evaluaciones',
          link: '/legacy/config-documentos',
        },
        {
          label: 'Operaciones',
          icon: 'reparaciones',
          link: '/legacy/operaciones',
        },

      ],
    },
    {
      label: 'Usuarios',
      icon: 'usuarios',
      link: '/modulesShared/usuarios',
      roles: ['ADMINISTRACION'],
    },
    {
      label: 'Calendario',
      icon: 'calendario',
      link: '/dashboard/calendario',
      roles: ['ADMINISTRACION', 'RECEPCION', 'SOPORTE'],
      notify: true
    },

  ];


  mainNav: NavItem[] = [];
  secondaryNav: NavItem[] = [];

  constructor(
    private authService: SecureAuthService,
    private router: Router
  ) {
    this.user$ = this.authService.currentUser$ as Observable<User | null>;
    this.currentUserRole$ = this.user$.pipe(
      map(user => user?.role || null)
    );
  }

  ngOnInit(): void {
    // Filtrar navegación según el rol del usuario
    this.currentUserRole$.subscribe(role => {
      if (role) {
        this.mainNav = this.filterNavByRole(this.allNavItems, role);
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
    if (!item.children || item.children.length === 0) return false;
    return item.children.some(child => {
      if (child.link) {
        return this.isActive(child.link);
      }
      return false;
    });
  }

  /**
   * Toggle sidebar collapsed state
   */
  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }

}
