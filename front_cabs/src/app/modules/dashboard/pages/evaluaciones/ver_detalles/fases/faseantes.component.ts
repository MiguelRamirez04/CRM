import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { EvaluacionService, DtoEvaDetallesResponse, EvaluacionFotoResponseDto } from '../../services/evaluaciones-fases.service';

interface FotoConDetalle extends EvaluacionFotoResponseDto {
  urlImagen?: string;
}

@Component({
  selector: 'app-faseantes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FaseantesComponent implements OnInit {
  @Input() detalleId?: number; // Puede recibirse desde un componente padre
  @Input() evaluacionId?: number; // O usar evaluacionId para buscar

  datosFase?: DtoEvaDetallesResponse;
  fotos: FotoConDetalle[] = [];
  isLoading = true;
  error?: string;

  constructor(
    private evaluacionService: EvaluacionService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Obtener ID desde los parámetros de la ruta si no se recibe como Input
    if (!this.detalleId && !this.evaluacionId) {
      this.route.params.subscribe(params => {
        if (params['detalleId']) {
          this.detalleId = +params['detalleId'];
          this.cargarDetalle();
        } else if (params['evaluacionId']) {
          this.evaluacionId = +params['evaluacionId'];
          this.cargarDetallesPorEvaluacion();
        }
      });
    } else {
      if (this.detalleId) {
        this.cargarDetalle();
      } else if (this.evaluacionId) {
        this.cargarDetallesPorEvaluacion();
      }
    }
  }

  /**
   * Carga un detalle específico por su ID
   */
  private cargarDetalle(): void {
    if (!this.detalleId) return;

    this.isLoading = true;
    this.evaluacionService.getDetalleById(this.detalleId).subscribe({
      next: (detalle) => {
        this.datosFase = detalle;
        this.cargarFotos();
      },
      error: (err) => {
        console.error('Error al cargar detalle:', err);
        this.error = 'Error al cargar los datos de la fase';
        this.isLoading = false;
      }
    });
  }

  /**
   * Carga los detalles de una evaluación y filtra por fase "ANTES"
   */
  private cargarDetallesPorEvaluacion(): void {
    if (!this.evaluacionId) return;

    this.isLoading = true;
    this.evaluacionService.getDetallesByEvaluacionId(this.evaluacionId).subscribe({
      next: (detalles) => {
        // Filtrar solo la fase "ANTES"
        const detalleAntes = detalles.find(d => d.fase.toUpperCase() === 'ANTES');
        
        if (detalleAntes) {
          this.datosFase = detalleAntes;
          this.detalleId = detalleAntes.id;
          this.cargarFotos();
        } else {
          this.error = 'No se encontró la fase ANTES para esta evaluación';
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error('Error al cargar detalles:', err);
        this.error = 'Error al cargar los datos de la evaluación';
        this.isLoading = false;
      }
    });
  }

  /**
   * Carga las fotos asociadas al detalle
   */
  private cargarFotos(): void {
    if (!this.detalleId) {
      this.isLoading = false;
      return;
    }

    // Obtener todas las fotos y filtrar por detalleId
    this.evaluacionService.getAllFotos().subscribe({
      next: (todasLasFotos) => {
        this.fotos = todasLasFotos
          .filter(foto => foto.detalleId === this.detalleId)
          .map(foto => ({
            ...foto,
            urlImagen: foto.urlDescarga || this.evaluacionService.getFotoDownloadUrl(foto.id)
          }));
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar fotos:', err);
        this.error = 'Error al cargar las fotos';
        this.isLoading = false;
      }
    });
  }

  /**
   * Calcula el ancho de la barra de progreso del score
   */
  get scoreWidth(): string {
    return this.datosFase?.scoreFase ? `${this.datosFase.scoreFase}%` : '0%';
  }

  /**
   * Formatea una fecha para mostrarla
   */
  formatearFecha(fecha: Date | string): string {
    const date = typeof fecha === 'string' ? new Date(fecha) : fecha;
    return date.toLocaleDateString('es-MX', { 
      year: 'numeric', 
      month: '2-digit', 
      day: '2-digit' 
    });
  }
}