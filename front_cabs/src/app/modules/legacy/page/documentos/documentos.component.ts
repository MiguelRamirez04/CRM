import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CotizacionLegacyService } from '../../../../core/services/cotizacion-legacy.service';
import { ClienteLegacyService } from '../../../../core/services/cliente-legacy.service';
import { ProductoLegacyService } from '../../../../core/services/producto-legacy.service';
import { AlmacenLegacyService } from '../../../../core/services/almacen-legacy.service';
import {
  CotizacionLegacyResponse,
  CotizacionLegacyFiltros,
  CotizacionLegacyCreateRequest,
  CotizacionMovimientoLegacyDto
} from '../../../../core/models/cotizacion-legacy.interface';
import { ClienteLegacyBusqueda } from '../../../../core/models/cliente-legacy.interface';
import { ProductoLegacyBusqueda } from '../../../../core/models/producto-legacy.interface';
import { AlmacenLegacySimple } from '../../../../core/models/almacen-legacy.interface';

interface ProductoSeleccionado extends CotizacionMovimientoLegacyDto {
  nombreProducto?: string;
  nombreAlmacen?: string;
}

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documentos.component.html',
  styleUrl: './documentos.component.css'
})
export class DocumentosComponent implements OnInit {
  // Exponer Math para el template
  Math = Math;
  
  // Signals para estado reactivo
  cotizaciones = signal<CotizacionLegacyResponse[]>([]);
  clientes = signal<ClienteLegacyBusqueda[]>([]);
  productos = signal<ProductoLegacyBusqueda[]>([]);
  almacenes = signal<AlmacenLegacySimple[]>([]);
  
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  
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
  showCreateModal = signal<boolean>(false);
  showDetailModal = signal<boolean>(false);
  selectedCotizacion = signal<CotizacionLegacyResponse | null>(null);
  
  // Formulario de creación
  nuevaCotizacion: CotizacionLegacyCreateRequest = {
    idCliente: 0,
    productos: [],
    aplicarIVA: true,
    porcentajeIVA: 16.0
  };
  
  productosSeleccionados = signal<ProductoSeleccionado[]>([]);
  
  // Búsqueda de clientes/productos
  busquedaCliente = '';
  busquedaProducto = '';
  clienteSeleccionado: ClienteLegacyBusqueda | null = null;
  
  // Producto temporal para agregar
  productoTemporal: ProductoSeleccionado = {
    idProducto: 0,
    idAlmacen: 0,
    unidades: 1,
    precio: 0,
    porcentajeDescuento: 0
  };

  constructor(
    private cotizacionService: CotizacionLegacyService,
    private clienteService: ClienteLegacyService,
    private productoService: ProductoLegacyService,
    private almacenService: AlmacenLegacyService
  ) {}

  ngOnInit(): void {
    this.cargarCotizaciones();
    this.cargarAlmacenes();
  }

  // ==================== CARGA DE DATOS ====================
  
  cargarCotizaciones(): void {
    this.loading.set(true);
    this.error.set(null);
    
    this.filtros.page = this.currentPage();
    this.filtros.pageSize = this.pageSize();
    
    this.cotizacionService.buscar(this.filtros).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.cotizaciones.set(response.data.data);
          this.totalRecords.set(response.data.pagination.totalRecords);
          this.loading.set(false);
        }
      },
      error: (err) => {
        this.error.set(err.mensaje || 'Error al cargar cotizaciones');
        this.loading.set(false);
      }
    });
  }

  cargarAlmacenes(): void {
    this.almacenService.obtenerActivos().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.almacenes.set(response.data);
        }
      },
      error: (err) => console.error('Error al cargar almacenes:', err)
    });
  }

  // ==================== BÚSQUEDAS ====================
  
  buscarClientes(): void {
    if (this.busquedaCliente.length < 2) {
      this.clientes.set([]);
      return;
    }
    
    this.clienteService.buscarSimplificado(this.busquedaCliente).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.clientes.set(response.data);
        }
      },
      error: (err) => console.error('Error al buscar clientes:', err)
    });
  }

  seleccionarCliente(cliente: ClienteLegacyBusqueda): void {
    this.clienteSeleccionado = cliente;
    this.nuevaCotizacion.idCliente = cliente.idCliente;
    this.busquedaCliente = cliente.razonSocial;
    this.clientes.set([]);
  }

  buscarProductos(): void {
    if (this.busquedaProducto.length < 2) {
      this.productos.set([]);
      return;
    }
    
    this.productoService.buscarSimplificado(this.busquedaProducto).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.productos.set(response.data);
        }
      },
      error: (err) => console.error('Error al buscar productos:', err)
    });
  }

  seleccionarProducto(producto: ProductoLegacyBusqueda): void {
    this.productoTemporal.idProducto = producto.idProducto;
    this.productoTemporal.nombreProducto = producto.nombreProducto;
    this.productoTemporal.precio = producto.precio;
    this.busquedaProducto = producto.nombreProducto;
    this.productos.set([]);
  }

  // ==================== MANEJO DE PRODUCTOS ====================
  
  agregarProducto(): void {
    if (this.productoTemporal.idProducto === 0 || this.productoTemporal.idAlmacen === 0) {
      alert('Debe seleccionar un producto y un almacén');
      return;
    }
    
    if (this.productoTemporal.unidades <= 0 || this.productoTemporal.precio <= 0) {
      alert('La cantidad y precio deben ser mayores a 0');
      return;
    }
    
    const almacen = this.almacenes().find(a => a.idAlmacen === this.productoTemporal.idAlmacen);
    
    const nuevo: ProductoSeleccionado = {
      ...this.productoTemporal,
      nombreAlmacen: almacen?.nombreAlmacen
    };
    
    this.productosSeleccionados.update(productos => [...productos, nuevo]);
    
    // Reset
    this.productoTemporal = {
      idProducto: 0,
      idAlmacen: 0,
      unidades: 1,
      precio: 0,
      porcentajeDescuento: 0
    };
    this.busquedaProducto = '';
  }

  eliminarProducto(index: number): void {
    this.productosSeleccionados.update(productos => 
      productos.filter((_, i) => i !== index)
    );
  }

  // ==================== TOTALES ====================
  
  calcularTotales(): { subtotal: number; iva: number; total: number } {
    const productos = this.productosSeleccionados();
    let subtotal = 0;
    
    productos.forEach(p => {
      const neto = p.unidades * p.precio;
      const descuento = neto * ((p.porcentajeDescuento || 0) / 100);
      subtotal += (neto - descuento);
    });
    
    // Aplicar descuentos a nivel documento
    if (this.nuevaCotizacion.descuentoDoc1) {
      subtotal -= subtotal * (this.nuevaCotizacion.descuentoDoc1 / 100);
    }
    if (this.nuevaCotizacion.descuentoDoc2) {
      subtotal -= subtotal * (this.nuevaCotizacion.descuentoDoc2 / 100);
    }
    
    const iva = this.nuevaCotizacion.aplicarIVA 
      ? subtotal * ((this.nuevaCotizacion.porcentajeIVA || 16) / 100)
      : 0;
    
    return {
      subtotal,
      iva,
      total: subtotal + iva
    };
  }

  // ==================== CRUD OPERATIONS ====================
  
  abrirModalCrear(): void {
    this.showCreateModal.set(true);
    this.resetFormulario();
  }

  cerrarModalCrear(): void {
    this.showCreateModal.set(false);
    this.resetFormulario();
  }

  resetFormulario(): void {
    this.nuevaCotizacion = {
      idCliente: 0,
      productos: [],
      aplicarIVA: true,
      porcentajeIVA: 16.0
    };
    this.productosSeleccionados.set([]);
    this.clienteSeleccionado = null;
    this.busquedaCliente = '';
    this.busquedaProducto = '';
  }

  crearCotizacion(): void {
    if (this.nuevaCotizacion.idCliente === 0) {
      alert('Debe seleccionar un cliente');
      return;
    }
    
    if (this.productosSeleccionados().length === 0) {
      alert('Debe agregar al menos un producto');
      return;
    }
    
    this.loading.set(true);
    
    // Preparar request
    const request: CotizacionLegacyCreateRequest = {
      ...this.nuevaCotizacion,
      productos: this.productosSeleccionados().map(p => ({
        idProducto: p.idProducto,
        idAlmacen: p.idAlmacen,
        unidades: p.unidades,
        precio: p.precio,
        porcentajeDescuento: p.porcentajeDescuento
      }))
    };
    
    this.cotizacionService.crear(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          alert(`Cotización creada exitosamente. Folio: ${response.data.folio}`);
          this.cerrarModalCrear();
          this.cargarCotizaciones();
        }
        this.loading.set(false);
      },
      error: (err) => {
        alert(err.mensaje || 'Error al crear cotización');
        this.loading.set(false);
      }
    });
  }

  verDetalle(cotizacion: CotizacionLegacyResponse): void {
    this.loading.set(true);
    
    this.cotizacionService.obtenerPorId(cotizacion.idDocumento).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.selectedCotizacion.set(response.data);
          this.showDetailModal.set(true);
        }
        this.loading.set(false);
      },
      error: (err) => {
        alert(err.mensaje || 'Error al cargar detalle');
        this.loading.set(false);
      }
    });
  }

  cerrarModalDetalle(): void {
    this.showDetailModal.set(false);
    this.selectedCotizacion.set(null);
  }

  cancelarCotizacion(id: number): void {
    const motivo = prompt('Ingrese el motivo de cancelación:');
    if (!motivo) return;
    
    this.loading.set(true);
    
    this.cotizacionService.cancelar({ idDocumento: id, motivo }).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Cotización cancelada exitosamente');
          this.cerrarModalDetalle();
          this.cargarCotizaciones();
        }
        this.loading.set(false);
      },
      error: (err) => {
        alert(err.mensaje || 'Error al cancelar cotización');
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