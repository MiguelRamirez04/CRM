import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { EstadoCotizacion } from '../enums/estado-cotizacion.enum';
import { CotizacionService } from './cotizacion.service';

@Injectable({
  providedIn: 'root'
})
export class VencimientoService {
  private readonly INTERVALO_VERIFICACION = 1000 * 60 * 60; // Verificar cada hora

  constructor(
    private http: HttpClient,
    private cotizacionService: CotizacionService
  ) {
    // Iniciar verificación automática
    this.iniciarVerificacionAutomatica();
  }

  /**
   * Inicia el proceso automático de verificación de vencimientos
   */
  private iniciarVerificacionAutomatica(): void {
    interval(this.INTERVALO_VERIFICACION)
      .pipe(
        switchMap(() => this.verificarVencimientos())
      )
      .subscribe();
  }

  /**
   * Verifica todas las cotizaciones activas y actualiza su estado si están vencidas
   */
  private verificarVencimientos(): Observable<void> {
    return this.cotizacionService.obtenerCotizacionesActivas().pipe(
      map(cotizaciones => {
        const hoy = new Date();
        
        cotizaciones.forEach((cotizacion: { fechaCreacion: string | number | Date; validezDias: number; estado: EstadoCotizacion; id: any; }) => {
          // Calcular fecha de vencimiento considerando meses variables
          const fechaCreacion = new Date(cotizacion.fechaCreacion);
          const fechaVencimiento = this.calcularFechaVencimiento(fechaCreacion, cotizacion.validezDias);
          
          // Si ya pasó la fecha de vencimiento y no está marcada como vencida
          if (hoy > fechaVencimiento && cotizacion.estado !== EstadoCotizacion.VENCIDA) {
            // Actualizar a estado vencido
            this.cotizacionService.actualizarEstado(cotizacion.id, EstadoCotizacion.VENCIDA)
              .subscribe(() => {
                console.log(`Cotización ${cotizacion.id} marcada como vencida`);
              });
          }
        });
      })
    );
  }

  /**
   * Calcula la fecha real de vencimiento considerando meses variables
   * @param fechaInicio Fecha de inicio
   * @param dias Días de validez
   */
  private calcularFechaVencimiento(fechaInicio: Date, dias: number): Date {
    const fechaVencimiento = new Date(fechaInicio);
    
    // Iterar día por día para considerar correctamente los cambios de mes
    for (let i = 0; i < dias; i++) {
      fechaVencimiento.setDate(fechaVencimiento.getDate() + 1);
    }
    
    return fechaVencimiento;
  }
}