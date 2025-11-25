# CRM - Sistema de Gestión de Clientes

Esta carpeta contiene toda la lógica del sistema CRM organizada de manera similar a un proyecto Node.js/JavaScript para facilitar la comprensión de los becarios.

## 📁 Estructura de Carpetas

```
CRM/
├── controllers/      # Controladores de API (como Express.js)
├── services/         # Lógica de negocio (como services en Node.js)
├── models/          # Entidades de base de datos (como Mongoose models)
├── DTOs/            # Data Transfer Objects (.NET específico)
├── middleware/      # Middleware personalizado (como Express middleware)
├── routes/          # Configuración de rutas (si es necesario)
├── utils/           # Utilidades y helpers (como utils en JS)
├── validators/      # Validadores con FluentValidation (.NET específico)
├── enums/          # Enumeraciones (como enums en TS)
├── scripts/        # Scripts de utilidad (como scripts en package.json)
├── uploads/        # Archivos subidos por usuarios
└── config/         # Configuraciones centralizadas
```

## 🔄 Flujo de Datos

1. **Request** → **Controller** → **Service** → **Model/Database**
2. **Database** → **Model** → **Service** → **DTO** → **Controller** → **Response**

## 🎯 Módulos del Sistema

### Administración
- Gestión de empleados
- Roles y permisos
- Configuración del sistema

### Recepción
- Gestión de clientes
- Creación y seguimiento de pedidos
- Comunicación inicial

### Soporte
- Sistema de tickets
- Chat en tiempo real
- Resolución de problemas

## 🛠️ Tecnologías Integradas

- **Entity Framework**: ORM para base de datos
- **MediatR**: Patrón CQRS simplificado
- **FluentValidation**: Validación de datos
- **Serilog**: Logging estructurado
- **JWT**: Autenticación
- **SignalR**: Comunicación en tiempo real

## 📝 Convenciones de Nomenclatura

### Archivos
- **Controllers**: `{Entidad}Controller.cs`
- **Services**: `{Entidad}Service.cs`
- **Models**: `{Entidad}.cs`
- **DTOs**: `{Entidad}{Operacion}Dto.cs`
- **Validators**: `{Dto}Validator.cs`

### Métodos
- **GET**: `Get{Entidades}`, `Get{Entidad}ById`
- **POST**: `Create{Entidad}`
- **PUT**: `Update{Entidad}`
- **DELETE**: `Delete{Entidad}`

## 🚀 Comenzando a Desarrollar

1. **Definir modelo** en `models/{Modulo}/`
2. **Crear DTOs** en `DTOs/{Modulo}/`
3. **Implementar servicio** en `services/{Modulo}/`
4. **Crear controlador** en `controllers/{Modulo}/`
5. **Agregar validaciones** en `validators/`

Consulta el README.md de cada carpeta para detalles específicos.