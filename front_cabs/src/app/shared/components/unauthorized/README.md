# Componente Unauthorized - Página de Acceso Denegado

Componente mostrado cuando un usuario no tiene permisos para acceder a una ruta específica.

## 🚫 Casos de Uso

- Usuario sin rol requerido
- Usuario sin permisos específicos
- Acceso denegado por políticas de seguridad
- Redirección desde guards

## 🎨 Funcionalidades

- Mensaje claro de acceso denegado
- Información sobre permisos requeridos
- Enlace para volver a área permitida
- Diseño amigable con Bootstrap

## 🔐 Integración con Guards

```typescript
// En SecureAuthGuard
if (!this.authService.hasPermission(requiredPermission)) {
  this.router.navigate(['/unauthorized']);
  return false;
}
```

## 🎯 UX/UI

- Icono de escudo/candado
- Mensaje explicativo
- Botón de regreso al dashboard
- Información de contacto para solicitar permisos