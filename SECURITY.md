# Arquitectura de Seguridad

## HttpOnly Cookies

No almacenamos JWT en `localStorage` ni `sessionStorage`. Los tokens se manejan exclusivamente via cookies HttpOnly configuradas en el backend.

```typescript
// Correcto
this.http.get('/api/data', { withCredentials: true });

// Incorrecto - nunca hacer esto
// localStorage.setItem('token', jwt);
```

## Capas de protección

1. **HttpOnly Cookies**: tokens inaccesibles desde JavaScript
2. **CORS estricto**: solo orígenes autorizados por ambiente
3. **CSRF Protection**: header `X-XSRF-TOKEN` + cookie `SameSite`
4. **Security Headers**: HSTS, X-Frame-Options, CSP, X-Content-Type-Options
5. **Refresh Tokens**: renovación automática antes del vencimiento
6. **Server-side validation**: nunca confiar solo en el frontend

## Configuración por ambiente

### Desarrollo
- Backend: `http://localhost:5176`
- Frontend: `http://localhost:4200`
- Cookies: `SameSite=Lax`, `Secure` según request

### Producción
- Solo HTTPS
- Cookies: `SameSite=Strict`, `Secure=Always`
- Orígenes CORS explícitos

## Pruebas de seguridad recomendadas

```bash
# Verificar que no hay tokens en storage
# (Debe retornar null)
localStorage.getItem('token');
sessionStorage.getItem('token');

# Verificar cookies desde DevTools
# (No debe mostrar AuthToken)
document.cookie;
```

## Checklist para desarrolladores

- [ ] Usar `withCredentials: true` en todas las peticiones HTTP
- [ ] Nunca almacenar tokens en storage del navegador
- [ ] Nunca enviar tokens manualmente en headers
- [ ] Validar siempre en el servidor
- [ ] Usar templates de configuración para secrets
