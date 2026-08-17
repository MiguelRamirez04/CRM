export const environment = {
  production: true,

  apiUrl: 'https://back-crm-api.onrender.com',

  security: {
    useHttpOnlyCookies: true,
    csrfTokenHeader: 'X-CSRF-Token',
    cookieSettings: {
      secure: true,
      sameSite: 'Strict',
      httpOnly: true
    },
    sessionTimeout: 15 * 60 * 1000,
    refreshTokenBefore: 2 * 60 * 1000,
    rateLimiting: {
      loginAttempts: 3,
      lockoutTime: 30 * 60 * 1000
    }
  },

  allowedOrigins: ['https://crm-nine-tau-43.vercel.app'],

  logging: {
    level: 'error',
    logSecurityEvents: true
  }
};
