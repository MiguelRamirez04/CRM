# Modal de Registro / Edición

## Descripción
Modal completo para crear o editar registros. Incluye navegación por secciones (tabs), formularios con componentes reutilizables y manejo de estados.


---

## Estructura del Modal

El modal de registro se compone de:
1. **Header** - Título + Badge de modo (NUEVA/EDITANDO)
2. **Contenido** - Formulario con tabs/secciones
3. **Footer** - Navegación de secciones + Botones de acción

---

## Componentes Necesarios

```typescript
// Shared Components
import {
  FormInputComponent,
  FormSelectComponent,
  FormTextareaComponent,
  FormToggleComponent,
  LockedFieldComponent,
  FormRowComponent,
  FormSectionComponent,
  FormInfoAlertComponent,
  ScoreDisplayComponent,
  LoadingOverlayComponent
} from 'shared/form-system.index';

import { UiBotonComponent } from 'shared/atoms/boton/boton.component';
```

---

## HTML del Modal de Registro

```html
<!-- Loading Overlay -->
<app-loading-overlay 
  [visible]="cargando" 
  [mensaje]="'Cargando datos...'"
  [fixed]="true">
</app-loading-overlay>

<!-- Modal -->
<div *ngIf="mostrarModal" class="fixed inset-0 z-50 overflow-hidden">
  
  <!-- Overlay -->
  <div class="absolute inset-0 bg-black/40" (click)="intentarCerrar()"></div>

  <!-- Panel del Modal -->
  <div class="absolute inset-4 md:inset-8 lg:inset-12 flex items-center justify-center">
    <div class="w-full max-w-4xl max-h-full bg-white rounded-2xl shadow-2xl flex flex-col overflow-hidden">
      
      <!-- ========== HEADER ========== -->
      <div class="flex justify-between items-center px-6 py-4 border-b border-gray-200 bg-white">
        <div class="flex items-center gap-3">
          <h2 class="text-xl font-bold text-gray-800">
            {{ modoOperacion === 'editar' ? 'Editar Evaluación #' + evaluacionId : 'Nueva Evaluación' }}
            - {{ tituloSeccionActual }}
          </h2>
        </div>
        
        <button 
          class="p-2 text-gray-500 hover:text-gray-700 rounded-lg hover:bg-gray-100"
          (click)="intentarCerrar()">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>

      <!-- ========== CONTENIDO ========== -->
      <div class="flex-1 overflow-y-auto px-6 py-6">
        
        <!-- Vista: Info General -->
        <div *ngIf="vistaActual === 'infoGeneral'">
          
          <!-- Alerta informativa -->
          <app-form-info-alert
            [type]="modoOperacion === 'editar' ? 'warning' : 'info'"
            [message]="mensajeInformativo"
            [showIcon]="true">
          </app-form-info-alert>

          <div style="margin-top: 16px;"></div>

          <!-- Fila: Orden y Ejecución -->
          <app-form-row [columns]="2" [gap]="'medium'">
            
            <!-- Modo Crear: Select -->
            <app-form-select
              *ngIf="modoOperacion === 'crear'"
              [label]="'Orden de trabajo'"
              [required]="true"
              [options]="ordenesOptions"
              [loading]="cargandoCatalogos.ordenes"
              [placeholder]="'Seleccione una orden'"
              [(ngModel)]="formulario.ordenTrabajoId"
              (valueChange)="onOrdenChange()">
            </app-form-select>

            <!-- Modo Editar: Campo bloqueado -->
            <app-locked-field
              *ngIf="modoOperacion === 'editar'"
              [label]="'Orden de trabajo'"
              [required]="true"
              [value]="textoOrdenBloqueada"
              [helpText]="'No se puede modificar'">
            </app-locked-field>

            <!-- Segundo campo... -->
          </app-form-row>

          <!-- Más campos del formulario... -->
          <app-form-input
            [label]="'Objetivo de la evaluación'"
            [placeholder]="'Ej: verificar la calidad del servicio'"
            [(ngModel)]="formulario.objetivo"
            (valueChange)="onFormularioChange()">
          </app-form-input>

          <app-form-textarea
            [label]="'Comentarios generales'"
            [placeholder]="'Observaciones...'"
            [rows]="4"
            [(ngModel)]="formulario.comentarios"
            (valueChange)="onFormularioChange()">
          </app-form-textarea>

          <!-- Fila: Score y Toggle -->
          <app-form-row [columns]="3" [gap]="'medium'" [alignItems]="'center'">
            
            <app-score-display
              [label]="'Score de calidad'"
              [score]="formulario.scoreCalidad"
              [maxScore]="100"
              [readonly]="true">
            </app-score-display>

            <app-form-toggle
              [label]="'¿Requiere seguimiento?'"
              [(ngModel)]="formulario.requiereSeguimiento"
              (toggle)="onFormularioChange()">
            </app-form-toggle>

          </app-form-row>

        </div>

        <!-- Vista: Fase Antes -->
        <div *ngIf="vistaActual === 'faseAntes'">
          <app-fase-antes
            [modoOperacion]="modoOperacion"
            [evaluacionId]="evaluacionId">
          </app-fase-antes>
        </div>

        <!-- Vista: Fase Después -->
        <div *ngIf="vistaActual === 'faseDespues'">
          <app-fase-despues
            [modoOperacion]="modoOperacion"
            [evaluacionId]="evaluacionId">
          </app-fase-despues>
        </div>

      </div>

      <!-- ========== FOOTER ========== -->
      <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
        <div class="flex items-center justify-between">
          
          <!-- Tabs de navegación -->
          <div class="flex gap-2">
            <button 
              class="tab-btn"
              [ngClass]="{'tab-activo': vistaActual === 'infoGeneral'}"
              (click)="navegarA('infoGeneral')">
              <span class="tab-icono">ℹ️</span>
              Info General
              <span *ngIf="modoOperacion === 'editar'" class="tab-badge">EDITANDO</span>
            </button>
            
            <button 
              class="tab-btn"
              [ngClass]="{
                'tab-activo': vistaActual === 'faseAntes',
                'tab-completado': fases.antes.estado === 'completada'
              }"
              (click)="navegarA('faseAntes')">
              <span class="tab-icono">✓</span>
              Fase ANTES
            </button>
            
            <button 
              class="tab-btn"
              [ngClass]="{
                'tab-activo': vistaActual === 'faseDespues',
                'tab-completado': fases.despues.estado === 'completada'
              }"
              (click)="navegarA('faseDespues')">
              <span class="tab-icono">✓</span>
              Fase DESPUÉS
            </button>
          </div>

          <!-- Botones de acción -->
          <div class="flex gap-3">
            <app-ui-boton
              variante="cancelar"
              texto="Cerrar"
              (alClickear)="intentarCerrar()">
            </app-ui-boton>
            
            <app-ui-boton
              variante="primario"
              [texto]="modoOperacion === 'editar' ? 'Guardar Cambios' : 'Guardar Evaluación'"
              (alClickear)="guardar()">
            </app-ui-boton>
          </div>

        </div>
      </div>

    </div>
  </div>
</div>
```

---

## TypeScript del Componente

```typescript
@Component({
  selector: 'app-modal-registro',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
    FormToggleComponent,
    LockedFieldComponent,
    FormRowComponent,
    FormSectionComponent,
    FormInfoAlertComponent,
    ScoreDisplayComponent,
    LoadingOverlayComponent,
    UiBotonComponent
  ],
  templateUrl: './modal-registro.component.html',
  styleUrls: ['./modal-registro.component.css']
})
export class ModalRegistroComponent implements OnInit {
  @Input() modoOperacion: 'crear' | 'editar' = 'crear';
  @Input() evaluacionId: number | null = null;
  @Output() cerrar = new EventEmitter<void>();
  @Output() guardado = new EventEmitter<number>();

  mostrarModal = true;
  cargando = false;
  vistaActual: 'infoGeneral' | 'faseAntes' | 'faseDespues' = 'infoGeneral';

  // Datos del formulario
  formulario = {
    ordenTrabajoId: '',
    ejecucionId: '',
    evaluadorId: '',
    objetivo: '',
    comentarios: '',
    scoreCalidad: 0,
    requiereSeguimiento: false
  };

  // Catálogos
  ordenesOptions: SelectOption[] = [];
  cargandoCatalogos = {
    ordenes: false,
    evaluadores: false
  };

  // Textos para campos bloqueados (modo editar)
  textoOrdenBloqueada = '';

  // Estado de fases
  fases = {
    antes: { estado: 'sin-inicializar' as const },
    despues: { estado: 'sin-inicializar' as const }
  };

  get mensajeInformativo(): string {
    return this.modoOperacion === 'editar'
      ? 'Los campos Orden, Ejecución y Evaluador no se pueden modificar'
      : 'Complete todos los campos requeridos';
  }

  get tituloSeccionActual(): string {
    const titulos = {
      'infoGeneral': 'Información General',
      'faseAntes': 'Evaluación ANTES',
      'faseDespues': 'Evaluación DESPUÉS'
    };
    return titulos[this.vistaActual];
  }

  ngOnInit(): void {
    this.cargarCatalogos();
    
    if (this.modoOperacion === 'editar' && this.evaluacionId) {
      this.cargarDatosExistentes();
    }
  }

  navegarA(seccion: 'infoGeneral' | 'faseAntes' | 'faseDespues'): void {
    this.vistaActual = seccion;
  }

  onOrdenChange(): void {
    // Cargar datos dependientes
  }

  onFormularioChange(): void {
    // Guardar en servicio compartido si es necesario
  }

  intentarCerrar(): void {
    const mensaje = this.modoOperacion === 'editar'
      ? '¿Salir sin guardar cambios?'
      : '¿Cerrar? Los datos no guardados se perderán';

    if (confirm(mensaje)) {
      this.cerrar.emit();
    }
  }

  async guardar(): Promise<void> {
    // Validaciones
    if (!this.formulario.ordenTrabajoId) {
      alert('Seleccione una orden de trabajo');
      return;
    }

    try {
      this.cargando = true;
      // Llamar al servicio para guardar
      // const resultado = await this.servicio.guardar(this.formulario);
      // this.guardado.emit(resultado.id);
      this.cerrar.emit();
    } catch (error) {
      alert('Error al guardar');
    } finally {
      this.cargando = false;
    }
  }

  private cargarCatalogos(): void {
    this.cargandoCatalogos.ordenes = true;
    // Cargar opciones...
  }

  private cargarDatosExistentes(): void {
    this.cargando = true;
    // Cargar datos del servidor...
  }
}
```

---

## CSS para Tabs

```css
.tab-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #6b7280;
  background: white;
  border: 1px solid #e5e7eb;
  cursor: pointer;
  transition: all 0.2s;
}

.tab-btn:hover {
  background: #f9fafb;
  border-color: #d1d5db;
}

.tab-activo {
  color: white;
  background: #34428f;
  border-color: #34428f;
}

.tab-completado {
  color: #059669;
  border-color: #10b981;
}

.tab-completado .tab-icono {
  color: #10b981;
}

.tab-badge {
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 10px;
  background: #fef3c7;
  color: #92400e;
  font-weight: 600;
}

.tab-activo .tab-badge {
  background: rgba(255,255,255,0.2);
  color: white;
}
```

---

## Flujo de Uso

### Abrir modal para CREAR
```typescript
abrirModalCrear(): void {
  this.modoOperacion = 'crear';
  this.evaluacionId = null;
  this.mostrarModal = true;
}
```

### Abrir modal para EDITAR
```typescript
abrirModalEditar(id: number): void {
  this.modoOperacion = 'editar';
  this.evaluacionId = id;
  this.mostrarModal = true;
}
```

### Manejar eventos del modal
```html
<app-modal-registro
  *ngIf="mostrarModal"
  [modoOperacion]="modoOperacion"
  [evaluacionId]="evaluacionId"
  (cerrar)="mostrarModal = false"
  (guardado)="onGuardado($event)">
</app-modal-registro>
```

---

## Componentes de Formulario Disponibles

| Componente | Uso |
|------------|-----|
| `app-form-input` | Inputs de texto, número, email |
| `app-form-select` | Selects con opciones |
| `app-form-textarea` | Áreas de texto |
| `app-form-toggle` | Switches on/off |
| `app-locked-field` | Campos bloqueados (modo editar) |
| `app-form-row` | Filas de formulario (grid) |
| `app-form-section` | Secciones con título |
| `app-form-info-alert` | Alertas informativas |
| `app-score-display` | Mostrar puntuación |

---

## Diferencias Crear vs Editar

| Aspecto | Crear | Editar |
|---------|-------|--------|
| Título | "Nueva Evaluación" | "Editar Evaluación #X" |
| Campos clave | Editables (select) | Bloqueados (locked-field) |
| Alerta | Info (azul) | Warning (amarillo) |
| Badge | "NUEVA" (verde) | "EDITANDO" (azul) |
| Carga inicial | Solo catálogos | Catálogos + datos existentes |