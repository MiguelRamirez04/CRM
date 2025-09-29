import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, timer, throwError, of } from 'rxjs';
import { map, tap, catchError, switchMap, share } from 'rxjs/operators';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../../../environments/environment';
import { RolUsuario } from '../enums/rol-usuario.enum';
import { TipoTransmision } from '../enums/tipo-transmision.enum';

export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  nombreCompleto: string;
  email: string;
  contrasena: string;
  confirmarContrasena: string; // Enviar confirmación (requerida por el backend)
  rol: RolUsuario;
  licenciaConducir: boolean;
  transmisionHabilitada: TipoTransmision;
}

// Respuesta de login del backend (AuthController.Login)
export interface AuthResponse {
  user: User;
  token?: string;        // Backend devuelve 'token'
  accessToken?: string;  // Compatibilidad si cambiamos nombre en el futuro
  refreshToken?: string; // Solo para desarrollo
  expiresIn: number;
}

// Respuesta de registro del backend (RegistroExitosoResponseDto)
export interface RegistroResponse {
  usuario: {
    id: string;
    email: string;
    nombreCompleto?: string;
    rol?: string;
    permisos?: string[];
  };
  token?: string;
  expiraEn?: string | Date | null;
  exitoso?: boolean;
  mensaje?: string;
}

export interface RefreshResponse {
  accessToken?: string; // Solo para desarrollo
  expiresIn: number;
}

@Injectable({
  providedIn: 'root'
})
export class SecureAuthService {
  private readonly baseUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private refreshTokenRequest: Observable<RefreshResponse> | null = null;

  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map(user => !!user));

  constructor(
    private http: HttpClient,
    private cookieService: CookieService
  ) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    // Verificar si hay un usuario guardado en cookies
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

  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, loginData)
      .pipe(
        tap(response => {
          // Guardar usuario en cookie (no sensible)
          this.cookieService.set('user', JSON.stringify(response.user), {
            path: '/',
            secure: environment.production,
            sameSite: 'Strict'
          });

          this.currentUserSubject.next(response.user);
        }),
        catchError(this.handleError)
      );
  }

  register(registerData: RegisterRequest): Observable<RegistroResponse> {
    return this.http.post<RegistroResponse>(`${this.baseUrl}/auth/registro`, registerData)
      .pipe(
        tap(response => {
          // Opcional: Iniciar sesión automáticamente o manejar la respuesta
          console.log('Registro exitoso:', response);
        }),
        catchError(this.handleError)
      );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/logout`, {})
      .pipe(
        tap(() => {
          this.clearAuthData();
        }),
        catchError(error => {
          // Even if logout fails on server, clear local data
          this.clearAuthData();
          return throwError(error);
        })
      );
  }

  private clearAuthData(): void {
    this.cookieService.delete('user', '/');
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser();
  }

  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    return user ? user.permissions.includes(permission) : false;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user ? user.role === role : false;
  }

  /**
   * Verificar estado de autenticación (para guards)
   */
  checkAuthStatus(): Observable<boolean> {
    if (this.isAuthenticated()) {
      return this.isLoggedIn$;
    }

    // Si no hay usuario local, intentar verificar con el servidor
    return this.http.get<User>(`${this.baseUrl}/auth/me`)
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

  /**
   * Refresh token (para interceptor)
   */
  refreshToken(): Observable<RefreshResponse> {
    if (this.refreshTokenRequest) {
      return this.refreshTokenRequest;
    }

    this.refreshTokenRequest = this.http.post<RefreshResponse>(`${this.baseUrl}/auth/refresh`, {})
      .pipe(
        tap(response => {
          // Actualizar tiempo de expiración si viene
          if (response.expiresIn) {
            // Podrías guardar el nuevo tiempo de expiración aquí
          }
        }),
        catchError(error => {
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

  /**
   * Solicitar reset de contraseña
   */
  requestPasswordReset(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/forgot-password`, { email });
  }

  /**
   * Reset de contraseña con token
   */
  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/reset-password`, {
      token,
      newPassword
    });
  }

  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'Ha ocurrido un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = error.error.message;
    } else {
      // Error del lado del servidor o de red
      switch (error.status) {
        case 0:
          errorMessage = 'No se pudo conectar con el servidor. Verifica que el backend esté encendido en la URL configurada y que no haya bloqueos de CORS/firewall.';
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