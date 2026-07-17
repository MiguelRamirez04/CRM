import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables, ChartConfiguration, ChartType } from 'chart.js';
import { ReportesCotizacionesService } from '../../../../core/services/reportes-cotizaciones.service';
import { 
  EstadisticasGeneralesDto, 
  TopClienteDto, 
  RendimientoAgenteDto, 
  ProductoCotizadoDto, 
  CotizacionPorRangoDto, 
  CotizacionVencimientoDto 
} from '../../../../core/models/reportes-cotizaciones.interface';

Chart.register(...registerables);

@Component({
  selector: 'app-metricas-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './metricas-documentos.component.html',
  styleUrl: './metricas-documentos.component.css'
})
export class MetricasDocumentosComponent implements OnInit, OnDestroy {
  private reportesService = inject(ReportesCotizacionesService);

  // Exponer Math para usar en el template
  Math = Math;

  activeTab: string = 'general';
  loading: boolean = false;
  error: string | null = null;

  // Filtros de fecha
  fechaInicio: string = '';
  fechaFin: string = '';

  // Paginación para próximas a vencer
  proximasVencerPage: number = 1;
  proximasVencerPageSize: number = 20;
  proximasVencerTotal: number = 0;

  // Data
  estadisticasGenerales: EstadisticasGeneralesDto | null = null;
  topClientes: TopClienteDto[] = [];
  rendimientoAgentes: RendimientoAgenteDto[] = [];
  productosMasCotizados: ProductoCotizadoDto[] = [];
  cotizacionesPorRango: CotizacionPorRangoDto[] = [];
  proximasVencer: CotizacionVencimientoDto[] = [];

  // Chart instances
  private charts: { [key: string]: Chart } = {};

  ngOnInit() {
    // Establecer rango de fecha por defecto (último mes)
    const today = new Date();
    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    
    this.fechaInicio = lastMonth.toISOString().split('T')[0];
    this.fechaFin = today.toISOString().split('T')[0];
    
    this.loadEstadisticasGenerales();
  }

  ngOnDestroy() {
    this.destroyCharts();
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.error = null;
    // Pequeño delay para asegurar que el canvas esté en el DOM
    setTimeout(() => {
      this.loadDataForTab(tab);
    }, 50);
  }

  loadDataForTab(tab: string) {
    switch (tab) {
      case 'general':
        this.loadEstadisticasGenerales();
        break;
      case 'clientes':
        this.loadTopClientes();
        break;
      case 'agentes':
        this.loadRendimientoAgentes();
        break;
      case 'productos':
        this.loadProductosMasCotizados();
        break;
      case 'rangos':
        this.loadCotizacionesPorRango();
        break;
      case 'vencimientos':
        this.loadProximasVencer();
        break;
    }
  }

  private destroyCharts() {
    Object.values(this.charts).forEach(chart => chart.destroy());
    this.charts = {};
  }

  private destroyChart(key: string) {
    if (this.charts[key]) {
      this.charts[key].destroy();
      delete this.charts[key];
    }
  }

  // ==========================================
  // LOADERS
  // ==========================================

  loadEstadisticasGenerales() {
    this.loading = true;
    this.reportesService.getEstadisticasGenerales(this.fechaInicio, this.fechaFin).subscribe({
      next: (response) => {
        if (response.success) {
          this.estadisticasGenerales = response.data;
          // Esperar a que Angular actualice el DOM antes de renderizar la gráfica
          setTimeout(() => this.renderGeneralCharts(), 100);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar estadísticas generales';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadTopClientes() {
    this.loading = true;
    this.reportesService.getTopClientes(10, this.fechaInicio, this.fechaFin).subscribe({
      next: (response) => {
        if (response.success) {
          this.topClientes = response.data;
          // Esperar a que Angular actualice el DOM antes de renderizar la gráfica
          setTimeout(() => this.renderTopClientesChart(), 100);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar top clientes';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadRendimientoAgentes() {
    this.loading = true;
    this.reportesService.getRendimientoAgentes(this.fechaInicio, this.fechaFin).subscribe({
      next: (response) => {
        if (response.success) {
          this.rendimientoAgentes = response.data;
          // Esperar a que Angular actualice el DOM antes de renderizar la gráfica
          setTimeout(() => this.renderRendimientoAgentesChart(), 100);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar rendimiento de agentes';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadProductosMasCotizados() {
    this.loading = true;
    this.reportesService.getProductosMasCotizados(10, this.fechaInicio, this.fechaFin).subscribe({
      next: (response) => {
        if (response.success) {
          this.productosMasCotizados = response.data;
          // Esperar a que Angular actualice el DOM antes de renderizar la gráfica
          setTimeout(() => this.renderProductosChart(), 100);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar productos más cotizados';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadCotizacionesPorRango() {
    this.loading = true;
    this.reportesService.getCotizacionesPorRangoMonto(this.fechaInicio, this.fechaFin).subscribe({
      next: (response) => {
        if (response.success) {
          this.cotizacionesPorRango = response.data;
          // Esperar a que Angular actualice el DOM antes de renderizar la gráfica
          setTimeout(() => this.renderRangosChart(), 100);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar cotizaciones por rango';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadProximasVencer() {
    this.loading = true;
    this.reportesService.getCotizacionesProximasVencer(30, this.proximasVencerPage, this.proximasVencerPageSize).subscribe({
      next: (response) => {
        if (response.success) {
          this.proximasVencer = response.data;
          // Extraer el total de la paginación del response
          if (response.pagination) {
            this.proximasVencerTotal = response.pagination.totalRecords;
          }
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar cotizaciones próximas a vencer';
        this.loading = false;
        console.error(err);
      }
    });
  }

  get totalProximasVencerPages(): number {
    return Math.ceil(this.proximasVencerTotal / this.proximasVencerPageSize);
  }

  nextProximasVencerPage() {
    if (this.proximasVencerPage < this.totalProximasVencerPages) {
      this.proximasVencerPage++;
      this.loadProximasVencer();
    }
  }

  previousProximasVencerPage() {
    if (this.proximasVencerPage > 1) {
      this.proximasVencerPage--;
      this.loadProximasVencer();
    }
  }

  applyFilters() {
    this.error = null;
    this.proximasVencerPage = 1;
    this.loadDataForTab(this.activeTab);
  }

  clearFilters() {
    // Limpiar fechas (establecer a null/empty permite ver todos los datos históricos)
    this.fechaInicio = '';
    this.fechaFin = '';
    this.error = null;
    this.proximasVencerPage = 1;
    // Recargar datos sin filtros
    this.loadDataForTab(this.activeTab);
  }

  // ==========================================
  // CHARTS RENDERING
  // ==========================================

  renderGeneralCharts() {
    if (!this.estadisticasGenerales) return;

    this.destroyChart('generalStatus');
    
    const ctx = document.getElementById('generalStatusChart') as HTMLCanvasElement;
    if (ctx) {
      this.charts['generalStatus'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['Activas', 'Canceladas'],
          datasets: [{
            data: [
              this.estadisticasGenerales.cotizacionesActivas,
              this.estadisticasGenerales.cotizacionesCanceladas
            ],
            backgroundColor: ['#10B981', '#EF4444'],
            hoverOffset: 4
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { position: 'bottom' },
            title: { display: true, text: 'Estado de Cotizaciones' }
          }
        }
      });
    }
  }

  renderTopClientesChart() {
    if (!this.topClientes.length) return;

    this.destroyChart('topClientes');

    const ctx = document.getElementById('topClientesChart') as HTMLCanvasElement;
    if (ctx) {
      this.charts['topClientes'] = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.topClientes.map(c => (c.razonSocial || 'Cliente Desconocido').substring(0, 20) + '...'),
          datasets: [{
            label: 'Monto Total ($)',
            data: this.topClientes.map(c => c.montoTotal),
            backgroundColor: '#3B82F6',
            borderColor: '#2563EB',
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          indexAxis: 'y',
          plugins: {
            legend: { display: false },
            title: { display: true, text: 'Top 10 Clientes por Monto' }
          }
        }
      });
    }
  }

  renderRendimientoAgentesChart() {
    if (!this.rendimientoAgentes.length) return;

    this.destroyChart('rendimientoAgentes');

    const ctx = document.getElementById('rendimientoAgentesChart') as HTMLCanvasElement;
    if (ctx) {
      this.charts['rendimientoAgentes'] = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.rendimientoAgentes.map(a => a.nombreAgente),
          datasets: [
            {
              label: 'Cotizaciones Ganadas',
              data: this.rendimientoAgentes.map(a => a.cotizacionesActivas || 0), // Asumiendo activas como ganadas/en proceso
              backgroundColor: '#10B981'
            },
            {
              label: 'Cotizaciones Canceladas',
              data: this.rendimientoAgentes.map(a => a.cotizacionesCanceladas || 0),
              backgroundColor: '#EF4444'
            }
          ]
        },
        options: {
          responsive: true,
          scales: {
            x: { stacked: true },
            y: { stacked: true }
          },
          plugins: {
            title: { display: true, text: 'Rendimiento por Agente' }
          }
        }
      });
    }
  }

  renderProductosChart() {
    if (!this.productosMasCotizados.length) return;

    this.destroyChart('productos');

    const ctx = document.getElementById('productosChart') as HTMLCanvasElement;
    if (ctx) {
      this.charts['productos'] = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.productosMasCotizados.map(p => (p.nombreProducto || 'Producto Desconocido').substring(0, 15) + '...'),
          datasets: [{
            label: 'Frecuencia de Cotización',
            data: this.productosMasCotizados.map(p => p.totalCotizaciones),
            backgroundColor: '#8B5CF6',
            borderColor: '#7C3AED',
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { display: false },
            title: { display: true, text: 'Top 10 Productos Más Cotizados' }
          }
        }
      });
    }
  }

  renderRangosChart() {
    if (!this.cotizacionesPorRango.length) return;

    this.destroyChart('rangos');

    const ctx = document.getElementById('rangosChart') as HTMLCanvasElement;
    if (ctx) {
      this.charts['rangos'] = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: this.cotizacionesPorRango.map(r => r.rangoMonto),
          datasets: [{
            data: this.cotizacionesPorRango.map(r => r.totalCotizaciones),
            backgroundColor: [
              '#60A5FA', '#34D399', '#FBBF24', '#F87171', '#A78BFA', '#F472B6'
            ]
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { position: 'right' },
            title: { display: true, text: 'Distribución por Rango de Monto' }
          }
        }
      });
    }
  }
}
