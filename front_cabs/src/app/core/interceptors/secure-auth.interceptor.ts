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
    const token = localStorage.getItem('access_token');

    // 🔥 CRÍTICO: NO agregar Content-Type para FormData
    // Angular lo maneja automáticamente con boundary correcto
    let headers: any = {
      'X-Requested-With': 'XMLHttpRequest'
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    // ⚠️ NO agregar Content-Type aquí
    // Si es FormData, Angular lo configura automáticamente

    const secureReq = req.clone({
      setHeaders: headers,
      withCredentials: false
    });

    // 🔥 Log para debugging (remover en producción)
    if (req.body instanceof FormData) {
      console.log('🔍 Interceptor detectó FormData');
      console.log('Headers finales:', secureReq.headers.keys());
      
      // ⚠️ NO debes ver 'content-type' aquí para FormData
      // Angular lo agrega automáticamente en el request final
    }

    return next.handle(secureReq).pipe(
      catchError(error => {
        // Error 415: Unsupported Media Type
        if (error.status === 415) {
          console.error('❌ Error 415 - Unsupported Media Type');
          console.error('URL:', req.url);
          console.error('Método:', req.method);
          console.error('Body type:', req.body?.constructor.name);
          console.error('Content-Type enviado:', error.error?.contentType);
          console.error('Content-Type esperado:', error.error?.expected);
        }
        
        if (error.status === 401) {
          return this.handle401Error(secureReq, next);
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
          
          // Reenviar request original
          return next.handle(request);
        }),
        catchError((err) => {
          this.isRefreshing = false;
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
}

