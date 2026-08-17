const runtimeEnv = (globalThis as typeof globalThis & {
  process?: {
    env?: Record<string, string | undefined>;
  };
}).process?.env;

export const environment = {
  production: true,

  apiUrl: runtimeEnv?.['NG_APP_API_URL'] || 'https://tu-backend-production.com/api',

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

  allowedOrigins: runtimeEnv?.['NG_APP_ALLOWED_ORIGINS']
    ? runtimeEnv?.['NG_APP_ALLOWED_ORIGINS']?.split(',').filter((origin: string) => origin.trim().length > 0)
    : ['https://your-frontend-domain.com'],

  logging: {
    level: 'error',
    logSecurityEvents: true
  }
};
