# 🔐 Arquitectura de Seguridad - Frontend y Backend

## ⚠️ IMPORTANTE: NO almacenar JWT en localStorage o sessionStorage

Este proyecto implementa una arquitectura de seguridad robusta para prevenir las vulnerabilidades más comunes en aplicaciones web.

## 🍪 Cookies HttpOnly - Almacenamiento Seguro

### ❌ Lo que NO hacemos (inseguro):
```typescript
// ❌ NUNCA hacer esto - vulnerable a XSS
localStorage.setItem('token', jwt);
sessionStorage.setItem('token', jwt);
```

### ✅ Lo que SÍ hacemos (seguro):
```typescript
// ✅ Cookies HttpOnly manejadas automáticamente por el navegador
// Los tokens se almacenan en cookies que solo el servidor puede leer
login(credentials).subscribe(response => {
  // Los tokens ya están seguros en cookies HttpOnly
  // No necesitamos manejarlos manualmente
});
```

## 🛡️ Capas de Protección Implementadas

### 1. **HttpOnly Cookies**
- **Qué es**: Cookies que solo pueden ser leídas por el servidor
- **Protege contra**: Scripts maliciosos (XSS) que roban tokens
- **Ubicación**: Backend `AuthController.cs` y Frontend `SecureAuthService`

### 2. **CORS Estricto**
- **Desarrollo**: Solo `localhost:4200` y `localhost:3000`
- **Producción**: Solo dominios específicos con HTTPS
- **Configuración**: `Program.cs` líneas 21-42

### 3. **CSRF Protection**
- **Método**: Header `X-Requested-With` y cookies `SameSite`
- **Protege contra**: Ataques de sitios cruzados
- **Configuración**: Interceptores y middleware de headers

### 4. **Security Headers**
- **X-Frame-Options**: Previene clickjacking
- **X-Content-Type-Options**: Previene MIME sniffing
- **X-XSS-Protection**: Protección adicional XSS
- **Content-Security-Policy**: Control estricto de recursos
- **HSTS**: Forzar HTTPS en producción

### 5. **Refresh Token Automático**
- **Access Token**: 30 minutos de vida
- **Refresh Token**: 7 días, solo para renovar access token
- **Proceso**: Renovación transparente antes del vencimiento

## 🔧 Configuración por Ambiente

### Desarrollo (localhost)
```typescript
// environment.ts
security: {
  cookieSettings: {
    secure: false,        // HTTP permitido
    sameSite: 'Lax'      // Menos restrictivo para desarrollo
  }
}
```

### Producción (HTTPS)
```typescript
// environment.prod.ts
security: {
  cookieSettings: {
    secure: true,         // Solo HTTPS
    sameSite: 'Strict'    // Máxima protección CSRF
  }
}
```

## 🚨 Vulnerabilidades Prevenidas

### ✅ Cross-Site Scripting (XSS)
- **Cómo**: HttpOnly cookies + Security headers
- **Resultado**: Scripts maliciosos no pueden acceder a tokens

### ✅ Cross-Site Request Forgery (CSRF)
- **Cómo**: SameSite cookies + X-Requested-With header
- **Resultado**: Sitios externos no pueden hacer requests

### ✅ Man-in-the-Middle (MITM)
- **Cómo**: HTTPS obligatorio + HSTS headers
- **Resultado**: Comunicaciones siempre encriptadas

### ✅ Session Hijacking
- **Cómo**: Secure cookies + expiración automática
- **Resultado**: Tokens robados expiran rápidamente

## 📋 Checklist de Seguridad para Desarrolladores

### Backend (.NET)
- [ ] ✅ Cookies HttpOnly configuradas
- [ ] ✅ CORS estricto por ambiente
- [ ] ✅ Security headers implementados
- [ ] ✅ JWT con expiración corta (30 min)
- [ ] ✅ Refresh tokens seguros (7 días)
- [ ] ⚠️ TODO: Hash de contraseñas (bcrypt)
- [ ] ⚠️ TODO: Rate limiting para login
- [ ] ⚠️ TODO: Validaciones de entrada

### Frontend (Angular)
- [ ] ✅ No almacenar tokens en localStorage
- [ ] ✅ Interceptors para headers de seguridad
- [ ] ✅ Guards con verificación en servidor
- [ ] ✅ Refresh automático de tokens
- [ ] ✅ withCredentials: true para cookies
- [ ] ✅ Error handling robusto
- [ ] ⚠️ TODO: Validación de formularios
- [ ] ⚠️ TODO: Sanitización de inputs

## 🔍 Testing de Seguridad

### 1. Test de XSS
```javascript
// En DevTools Console (debe fallar)
console.log(document.cookie); // No debe mostrar tokens
localStorage.getItem('token'); // Debe ser null
```

### 2. Test de CSRF
```bash
# Request desde otro dominio (debe fallar)
curl -X POST http://localhost:3000/api/auth/login \
  -H "Origin: http://malicious-site.com" \
  -d '{"email":"test","password":"test"}'
```

### 3. Test de CORS
```javascript
// Fetch desde otro dominio (debe fallar)
fetch('http://localhost:3000/api/auth/me', {
  method: 'GET',
  credentials: 'include'
});
```

## 📚 Para Desarrolladores Junior

### ¿Por qué no localStorage?
```typescript
// ❌ Problema: Cualquier script puede leer esto
const token = localStorage.getItem('jwt');
// Si una librería tiene XSS, roban el token

// ✅ Solución: HttpOnly cookies
// Solo el servidor puede leer/escribir el token
// Scripts maliciosos no tienen acceso
```

### ¿Cómo funciona el flujo seguro?
1. **Login**: Usuario envía credenciales
2. **Backend**: Valida y crea cookies HttpOnly
3. **Frontend**: No maneja tokens directamente
4. **Requests**: Navegador envía cookies automáticamente
5. **Refresh**: Renovación transparente de tokens

### ¿Qué hacer en cada request?
```typescript
// ✅ SIEMPRE incluir esto:
this.http.get('/api/data', {
  withCredentials: true  // Para enviar cookies HttpOnly
});

// ❌ NUNCA hacer esto:
headers: {
  'Authorization': `Bearer ${localStorage.getItem('token')}`
}
```

## 🚀 Comandos de Desarrollo

```bash
# Backend - Ejecutar con HTTPS
dotnet run --launch-profile https

# Frontend - Desarrollo
ng serve --ssl

# Verificar headers de seguridad
curl -I http://localhost:3000/api/auth/login
```

## 📞 Contacto y Soporte

Para dudas sobre seguridad:
1. Revisar este documento primero
2. Consultar con el tech lead
3. Nunca implementar autenticación por tu cuenta
4. Seguir siempre las buenas prácticas establecidas

---

**Recuerda**: La seguridad no es opcional. Cada línea de código debe seguir estos principios.