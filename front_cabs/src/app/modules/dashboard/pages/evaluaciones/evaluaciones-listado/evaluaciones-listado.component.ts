import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EvaluacionesService, Evaluacion } from '../../../../../core/services/evaluaciones-listado.service';

@Component({
  selector: 'app-evaluaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluaciones-listado.component.html',
  styleUrls: ['./evaluaciones-listado.component.css']
})
export class EvaluacionesComponent implements OnInit {
  searchTerm: string = '';
  currentPage: number = 1;
  itemsPerPage: number = 9;
  
  evaluacionesOriginales: Evaluacion[] = [];
  evaluacionesFiltradas: Evaluacion[] = [];
  evaluaciones: Evaluacion[] = [];
  
  cargando: boolean = false;
  error: string = '';

  constructor(
    private evaluacionesService: EvaluacionesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarEvaluaciones();
  }

  cargarEvaluaciones(): void {
    this.cargando = true;
    this.error = '';
    
    this.evaluacionesService.obtenerEvaluaciones().subscribe({
      next: (data) => {
        this.evaluacionesOriginales = data;
        this.evaluacionesFiltradas = data;
        this.actualizarPaginacion();
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar evaluaciones:', err);
        this.error = 'Error al cargar las evaluaciones. Por favor, intenta nuevamente.';
        this.cargando = false;
      }
    });
  }

  onSearch(): void {
    if (!this.searchTerm.trim()) {
      this.evaluacionesFiltradas = this.evaluacionesOriginales;
    } else {
      const termino = this.searchTerm.toLowerCase().trim();
      this.evaluacionesFiltradas = this.evaluacionesOriginales.filter(ev => 
        ev.id.toString().includes(termino) ||
        ev.evaluadorId.toString().includes(termino) ||
        ev.objetivo?.toLowerCase().includes(termino)
      );
    }
    this.currentPage = 1;
    this.actualizarPaginacion();
  }

  onFilter(): void {
    console.log('Abriendo filtros');
    // Implementar lógica de filtro avanzado
  }

  onNuevaEvaluacion(): void {
    console.log('Nueva evaluación');
    // Navegar al formulario de nueva evaluación
    this.router.navigate(['/dashboard/evaluaciones/registro']);
  }

  // ✅ ACTUALIZADO: Navegación correcta según tus rutas del dashboard
  onVerDetalles(evaluacion: Evaluacion): void {
    console.log('Ver detalles de evaluación:', evaluacion.id);
    // Navega a la ruta: /dashboard/evaluacion/:id
    this.router.navigate(['/dashboard/evaluacion', evaluacion.id]);
  }


  onEditar(evaluacion: Evaluacion): void {
    console.log('Editar evaluación:', evaluacion.id);
    // Navegar al formulario de edición
    this.router.navigate(['/dashboard/evaluaciones/editar', evaluacion.id]);
  }

  actualizarPaginacion(): void {
    const inicio = (this.currentPage - 1) * this.itemsPerPage;
    const fin = inicio + this.itemsPerPage;
    this.evaluaciones = this.evaluacionesFiltradas.slice(inicio, fin);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.actualizarPaginacion();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.actualizarPaginacion();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.actualizarPaginacion();
    }
  }

  getVisiblePages(): (number | string)[] {
    const pages: (number | string)[] = [];
    
    if (this.totalPages <= 6) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        pages.push(1, 2, 3, 4, '...', this.totalPages);
      } else if (this.currentPage >= this.totalPages - 2) {
        pages.push(1, '...', this.totalPages - 3, this.totalPages - 2, this.totalPages - 1, this.totalPages);
      } else {
        pages.push(1, '...', this.currentPage - 1, this.currentPage, this.currentPage + 1, '...', this.totalPages);
      }
    }
    
    return pages;
  }

  formatearFecha(fecha: string): string {
    const date = new Date(fecha);
    return date.toLocaleDateString('es-MX', { 
      year: 'numeric', 
      month: '2-digit', 
      day: '2-digit' 
    });
  }

  // Getters calculados
  get totalPages(): number {
    return Math.ceil(this.evaluacionesFiltradas.length / this.itemsPerPage);
  }

  get totalItems(): number {
    return this.evaluacionesFiltradas.length;
  }

  get startItem(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.itemsPerPage + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.itemsPerPage, this.totalItems);
  }
}