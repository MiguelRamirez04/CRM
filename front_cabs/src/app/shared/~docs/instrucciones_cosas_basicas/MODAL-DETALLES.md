# Modal de Detalles (Ver Detalles)

## Descripción
Modal lateral para mostrar información detallada de un registro. Se construye combinando componentes existentes del sistema.


---

## Estructura Recomendada

El modal de detalles no es un componente único, sino una **combinación de elementos**:
- Overlay + Panel lateral (similar a modal-filtros)
- Secciones de información
- StatusDot para estados
- Botones de acción

---

## Componentes Necesarios

```typescript
import { CommonModule } from '@angular/common';
import { StatusDotComponent } from 'shared/atoms/status-dot/status-dot.component';
import { UiBotonComponent } from 'shared/atoms/boton/boton.component';
import { LoadingSpinnerComponent } from 'shared/atoms/loading-spinner/loading-spinner.component';
```

---

## HTML del Modal de Detalles

```html
<!-- Modal de Detalles -->
<div *ngIf="mostrarDetalles" class="fixed inset-0 z-50 overflow-hidden">
  
  <!-- Overlay -->
  <div 
    class="absolute inset-0 bg-black/30 transition-opacity" 
    (click)="cerrarDetalles()">
  </div>

  <!-- Panel -->
  <div class="absolute inset-y-0 right-0 flex max-w-full pl-10">
    <div class="w-screen max-w-md m-4 bg-white rounded-2xl shadow-xl flex flex-col overflow-hidden">
      
      <!-- Header -->
      <div class="flex justify-between items-center px-6 py-4 border-b border-gray-200">
        <h2 class="text-xl font-bold text-gray-800">Detalles de Evaluación</h2>
        <button 
          class="p-2 text-gray-500 hover:text-gray-700 rounded-lg hover:bg-gray-100"
          (click)="cerrarDetalles()">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>

      <!-- Contenido scrolleable -->
      <div class="flex-1 overflow-y-auto px-6 py-6">
        
        <!-- Estado de carga -->
        <div *ngIf="cargandoDetalles" class="flex justify-center py-8">
          <app-loading-spinner [mensaje]="'Cargando información detallada...'"></app-loading-spinner>
        </div>

        <!-- Contenido cuando ya cargó -->
        <div *ngIf="!cargandoDetalles && evaluacionSeleccionada">
          
          <!-- Badge de fases -->
          <div class="flex items-center justify-between mb-4">
            <app-status-dot
              [texto]="evaluacionSeleccionada.fasesCompletadas + ' de 2 fases completadas'"
              tipo="completado">
            </app-status-dot>
            <span class="text-sm text-gray-500">ID: #{{ evaluacionSeleccionada.id }}</span>
          </div>

          <!-- Sección: Información General -->
          <div class="seccion-detalle">
            <div class="seccion-header">
              <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
              <span class="seccion-titulo">Información General</span>
            </div>
            
            <!-- Campos de información -->
            <div class="campo-detalle">
              <span class="campo-label">Orden de trabajo:</span>
              <span class="campo-valor">{{ evaluacionSeleccionada.ordenTrabajo }}</span>
            </div>
            
            <div class="campo-detalle">
              <span class="campo-label">Evaluador:</span>
              <span class="campo-valor">{{ evaluacionSeleccionada.evaluador }}</span>
            </div>
            
            <div class="campo-detalle">
              <span class="campo-label">Fecha:</span>
              <span class="campo-valor">{{ evaluacionSeleccionada.fecha | date:'dd/MM/yyyy HH:mm' }}</span>
            </div>
          </div>

          <!-- Sección: Objetivo -->
          <div class="seccion-detalle">
            <div class="seccion-header">
              <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
              <span class="seccion-titulo">Objetivo de la Evaluación</span>
            </div>
            <p class="texto-contenido">{{ evaluacionSeleccionada.objetivo }}</p>
          </div>

          <!-- Sección: Score -->
          <div class="seccion-detalle">
            <div class="seccion-header">
              <svg class="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
              </svg>
              <span class="seccion-titulo">Score de Calidad Total</span>
            </div>
            
            <div class="score-container">
              <span class="score-valor" [ngClass]="getClaseScore(evaluacionSeleccionada.scoreCalidad)">
                {{ evaluacionSeleccionada.scoreCalidad }}
              </span>
              <span class="score-max">/100</span>
              <app-status-dot
                [texto]="getEtiquetaScore(evaluacionSeleccionada.scoreCalidad)"
                [tipo]="getTipoScore(evaluacionSeleccionada.scoreCalidad)">
              </app-status-dot>
            </div>
            
            <!-- Barra de progreso -->
            <div class="barra-progreso">
              <div 
                class="barra-progreso-fill"
                [ngClass]="getClaseBarraScore(evaluacionSeleccionada.scoreCalidad)"
                [style.width.%]="evaluacionSeleccionada.scoreCalidad">
              </div>
            </div>
          </div>

          <!-- Sección: Comentarios -->
          <div class="seccion-detalle">
            <div class="seccion-header">
              <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path>
              </svg>
              <span class="seccion-titulo">Comentarios Generales</span>
            </div>
            <p class="texto-contenido">{{ evaluacionSeleccionada.comentarios || 'Sin comentarios' }}</p>
          </div>

          <!-- Alerta de seguimiento -->
          <div *ngIf="evaluacionSeleccionada.requiereSeguimiento" class="alerta-seguimiento">
            <svg class="w-5 h-5 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
            </svg>
            <span>Requiere seguimiento</span>
          </div>

        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
        <app-ui-boton
          variante="primario"
          texto="Volver al listado"
          [anchoCompleto]="true"
          (alClickear)="cerrarDetalles()">
        </app-ui-boton>
      </div>

    </div>
  </div>
</div>
```

---

## TypeScript del Componente

```typescript
// Propiedades
mostrarDetalles = false;
cargandoDetalles = false;
evaluacionSeleccionada: EvaluacionDetalle | null = null;

// Métodos
verDetalles(evaluacion: Evaluacion): void {
  this.mostrarDetalles = true;
  this.cargandoDetalles = true;
  
  // Cargar detalles completos
  this.evaluacionService.obtenerDetalle(evaluacion.id).subscribe({
    next: (detalle) => {
      this.evaluacionSeleccionada = detalle;
      this.cargandoDetalles = false;
    },
    error: (error) => {
      console.error('Error al cargar detalles:', error);
      this.cargandoDetalles = false;
    }
  });
}

cerrarDetalles(): void {
  this.mostrarDetalles = false;
  this.evaluacionSeleccionada = null;
}

// Helpers para Score
getClaseScore(score: number): string {
  if (score >= 80) return 'score-excelente';
  if (score >= 60) return 'score-bueno';
  return 'score-bajo';
}

getEtiquetaScore(score: number): string {
  if (score >= 80) return 'Excelente';
  if (score >= 60) return 'Regular';
  return 'Deficiente';
}

getTipoScore(score: number): StatusType {
  if (score >= 80) return 'completado';
  if (score >= 60) return 'atencion';
  return 'rechazado';
}

getClaseBarraScore(score: number): string {
  if (score >= 80) return 'bg-green-500';
  if (score >= 60) return 'bg-yellow-500';
  return 'bg-red-500';
}
```

---

## CSS Necesario

```css
/* Secciones */
.seccion-detalle {
  background: #f9fafb;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 16px;
}

.seccion-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.seccion-titulo {
  font-size: 14px;
  font-weight: 600;
  color: #374151;
}

/* Campos */
.campo-detalle {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #e5e7eb;
}

.campo-detalle:last-child {
  border-bottom: none;
}

.campo-label {
  font-size: 13px;
  color: #6b7280;
}

.campo-valor {
  font-size: 13px;
  color: #111827;
  font-weight: 500;
}

.texto-contenido {
  font-size: 14px;
  color: #374151;
  line-height: 1.5;
}

/* Score */
.score-container {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.score-valor {
  font-size: 36px;
  font-weight: 700;
  line-height: 1;
}

.score-valor.score-excelente { color: #22c55e; }
.score-valor.score-bueno { color: #f59e0b; }
.score-valor.score-bajo { color: #ef4444; }

.score-max {
  font-size: 18px;
  color: #9ca3af;
}

/* Barra de progreso */
.barra-progreso {
  height: 8px;
  background: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
}

.barra-progreso-fill {
  height: 100%;
  border-radius: 4px;
  transition: width 0.3s ease;
}

/* Alerta seguimiento */
.alerta-seguimiento {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: #fef3c7;
  border-left: 4px solid #f59e0b;
  border-radius: 8px;
  color: #92400e;
  font-size: 14px;
  font-weight: 500;
}
```

---

## Llamar desde la Tabla

```typescript
// En la configuración de acciones de la tabla
accionesTabla: AccionTabla[] = [
  {
    etiqueta: 'Ver detalles',
    clase: 'boton-ver',
    accion: (item) => this.verDetalles(item)
  }
];
```

---

## Interface de Datos

```typescript
interface EvaluacionDetalle {
  id: number;
  ordenTrabajo: string;
  evaluador: string;
  fecha: Date;
  objetivo: string;
  comentarios: string;
  scoreCalidad: number;
  requiereSeguimiento: boolean;
  fasesCompletadas: number;
  // ... más campos según necesites
}
```