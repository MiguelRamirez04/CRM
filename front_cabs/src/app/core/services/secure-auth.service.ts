// ============================================
// ARCHIVO COMPLETO: secure-auth.service.ts
// ============================================

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { map, tap, catchError, switchMap, share, take } from 'rxjs/operators';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { RolUsuario } from '../enums/rol-usuario.enum';
import { TipoTransmision } from '../enums/tipo-transmision.enum';

// ============================================
// INTERFACES (NO ELIMINAR)
// ============================================

export interface User {
  id: number;
  nombre: string;
  apellido: string;
  nombreCompleto?: string;
  telefono?: number | null;
  email: string;
  rol?: number | null;
  name?: string;
  role?: string;
  permissions?: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  nombre: string;
  apellido: string;
  telefono?: number | null;
  email: string;
  contrasena: string;
  confirmarContrasena: string;
  rol: RolUsuario;
  transmisionHabilitada?: TipoTransmision | string | null;
  activo?: boolean;
}

export interface AuthResponse {
  user: User;
  token?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresIn: number;
}

export interface RegistroResponse {
  usuario: {
    id: number;
    nombre: string;
    apellido: string;
    nombreCompleto?: string;
    telefono?: number | null;
    email: string;
    rol?: number | null;
    permisos?: string[];
  };
  token?: string;
  expiraEn?: string | Date | null;
  exitoso?: boolean;
  mensaje?: string;
}

export interface RefreshResponse {
  accessToken?: string;
  expiresIn: number;
}

// ============================================
// SERVICIO
// ============================================

@Injectable({
  providedIn: 'root'
})
export class SecureAuthService {
  private readonly baseUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private refreshTokenRequest: Observable<RefreshResponse> | null = null;
  private logoutPerformed = false;
  private csrfTokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map(user => !!user));
  public csrfToken$ = this.csrfTokenSubject.asObservable();

  private router = inject(Router);

  constructor(
    private http: HttpClient,
    private cookieService: CookieService
  ) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const userCookie = this.cookieService.get('user');
    if (userCookie) {
      try {
        const user = JSON.parse(userCookie);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error parsing user cookie:', error);
        this.logout();
      }
    }
  }

  // ============================================
  // LOGIN - CON GUARDADO DE TOKEN JWT
  // ============================================
  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/login`, loginData)
      .pipe(
        tap(response => {
          console.log('✅ Respuesta de login recibida:', response);
          
          this.logoutPerformed = false;

          // ⚠️ CRÍTICO: Guardar el TOKEN JWT
          const token = response.token || response.accessToken;
          
          if (token) {
            localStorage.setItem('access_token', token);
            console.log('✅ Token JWT guardado:', token.substring(0, 30) + '...');
          } else {
            console.error('❌ ERROR: El backend NO devolvió el token JWT');
          }

          // Guardar refreshToken si existe
          if (response.refreshToken) {
            localStorage.setItem('refresh_token', response.refreshToken);
          }

          // Guardar expiración
          if (response.expiresIn) {
            const expiresAt = Date.now() + (response.expiresIn * 1000);
            localStorage.setItem('token_expires_at', expiresAt.toString());
          }

          // Guardar usuario en cookie
          this.cookieService.set('user', JSON.stringify(response.user), {
            path: '/',
            secure: environment.production,
            sameSite: 'Strict'
          });

          this.currentUserSubject.next(response.user);
          
          console.log('✅ Autenticación completada correctamente');
        }),
        catchError(this.handleError)
      );
  }

  // ============================================
  // OBTENER TOKEN
  // ============================================
  getToken(): string | null {
    const token = localStorage.getItem('access_token');
    
    if (token && this.isTokenExpired()) {
      console.warn('⚠️ Token expirado, limpiando...');
      this.clearAuthData();
      return null;
    }
    
    return token;
  }

  // ============================================
  // VERIFICAR EXPIRACIÓN
  // ============================================
  private isTokenExpired(): boolean {
    const expiresAt = localStorage.getItem('token_expires_at');
    if (!expiresAt) return false;
    
    const expirationTime = parseInt(expiresAt, 10);
    const now = Date.now();
    
    return now >= (expirationTime - 300000);
  }

  // ============================================
  // REGISTER
  // ============================================
  register(registerData: RegisterRequest): Observable<RegistroResponse> {
    return this.http.post<RegistroResponse>(`${this.baseUrl}/api/auth/registro`, registerData)
      .pipe(
        tap(response => {
          console.log('Registro exitoso:', response);
          
          if (response.token) {
            localStorage.setItem('access_token', response.token);
          }
        }),
        catchError(this.handleError)
      );
  }

  // ============================================
  // LOGOUT
  // ============================================
  logout(): Observable<void> {
    this.logoutPerformed = true;
    this.forceLogout();
    return of(void 0);
  }

  forceLogout(): void {
    this.logoutPerformed = true;
    this.clearAuthData();
    this.redirectToLogin();
  }

  clearSession(): void {
    this.clearAuthData();
  }

  handleLoginSuccess(returnUrl?: string): void {
    const targetUrl = returnUrl && returnUrl !== '/auth/login' ? returnUrl : '/dashboard';
    setTimeout(() => {
      this.router.navigate([targetUrl], { replaceUrl: true });
    }, 100);
  }

  // ============================================
  // LIMPIAR DATOS
  // ============================================
  private clearAuthData(): void {
    this.cookieService.delete('user', '/');
    
    // Limpiar tokens
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expires_at');
    
    this.currentUserSubject.next(null);
    this.csrfTokenSubject.next(null);
    
    console.log('✅ Datos de autenticación limpiados');
  }

  private redirectToLogin(): void {
    setTimeout(() => {
      this.router.navigate(['/auth/login'], { 
        replaceUrl: true,
        queryParams: { message: 'Sesión cerrada correctamente' }
      });
    }, 100);
  }

  // ============================================
  // MÉTODOS DE USUARIO
  // ============================================
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser() && !!this.getToken();
  }

  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    return user ? (user.permissions?.includes(permission) ?? false) : false;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user ? user.role === role : false;
  }

  // ============================================
  // CHECK AUTH STATUS
  // ============================================
  checkAuthStatus(): Observable<boolean> {
    if (this.logoutPerformed) {
      return of(false);
    }

    if (!this.getToken()) {
      this.clearAuthData();
      return of(false);
    }

    if (this.isAuthenticated()) {
      return this.isLoggedIn$;
    }

    return this.http.get<User>(`${this.baseUrl}/api/auth/me`)
      .pipe(
        map(user => {
          this.currentUserSubject.next(user);
          this.cookieService.set('user', JSON.stringify(user), {
            path: '/',
            secure: environment.production,
            sameSite: 'Strict'
          });
          return true;
        }),
        catchError(() => {
          this.clearAuthData();
          return of(false);
        })
      );
  }

  // ============================================
  // REFRESH TOKEN
  // ============================================
  refreshToken(): Observable<RefreshResponse> {
    if (this.refreshTokenRequest) {
      return this.refreshTokenRequest;
    }

    this.refreshTokenRequest = this.http.post<RefreshResponse>(`${this.baseUrl}/api/auth/refresh`, {})
      .pipe(
        tap(response => {
          if (response.accessToken) {
            localStorage.setItem('access_token', response.accessToken);
            console.log('✅ Token refrescado correctamente');
          }
          
          if (response.expiresIn) {
            const expiresAt = Date.now() + (response.expiresIn * 1000);
            localStorage.setItem('token_expires_at', expiresAt.toString());
          }
        }),
        catchError(error => {
          console.error('❌ Error al refrescar token:', error);
          this.clearAuthData();
          return throwError(error);
        }),
        share(),
        tap(() => {
          this.refreshTokenRequest = null;
        })
      );

    return this.refreshTokenRequest;
  }

  // ============================================
  // PASSWORD RESET
  // ============================================
  requestPasswordReset(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/auth/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/auth/reset-password`, {
      token,
      newPassword
    });
  }

  // ============================================
  // CSRF TOKEN
  // ============================================
  obtenerCsrfToken(): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/auth/csrf-token`, {
      withCredentials: true
    }).pipe(
      tap((response: any) => {
        if (response?.csrfToken) {
          this.csrfTokenSubject.next(response.csrfToken);
          console.log('✅ Token CSRF almacenado');
        }
      }),
      catchError(error => {
        console.error('❌ Error obteniendo token CSRF:', error);
        return throwError(() => error);
      })
    );
  }

  getCsrfToken(): string | null {
    return this.csrfTokenSubject.value;
  }

  inicializarCsrfToken(): Observable<any> {
    return this.isLoggedIn$.pipe(
      take(1),
      switchMap(isLoggedIn => {
        if (isLoggedIn) {
          console.log('🔄 Inicializando token CSRF...');
          return this.obtenerCsrfToken();
        }
        return of(null);
      })
    );
  }

  // ============================================
  // ERROR HANDLER
  // ============================================
  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'Ha ocurrido un error desconocido';

    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else {
      switch (error.status) {
        case 0:
          errorMessage = 'No se pudo conectar con el servidor';
          break;
        case 400:
          errorMessage = error.error?.message || 'Datos inválidos';
          break;
        case 401:
          errorMessage = 'Credenciales incorrectas';
          break;
        case 403:
          errorMessage = 'No tienes permisos para esta acción';
          break;
        case 404:
          errorMessage = 'Recurso no encontrado';
          break;
        case 409:
          errorMessage = 'El usuario ya existe';
          break;
        case 422:
          errorMessage = 'Datos de entrada inválidos';
          break;
        case 500:
          errorMessage = 'Error interno del servidor';
          break;
        default:
          errorMessage = error.error?.message || `Error ${error.status}`;
      }
    }

    console.error('Auth error:', error);
    return throwError(() => new Error(errorMessage));
  };
}