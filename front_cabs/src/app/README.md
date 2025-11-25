# 🅰️ Frontend CRM - Angular Structure

Estructura del frontend Angular que encaja perfectamente con el backend .NET organizado en la carpeta CRM.

## 📁 Estructura Principal

```
src/app/
├── modules/                    # 🎯 Módulos principales (coinciden con backend)
│   ├── administracion/         # ✅ Módulo de empleados, roles, departamentos
│   │   ├── components/         # Componentes específicos del módulo
│   │   ├── pages/              # Páginas/vistas principales
│   │   └── services/           # Servicios específicos del módulo
│   ├── recepcion/              # ✅ Módulo de clientes y pedidos
│   │   ├── components/         # Componentes de recepción
│   │   ├── pages/              # Páginas de gestión de pedidos
│   │   └── services/           # Servicios de clientes y pedidos
│   └── soporte/                # ✅ Módulo de tickets y mensajes
│       ├── components/         # Componentes de soporte
│       ├── pages/              # Páginas de tickets
│       └── services/           # Servicios de tickets
├── shared/                     # 🔄 Componentes reutilizables
│   ├── components/             # Componentes compartidos
│   ├── directives/             # Directivas personalizadas
│   ├── pipes/                  # Pipes personalizados
│   └── utils/                  # Utilidades compartidas
├── core/                       # 🔧 Funcionalidades core del sistema
│   ├── services/               # Servicios globales (auth, api, etc.)
│   ├── guards/                 # Guards de rutas
│   ├── interceptors/           # HTTP interceptors
│   ├── models/                 # Interfaces y tipos
│   ├── enums/                  # Enumeraciones (coinciden con backend)
│   └── constants/              # Constantes de la aplicación
├── layout/                     # 🎨 Componentes de layout
│   ├── header/                 # Header/navbar
│   ├── sidebar/                # Menú lateral
│   └── footer/                 # Footer
├── app.component.*             # Componente raíz
├── app.config.ts               # Configuración de la app
└── app.routes.ts               # Configuración de rutas
```

## 🔗 Conexión con Backend

### API Endpoints (coinciden con controllers backend)
- **Administración**: `/api/empleado`, `/api/rol`, `/api/departamento`
- **Recepción**: `/api/cliente`, `/api/pedido`
- **Soporte**: `/api/ticket`, `/api/mensaje`

### Modelos TypeScript (coinciden con DTOs backend)
- **Administración**: `EmpleadoResponse`, `EmpleadoCreate`, `RolResponse`
- **Recepción**: `ClienteResponse`, `PedidoResponse`, `PedidoCreate`
- **Soporte**: `TicketResponse`, `TicketCreate`, `MensajeResponse`

### Enums TypeScript (coinciden con enums backend)
- `EstadoTicket`, `PrioridadTicket`, `EstadoPedido`, `EstadoEmpleado`

## 🎯 Flujo de Desarrollo

### 1. Para crear una nueva funcionalidad:
```
1. Definir modelo/interface en core/models/
2. Crear servicio en modules/{modulo}/services/
3. Crear componentes en modules/{modulo}/components/
4. Crear páginas en modules/{modulo}/pages/
5. Configurar rutas en app.routes.ts
```

### 2. Para componentes reutilizables:
```
1. Crear en shared/components/
2. Exportar en shared.module.ts
3. Importar donde se necesite
```

## 🚀 Tecnologías Principales

- **Angular 17+** - Framework principal
- **TypeScript** - Lenguaje principal
- **RxJS** - Programación reactiva
- **Angular Material** - UI Components (recomendado)
- **NgRx** - State management (opcional para casos complejos)

## 📝 Convenciones de Nomenclatura

### Archivos
- **Components**: `empleado-list.component.ts`
- **Services**: `empleado.service.ts`
- **Pages**: `empleados-page.component.ts`
- **Models**: `empleado.model.ts`
- **Enums**: `estado-ticket.enum.ts`

### Clases y Interfaces
- **Components**: `EmpleadoListComponent`
- **Services**: `EmpleadoService`
- **Interfaces**: `IEmpleado`, `EmpleadoResponse`
- **Enums**: `EstadoTicket`

## 🎨 Arquitectura Recomendada

### Lazy Loading por Módulos
- Cada módulo (administracion, recepcion, soporte) se carga cuando se necesita
- Mejora el rendimiento inicial de la aplicación

### Smart vs Dumb Components
- **Smart**: En `pages/` - manejan estado y lógica
- **Dumb**: En `components/` - solo reciben datos y emiten eventos

### Estado Global vs Local
- **Global**: Authentication, user info (en core/services)
- **Local**: Estado específico de módulo (en services del módulo)

¡La estructura está lista para que los becarios desarrollen de manera organizada y eficiente! 🚀