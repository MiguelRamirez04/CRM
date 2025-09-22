# 🚀 CRM Sistema - Proyecto Fullstack Configurado

## ✅ Estado del Proyecto: LISTO PARA DESARROLLO

Tu proyecto fullstack está completamente configurado con arquitectura de seguridad robusta y estructura familiar para desarrolladores JavaScript.

## 🛡️ Seguridad Implementada

### ❌ Lo que NO hacemos (vulnerable):
- ❌ Almacenar JWT en `localStorage` o `sessionStorage`
- ❌ CORS permisivo (`*`)
- ❌ Tokens enviados en URLs o headers sin protección

### ✅ Lo que SÍ hacemos (seguro):
- ✅ **HttpOnly Cookies** - Tokens inaccesibles desde JavaScript
- ✅ **CORS Estricto** - Solo dominios autorizados
- ✅ **CSRF Protection** - Headers `X-Requested-With` + `SameSite`
- ✅ **Security Headers** - Protección completa del navegador
- ✅ **Refresh Tokens** - Renovación automática transparente

## 📁 Estructura Implementada

### Backend (.NET 8)
```
back_cabs/
├── Controllers/
│   └── AuthController.cs          ✅ Cookies HttpOnly + CSRF
├── CRM/                          ✅ Clean Architecture
│   ├── config/                   ✅ Configuraciones centralizadas
│   ├── controllers/              ✅ Controladores organizados
│   ├── services/                 ✅ Lógica de negocio
│   ├── models/                   ✅ Modelos de datos
│   └── contexts/                 ✅ Read/Write separados (CQRS)
└── Program.cs                    ✅ CORS + Security Headers
```

### Frontend (Angular 17+)
```
front_cabs/src/app/
├── core/                         ✅ Servicios seguros
│   ├── services/
│   │   └── secure-auth.service.ts ✅ Sin localStorage
│   ├── guards/
│   │   └── secure-auth.guard.ts   ✅ Verificación en servidor
│   └── interceptors/
│       ├── secure-auth.interceptor.ts ✅ Refresh automático
│       └── security-headers.interceptor.ts ✅ Headers CSRF
├── features/                     ✅ Módulos de funcionalidad
│   └── auth/
│       └── pages/login/          ✅ UI Bootstrap + validación
├── shared/                       ✅ Componentes reutilizables
└── modules/                      ✅ Administración, Recepción, Soporte
```

## 🔧 Tecnologías Configuradas

### Backend
- ✅ .NET 8 Web API
- ✅ JWT Authentication (8.0.8) con cookies HttpOnly
- ✅ Entity Framework Core (8.0.8) con SQL Server
- ✅ FluentValidation (11.9.0) para validaciones
- ✅ MediatR (11.1.0) para CQRS
- ✅ Serilog (8.0.1) para logging
- ✅ Swagger/OpenAPI (6.6.2) para documentación
- ✅ HealthChecks (8.0.1) para monitoreo

### Frontend
- ✅ Angular 17+ con Standalone Components
- ✅ Bootstrap 5 + ng-bootstrap para UI
- ✅ FontAwesome para iconos
- ✅ ngx-cookie-service para manejo seguro
- ✅ Reactive Forms para validación
- ✅ RxJS para programación reactiva

## 🚦 Comandos para Empezar

### 1. Iniciar Backend (Puerto 3000)
```bash
cd back_cabs
dotnet run --launch-profile https
```

### 2. Iniciar Frontend (Puerto 4200)
```bash
cd front_cabs
ng serve --ssl
```

### 3. Verificar Comunicación
- Backend: https://localhost:3000/swagger
- Frontend: https://localhost:4200
- Login: admin@test.com / 123456

## 🧪 Testing de Seguridad

### 1. Verificar que NO hay tokens en localStorage
```javascript
// En DevTools Console (debe ser null)
localStorage.getItem('token');
sessionStorage.getItem('token');
```

### 2. Verificar cookies HttpOnly
```javascript
// En DevTools Console (NO debe mostrar AuthToken)
document.cookie;
```

### 3. Test de CORS
```bash
# Debe fallar desde otro dominio
curl -X POST https://localhost:3000/api/auth/login \
  -H "Origin: http://malicious-site.com"
```

## 📋 Para los Desarrolladores

### 🔐 Reglas de Seguridad OBLIGATORIAS

1. **NUNCA almacenar tokens en localStorage/sessionStorage**
2. **SIEMPRE usar `withCredentials: true`** en requests HTTP
3. **NUNCA enviar tokens en headers manualmente**
4. **SIEMPRE validar en el servidor** (no confiar en frontend)

### ✅ Patrón de Request Correcto
```typescript
// ✅ CORRECTO
this.http.get('/api/data', {
  withCredentials: true  // Para cookies HttpOnly
});

// ❌ INCORRECTO
this.http.get('/api/data', {
  headers: {
    'Authorization': `Bearer ${token}` // ¡NO HACER!
  }
});
```

### 🎯 Flujo de Autenticación
1. **Login** → Backend crea cookies HttpOnly
2. **Requests** → Navegador envía cookies automáticamente
3. **Refresh** → Renovación transparente antes de expirar
4. **Logout** → Backend limpia cookies HttpOnly

## 📚 Documentación Disponible

- `SECURITY.md` - Guía completa de seguridad
- `front_cabs/src/app/*/README.md` - Documentación de cada módulo
- `back_cabs/CRM/*/README.md` - Documentación de arquitectura backend

## 🎉 ¡Listos para Codificar!

La estructura está **100% preparada** para que los becarios empiecen a desarrollar:

- ✅ Seguridad robusta implementada
- ✅ Arquitectura limpia y familiar
- ✅ Documentación exhaustiva
- ✅ Bootstrap configurado para UI
- ✅ Guards y permisos preparados
- ✅ Error handling robusto

## 🆘 Soporte

Para dudas:
1. Revisar `SECURITY.md` para temas de seguridad
2. Revisar `README.md` de cada carpeta
3. Consultar con el tech lead
4. **NUNCA** implementar autenticación por cuenta propia

---

**¡Happy Coding! 🚀** El proyecto está blindado contra las vulnerabilidades más comunes y listo para el desarrollo.