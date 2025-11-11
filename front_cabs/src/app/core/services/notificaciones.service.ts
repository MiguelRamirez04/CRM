// =====================================================================================
// SERVICIO NOTIFICACIONES SISTEMA - notificaciones.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Gestiona las notificaciones del sistema desde el backend.
// Integra SignalR para notificaciones en tiempo real.
//
// FUNCIONALIDADES:
// - Obtener notificaciones del usuario
// - Marcar notificaciones como leídas
// - Integración con SignalR para tiempo real
// - Gestión del estado de notificaciones
//
// =====================================================================================

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notificacion, NotificacionResponseDto } from '../models/notificacion.interface';
import { SignalRService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class NotificacionesService {
  private apiUrl = `${environment.apiUrl}/api/Notificaciones`;
  private notificaciones$ = new BehaviorSubject<Notificacion[]>([]);

  constructor(
    private http: HttpClient,
    private signalRService: SignalRService
  ) {
    // Suscribirse a notificaciones en tiempo real
    this.signalRService.notificaciones.subscribe(notificaciones => {
      this.notificaciones$.next(notificaciones);
    });
  }

  // Observable para acceder a las notificaciones
  get notificaciones(): Observable<Notificacion[]> {
    return this.notificaciones$.asObservable();
  }

  // Obtener notificaciones del usuario actual
  getNotificaciones(): Observable<Notificacion[]> {
    return this.http.get<Notificacion[]>(this.apiUrl).pipe(
      tap(notificaciones => {
        this.notificaciones$.next(notificaciones);
      })
    );
  }

  // Marcar notificación como leída
  marcarComoLeida(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/marcar-leida`, {}).pipe(
      tap(() => {
        // Actualizar el estado local
        const current = this.notificaciones$.value;
        const updated = current.map(n =>
          n.id === id ? { ...n, leida: true, fechaLectura: new Date().toISOString() } : n
        );
        this.notificaciones$.next(updated);
      })
    );
  }

  // Obtener cantidad de notificaciones no leídas
  getNotificacionesNoLeidas(): Observable<number> {
    return this.notificaciones.pipe(
      map(notificaciones => notificaciones.filter(n => !n.leida).length)
    );
  }

  // Limpiar notificaciones (útil al cerrar sesión)
  clearNotificaciones(): void {
    this.notificaciones$.next([]);
    this.signalRService.clearNotificaciones();
  }

  // Método auxiliar para obtener el conteo actual
  getNotificacionesNoLeidasCount(): number {
    return this.notificaciones$.value.filter(n => !n.leida).length;
  }
}