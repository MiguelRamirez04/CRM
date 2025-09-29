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
    const secureReq = req.clone({
      setHeaders: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest' // CSRF protection
      },
      withCredentials: true // CRÍTICO: para cookies HttpOnly
    });

    return next.handle(secureReq).pipe(
      catchError((error: HttpErrorResponse) => {
        // Si es error 401 y no es login/refresh, intentar refresh automático
        if (error.status === 401 && !this.isAuthRoute(req.url)) {
          return this.handle401Error(secureReq, next);
        }

        // Si es error 403, verificar CSRF
        if (error.status === 403) {
          console.warn('Posible ataque CSRF detectado o permisos insuficientes');
        }

        return throwError(() => error);
      })
    );
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
          
          // Si falla el refresh, redirigir a login
          this.authService.logout().subscribe();
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
    return u.includes('/auth/login') || 
           u.includes('/auth/refresh') || 
           u.includes('/auth/register') ||
           u.includes('/auth/registro') ||
           u.includes('/auth/logout');
  }
}