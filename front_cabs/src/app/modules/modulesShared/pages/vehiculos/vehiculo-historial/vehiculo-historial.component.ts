import { Component, Input, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';
import { Vehiculo, VehiculoHistorial } from '../../../../../core/models/vehiculo.interface';

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
  historial = signal<VehiculoHistorial[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);

  // ID del vehículo desde la ruta
  vehiculoId = signal<number>(0);

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.vehiculoId.set(+id);
      this.cargarVehiculo();
      this.cargarHistorial();
    }
  }

  private cargarVehiculo(): void {
    this.vehiculoService.getVehiculoById(this.vehiculoId()).subscribe({
      next: (vehiculo) => {
        this.vehiculo.set(vehiculo);
      },
      error: (err) => {
        console.error('Error al cargar vehículo:', err);
        this.error.set('Error al cargar el vehículo');
        this.cargando.set(false);
      }
    });
  }

  private cargarHistorial(): void {
    this.vehiculoService.getVehiculoHistorial(this.vehiculoId()).subscribe({
      next: (historial) => {
        this.historial.set(historial);
        this.cargando.set(false);
      },
      error: (err) => {
        console.error('Error al cargar historial:', err);
        this.error.set('Error al cargar el historial');
        this.cargando.set(false);
      }
    });
  }

  volver(): void {
    this.router.navigate(['/vehiculos']);
  }

  formatearFecha(fecha: Date): string {
    return new Date(fecha).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

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
