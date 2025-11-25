import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class SecurityHeadersInterceptor implements HttpInterceptor {
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Headers de seguridad para todas las requests
    const secureReq = req.clone({
      setHeaders: {
        // CSRF Protection - Token personalizado
        'X-Requested-With': 'XMLHttpRequest',
        
        // Content Security Policy
        'X-Content-Type-Options': 'nosniff',
        
        // Prevenir XSS
        'X-XSS-Protection': '1; mode=block',
        
        // Forzar HTTPS en producción
        ...(location.protocol === 'https:' && {
          'Strict-Transport-Security': 'max-age=31536000; includeSubDomains'
        }),
        
        // Prevenir clickjacking
        'X-Frame-Options': 'DENY',
        
        // Referrer Policy
        'Referrer-Policy': 'strict-origin-when-cross-origin'
      }
    });

    return next.handle(secureReq);
  }
}