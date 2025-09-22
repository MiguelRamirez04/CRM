import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { SecureAuthService } from '../services/secure-auth.service';

@Injectable({
  providedIn: 'root'
})
export class SecureAuthGuard implements CanActivate, CanActivateChild {
  
  constructor(
    private authService: SecureAuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    return this.checkAuth(route);
  }

  canActivateChild(route: ActivatedRouteSnapshot): Observable<boolean> {
    return this.checkAuth(route);
  }

  private checkAuth(route: ActivatedRouteSnapshot): Observable<boolean> {
    // Verificar autenticación con el servidor (cookies HttpOnly)
    return this.authService.checkAuthStatus().pipe(
      map(user => {
        if (user) {
          // Verificar permisos específicos de la ruta
          const requiredPermission = route.data?.['permission'];
          const requiredRole = route.data?.['role'];
          
          if (requiredPermission && !this.authService.hasPermission(requiredPermission)) {
            this.router.navigate(['/unauthorized']);
            return false;
          }
          
          if (requiredRole && !this.authService.hasRole(requiredRole)) {
            this.router.navigate(['/unauthorized']);
            return false;
          }
          
          return true;
        } else {
          // Guardar URL intentada para redireccionar después del login
          const returnUrl = route.url.map(segment => segment.path).join('/');
          this.router.navigate(['/auth/login'], { 
            queryParams: { returnUrl: returnUrl }
          });
          return false;
        }
      }),
      catchError(() => {
        // Error en verificación - redirigir a login
        this.router.navigate(['/auth/login']);
        return of(false);
      })
    );
  }
}

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private authService: SecureAuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (this.authService.hasRole('admin')) {
      return true;
    }
    
    this.router.navigate(['/unauthorized']);
    return false;
  }
}

@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate {
  constructor(
    private authService: SecureAuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (!this.authService.isAuthenticated()) {
      return true;
    }
    
    // Si ya está logueado, redirigir al dashboard
    this.router.navigate(['/dashboard']);
    return false;
  }
}