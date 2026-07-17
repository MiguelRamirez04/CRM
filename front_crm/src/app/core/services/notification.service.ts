import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private snackBar: MatSnackBar) { }

  /**
   * Muestra una notificación de éxito
   */
  success(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: duration,
      panelClass: ['snackbar-success'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  /**
   * Muestra una notificación de error
   */
  error(message: string, duration: number = 5000): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: duration,
      panelClass: ['snackbar-error'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  /**
   * Muestra una notificación de información
   */
  info(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: duration,
      panelClass: ['snackbar-info'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  /**
   * Muestra una notificación de advertencia
   */
  warning(message: string, duration: number = 4000): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: duration,
      panelClass: ['snackbar-warning'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
}