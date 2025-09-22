import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, HTTP_INTERCEPTORS } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CookieService } from 'ngx-cookie-service';

import { routes } from './app.routes';
import { SecureAuthInterceptor } from './core/interceptors/secure-auth.interceptor';
import { SecurityHeadersInterceptor } from './core/interceptors/security-headers.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    
    // HTTP Client con interceptores de seguridad
    provideHttpClient(),
    
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
    
    // ng-bootstrap
    importProvidersFrom(NgbModule)
  ]
};
