export const environment = {
  production: false,
  apiUrl: 'http://localhost:3000/api',
  
  // Configuración de seguridad
  security: {
    // NO almacenar tokens en frontend - usar HttpOnly cookies
    useHttpOnlyCookies: true,
    
    // CSRF Protection
    csrfTokenHeader: 'X-CSRF-Token',
    
    // Configuración de cookies (para desarrollo)
    cookieSettings: {
      secure: false, // true en producción con HTTPS
      sameSite: 'Lax', // 'Strict' en producción
      httpOnly: true // Manejado por el backend
    },
    
    // Timeouts de seguridad
    sessionTimeout: 30 * 60 * 1000, // 30 minutos
    refreshTokenBefore: 5 * 60 * 1000, // Refresh 5 min antes
    
    // Rate limiting del lado cliente
    rateLimiting: {
      loginAttempts: 5,
      lockoutTime: 15 * 60 * 1000 // 15 minutos
    }
  },
  
  // URLs permitidas para CORS
  allowedOrigins: [
    'http://localhost:4200',
    'http://localhost:3000'
  ],
  
  // Configuración de logging
  logging: {
    level: 'debug',
    logSecurityEvents: true
  }
};