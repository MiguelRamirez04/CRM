import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CotizacionService } from '../../../../core/services/cotizacion.service';
import { CotizacionResponse } from '../../../../core/models/cotizacion.interface';
import { EstadoCotizacion } from '../../../../core/enums/estado-cotizacion.enum';
import { CotizacionesDialogComponent } from './cotizaciones-dialog/cotizaciones-dialog.component';

@Component({
  selector: 'app-cotizaciones-vista',
  standalone: true,
  imports: [CommonModule, FormsModule, CotizacionesDialogComponent],
  templateUrl:
   './cotizaciones-vista.component.html',
  styleUrls: ['./cotizaciones-vista.component.css']
})
export class CotizacionesVistaComponent implements OnInit {
  private cotizacionService = inject(CotizacionService);
  private router = inject(Router);

  // Datos
  cotizaciones: CotizacionResponse[] = [];
  cotizacionesFiltradas: CotizacionResponse[] = [];
  cotizacionSeleccionada: CotizacionResponse | null = null;

  // Estados de carga
  cargando = false;
  error = '';
  mensajeExito = '';

  // Filtros
  filtroEstado: EstadoCotizacion | '' = '';
  filtroCliente = '';
  filtroFolio = '';
  busquedaGlobal = '';

  // Paginación
  paginaActual = 1;
  itemsPorPagina = 10;
  totalPaginas = 0;

  // Ordenamiento
  columnaOrden: keyof CotizacionResponse = 'id';
  ordenAscendente = false;

  // Estados disponibles para el filtro
  estadosDisponibles = Object.values(EstadoCotizacion);

  // Modal detalle
  mostrarModalDetalle = false;

  ngOnInit(): void {
    this.cargarCotizaciones();
  }

  /**
   * Cargar todas las cotizaciones desde la API
   */
  cargarCotizaciones(): void {
    console.log('🔄 Cargando cotizaciones...');
    this.cargando = true;
    this.error = '';

    this.cotizacionService.obtenerTodas().subscribe({
      next: (data) => {
        console.log('✅ Cotizaciones cargadas:', data);
        this.cotizaciones = data;
        this.aplicarFiltros();
        this.cargando = false;
      },
      error: (err) => {
        console.error('❌ Error al cargar cotizaciones:', err);
        this.error = 'Error al cargar las cotizaciones. Por favor intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  /**
   * Aplicar todos los filtros a las cotizaciones
   */
  aplicarFiltros(): void {
    let resultados = [...this.cotizaciones];

    // Filtro por estado
    if (this.filtroEstado) {
      resultados = resultados.filter(c => c.estado === this.filtroEstado);
    }

    // Filtro por cliente
    if (this.filtroCliente.trim()) {
      resultados = resultados.filter(c => 
        c.cliente?.toLowerCase().includes(this.filtroCliente.toLowerCase())
      );
    }

    // Filtro por folio
    if (this.filtroFolio.trim()) {
      resultados = resultados.filter(c => 
        c.folio?.toLowerCase().includes(this.filtroFolio.toLowerCase())
      );
    }

    // Búsqueda global
    if (this.busquedaGlobal.trim()) {
      const busqueda = this.busquedaGlobal.toLowerCase();
      resultados = resultados.filter(c => 
        c.cliente?.toLowerCase().includes(busqueda) ||
        c.folio?.toLowerCase().includes(busqueda) ||
        c.rfc?.toLowerCase().includes(busqueda) ||
        c.descripcionServicio?.toLowerCase().includes(busqueda)
      );
    }

    this.cotizacionesFiltradas = resultados;
    this.calcularPaginacion();
    this.paginaActual = 1; // Reset a primera página
  }

  /**
   * Calcular número total de páginas
   */
  calcularPaginacion(): void {
    this.totalPaginas = Math.ceil(this.cotizacionesFiltradas.length / this.itemsPorPagina);
  }

  /**
   * Obtener cotizaciones para la página actual
   */
  get cotizacionesPaginadas(): CotizacionResponse[] {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    const fin = inicio + this.itemsPorPagina;
    return this.cotizacionesFiltradas.slice(inicio, fin);
  }

  /**
   * Cambiar de página
   */
  cambiarPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;
    }
  }

  /**
   * Ordenar por columna
   */
  ordenarPor(columna: keyof CotizacionResponse): void {
    if (this.columnaOrden === columna) {
      this.ordenAscendente = !this.ordenAscendente;
    } else {
      this.columnaOrden = columna;
      this.ordenAscendente = true;
    }

    this.cotizacionesFiltradas.sort((a, b) => {
      const valorA = a[columna];
      const valorB = b[columna];

      if (valorA === null || valorA === undefined) return 1;
      if (valorB === null || valorB === undefined) return -1;

      let comparacion = 0;
      if (valorA < valorB) comparacion = -1;
      if (valorA > valorB) comparacion = 1;

      return this.ordenAscendente ? comparacion : -comparacion;
    });
  }

  /**
   * Ver detalle de cotización - Abre el dialog
   */
  verDetalle(cotizacion: CotizacionResponse): void {
    console.log('👁️ Ver detalle de cotización:', cotizacion.id);
    this.cotizacionSeleccionada = cotizacion;
    this.mostrarModalDetalle = true;
  }

  /**
   * Cerrar modal de detalle
   */
  cerrarModalDetalle(): void {
    this.mostrarModalDetalle = false;
    this.cotizacionSeleccionada = null;
  }

  /**
   * Limpiar todos los filtros
   */
  limpiarFiltros(): void {
    this.filtroEstado = '';
    this.filtroCliente = '';
    this.filtroFolio = '';
    this.busquedaGlobal = '';
    this.aplicarFiltros();
  }

  /**
   * Navegar a crear nueva cotización
   */
  crearNuevaCotizacion(): void {
    this.router.navigate(['/dashboard/cotizaciones/nueva']);
  }

  /**
   * Obtener clase CSS para badge de estado
   */
  obtenerClaseEstado(estado: EstadoCotizacion): string {
    return this.cotizacionService.obtenerClaseEstado(estado);
  }

  /**
   * Obtener label en español para estado
   */
  obtenerLabelEstado(estado: EstadoCotizacion): string {
    return this.cotizacionService.obtenerLabelEstado(estado);
  }

  /**
   * Formatear fecha para display
   */
  formatearFecha(fecha: string | Date): string {
    if (!fecha) return '-';
    const f = new Date(fecha);
    return f.toLocaleDateString('es-MX', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  /**
   * Formatear moneda
   */
  formatearMoneda(valor: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(valor);
  }
}
