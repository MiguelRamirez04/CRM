export const environment = {
  production: true,

  apiUrl: 'https://tu-backend-production.com/api',
  
  // Configuración de seguridad para producción
  security: {
    // OBLIGATORIO: usar HttpOnly cookies en producción
    useHttpOnlyCookies: true,
    
    // CSRF Protection
    csrfTokenHeader: 'X-CSRF-Token',
    
    // Configuración estricta de cookies para producción
    cookieSettings: {
      secure: true, // OBLIGATORIO con HTTPS
      sameSite: 'Strict', // Máxima protección CSRF
      httpOnly: true // Manejado por el backend
    },
    
    // Timeouts de seguridad más estrictos
    sessionTimeout: 15 * 60 * 1000, // 15 minutos en producción
    refreshTokenBefore: 2 * 60 * 1000, // Refresh 2 min antes
    
    // Rate limiting estricto
    rateLimiting: {
      loginAttempts: 3,
      lockoutTime: 30 * 60 * 1000 // 30 minutos
    }
  },
  
  // URLs permitidas para CORS (solo producción)
  allowedOrigins: [
    'https://your-frontend-domain.com'
  ],
  
  // Logging mínimo en producción
  logging: {
    level: 'error',
    logSecurityEvents: true
  }
};