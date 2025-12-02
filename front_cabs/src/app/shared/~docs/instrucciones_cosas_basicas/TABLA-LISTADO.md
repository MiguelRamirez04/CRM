# Tabla de Listado

## Descripción
Componente de tabla reutilizable con soporte para plantillas personalizadas, acciones y estados de carga.



---

## Componentes Necesarios

```typescript
import { 
  TablaListadoComponent, 
  ConfiguracionColumna, 
  AccionTabla 
} from 'shared/components/tabla-listado/tabla-listado.component';

import { StatusDotComponent } from 'shared/atoms/status-dot/status-dot.component';
```

---

## Estructura HTML

```html
<app-tabla-listado
  [datos]="evaluaciones"
  [columnas]="columnasTabla"
  [acciones]="accionesTabla"
  [cargando]="cargandoDatos"
  [mensajeSinDatos]="'No se encontraron evaluaciones'"
  [mostrarIndice]="false"
  (filaClick)="onFilaClick($event)">
</app-tabla-listado>

<!-- PLANTILLAS PERSONALIZADAS (fuera del componente) -->

<!-- Plantilla para Score con badge de color -->
<ng-template #plantillaScore let-item let-dato="dato">
  <span class="score-badge" [ngClass]="getClaseScore(dato)">
    {{ dato }}
  </span>
</ng-template>

<!-- Plantilla para Status con StatusDot -->
<ng-template #plantillaEstatus let-item>
  <app-status-dot
    [texto]="item.requiereSeguimiento ? 'Requiere seguimiento' : 'Completado'"
    [tipo]="item.requiereSeguimiento ? 'atencion' : 'completado'">
  </app-status-dot>
</ng-template>
```

---

## Configuración en el Componente (.ts)

```typescript
import { Component, ViewChild, TemplateRef, AfterViewInit } from '@angular/core';

@Component({...})
export class EvaluacionesComponent implements AfterViewInit {
  
  // Referencias a las plantillas
  @ViewChild('plantillaScore') plantillaScore!: TemplateRef<any>;
  @ViewChild('plantillaEstatus') plantillaEstatus!: TemplateRef<any>;
  
  // Datos
  evaluaciones: Evaluacion[] = [];
  cargandoDatos = false;
  
  // Configuración (se asigna después de que las plantillas estén disponibles)
  columnasTabla: ConfiguracionColumna<Evaluacion>[] = [];
  accionesTabla: AccionTabla<Evaluacion>[] = [];

  ngAfterViewInit(): void {
    // Configurar columnas DESPUÉS de que las plantillas existan
    setTimeout(() => {
      this.configurarTabla();
    });
  }

  private configurarTabla(): void {
    this.columnasTabla = [
      { 
        encabezado: 'ID', 
        campo: 'id',
        ancho: '80px',
        alineacion: 'center'
      },
      { 
        encabezado: 'Objetivo', 
        campo: 'objetivo',
        alineacion: 'left'
      },
      { 
        encabezado: 'Evaluador', 
        campo: 'evaluador',
        alineacion: 'left'
      },
      { 
        encabezado: 'Fecha', 
        campo: 'fecha',
        alineacion: 'center'
      },
      { 
        encabezado: 'Score', 
        campo: 'score',
        plantilla: this.plantillaScore,  // <-- Plantilla personalizada
        alineacion: 'center'
      },
      { 
        encabezado: 'Seguimiento', 
        campo: 'requiereSeguimiento',
        plantilla: this.plantillaEstatus,  // <-- Plantilla personalizada
        alineacion: 'center'
      }
    ];

    this.accionesTabla = [
      {
        etiqueta: 'Ver detalles',
        clase: 'boton-ver',
        accion: (item) => this.verDetalles(item)
      },
      {
        etiqueta: 'Editar',
        clase: 'boton-editar',
        accion: (item) => this.editarEvaluacion(item)
      }
    ];
  }

  // Métodos de acciones
  verDetalles(evaluacion: Evaluacion): void {
    console.log('Ver:', evaluacion);
  }

  editarEvaluacion(evaluacion: Evaluacion): void {
    console.log('Editar:', evaluacion);
  }

  onFilaClick(evaluacion: Evaluacion): void {
    console.log('Fila clickeada:', evaluacion);
  }

  // Helper para clases del score
  getClaseScore(score: number): string {
    if (score >= 80) return 'score-excelente';
    if (score >= 60) return 'score-bueno';
    if (score >= 40) return 'score-regular';
    return 'score-bajo';
  }
}
```

---

## Interfaces

### ConfiguracionColumna
```typescript
interface ConfiguracionColumna<T = any> {
  encabezado: string;           // Texto del header
  campo?: keyof T;              // Propiedad del objeto a mostrar
  plantilla?: TemplateRef<any>; // Plantilla personalizada
  ancho?: string;               // Ej: '100px', '20%'
  alineacion?: 'left' | 'center' | 'right';
}
```

### AccionTabla
```typescript
interface AccionTabla<T = any> {
  etiqueta: string;                    // Texto del botón
  icono?: TemplateRef<any>;            // Icono opcional
  clase?: string;                      // Clases CSS
  accion: (item: T) => void;           // Función a ejecutar
  mostrar?: (item: T) => boolean;      // Condición para mostrar
}
```

---

## Propiedades del Componente

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `datos` | T[] | [] | Array de datos a mostrar |
| `columnas` | ConfiguracionColumna[] | [] | Configuración de columnas |
| `acciones` | AccionTabla[] | [] | Botones de acción por fila |
| `cargando` | boolean | false | Muestra estado de carga |
| `mensajeSinDatos` | string | 'No se encontraron registros' | Mensaje cuando no hay datos |
| `mensajeCargando` | string | 'Cargando datos...' | Mensaje mientras carga |
| `mostrarIndice` | boolean | false | Muestra columna # con índice |
| `clasePersonalizada` | string | '' | Clase CSS adicional |

---

## Eventos

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `filaClick` | T | Click simple en una fila |
| `filaDobleClick` | T | Doble click en una fila |

---

## Plantillas Personalizadas

### Contexto disponible en plantillas
```html
<ng-template #miPlantilla let-item let-dato="dato">
  <!-- item = objeto completo de la fila -->
  <!-- dato = valor específico de la columna (campo) -->
</ng-template>
```

### Ejemplo: Badge de Score
```html
<ng-template #plantillaScore let-dato="dato">
  <span 
    class="inline-flex items-center justify-center w-10 h-10 rounded-full text-white font-bold"
    [ngClass]="{
      'bg-green-500': dato >= 80,
      'bg-yellow-500': dato >= 60 && dato < 80,
      'bg-orange-500': dato >= 40 && dato < 60,
      'bg-red-500': dato < 40
    }">
    {{ dato }}
  </span>
</ng-template>
```

### Ejemplo: Status con StatusDot
```html
<ng-template #plantillaEstatus let-item>
  <app-status-dot
    [texto]="item.estado"
    [tipo]="mapearEstadoATipo(item.estado)">
  </app-status-dot>
</ng-template>
```

```typescript
mapearEstadoATipo(estado: string): StatusType {
  const mapa: Record<string, StatusType> = {
    'Completado': 'completado',
    'Pendiente': 'atencion',
    'En proceso': 'revision',
    'Rechazado': 'rechazado'
  };
  return mapa[estado] || 'neutral';
}
```

---

## Estilos para Acciones (CSS)

```css
/* En tu componente o global */
.boton-ver {
  background-color: #10b981;
  color: white;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
}

.boton-ver:hover {
  background-color: #059669;
}

.boton-editar {
  background-color: #f0f9ff;
  color: #0369a1;
  border: 1px solid #0369a1;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
}

.boton-editar:hover {
  background-color: #0369a1;
  color: white;
}
```

---

## Acciones Condicionales

```typescript
accionesTabla: AccionTabla<Evaluacion>[] = [
  {
    etiqueta: 'Editar',
    clase: 'boton-editar',
    accion: (item) => this.editar(item),
    // Solo mostrar si NO está completado
    mostrar: (item) => item.estado !== 'Completado'
  },
  {
    etiqueta: 'Ver',
    clase: 'boton-ver',
    accion: (item) => this.ver(item)
    // Sin condición = siempre visible
  }
];
```

---

## Ejemplo Completo

```typescript
@Component({
  selector: 'app-mi-listado',
  standalone: true,
  imports: [
    CommonModule,
    TablaListadoComponent,
    StatusDotComponent
  ],
  template: `
    <app-tabla-listado
      [datos]="datos"
      [columnas]="columnas"
      [acciones]="acciones"
      [cargando]="cargando"
      [mostrarIndice]="true">
    </app-tabla-listado>

    <ng-template #tplEstado let-item>
      <app-status-dot
        [texto]="item.estado"
        [tipo]="item.activo ? 'completado' : 'atencion'">
      </app-status-dot>
    </ng-template>
  `
})
export class MiListadoComponent implements AfterViewInit {
  @ViewChild('tplEstado') tplEstado!: TemplateRef<any>;
  
  datos = [];
  columnas: ConfiguracionColumna[] = [];
  acciones: AccionTabla[] = [];
  cargando = false;

  ngAfterViewInit() {
    setTimeout(() => {
      this.columnas = [
        { encabezado: 'Nombre', campo: 'nombre' },
        { encabezado: 'Estado', campo: 'estado', plantilla: this.tplEstado }
      ];
      
      this.acciones = [
        { etiqueta: 'Ver', clase: 'btn-ver', accion: (i) => console.log(i) }
      ];
    });
  }
}
```