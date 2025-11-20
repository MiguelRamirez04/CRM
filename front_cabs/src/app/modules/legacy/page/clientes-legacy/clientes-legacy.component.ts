import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ClienteLegacyService } from '../../../../core/services/cliente-legacy.service';
import { ClienteLegacyResponse, ClienteLegacyPaginado } from '../../../../core/models/cliente-legacy.interface';
import { DialogClientesComponent } from './dialog-clientes/dialog-clientes.component';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-clientes-legacy',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, DialogClientesComponent],
  templateUrl: './clientes-legacy.component.html',
  styleUrl: './clientes-legacy.component.css'
})
export class ClientesLegacyComponent implements OnInit {
  private clienteService = inject(ClienteLegacyService);
  private searchSubject = new Subject<string>();

  // Signals
  clientes = signal<ClienteLegacyResponse[]>([]);
  loading = signal<boolean>(false);
  pagination = signal<ClienteLegacyPaginado['pagination'] | null>(null);
  selectedClienteId = signal<number | null>(null);
  
  // Filtros
  searchTerm: string = '';
  Math = Math; // Para usar en template

  constructor() {
    // Configurar búsqueda en tiempo real con debounce de 500ms
    this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      console.log('🔍 Búsqueda automática activada:', searchTerm);
      this.cargarClientes(1);
    });
  }

  ngOnInit(): void {
    this.cargarClientes();
  }

  onSearchChange(): void {
    const trimmedSearch = this.searchTerm.trim();
    
    // Solo buscar si hay 3 o más caracteres, o si está vacío (para mostrar todos)
    if (trimmedSearch.length >= 3 || trimmedSearch.length === 0) {
      this.searchSubject.next(trimmedSearch);
    }
  }

  buscar(): void {
    this.cargarClientes(1);
  }

  cambiarPagina(page: number): void {
    this.cargarClientes(page);
  }

  cargarClientes(page: number = 1): void {
    this.loading.set(true);
    
    const filtros = {
      numeroPagina: page,
      tamanoPagina: 50,
      razonSocial: this.searchTerm.trim() || undefined,
      estatus: 1, // Siempre solo activos
      incluirDetalleUbicacion: true
    };

    console.log('🔍 Cargando clientes con filtros:', filtros);

    this.clienteService.buscarPaginado(filtros).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.clientes.set(response.data.data);
          this.pagination.set(response.data.pagination);
          console.log('✅ Clientes cargados:', response.data.data.length);
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar clientes:', err);
        this.loading.set(false);
      }
    });
  }

  verDetalles(clienteId: number): void {
    console.log('👁️ Abriendo detalles del cliente:', clienteId);
    this.selectedClienteId.set(clienteId);
  }

  cerrarDialog(): void {
    console.log('❌ Cerrando dialog');
    this.selectedClienteId.set(null);
  }

  getDireccionCompleta(cliente: ClienteLegacyResponse): { calle: string, colonia?: string, ciudad: string } | null {
    // Usar detalle de ubicación si existe
    if (cliente.ubicacionDetalle) {
      const det = cliente.ubicacionDetalle;
      return {
        calle: `${det.calle} ${det.numeroExterior} ${det.numeroInterior ? 'Int. ' + det.numeroInterior : ''}`.trim(),
        colonia: det.colonia || undefined,
        ciudad: `${det.ciudad || ''}, ${det.estado || ''}`.trim()
      };
    }
    
    // Fallback a la cadena de ubicación simple si no hay detalle
    if (cliente.ubicacion) {
      return {
        calle: cliente.ubicacion,
        colonia: undefined,
        ciudad: cliente.estado || ''
      };
    }
    
    return null;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(value);
  }
}

