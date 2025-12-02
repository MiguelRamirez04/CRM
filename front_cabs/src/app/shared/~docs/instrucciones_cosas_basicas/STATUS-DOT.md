# Status Dot (Indicador de Estado)

## Descripción
Componente átomo para mostrar estados con colores e íconos predefinidos. Ideal para badges de estado en tablas y listados.



---

## Componente Necesario

```typescript
import { StatusDotComponent, StatusType } from 'shared/atoms/status-dot/status-dot.component';
```

---

## Uso Básico

```html
<app-status-dot
  [texto]="'Completado'"
  [tipo]="'completado'">
</app-status-dot>
```

---

## Tipos Predefinidos

| Tipo | Color | Uso típico |
|------|-------|------------|
| `completado` | Verde | Tareas finalizadas, aprobados |
| `atencion` | Amarillo/Naranja | Requiere atención, pendientes |
| `nuevo` | Azul | Items nuevos, recién creados |
| `revision` | Morado | En proceso de revisión |
| `rechazado` | Rojo | Rechazados, errores |
| `enviada` | Azul claro | Enviados, en tránsito |
| `aprobado` | Verde | Similar a completado |
| `venta-realizada` | Verde | Ventas exitosas |
| `neutral` | Gris | Estado neutral |
| `personalizado` | Custom | Colores personalizados |

---

## Ejemplos por Tipo

```html
<!-- Completado (Verde) -->
<app-status-dot texto="Completado" tipo="completado"></app-status-dot>

<!-- Requiere atención (Amarillo) -->
<app-status-dot texto="Requiere seguimiento" tipo="atencion"></app-status-dot>

<!-- Nuevo (Azul) -->
<app-status-dot texto="Nuevo" tipo="nuevo"></app-status-dot>

<!-- En revisión (Morado) -->
<app-status-dot texto="En revisión" tipo="revision"></app-status-dot>

<!-- Rechazado (Rojo) -->
<app-status-dot texto="Rechazado" tipo="rechazado"></app-status-dot>
```

---

## Propiedades

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `texto` | string | '' | Texto a mostrar |
| `tipo` | StatusType | 'personalizado' | Tipo predefinido de estado |
| `mostrarIcono` | boolean | true | Mostrar/ocultar ícono |
| `colorTexto` | string | - | Color de texto personalizado |
| `colorFondo` | string | - | Color de fondo personalizado |
| `svgPersonalizado` | string | - | SVG personalizado como string |

---

## Sin Ícono

```html
<app-status-dot
  texto="Activo"
  tipo="completado"
  [mostrarIcono]="false">
</app-status-dot>
```

---

## Personalizado con Colores

```html
<app-status-dot
  texto="Especial"
  tipo="personalizado"
  colorTexto="#7c3aed"
  colorFondo="#ede9fe">
</app-status-dot>
```

---

## Con SVG Personalizado

```typescript
// En el componente
miSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-4 h-4">
  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
</svg>`;
```

```html
<app-status-dot
  texto="Destacado"
  tipo="personalizado"
  colorTexto="#f59e0b"
  colorFondo="#fef3c7"
  [svgPersonalizado]="miSvg">
</app-status-dot>
```

---

## Uso en Tablas

### En plantilla de tabla
```html
<ng-template #plantillaSeguimiento let-item>
  <app-status-dot
    [texto]="item.requiereSeguimiento ? 'Requiere seguimiento' : 'Completado'"
    [tipo]="item.requiereSeguimiento ? 'atencion' : 'completado'">
  </app-status-dot>
</ng-template>
```

### Mapeo de estados
```typescript
// En el componente
getStatusType(estado: string): StatusType {
  const mapa: Record<string, StatusType> = {
    'pendiente': 'atencion',
    'completado': 'completado',
    'en_proceso': 'revision',
    'cancelado': 'rechazado',
    'nuevo': 'nuevo'
  };
  return mapa[estado] || 'neutral';
}
```

```html
<ng-template #plantillaEstado let-item>
  <app-status-dot
    [texto]="item.estado"
    [tipo]="getStatusType(item.estado)">
  </app-status-dot>
</ng-template>
```

---

## Ejemplo: Estados de Evaluación

```typescript
// evaluaciones.component.ts
getStatusEvaluacion(evaluacion: any): { texto: string; tipo: StatusType } {
  if (evaluacion.requiereSeguimiento) {
    return { texto: 'Requiere seguimiento', tipo: 'atencion' };
  }
  
  if (evaluacion.scoreCalidad >= 80) {
    return { texto: 'Excelente', tipo: 'completado' };
  }
  
  if (evaluacion.scoreCalidad >= 60) {
    return { texto: 'Regular', tipo: 'nuevo' };
  }
  
  return { texto: 'Necesita mejora', tipo: 'rechazado' };
}
```

```html
<ng-template #plantillaEstado let-item>
  <app-status-dot
    [texto]="getStatusEvaluacion(item).texto"
    [tipo]="getStatusEvaluacion(item).tipo">
  </app-status-dot>
</ng-template>
```

---

## Variaciones Visuales

### Tamaño compacto (solo en CSS)
```css
/* Si necesitas un tamaño más pequeño */
app-status-dot {
  font-size: 12px;
}

app-status-dot ::ng-deep .status-container {
  padding: 2px 8px;
}
```

### En línea con texto
```html
<p>
  Estado actual: 
  <app-status-dot texto="Activo" tipo="completado" [mostrarIcono]="false"></app-status-dot>
</p>
```

---

## Referencia de Colores por Tipo

| Tipo | Fondo | Texto | Borde |
|------|-------|-------|-------|
| `completado` | #dcfce7 | #166534 | #22c55e |
| `atencion` | #fef3c7 | #92400e | #f59e0b |
| `nuevo` | #dbeafe | #1e40af | #3b82f6 |
| `revision` | #f3e8ff | #6b21a8 | #a855f7 |
| `rechazado` | #fee2e2 | #991b1b | #ef4444 |
| `enviada` | #e0f2fe | #075985 | #0ea5e9 |
| `neutral` | #f3f4f6 | #374151 | #9ca3af |