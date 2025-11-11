// =====================================================================================
// SERVICIO SIGNALR - signalr.service.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Gestiona la conexión SignalR para notificaciones en tiempo real.
// Mantiene la conexión activa y maneja eventos del servidor.
//
// FUNCIONALIDADES:
// - Conexión automática al hub de notificaciones
// - Registro del usuario en el grupo correspondiente
// - Recepción de notificaciones en tiempo real
// - Reconexión automática en caso de desconexión
//
// =====================================================================================

import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notificacion } from '../models/notificacion.interface';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private connectionState$ = new BehaviorSubject<'connecting' | 'connected' | 'disconnected' | 'reconnecting'>('disconnected');
  private notificaciones$ = new BehaviorSubject<Notificacion[]>([]);

  constructor() {}

  // Observable para el estado de la conexión
  get connectionState(): Observable<'connecting' | 'connected' | 'disconnected' | 'reconnecting'> {
    return this.connectionState$.asObservable();
  }

  // Observable para nuevas notificaciones
  get notificaciones(): Observable<Notificacion[]> {
    return this.notificaciones$.asObservable();
  }

  // Iniciar conexión SignalR
  async startConnection(userId: number): Promise<void> {
    try {
      this.connectionState$.next('connecting');

      // Crear conexión al hub
      // NOTA: No usamos accessTokenFactory porque el token JWT está en una cookie HttpOnly
      // que se envía automáticamente con las credenciales
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.apiUrl}/hubs/notificaciones`, {
          withCredentials: true // Importante: enviar cookies con la solicitud
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Configurar eventos
      this.setupConnectionEvents();

      // Iniciar conexión
      await this.hubConnection.start();
      console.log('✅ Conexión SignalR establecida, estado:', this.hubConnection.state);

      // Esperar a que la conexión esté completamente establecida
      await this.waitForConnection();
      console.log('✅ Conexión SignalR validada como Connected');

      // Registrar usuario en el grupo
      console.log(`📝 Registrando usuario ${userId} en SignalR...`);
      console.log('🔍 Tipo de userId:', typeof userId);
      console.log('🔍 Valor de userId:', userId);

      // Convertir a string para el backend
      const userIdStr = userId.toString();
      console.log('🔍 Enviando como string:', userIdStr);

      await this.hubConnection!.invoke('RegistrarUsuario', userIdStr);
      console.log(`✅ Usuario ${userIdStr} registrado exitosamente en el grupo`);

      this.connectionState$.next('connected');
      console.log('✅ Conectado al hub de notificaciones');

    } catch (error) {
      console.error('❌ Error al conectar con SignalR:', error);
      this.connectionState$.next('disconnected');
      throw error;
    }
  }

  // Detener conexión
  async stopConnection(userId?: number): Promise<void> {
    if (this.hubConnection && userId) {
      try {
        await this.hubConnection.invoke('DesregistrarUsuario', userId.toString());
      } catch (error) {
        console.warn('Error al desregistrar usuario:', error);
      }
    }

    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }

    this.connectionState$.next('disconnected');
    console.log('🔌 Desconectado del hub de notificaciones');
  }

  // Agregar notificación manualmente (para compatibilidad)
  addNotificacion(notificacion: Notificacion): void {
    const current = this.notificaciones$.value;
    this.notificaciones$.next([notificacion, ...current]);
  }

  // Limpiar notificaciones
  clearNotificaciones(): void {
    this.notificaciones$.next([]);
  }

  private setupConnectionEvents(): void {
    if (!this.hubConnection) return;

    // Evento de reconexión
    this.hubConnection.onreconnecting(() => {
      this.connectionState$.next('reconnecting');
      console.log('🔄 Reconectando al hub de notificaciones...');
    });

    // Evento de reconexión exitosa
    this.hubConnection.onreconnected(() => {
      this.connectionState$.next('connected');
      console.log('✅ Reconectado al hub de notificaciones');
    });

    // Evento de desconexión
    this.hubConnection.onclose(() => {
      this.connectionState$.next('disconnected');
      console.log('❌ Desconectado del hub de notificaciones');
    });

    // Evento de nueva notificación
    this.hubConnection.on('RecibirNotificacion', (notificacion: Notificacion) => {
      console.log('🔔 Nueva notificación recibida:', notificacion);
      this.addNotificacion(notificacion);
    });
  }

  private async waitForConnection(): Promise<void> {
    if (!this.hubConnection) return;

    // Esperar hasta que la conexión esté en estado 'Connected'
    const maxRetries = 10;
    const retryDelay = 100; // ms

    for (let i = 0; i < maxRetries; i++) {
      if (this.hubConnection.state === 'Connected') {
        return;
      }
      await new Promise(resolve => setTimeout(resolve, retryDelay));
    }

    throw new Error('Timeout esperando conexión SignalR');
  }

  private getAccessToken(): string {
    // Obtener token del localStorage o sessionStorage
    return localStorage.getItem('access_token') ||
           sessionStorage.getItem('access_token') || '';
  }
}