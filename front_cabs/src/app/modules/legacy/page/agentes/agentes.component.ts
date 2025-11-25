import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgenteLegacyService } from '../../../../core/services/agente-legacy.service';
import { AgenteLegacyResponse, AgenteLegacyPaginatedResponse } from '../../../../core/models/agente-legacy.interface';

@Component({
  selector: 'app-agentes',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './agentes.component.html',
  styles: [`
    .back-button {
      display: inline-block;
      margin-bottom: 1rem;
      color: #3b82f6;
      text-decoration: none;
      font-weight: 500;
    }
    .back-button:hover {
      text-decoration: underline;
    }
    .placeholder-title {
      font-size: 2rem;
      font-weight: bold;
      color: #1f2937;
      margin-bottom: 1rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgentesComponent implements OnInit {
  private agenteService = inject(AgenteLegacyService);

  // Señales para estado
  agentes = signal<AgenteLegacyResponse[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // Búsqueda
  searchQuery = signal<string>('');

  // Paginación
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  totalCount = signal<number>(0);
  hasPrevious = signal<boolean>(false);
  hasNext = signal<boolean>(false);

  ngOnInit(): void {
    this.cargarAgentes();
  }

  cargarAgentes(): void {
    this.loading.set(true);
    this.error.set(null);

    this.agenteService.getPaginated(this.currentPage(), 20).subscribe({
      next: (response) => {
        this.agentes.set(response.data);
        this.currentPage.set(response.pagination.currentPage);
        this.totalPages.set(response.pagination.totalPages);
        this.totalCount.set(response.pagination.totalCount);
        this.hasPrevious.set(response.pagination.hasPrevious);
        this.hasNext.set(response.pagination.hasNext);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar agentes:', err);
        this.error.set(err.message || 'Error al cargar agentes');
        this.loading.set(false);
      }
    });
  }

  buscarAgentes(): void {
    const query = this.searchQuery().trim();
    if (!query) {
      this.cargarAgentes();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.agenteService.searchPaginated(query, this.currentPage(), 20).subscribe({
      next: (response) => {
        this.agentes.set(response.data);
        this.currentPage.set(response.pagination.currentPage);
        this.totalPages.set(response.pagination.totalPages);
        this.totalCount.set(response.pagination.totalCount);
        this.hasPrevious.set(response.pagination.hasPrevious);
        this.hasNext.set(response.pagination.hasNext);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al buscar agentes:', err);
        this.error.set(err.message || 'Error al buscar agentes');
        this.loading.set(false);
      }
    });
  }

  limpiarBusqueda(): void {
    this.searchQuery.set('');
    this.currentPage.set(1);
    this.cargarAgentes();
  }

  cambiarPagina(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      if (this.searchQuery().trim()) {
        this.buscarAgentes();
      } else {
        this.cargarAgentes();
      }
    }
  }

  getTipoAgenteLabel(tipo: number): string {
    switch (tipo) {
      case 1: return 'Venta';
      case 2: return 'Cobro';
      default: return 'Otro';
    }
  }

  formatDate(dateString: string): string {
    try {
      return new Date(dateString).toLocaleDateString('es-ES', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    } catch {
      return dateString;
    }
  }
}