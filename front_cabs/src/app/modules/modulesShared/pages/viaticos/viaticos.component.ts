import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GastoViaticoService } from '../../../../core/services/gasto-viatico.service';
import {
  GastoViaticoResponse,
  GastoViaticoPaginatedResponse,
  GastoViaticoFiltros
} from '../../../../core/models/gasto-viatico.interface';
import { ViaticosDialogComponent } from './viaticos-dialog/viaticos-dialog.component';
import { ViaticosDetalleComponent } from './viaticos-detalle/viaticos-detalle.component';

@Component({
  selector: 'app-viaticos',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, ViaticosDialogComponent, ViaticosDetalleComponent],
  templateUrl: './viaticos.component.html',
  styleUrls: ['./viaticos.component.css']
})
export class ViaticosComponent implements OnInit {
  private router = inject(Router);
  private viaticoService = inject(GastoViaticoService);

  viaticos: GastoViaticoResponse[] = [];
  
  // Exponer Math para el template
  Math = Math;
  
  // Paginación
  paginaActual = 1;
  resultadosPorPagina = 10;
  totalPaginas = 0;
  totalItems = 0;
  
  // Control de UI
  cargando = false;
  mensajeExito = '';
  mensajeError = '';
  mostrarDialog = false;
  viaticoEditando: GastoViaticoResponse | null = null;
  
  // Control de panel de detalles
  mostrarDetalle = false;
  viaticoDetalleId: number | null = null;
  
  // Filtros
  filtroOrdenId: number | null = null;
  filtroFechaDesde: string = '';
  filtroFechaHasta: string = '';

  ngOnInit(): void {
    this.cargarViaticos();
  }

  /**
   * Cargar viáticos con filtros y paginación
   */
  cargarViaticos(): void {
    this.cargando = true;
    this.mensajeError = '';

    const filtros: GastoViaticoFiltros = {
      ordenId: this.filtroOrdenId,
      fechaDesde: this.filtroFechaDesde || undefined,
      fechaHasta: this.filtroFechaHasta || undefined,
      pageNumber: this.paginaActual,
      pageSize: this.resultadosPorPagina
    };

    this.viaticoService.obtenerViaticos(filtros).subscribe({
      next: (response: GastoViaticoPaginatedResponse) => {
        this.viaticos = response.items;
        this.totalItems = response.totalItems ?? 0;
        this.totalPaginas = response.totalPaginas ?? 0;
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar viáticos:', err);
        this.mensajeError = 'Error al cargar la lista de viáticos. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  /**
   * Mostrar dialog para crear nuevo viático
   */
  nuevoViatico(): void {
    this.viaticoEditando = null;
    this.mostrarDialog = true;
    this.mensajeExito = '';
    this.mensajeError = '';
  }

  /**
   * Mostrar dialog para editar viático existente
   */
  editarViatico(viatico: GastoViaticoResponse): void {
    this.viaticoEditando = viatico;
    this.mostrarDialog = true;
    this.mensajeExito = '';
    this.mensajeError = '';
  }

  /**
   * Cerrar dialog
   */
  cerrarDialog(): void {
    this.mostrarDialog = false;
    this.viaticoEditando = null;
  }

  /**
   * Abrir panel de detalles
   */
  verDetalle(viatico: GastoViaticoResponse): void {
    this.viaticoDetalleId = viatico.id;
    this.mostrarDetalle = true;
  }

  /**
   * Cerrar panel de detalles
   */
  cerrarDetalle(): void {
    this.mostrarDetalle = false;
    this.viaticoDetalleId = null;
  }

  /**
   * Manejar guardado exitoso desde el dialog
   */
  onViaticoGuardado(viatico: GastoViaticoResponse): void {
    if (this.viaticoEditando) {
      this.mensajeExito = `Viático #${viatico.id} actualizado exitosamente.`;
    } else {
      this.mensajeExito = `Viático #${viatico.id} creado exitosamente.`;
    }
    
    this.cerrarDialog();
    this.cargarViaticos(); // Recargar lista
    
    // Limpiar mensaje después de 5 segundos
    setTimeout(() => {
      this.mensajeExito = '';
    }, 5000);
  }

  /**
   * Aplicar filtros de búsqueda
   */
  aplicarFiltros(): void {
    this.paginaActual = 1; // Reset a página 1
    this.cargarViaticos();
  }

  /**
   * Limpiar filtros
   */
  limpiarFiltros(): void {
    this.filtroOrdenId = null;
    this.filtroFechaDesde = '';
    this.filtroFechaHasta = '';
    this.paginaActual = 1;
    this.cargarViaticos();
  }

  /**
   * Cambiar página
   */
  cambiarPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;
      this.cargarViaticos();
    }
  }

  /**
   * Formatear fecha para display
   */
  formatearFecha(fecha: string): string {
    return this.viaticoService.formatearFechaDisplay(fecha);
  }

  /**
   * Formatear monto
   */
  formatearMoneda(monto: number): string {
    return this.viaticoService.formatearMoneda(monto);
  }

  /**
   * Obtener label de factura
   */
  getLabelFactura(tieneFactura: boolean): string {
    return this.viaticoService.obtenerLabelFactura(tieneFactura);
  }

  /**
   * Obtener clase CSS para badge de factura
   */
  getClaseFactura(tieneFactura: boolean): string {
    return this.viaticoService.obtenerClaseFactura(tieneFactura);
  }

  /**
   * Generar array de páginas para paginación
   */
  get paginasArray(): number[] {
    return Array.from({ length: this.totalPaginas }, (_, i) => i + 1);
  }
}
