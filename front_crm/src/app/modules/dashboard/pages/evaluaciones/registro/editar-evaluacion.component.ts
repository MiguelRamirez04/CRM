import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { EvaluacionService } from '../../../../../core/services/evaluaciones.service';
import { SharedEvaluacionService } from '../../../../../core/services/shared-evaluacion.service';
import {
  mapResponseToFormulario,
  mapDetalleResponseToDatos
} from '../../../../../core/models/evaluaciones.interface';

/**
 * Componente intermediario que carga la evaluación y redirige al formulario
 * 
 * Flujo:
 * 1. Usuario navega a /evaluaciones/editar/:id
 * 2. Carga datos completos de la BD
 * 3. Guarda en SharedService
 * 4. Redirige a /evaluaciones/registro (mismo formulario)
 * 5. El formulario detecta que hay datos y trabaja en modo EDICIÓN
 */
@Component({
  selector: 'app-editar-evaluacion',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="cargando-container">
      <div *ngIf="cargando" class="spinner-container">
        <div class="spinner"></div>
        <p>Cargando evaluación...</p>
      </div>
      
      <div *ngIf="error" class="error-container">
        <h2>Error al cargar evaluación</h2>
        <p>{{ errorMensaje }}</p>
        <button (click)="volverALista()">Volver al listado</button>
      </div>
    </div>
  `,
  styles: [`
    .cargando-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 400px;
      padding: 40px;
    }

    .spinner-container {
      text-align: center;
    }

    .spinner {
      border: 4px solid #f3f3f3;
      border-top: 4px solid #3498db;
      border-radius: 50%;
      width: 50px;
      height: 50px;
      animation: spin 1s linear infinite;
      margin: 0 auto 20px;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .error-container {
      text-align: center;
      padding: 40px;
    }

    .error-container h2 {
      color: #e74c3c;
      margin-bottom: 15px;
    }

    .error-container button {
      margin-top: 20px;
      padding: 10px 20px;
      background: #3498db;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }

    .error-container button:hover {
      background: #2980b9;
    }
  `]
})
export class EditarEvaluacionComponent implements OnInit {
  cargando = true;
  error = false;
  errorMensaje = '';
  evaluacionId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService
  ) {}

  async ngOnInit() {
    // Obtener ID de la ruta
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (!idParam) {
      this.mostrarError('No se especificó el ID de la evaluación');
      return;
    }

    this.evaluacionId = parseInt(idParam, 10);

    if (isNaN(this.evaluacionId)) {
      this.mostrarError('ID de evaluación inválido');
      return;
    }

    // Cargar datos
    await this.cargarEvaluacion(this.evaluacionId);
  }

  /**
   * Carga la evaluación completa y prepara el servicio compartido
   */
  private async cargarEvaluacion(id: number) {
    try {
      console.log('Cargando evaluación ID:', id);

      // Usar el método del servicio único
      this.evaluacionService.cargarEvaluacionCompleta(id).subscribe({
        next: (data) => {
          console.log('Evaluación cargada:', data);

          // Mapear a formato del formulario
          const infoGeneral = mapResponseToFormulario(data.evaluacion);

          // Preparar datos de fase ANTES
          const datosAntes = data.detalleAntes 
            ? mapDetalleResponseToDatos(data.detalleAntes, data.fotosAntes)
            : undefined;

          // Preparar datos de fase DESPUÉS
          const datosDespues = data.detalleDespues
            ? mapDetalleResponseToDatos(data.detalleDespues, data.fotosDespues)
            : undefined;

          // Guardar en el servicio compartido
          this.sharedService.cargarEvaluacion(
            id,
            infoGeneral,
            datosAntes,
            datosDespues
          );

          console.log('Datos cargados en SharedService');

          // Redirigir al formulario
          this.cargando = false;
          
          // Pequeña pausa visual
          setTimeout(() => {
            this.router.navigate(['/dashboard/evaluaciones/registro']);
          }, 500);
        },
        error: (error) => {
          console.error('Error al cargar evaluación:', error);
          this.mostrarError('No se pudo cargar la evaluación');
        }
      });

    } catch (error) {
      console.error('Error al cargar evaluación:', error);
      this.mostrarError('No se pudo cargar la evaluación');
    }
  }

  /**
   * Muestra un mensaje de error
   */
  private mostrarError(mensaje: string) {
    this.cargando = false;
    this.error = true;
    this.errorMensaje = mensaje;
  }

  /**
   * Vuelve al listado de evaluaciones
   */
  volverALista() {
    this.router.navigate(['/dashboard/evaluaciones']);
  }
}