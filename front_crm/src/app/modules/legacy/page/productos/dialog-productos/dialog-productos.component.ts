import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductoLegacyService } from '../../../../../core/services/producto-legacy.service';
import { ProductoLegacyResponse } from '../../../../../core/models/producto-legacy.interface';

@Component({
  selector: 'app-dialog-productos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dialog-productos.component.html',
  styleUrl: './dialog-productos.component.css'
})
export class DialogProductosComponent implements OnChanges {
  @Input() productoId: number | null = null;
  @Output() cerrar = new EventEmitter<void>();

  private productoService = inject(ProductoLegacyService);

  producto = signal<ProductoLegacyResponse | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['productoId'] && this.productoId) {
      this.cargarProducto();
    } else if (changes['productoId'] && !this.productoId) {
      this.producto.set(null);
      this.error.set(null);
    }
  }

  cargarProducto(): void {
    if (!this.productoId) return;

    this.loading.set(true);
    this.error.set(null);

    this.productoService.obtenerPorId(this.productoId).subscribe({
      next: (producto) => {
        this.producto.set(producto);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar producto:', err);
        this.error.set('No se pudo cargar el producto');
        this.loading.set(false);
      }
    });
  }

  onCerrar(): void {
    this.cerrar.emit();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }
}
