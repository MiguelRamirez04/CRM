# CABS CRM

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Angular-20-FF0000?logo=angular" alt="Angular 20" />
  <img src="https://img.shields.io/badge/PostgreSQL-Supabase-336791?logo=postgresql" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis" alt="Redis" />
</p>

CRM full-stack para empresas de servicios técnicos. Gestiona recepción, soporte, cotizaciones, evaluaciones, vehículos, viáticos y un módulo legacy de facturación. Desarrollado con arquitectura limpia en backend y componentes standalone en frontend.

## Módulos principales

- **Recepción**: órdenes de trabajo, cotizaciones y seguimiento
- **Soporte**: reparaciones, componentes y fotos de evidencia
- **Evaluaciones**: inspecciones vehiculares con registro de fases y detalles
- **Vehículos**: control de flota, historial y estados
- **Viáticos**: gastos de operación con aprobaciones
- **Administración**: panel de control, métricas y configuración
- **Legacy**: integración con catálogos Adminpaq (documentos, productos, clientes)

## Stack tecnológico

### Backend
| Tecnología | Uso |
|------------|-----|
| .NET 8 Web API | API REST |
| Entity Framework Core | ORM (Npgsql para PostgreSQL) |
| MediatR | CQRS |
| FluentValidation | Validaciones |
| Serilog | Logging |
| Swagger / OpenAPI | Documentación |
| Health Checks | Monitoreo |
| Redis | Cache distribuida |
| JWT + HttpOnly Cookies | Autenticación |

### Frontend
| Tecnología | Uso |
|------------|-----|
| Angular 20+ | Framework SPA |
| Standalone Components | Arquitectura modular |
| RxJS | Programación reactiva |
| ngx-cookie-service | Manejo seguro de cookies |
| ng2-charts / Chart.js | Reportes gráficos |
| FullCalendar | Calendario interactivo |
| html2canvas + jsPDF | Exportación a PDF |

## Seguridad

- Cookies HttpOnly (sin localStorage/sessionStorage)
- CORS estricto por ambiente
- Protección CSRF (`X-XSRF-TOKEN` + `SameSite`)
- Refresh tokens transparentes
- Security headers (HSTS, X-Frame-Options, CSP)
- Validación en servidor (no confiar en frontend)

## Estructura del proyecto

```
FullStack_CABS/
├── back_crm/                 # Backend .NET 8
│   ├── crm/                 # Clean Architecture
│   │   ├── controllers/     # API Controllers
│   │   ├── services/        # Lógica de negocio
│   │   ├── repositories/    # Acceso a datos
│   │   ├── models/          # Entidades
│   │   ├── DTOs/            # Request/Response
│   │   ├── contexts/        # DbContext (Read/Write)
│   │   └── middleware/      # Security, errores, logging
│   ├── Tests/UnitTests/     # Pruebas unitarias
│   ├── Program.cs           # Startup
│   └── appsettings.json     # Configuración
├── front_crm/               # Frontend Angular 20
│   ├── src/app/
│   │   ├── core/            # Servicios, guards, interceptors
│   │   ├── features/        # Auth (login, reset)
│   │   ├── modules/         # Dashboard, Soporte, Legacy, Viáticos
│   │   └── shared/          # Componentes UI atómicos
│   ├── angular.json
│   └── package.json
└── database/                # Scripts SQL (PostgreSQL / Supabase)
```

## Puesta en marcha

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.dev/tools/cli)
- PostgreSQL (o cuenta en [Supabase](https://supabase.com/))

### 1. Base de datos

Ejecuta los scripts SQL en el orden indicado:

```bash
# Supabase SQL Editor o cliente PostgreSQL local
database/CRM_Demo_001_create.sql   # Esquema
database/CRM_Demo_002_seed.sql     # Datos demo
```

### 2. Backend

```bash
cd back_crm
dotnet restore
dotnet run --launch-profile http
```

- Swagger: `http://localhost:5176/swagger`
- HTTPS: `https://localhost:7275`

Configura tus secretos reales via [user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) o variables de entorno. Nunca comitas credenciales.

### 3. Frontend

```bash
cd front_crm
npm install
ng serve
```

- App: `http://localhost:4200`

## Credenciales de demo

| Campo | Valor |
|-------|-------|
| Usuario | `admin@test.com` |
| Password | `123456` |

## Capturas de pantalla

_(Agrega aquí tus screenshots del dashboard, cotizaciones, soporte y evaluaciones)_

## Licencia

Este proyecto se comparte como material de portafolio. El código puede reutilizarse con fines educativos.
