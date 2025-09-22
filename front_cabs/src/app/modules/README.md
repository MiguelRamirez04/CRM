# Modules - Módulos de Funcionalidad

Módulos principales que implementan la lógica de negocio específica. Cada módulo coincide exactamente con los módulos del backend CRM.

## 📋 Organización por Módulos

### 🏢 Administración
**Gestión de empleados, roles y departamentos**
- **Components**: Lista de empleados, formularios de creación/edición
- **Pages**: Página principal de empleados, página de roles
- **Services**: EmpleadoService, RolService, DepartamentoService

### 📞 Recepción  
**Gestión de clientes y pedidos**
- **Components**: Lista de clientes, formularios de pedidos
- **Pages**: Dashboard de recepción, gestión de pedidos
- **Services**: ClienteService, PedidoService

### 🎧 Soporte
**Sistema de tickets y mensajes**
- **Components**: Lista de tickets, chat en tiempo real
- **Pages**: Dashboard de soporte, vista de ticket individual
- **Services**: TicketService, MensajeService, SignalRService

## 📁 Estructura por Módulo

```
modules/{modulo}/
├── components/           # Componentes específicos del módulo
│   ├── {entidad}-list/   # Lista de entidades
│   ├── {entidad}-form/   # Formulario de entidad
│   └── {entidad}-card/   # Tarjeta de entidad
├── pages/                # Páginas principales del módulo
│   ├── {modulo}-dashboard.component.ts
│   ├── {entidad}-list-page.component.ts
│   └── {entidad}-detail-page.component.ts
├── services/             # Servicios específicos del módulo
│   ├── {entidad}.service.ts
│   └── {modulo}-helper.service.ts
└── {modulo}.routes.ts    # Rutas del módulo
```

## 🔗 Conexión con Backend

### URL Mapping
- **Administración** → `/api/empleado`, `/api/rol`, `/api/departamento`
- **Recepción** → `/api/cliente`, `/api/pedido`
- **Soporte** → `/api/ticket`, `/api/mensaje`

### Service Pattern
```typescript
@Injectable({ providedIn: 'root' })
export class EmpleadoService {
  constructor(private apiService: ApiService) {}

  getEmpleados(): Observable<EmpleadoResponse[]> {
    return this.apiService.get<EmpleadoResponse[]>('/empleado');
  }

  createEmpleado(empleado: EmpleadoCreate): Observable<EmpleadoResponse> {
    return this.apiService.post<EmpleadoResponse>('/empleado', empleado);
  }
}
```

## 🎯 Lazy Loading

Cada módulo se carga solo cuando se necesita:

```typescript
// En app.routes.ts
export const routes: Routes = [
  {
    path: 'administracion',
    loadChildren: () => import('./modules/administracion/administracion.routes').then(m => m.administracionRoutes)
  },
  {
    path: 'recepcion', 
    loadChildren: () => import('./modules/recepcion/recepcion.routes').then(m => m.recepcionRoutes)
  },
  {
    path: 'soporte',
    loadChildren: () => import('./modules/soporte/soporte.routes').then(m => m.soporteRoutes)
  }
];
```

## 💡 Ejemplo de Implementación

### Administración - Lista de Empleados
```typescript
// empleados-list-page.component.ts
@Component({
  template: `
    <app-empleado-list 
      [empleados]="empleados()" 
      (create)="onCreateEmpleado()"
      (edit)="onEditEmpleado($event)"
      (delete)="onDeleteEmpleado($event)">
    </app-empleado-list>
  `
})
export class EmpleadosListPageComponent {
  empleados = signal<EmpleadoResponse[]>([]);

  constructor(private empleadoService: EmpleadoService) {}

  ngOnInit() {
    this.loadEmpleados();
  }

  private loadEmpleados() {
    this.empleadoService.getEmpleados().subscribe({
      next: (empleados) => this.empleados.set(empleados),
      error: (error) => console.error('Error loading empleados', error)
    });
  }
}
```

## ⚙️ Arquitectura Smart/Dumb

### Smart Components (Pages)
- Manejan estado y lógica de negocio
- Llaman a servicios
- Manejan routing

### Dumb Components (Components)
- Solo reciben datos vía @Input()
- Emiten eventos vía @Output()
- No tienen dependencias de servicios
- Reutilizables y testeable

## 🔄 State Management

### Simple State (Signals)
```typescript
// Para estado simple del módulo
export class EmpleadoService {
  private empleadosSignal = signal<EmpleadoResponse[]>([]);
  empleados = this.empleadosSignal.asReadonly();
}
```

### Complejo State (NgRx - opcional)
Para casos complejos con múltiples componentes compartiendo estado.