import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, interval } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { CotizacionService } from './cotizacion.service';
import { EstadoCotizacion } from '../enums/estado-cotizacion.enum';
import { CotizacionResponse } from '../models/cotizacion.interface';

@Injectable({
  providedIn: 'root'
})
export class CotizacionVencimientoService {
  private cotizacionService = inject(CotizacionService);
  private destroy$ = new BehaviorSubject<boolean>(false);
  private checkInterval = 1000 * 60 * 60; // Verificar cada hora

  constructor() {
    this.iniciarVerificacionVencimientos();
  }

  /**
   * Inicia el proceso de verificación periódica de vencimientos
   */
  private iniciarVerificacionVencimientos(): void {
    interval(this.checkInterval)
      .pipe(
        takeUntil(this.destroy$),
        switchMap(() => this.cotizacionService.obtenerTodas())
      )
      .subscribe({
        next: (cotizaciones) => {
          this.verificarVencimientos(cotizaciones);
        },
        error: (error) => {
          console.error('Error al verificar vencimientos:', error);
        }
      });
  }

  /**
   * Verifica cada cotización y actualiza su estado si está vencida
   */
  private verificarVencimientos(cotizaciones: CotizacionResponse[]): void {
    const ahora = new Date();

    cotizaciones
      .filter(cot => cot.estado === EstadoCotizacion.NUEVA) // Solo verificar las nuevas
      .forEach(cotizacion => {
        // Calcula la fecha de vencimiento considerando los días de validez
        const fechaCreacion = new Date(cotizacion.fechaCreacion);
        const fechaVencimiento = this.calcularFechaVencimiento(fechaCreacion, cotizacion.validezDias);

        if (ahora > fechaVencimiento) {
          console.log(`Cotización ${cotizacion.id} vencida. Actualizando estado...`);
          this.actualizarEstadoVencido(cotizacion.id);
        }
      });
  }

  /**
   * Calcula la fecha de vencimiento considerando meses variables
   * @param fechaInicio Fecha desde la que contar
   * @param dias Días de validez
   */
  private calcularFechaVencimiento(fechaInicio: Date, dias: number): Date {
    const fecha = new Date(fechaInicio);
    
    // Usamos un bucle para manejar correctamente el cambio de mes
    for (let i = 0; i < dias; i++) {
      fecha.setDate(fecha.getDate() + 1);
      
      // Si cambiamos de mes y el día es 1, significa que el mes anterior
      // no tenía suficientes días, así que ya contamos correctamente
      if (fecha.getDate() === 1) {
        const diasRestantes = dias - (i + 1);
        if (diasRestantes > 0) {
          fecha.setDate(fecha.getDate() + diasRestantes);
        }
        break;
      }
    }
    
    return fecha;
  }

  /**
   * Actualiza el estado de una cotización a VENCIDA
   */
  private actualizarEstadoVencido(id: number): void {
    this.cotizacionService
      .actualizar(id, { estado: EstadoCotizacion.VENCIDA })
      .subscribe({
        next: () => {
          console.log(`✅ Cotización ${id} marcada como vencida`);
        },
        error: (error) => {
          console.error(`❌ Error al marcar cotización ${id} como vencida:`, error);
        }
      });
  }

  /**
   * Detiene el proceso de verificación (cleanup)
   */
  destruir(): void {
    this.destroy$.next(true);
    this.destroy$.complete();
  }
}