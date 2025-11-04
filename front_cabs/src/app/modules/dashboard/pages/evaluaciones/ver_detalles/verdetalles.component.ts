import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { EvaluacionService, Evaluacion, Fase } from '../services/evaluaciones-verdetalles.service';

@Component({
  selector: 'app-verdetalles',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './verdetalles.component.html',
  styleUrls: ['./verdetalles.component.css'],
  providers: [EvaluacionService]
})
export class VerdetallesComponent implements OnInit {
  evaluacion: Evaluacion | null = null;
  cargando: boolean = true;
  error: string | null = null;

  constructor(
    private evaluacionService: EvaluacionService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Obtener el ID de la evaluación desde la ruta
    // Por ejemplo: /evaluacion/5
    this.route.params.subscribe(params => {
      const evaluacionId = +params['id']; // El '+' convierte string a número
      if (evaluacionId) {
        this.cargarEvaluacion(evaluacionId);
      } else {
        this.error = 'No se proporcionó un ID de evaluación válido';
        this.cargando = false;
      }
    });
  }

  /**
   * Carga los datos de la evaluación desde el backend
   */
  cargarEvaluacion(id: number): void {
    this.cargando = true;
    this.error = null;

    this.evaluacionService.getEvaluacionCompleta(id).subscribe({
      next: (data) => {
        this.evaluacion = data;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar la evaluación:', error);
        this.error = 'No se pudo cargar la evaluación. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  /**
   * Navega a los detalles de una fase específica
   */
verFase(tipoFase: 'antes' | 'despues'): void {
  if (!this.evaluacion) return;

  const fase = this.evaluacion.fases.find(f => f.tipo === tipoFase);

  if (fase && fase.completada && fase.id) {
    if (tipoFase === 'antes') {
      // Ruta absoluta
      this.router.navigate([`/dashboard/evaluacion/fase-antes/${this.evaluacion.id}`]);
    } else if (tipoFase === 'despues') {
      this.router.navigate([`/dashboard/evaluacion/fase-despues/${this.evaluacion.id}`]);
    }
  } else if (fase && !fase.completada) {
    console.log('Esta fase aún no ha sido completada');
  }
}



  /**
   * Getter para calcular el porcentaje del score
   */
  get porcentajeScore(): number {
    return this.evaluacion?.scoreTotal || 0;
  }

  /**
   * Reintentar la carga de datos
   */
  reintentar(): void {
    this.route.params.subscribe(params => {
      const evaluacionId = +params['id'];
      if (evaluacionId) {
        this.cargarEvaluacion(evaluacionId);
      }
    });
  }
}