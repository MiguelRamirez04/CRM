import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MonedaLegacyService } from '../../../../core/services/moneda-legacy.service';
import { MonedaLegacyResponse, MonedaLegacyPaginatedResponse } from '../../../../core/models/moneda-legacy.interface';

@Component({
  selector: 'app-monedas',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './monedas.component.html',
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
export class MonedasComponent implements OnInit {
  private monedaService = inject(MonedaLegacyService);

  // Señales para estado
  monedas = signal<MonedaLegacyResponse[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // Paginación
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  totalCount = signal<number>(0);
  hasPrevious = signal<boolean>(false);
  hasNext = signal<boolean>(false);

  ngOnInit(): void {
    this.cargarMonedas();
  }

  cargarMonedas(): void {
    this.loading.set(true);
    this.error.set(null);

    this.monedaService.getPaginated(this.currentPage(), 20).subscribe({
      next: (response) => {
        this.monedas.set(response.data);
        this.currentPage.set(response.pagination.currentPage);
        this.totalPages.set(response.pagination.totalPages);
        this.totalCount.set(response.pagination.totalCount);
        this.hasPrevious.set(response.pagination.hasPrevious);
        this.hasNext.set(response.pagination.hasNext);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar monedas:', err);
        this.error.set(err.message || 'Error al cargar monedas');
        this.loading.set(false);
      }
    });
  }

  cambiarPagina(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.cargarMonedas();
    }
  }

  formatDate(dateString: string): string {
    try {
      return new Date(dateString).toLocaleDateString('es-ES', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateString;
    }
  }
}