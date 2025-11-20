import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CotizacionLegacyService } from '../../../../../core/services/cotizacion-legacy.service';
import { ClienteLegacyService } from '../../../../../core/services/cliente-legacy.service';
import { ProductoLegacyService } from '../../../../../core/services/producto-legacy.service';
import {
  CotizacionLegacyCreateRequest
} from '../../../../../core/models/cotizacion-legacy.interface';
import { ClienteLegacyBusqueda } from '../../../../../core/models/cliente-legacy.interface';
import { ProductoLegacyBusqueda } from '../../../../../core/models/producto-legacy.interface';

interface ProductoFormulario {
  idProducto: number;
  nombreProducto: string;
  codigoProducto: string;
  unidades: number;
  precio: number;
  descuentoImporte: number; // Descuento en valores absolutos
  subtotal: number; // Calculado: (unidades * precio) - descuento
}

@Component({
  selector: 'app-documento-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documento-create.component.html',
  styleUrl: './documento-create.component.css'
})
export class DocumentoCreateComponent implements OnInit {
  // Signals
  loading = signal<boolean>(false);
  clientes = signal<ClienteLegacyBusqueda[]>([]);
  productos = signal<ProductoLegacyBusqueda[]>([]);
  productosSeleccionados = signal<ProductoFormulario[]>([]);

  // Búsqueda
  busquedaCliente = '';
  busquedaProducto = '';
  clienteSeleccionado: ClienteLegacyBusqueda | null = null;
  
  // Producto temporal
  productoTemporal: ProductoFormulario = {
    idProducto: 0,
    nombreProducto: '',
    codigoProducto: '',
    unidades: 1,
    precio: 0,
    descuentoImporte: 0,
    subtotal: 0
  };

  // Formulario principal
  formulario = {
    idCliente: 0,
    aplicarIVA: false,
    porcentajeIVA: 16.0,
    cTotal: 0, // CTOTAL ingresado manualmente por el usuario
    descuentoDoc1: 0, // Descuento nivel documento 1 (%)
    descuentoDoc2: 0, // Descuento nivel documento 2 (%)
    descuentoDoc3: 0, // Descuento nivel documento 3 ($)
    observaciones: '',
    referencia: ''
  };

  constructor(
    private cotizacionService: CotizacionLegacyService,
    private clienteService: ClienteLegacyService,
    private productoService: ProductoLegacyService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Inicialización
  }

  // ==================== BÚSQUEDA CLIENTES ====================
  
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
    this.formulario.idCliente = cliente.idCliente;
    this.busquedaCliente = `${cliente.razonSocial} (${cliente.rfc})`;
    this.clientes.set([]);
  }

  // ==================== BÚSQUEDA PRODUCTOS ====================
  
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
    this.productoTemporal = {
      idProducto: producto.idProducto,
      nombreProducto: producto.nombreProducto,
      codigoProducto: producto.codigoProducto,
      unidades: 1,
      precio: producto.precio,
      descuentoImporte: 0,
      subtotal: producto.precio
    };
    this.busquedaProducto = `${producto.codigoProducto} - ${producto.nombreProducto}`;
    this.productos.set([]);
    this.calcularSubtotalProducto();
  }

  // ==================== MANEJO DE PRODUCTOS ====================
  
  calcularSubtotalProducto(): void {
    const neto = this.productoTemporal.unidades * this.productoTemporal.precio;
    this.productoTemporal.subtotal = neto - this.productoTemporal.descuentoImporte;
  }

  agregarProducto(): void {
    if (this.productoTemporal.idProducto === 0) {
      alert('Debe seleccionar un producto');
      return;
    }
    
    if (this.productoTemporal.unidades <= 0 || this.productoTemporal.precio <= 0) {
      alert('La cantidad y precio deben ser mayores a 0');
      return;
    }

    // Agregar a la lista
    this.productosSeleccionados.update(productos => [
      ...productos,
      { ...this.productoTemporal }
    ]);

    // Resetear formulario de producto
    this.productoTemporal = {
      idProducto: 0,
      nombreProducto: '',
      codigoProducto: '',
      unidades: 1,
      precio: 0,
      descuentoImporte: 0,
      subtotal: 0
    };
    this.busquedaProducto = '';
  }

  eliminarProducto(index: number): void {
    this.productosSeleccionados.update(productos => 
      productos.filter((_, i) => i !== index)
    );
  }

  // ==================== CÁLCULOS ====================
  
  calcularSubtotalGeneral(): number {
    return this.productosSeleccionados().reduce((sum, p) => sum + p.subtotal, 0);
  }

  calcularIVA(): number {
    if (!this.formulario.aplicarIVA) return 0;
    return this.formulario.cTotal * (this.formulario.porcentajeIVA / 100);
  }

  calcularTotalConIVA(): number {
    return this.formulario.cTotal + this.calcularIVA();
  }

  // ==================== CREAR COTIZACIÓN ====================
  
  crearCotizacion(): void {
    // Validaciones
    if (this.formulario.idCliente === 0) {
      alert('❌ Debe seleccionar un cliente');
      return;
    }
    
    if (this.productosSeleccionados().length === 0) {
      alert('❌ Debe agregar al menos un producto');
      return;
    }

    if (this.formulario.cTotal <= 0) {
      alert('❌ Debe ingresar el CTOTAL (monto total de la cotización)');
      return;
    }

    this.loading.set(true);

    // Preparar request
    const request: CotizacionLegacyCreateRequest = {
      idCliente: this.formulario.idCliente,
      aplicarIVA: this.formulario.aplicarIVA,
      porcentajeIVA: this.formulario.porcentajeIVA,
      cTotal: this.formulario.cTotal, // ✅ CTOTAL manual obligatorio
      descuentoDoc1: this.formulario.descuentoDoc1 > 0 ? this.formulario.descuentoDoc1 : null,
      descuentoDoc2: this.formulario.descuentoDoc2 > 0 ? this.formulario.descuentoDoc2 : null,
      descuentoDoc3: this.formulario.descuentoDoc3 > 0 ? this.formulario.descuentoDoc3 : null,
      observaciones: this.formulario.observaciones || null,
      referencia: this.formulario.referencia || null,
      productos: this.productosSeleccionados().map(p => ({
        idProducto: p.idProducto,
        idAlmacen: 0, // ✅ Automático en backend
        unidades: p.unidades, // ✅ Puede ser 0
        precio: p.precio,
        descuentoImporte: p.descuentoImporte // ✅ Descuento por importe fijo
      }))
    };

    this.cotizacionService.crear(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          alert(`✅ Cotización creada exitosamente\nFolio: ${response.data.folio}\nTotal: $${response.data.total.toFixed(2)}`);
          this.router.navigate(['../'], { relativeTo: this.route });
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error creando cotización:', err);
        alert(`❌ Error: ${err.error?.message || err.message || 'No se pudo crear la cotización'}`);
        this.loading.set(false);
      }
    });
  }

  // ==================== UTILIDADES ====================
  
  cancelar(): void {
    this.router.navigate(['../'], { relativeTo: this.route });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }
}
