import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { SecureAuthService } from '../services/secure-auth.service';

@Injectable()
export class SecureAuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private authService: SecureAuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Asegurar que todas las requests incluyan credentials para cookies HttpOnly
    const headers: any = {
      'X-Requested-With': 'XMLHttpRequest' // CSRF protection
    };

    // 🔥 CRÍTICO: SOLO agregar Content-Type si NO es FormData
    // Angular maneja automáticamente el Content-Type con boundary para FormData
    if (!(req.body instanceof FormData)) {
      headers['Content-Type'] = 'application/json';
    }

    // Para métodos que modifican datos, incluir el token CSRF desde el servicio
    if (this.requiresCsrfToken(req.method)) {
      const csrfToken = this.authService.getCsrfToken();
      if (csrfToken) {
        headers['X-XSRF-TOKEN'] = csrfToken;
        console.log(`🔒 CSRF Token incluido para ${req.method} ${req.url}`, csrfToken.substring(0, 20) + '...');
      } else {
        console.warn(`⚠️ CSRF Token NO encontrado en servicio para ${req.method} ${req.url}`);
        console.warn('Asegúrate de llamar obtenerCsrfToken() después del login');
      }
    }


    let secureReq = req.clone({
      setHeaders: headers,
      withCredentials: true // CRÍTICO: para cookies HttpOnly
    });

    return next.handle(secureReq).pipe(
      catchError((error: HttpErrorResponse) => {
        // Error 415: Unsupported Media Type
        if (error.status === 415) {
          console.error('❌ Error 415 - Unsupported Media Type');
          console.error('URL:', req.url);
          console.error('Método:', req.method);
          console.error('Body type:', req.body?.constructor.name);
          console.error('Verifica que el servidor acepte el Content-Type enviado');
        }

        // Si es error 401 y no es login/refresh, intentar refresh automático
        if (error.status === 401 && !this.isAuthRoute(req.url)) {
          return this.handle401Error(secureReq, next);
        }

        // Si es error 403, verificar CSRF
        if (error.status === 403) {
          console.warn('⚠️ Error 403 - Posible problema con CSRF o permisos insuficientes');
          console.warn('Verifica que el token CSRF esté configurado correctamente');
        }

        return throwError(() => error);
      })
    );
  }

  private requiresCsrfToken(method: string): boolean {
    return ['POST', 'PUT', 'DELETE', 'PATCH'].includes(method.toUpperCase());
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.authService.refreshToken().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(true);
          
          // Reenviar request original con nuevas cookies
          return next.handle(request);
        }),
        catchError((err) => {
          this.isRefreshing = false;
          
          // Si falla el refresh, forzar logout inmediato
          this.authService.forceLogout();
          return throwError(() => err);
        })
      );
    } else {
      // Si ya está refrescando, esperar a que termine
      return this.refreshTokenSubject.pipe(
        filter(result => result !== null),
        take(1),
        switchMap(() => next.handle(request))
      );
    }
  }

  private isAuthRoute(url: string): boolean {
    const u = url.toLowerCase();
    return u.includes('/api/auth/login') || 
           u.includes('/api/auth/refresh') || 
           u.includes('/api/auth/register') ||
           u.includes('/api/auth/registro') ||
           u.includes('/api/auth/logout');
  }
}