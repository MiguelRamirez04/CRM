import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductoLegacyService } from '../../../../core/services/producto-legacy.service';
import { ProductoLegacyResponse, ProductoLegacyPaginado } from '../../../../core/models/producto-legacy.interface';
import { DialogProductosComponent } from './dialog-productos/dialog-productos.component';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, DialogProductosComponent],
  templateUrl: './productos.component.html',
  styleUrls: ['./productos.component.css']
})
export class ProductosComponent implements OnInit {
  private productoService = inject(ProductoLegacyService);
  private searchSubject = new Subject<string>();

  // Signals
  productos = signal<ProductoLegacyResponse[]>([]);
  loading = signal<boolean>(false);
  pagination = signal<ProductoLegacyPaginado['pagination'] | null>(null);
  selectedProductId = signal<number | null>(null);
  
  // Filtros
  searchTerm: string = '';
  statusFilter: string = 'null'; // String para select, se convierte a number | null
  Math = Math; // Para usar en template

  constructor() {
    // Configurar búsqueda en tiempo real con debounce de 500ms
    this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      console.log('🔍 Búsqueda automática activada:', searchTerm);
      this.cargarProductos(1);
    });
  }

  ngOnInit(): void {
    this.cargarProductos();
  }

  onSearchChange(): void {
    const trimmedSearch = this.searchTerm.trim();
    
    // Solo buscar si hay 3 o más caracteres, o si está vacío (para mostrar todos)
    if (trimmedSearch.length >= 3 || trimmedSearch.length === 0) {
      this.searchSubject.next(trimmedSearch);
    }
  }

  buscar(): void {
    this.cargarProductos(1);
  }

  cambiarPagina(page: number): void {
    this.cargarProductos(page);
  }

  cargarProductos(page: number = 1): void {
    this.loading.set(true);
    
    // Convertir statusFilter de string a number | null
    let statusValue: number | null = null;
    if (this.statusFilter === '1') {
      statusValue = 1;
    } else if (this.statusFilter === '0') {
      statusValue = 0;
    }
    
    const filtros = {
      page,
      pageSize: 50,
      nombreProducto: this.searchTerm.trim() || undefined,
      status: statusValue
    };

    console.log('🔍 Cargando productos con filtros:', filtros);

    this.productoService.buscarPaginado(filtros).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.productos.set(response.data.data);
          this.pagination.set(response.data.pagination);
          console.log('✅ Productos cargados:', response.data.data.length);
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar productos:', err);
        this.loading.set(false);
      }
    });
  }

  cambiarFiltroEstado(): void {
    console.log('📊 Filtro de estado cambiado a:', this.statusFilter);
    this.cargarProductos(1);
  }

  verDetalles(productoId: number): void {
    this.selectedProductId.set(productoId);
  }

  cerrarDialog(): void {
    this.selectedProductId.set(null);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }
}