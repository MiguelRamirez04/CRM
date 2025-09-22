# Core - Funcionalidades Centrales

Módulo que contiene la funcionalidad central de la aplicación: autenticación, servicios globales, guards, interceptors y tipos de datos.

## 📋 Responsabilidades

- Servicios globales de la aplicación
- Autenticación y autorización
- Interceptors HTTP
- Guards de rutas
- Modelos e interfaces TypeScript
- Enumeraciones que coinciden con el backend
- Constantes de la aplicación

## 📁 Estructura

### services/
Servicios globales que se usan en toda la aplicación:
- **auth.service.ts** - Autenticación JWT
- **api.service.ts** - Cliente HTTP base
- **notification.service.ts** - Manejo de notificaciones
- **loading.service.ts** - Estado de carga global
- **storage.service.ts** - LocalStorage/SessionStorage
- **signal-r.service.ts** - Conexión tiempo real (soporte)

### guards/
Guards para proteger rutas:
- **auth.guard.ts** - Verificar autenticación
- **role.guard.ts** - Verificar roles/permisos
- **unsaved-changes.guard.ts** - Advertir cambios sin guardar

### interceptors/
Interceptors HTTP:
- **auth.interceptor.ts** - Agregar JWT token
- **error.interceptor.ts** - Manejo global de errores
- **loading.interceptor.ts** - Mostrar/ocultar loading
- **base-url.interceptor.ts** - URL base de API

### models/
Interfaces y tipos TypeScript (coinciden con DTOs del backend):
- **empleado.model.ts** - `EmpleadoResponse`, `EmpleadoCreate`, `EmpleadoUpdate`
- **cliente.model.ts** - `ClienteResponse`, `ClienteCreate`
- **pedido.model.ts** - `PedidoResponse`, `PedidoCreate`
- **ticket.model.ts** - `TicketResponse`, `TicketCreate`
- **auth.model.ts** - `LoginRequest`, `LoginResponse`, `User`
- **common.model.ts** - `ApiResponse<T>`, `PaginatedResponse<T>`

### enums/
Enumeraciones (coinciden exactamente con backend):
- **estado-ticket.enum.ts** - `EstadoTicket`
- **prioridad-ticket.enum.ts** - `PrioridadTicket`
- **estado-pedido.enum.ts** - `EstadoPedido`
- **estado-empleado.enum.ts** - `EstadoEmpleado`

### constants/
Constantes de la aplicación:
- **api-routes.ts** - URLs de endpoints
- **app-config.ts** - Configuración de la app
- **error-messages.ts** - Mensajes de error
- **local-storage-keys.ts** - Keys para localStorage

## 🔗 Conexiones Backend

### API Service Base
```typescript
@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'https://localhost:5001/api';
  
  // Métodos base para comunicación con backend CRM
}
```

### Modelos TypeScript ↔ DTOs Backend
```typescript
// Frontend (TypeScript)
export interface EmpleadoResponse {
  id: number;
  nombre: string;
  email: string;
  rolNombre: string;
  fechaCreacion: Date;
}

// Backend (C# DTO) - COINCIDE EXACTAMENTE
public class EmpleadoResponseDto {
  public int Id { get; set; }
  public string Nombre { get; set; }
  public string Email { get; set; }
  public string RolNombre { get; set; }
  public DateTime FechaCreacion { get; set; }
}
```

### Auth Service ↔ JWT Backend
```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  login(credentials: LoginRequest): Observable<LoginResponse> {
    // POST /api/auth/login → JWT token del backend
  }
}
```

## ⚙️ Configuración

### En app.config.ts
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    // Guards
    provideRouter(routes, withGuards([AuthGuard])),
    // Interceptors
    provideHttpClient(withInterceptors([
      authInterceptor,
      errorInterceptor,
      loadingInterceptor
    ])),
    // Services globales se registran automáticamente con providedIn: 'root'
  ]
};
```

## 💡 Ejemplo de Uso

```typescript
// En cualquier componente
constructor(
  private authService: AuthService,
  private apiService: ApiService,
  private notificationService: NotificationService
) {}

async login() {
  try {
    const response = await this.authService.login(credentials);
    this.notificationService.success('Login exitoso');
  } catch (error) {
    // Error manejado automáticamente por interceptor
  }
}
```

## ⚙️ Singleton Pattern

Todos los servicios en core/ son singleton (providedIn: 'root') para garantizar una única instancia en toda la aplicación.