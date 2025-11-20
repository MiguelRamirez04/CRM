import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CotizacionLegacyService } from '../../../../core/services/cotizacion-legacy.service';
import {
  CotizacionLegacyResponse,
  CotizacionLegacyFiltros
} from '../../../../core/models/cotizacion-legacy.interface';
// import { DialogDocumentosComponent } from './dialog-documentos/dialog-documentos.component'; // Removed
import { DialogVistaDocumentosComponent } from './dialog.vista-documentos/dialog.vista-documentos.component';

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogVistaDocumentosComponent],
  templateUrl: './documentos.component.html',
  styleUrl: './documentos.component.css'
})
export class DocumentosComponent implements OnInit {
  // Exponer Math para el template
  Math = Math;
  
  // Signals para estado reactivo
  cotizaciones = signal<CotizacionLegacyResponse[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  
  // Resumen
  resumenTotal = signal<number>(0);
  
  // Paginación
  currentPage = signal<number>(1);
  pageSize = signal<number>(20);
  totalRecords = signal<number>(0);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));
  
  // Filtros de búsqueda
  filtros: CotizacionLegacyFiltros = {
    page: 1,
    pageSize: 20,
    incluirMovimientos: false
  };
  
  // Modal states
  // showCreateModal = signal<boolean>(false); // Removed
  showDetailModal = signal<boolean>(false);
  idDocumentoSeleccionado = signal<number>(0);

  constructor(
    private cotizacionService: CotizacionLegacyService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.cargarCotizaciones();
    this.cargarResumen();
  }

  // ==================== CARGA DE DATOS ====================
  
  cargarResumen(): void {
    // Calcular rango de fechas (último mes)
    const fechaFin = new Date();
    const fechaInicio = new Date();
    fechaInicio.setMonth(fechaInicio.getMonth() - 1);
    
    const formatDate = (date: Date) => date.toISOString().split('T')[0];
    
    this.cotizacionService.obtenerResumen(
      formatDate(fechaInicio),
      formatDate(fechaFin)
    ).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.resumenTotal.set(response.data.totalDocumentos);
        }
      },
      error: (err) => console.error('Error al cargar resumen:', err)
    });
  }
  
  cargarCotizaciones(): void {
    this.loading.set(true);
    this.error.set(null);
    
    this.filtros.page = this.currentPage();
    this.filtros.pageSize = this.pageSize();
    
    this.cotizacionService.buscar(this.filtros).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cotizaciones.set(response.data.data || []);
          // Fix: Check if pagination exists and data is valid
          if (response.data && response.data.pagination) {
            this.totalRecords.set(response.data.pagination.totalRecords);
          } else {
            this.totalRecords.set(response.data?.data?.length || 0);
          }
          this.loading.set(false);
        }
      },
      error: (err) => {
        this.error.set(err.mensaje || 'Error al cargar cotizaciones');
        this.loading.set(false);
      }
    });
  }

  // ==================== MODAL OPERATIONS ====================
  
  abrirModalCrear(): void {
    this.router.navigate(['crear'], { relativeTo: this.route });
  }

  // cerrarModalCrear(): void { // Removed
  //   this.showCreateModal.set(false);
  // }

  // onCotizacionCreada(): void { // Removed
  //   this.cerrarModalCrear();
  //   this.cargarCotizaciones();
  //   this.cargarResumen();
  // }

  verDetalle(cotizacion: CotizacionLegacyResponse): void {
    this.idDocumentoSeleccionado.set(cotizacion.idDocumento);
    this.showDetailModal.set(true);
  }

  cerrarModalDetalle(): void {
    this.showDetailModal.set(false);
    this.idDocumentoSeleccionado.set(0);
  }

  cancelarCotizacion(idDocumento: number): void {
    const motivo = prompt('Ingrese el motivo de cancelación:');
    if (!motivo) return;
    
    this.loading.set(true);
    
    this.cotizacionService.cancelar({ idDocumento, motivo }).subscribe({
      next: (response) => {
        if (response.success) {
          alert('✅ Cotización cancelada exitosamente');
          this.cerrarModalDetalle();
          this.cargarCotizaciones();
        }
        this.loading.set(false);
      },
      error: (err) => {
        alert(`❌ Error: ${err.mensaje || 'No se pudo cancelar la cotización'}`);
        this.loading.set(false);
      }
    });
  }

  // ==================== PAGINACIÓN ====================
  
  cambiarPagina(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.cargarCotizaciones();
  }

  // ==================== FILTROS ====================
  
  aplicarFiltros(): void {
    this.currentPage.set(1);
    this.cargarCotizaciones();
  }

  limpiarFiltros(): void {
    this.filtros = {
      page: 1,
      pageSize: 20,
      incluirMovimientos: false
    };
    this.currentPage.set(1);
    this.cargarCotizaciones();
  }

  // ==================== UTILIDADES ====================
  
  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-MX');
  }
}