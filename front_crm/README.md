# Frontend CRM

Frontend del CRM desarrollado con Angular 20+. Aplicación SPA con arquitectura de componentes standalone, RxJS para programación reactiva y comunicación segura con el backend via cookies HttpOnly.

## Stack

- Angular 20 (Standalone Components)
- RxJS 7.8
- ngx-cookie-service
- Bootstrap 5 / Material
- ng2-charts + Chart.js
- FullCalendar
- html2canvas + jsPDF

## Estructura

```
src/app/
├── core/                  # Servicios globales, guards, interceptors, modelos
├── features/              # Auth (login, recuperación, reset)
├── modules/               # Dashboard, Soporte, Administración, Legacy, Viáticos
├── shared/                # Componentes UI reutilizables (átomos, moléculas, organismos)
├── layout/                # Layout principal, header, sidebar
└── environments/          # Configuración por ambiente
```

## Módulos

- **Dashboard**: landing, calendario, cotizaciones, evaluaciones
- **Soporte**: asignaciones, reparaciones, componentes, fotos
- **Administración**: panel de control, usuarios, configuración
- **Legacy**: integración con Adminpaq (documentos, productos, clientes, conceptos)
- **Viáticos**: registro, detalle y seguimiento de gastos
- **Vehículos**: flota, historial, estados y asignaciones

## Puesta en marcha

```bash
npm install
ng serve
```

La app corre en `http://localhost:4200` y apunta al backend en `http://localhost:5176`.

## Notas

- Todos los servicios usan `withCredentials: true` para cookies HttpOnly
- Los guards verifican autenticación contra el backend
- Los interceptores manejan refresh automático y headers CSRF
