import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClienteLegacyService } from '../../../../../core/services/cliente-legacy.service';
import { ClienteLegacyResponse } from '../../../../../core/models/cliente-legacy.interface';

@Component({
  selector: 'app-dialog-clientes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dialog-clientes.component.html',
  styleUrl: './dialog-clientes.component.css'
})
export class DialogClientesComponent implements OnChanges {
  @Input() clienteId: number | null = null;
  @Output() cerrar = new EventEmitter<void>();

  private clienteService = inject(ClienteLegacyService);

  cliente = signal<ClienteLegacyResponse | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['clienteId'] && this.clienteId) {
      this.cargarCliente();
    } else if (changes['clienteId'] && !this.clienteId) {
      this.cliente.set(null);
      this.error.set(null);
    }
  }

  cargarCliente(): void {
    if (!this.clienteId) return;

    this.loading.set(true);
    this.error.set(null);

    // Incluir detalles de ubicación completos
    this.clienteService.obtenerPorId(this.clienteId, true).subscribe({
      next: (cliente) => {
        this.cliente.set(cliente);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar cliente:', err);
        this.error.set('No se pudo cargar el cliente');
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
