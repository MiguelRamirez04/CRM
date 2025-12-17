import { Component, Input, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';
import { Vehiculo, VehiculoHistorial, UsoVehiculo } from '../../../../../core/models/vehiculo.interface';

@Component({
  selector: 'app-vehiculo-historial',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vehiculo-historial.component.html',
  styleUrl: './vehiculo-historial.component.css'
})
export class VehiculoHistorialComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Estados
  vehiculo = signal<Vehiculo | null>(null);
  historialCambios = signal<VehiculoHistorial[]>([]);
  historialUso = signal<UsoVehiculo[]>([]);

  // Tab activo: 'uso' | 'cambios'
  tabActivo = signal<'uso' | 'cambios'>('uso');

  cargando = signal(true);
  error = signal<string | null>(null);

  // ID del vehículo desde la ruta
  vehiculoId = signal<number>(0);

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.vehiculoId.set(+id);
      this.cargarDatos();
    }
  }

  cambiarTab(tab: 'uso' | 'cambios'): void {
    this.tabActivo.set(tab);
  }

  private cargarDatos(): void {
    this.cargando.set(true);

    // Cargar todo en paralelo
    const id = this.vehiculoId();

    // 1. Vehículo
    const reqVehiculo = this.vehiculoService.getVehiculoById(id);
    // 2. Historial de Uso
    const reqUso = this.vehiculoService.getHistorialUso(id);
    // 3. Historial de Cambios
    const reqCambios = this.vehiculoService.getVehiculoHistorial(id);

    // Usamos forkJoin manejado manualmente con promesas o suscripciones anidadas
    // Para simplificar sin RxJS forkJoin imports, los haremos secuenciales o independientes
    // Aquí, por simplicidad y robustez, los cargamos secuencialmente pero rápido.

    this.vehiculoService.getVehiculoById(id).subscribe({
      next: (val) => {
        this.vehiculo.set(val);
        this.cargarHistoriales(id);
      },
      error: (err) => {
        this.error.set('Error al cargar el vehículo');
        this.cargando.set(false);
      }
    });
  }

  private cargarHistoriales(id: number): void {
    // Cargar Historial de Uso
    this.vehiculoService.getHistorialUso(id).subscribe({
      next: (val) => this.historialUso.set(val),
      error: (err) => console.error('Error usos', err),
      complete: () => this.verificarCargaCompleta()
    });

    // Cargar Historial de Cambios
    this.vehiculoService.getVehiculoHistorial(id).subscribe({
      next: (val) => this.historialCambios.set(val),
      error: (err) => console.error('Error cambios', err),
      complete: () => this.verificarCargaCompleta()
    });
  }

  // Contador para saber cuando terminar loading (muy simple)
  private cargasCompletadas = 0;
  private verificarCargaCompleta(): void {
    this.cargasCompletadas++;
    if (this.cargasCompletadas >= 2) {
      this.cargando.set(false);
    }
  }

  volver(): void {
    this.router.navigate(['/vehiculos']);
  }

  formatearFecha(fecha: string | Date): string {
    if (!fecha) return '-';
    return new Date(fecha).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  calcularDuracion(inicio: string, fin?: string): string {
    if (!fin) return 'En curso';

    const start = new Date(inicio).getTime();
    const end = new Date(fin).getTime();
    const diffMs = end - start;

    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    if (hours > 24) {
      const days = Math.floor(hours / 24);
      return `${days}d ${hours % 24}h`;
    }

    return `${hours}h ${minutes}m`;
  }

  // Helpers para Historial Cambios
  getIconoCambio(tipo: string): string {
    switch (tipo) {
      case 'CREADO': return '➕';
      case 'ACTUALIZADO': return '✏️';
      case 'ELIMINADO': return '🗑️';
      default: return '📝';
    }
  }

  getColorCambio(tipo: string): string {
    switch (tipo) {
      case 'CREADO': return '#10b981';
      case 'ACTUALIZADO': return '#f59e0b';
      case 'ELIMINADO': return '#ef4444';
      default: return '#6b7280';
    }
  }
}
