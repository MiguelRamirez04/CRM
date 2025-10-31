import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { EvaluacionService, DtoEvaDetallesResponse, EvaluacionFotoResponseDto } from '../../services/evaluaciones-fases.service';

interface FotoConDetalle extends EvaluacionFotoResponseDto {
  urlImagen?: string;
}

@Component({
  selector: 'app-fasedespues',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FasedespuesComponent implements OnInit {
  @Input() detalleId?: number;
  @Input() evaluacionId?: number;

  datosFase?: DtoEvaDetallesResponse;
  fotos: FotoConDetalle[] = [];
  isLoading = true;
  error?: string;

  constructor(
    private evaluacionService: EvaluacionService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
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

  private cargarDetallesPorEvaluacion(): void {
    if (!this.evaluacionId) return;

    this.isLoading = true;
    this.evaluacionService.getDetallesByEvaluacionId(this.evaluacionId).subscribe({
      next: (detalles) => {
        // Filtrar solo la fase "DESPUES"
        const detalleDespues = detalles.find(d => d.fase.toUpperCase() === 'DESPUES');
        
        if (detalleDespues) {
          this.datosFase = detalleDespues;
          this.detalleId = detalleDespues.id;
          this.cargarFotos();
        } else {
          this.error = 'No se encontró la fase DESPUÉS para esta evaluación';
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

  private cargarFotos(): void {
    if (!this.detalleId) {
      this.isLoading = false;
      return;
    }

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

  get scoreWidth(): string {
    return this.datosFase?.scoreFase ? `${this.datosFase.scoreFase}%` : '0%';
  }

  formatearFecha(fecha: Date | string): string {
    const date = typeof fecha === 'string' ? new Date(fecha) : fecha;
    return date.toLocaleDateString('es-MX', { 
      year: 'numeric', 
      month: '2-digit', 
      day: '2-digit' 
    });
  }
}