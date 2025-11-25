# Layout - Componentes de Diseño

Componentes que definen la estructura visual y navegación de la aplicación.

## 📋 Responsabilidades

- Estructura visual de la aplicación
- Navegación principal
- Header con información del usuario
- Sidebar con menú de módulos
- Footer con información de la app

## 📁 Estructura

### header/
Componente de cabecera:
- **header.component.ts** - Barra superior
- Logo de la empresa
- Información del usuario logueado
- Menú de usuario (perfil, logout)
- Breadcrumb de navegación
- Notificaciones en tiempo real

### sidebar/
Menú lateral de navegación:
- **sidebar.component.ts** - Menú lateral
- Navegación por módulos (Administración, Recepción, Soporte)
- Indicadores de estado por módulo
- Collapsible/expandible
- Responsive para móviles

### footer/
Pie de página:
- **footer.component.ts** - Información del sistema
- Versión de la aplicación
- Links útiles
- Estado de conexión con backend

## 🎨 Layout Principal

```typescript
// layout.component.ts
@Component({
  template: `
    <div class="app-layout">
      <app-header></app-header>
      <div class="main-content">
        <app-sidebar></app-sidebar>
        <main class="content">
          <router-outlet></router-outlet>
        </main>
      </div>
      <app-footer></app-footer>
    </div>
  `
})
export class LayoutComponent { }
```

## 🔗 Navegación por Módulos

### Sidebar Menu Structure
```typescript
export interface MenuItem {
  label: string;
  route: string;
  icon: string;
  badge?: number; // Para notificaciones
  children?: MenuItem[];
}

export const MENU_ITEMS: MenuItem[] = [
  {
    label: 'Administración',
    route: '/administracion',
    icon: 'people',
    children: [
      { label: 'Empleados', route: '/administracion/empleados', icon: 'person' },
      { label: 'Roles', route: '/administracion/roles', icon: 'security' }
    ]
  },
  {
    label: 'Recepción',
    route: '/recepcion',
    icon: 'inbox',
    children: [
      { label: 'Clientes', route: '/recepcion/clientes', icon: 'contacts' },
      { label: 'Pedidos', route: '/recepcion/pedidos', icon: 'shopping_cart' }
    ]
  },
  {
    label: 'Soporte',
    route: '/soporte',
    icon: 'support',
    badge: 5, // Tickets pendientes
    children: [
      { label: 'Tickets', route: '/soporte/tickets', icon: 'assignment' },
      { label: 'Chat', route: '/soporte/chat', icon: 'chat' }
    ]
  }
];
```

## 🔔 Header Features

### User Info
```typescript
// header.component.ts
@Component({
  template: `
    <header class="app-header">
      <div class="logo">CRM Sistema</div>
      <nav class="breadcrumb">
        <app-breadcrumb></app-breadcrumb>
      </nav>
      <div class="user-section">
        <app-notifications></app-notifications>
        <div class="user-menu">
          <span>{{ currentUser?.nombre }}</span>
          <button (click)="logout()">Salir</button>
        </div>
      </div>
    </header>
  `
})
export class HeaderComponent {
  currentUser = this.authService.currentUser;
  
  constructor(private authService: AuthService) {}
  
  logout() {
    this.authService.logout();
  }
}
```

### Real-time Notifications
- Notificaciones de nuevos tickets (Soporte)
- Alertas de pedidos urgentes (Recepción)
- Notificaciones del sistema (Administración)

## 📱 Responsive Design

### Breakpoints
- **Desktop**: >= 1024px - Sidebar visible
- **Tablet**: 768px - 1023px - Sidebar collapsible  
- **Mobile**: < 768px - Sidebar como overlay

### Mobile Navigation
```typescript
// sidebar.component.ts
export class SidebarComponent {
  @HostBinding('class.mobile-open')
  mobileOpen = false;
  
  @HostListener('window:resize', ['$event'])
  onResize() {
    if (window.innerWidth >= 1024) {
      this.mobileOpen = false;
    }
  }
}
```

## 🎨 Themes y Styling

### CSS Variables para Theming
```css
:root {
  --primary-color: #1976d2;
  --secondary-color: #dc004e;
  --background-color: #fafafa;
  --sidebar-width: 260px;
  --header-height: 64px;
}
```

### Angular Material Integration
- Utilizar Angular Material para componentes consistentes
- Toolbar, Sidenav, Menu, Badge components
- Tema personalizado con colores de la empresa