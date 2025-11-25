import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { CookieService } from 'ngx-cookie-service';
import { routes } from './app.routes';
import { SecureAuthInterceptor } from './core/interceptors/secure-auth.interceptor';
import { SecurityHeadersInterceptor } from './core/interceptors/security-headers.interceptor';
import { SecureAuthService } from './core/services/secure-auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimations(), // Requerido para Angular Material

  // HTTP Client con interceptores de seguridad definidos por DI
  provideHttpClient(withInterceptorsFromDi()),

    // Servicios principales
    CookieService,

    // Interceptores de seguridad
    {
      provide: HTTP_INTERCEPTORS,
      useClass: SecurityHeadersInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: SecureAuthInterceptor,
      multi: true
    },

    // Inicializador de aplicación - obtiene token CSRF si el usuario está logueado
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: SecureAuthService) => () => {
        console.log('🚀 Inicializando aplicación - obteniendo token CSRF...');
        return authService.inicializarCsrfToken().toPromise();
      },
      deps: [SecureAuthService],
      multi: true
    }
  ]
};
