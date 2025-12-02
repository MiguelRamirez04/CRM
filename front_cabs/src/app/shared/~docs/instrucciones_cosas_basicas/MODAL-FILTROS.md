# Modal de Filtros

## Descripción
Modal lateral (panel) reutilizable para filtrar datos. Soporta checkboxes agrupados, filtros de fecha y selects.


---

## Componente Necesario

```typescript
import { 
  ModalFiltrosComponent,
  ConfiguracionModalFiltros,
  GrupoFiltroCheckbox,
  FiltroFecha,
  FiltroSelect,
  ResultadoFiltros
} from 'shared/components/modal-filtros/modal-filtros.component';
```

---

## Estructura HTML

```html
<app-modal-filtros
  [visible]="mostrarModalFiltros"
  [configuracion]="configFiltros"
  [filtrosAplicados]="filtrosActuales"
  (cerrar)="cerrarModalFiltros()"
  (aplicarFiltros)="onAplicarFiltros($event)"
  (limpiarFiltros)="onLimpiarFiltros()">
</app-modal-filtros>
```

---

## Configuración Completa (.ts)

```typescript
// Propiedades
mostrarModalFiltros = false;
filtrosActuales: ResultadoFiltros | undefined;

configFiltros: ConfiguracionModalFiltros = {
  titulo: 'Filtrar Evaluaciones',
  
  // GRUPOS DE CHECKBOXES
  gruposCheckbox: [
    {
      id: 'puntaje',
      titulo: 'Puntaje',
      opciones: [
        { valor: '90-100', etiqueta: '90-100', descripcion: 'Excelente' },
        { valor: '80-89', etiqueta: '80 - 89', descripcion: 'Bueno' },
        { valor: '60-79', etiqueta: '60 - 79', descripcion: 'Regular' },
        { valor: '0-59', etiqueta: '0 - 59', descripcion: 'Deficiente' }
      ]
    }
  ],
  
  // FILTROS DE FECHA
  filtrosFecha: [
    {
      id: 'fecha',
      titulo: 'Fecha',
      placeholder: 'Seleccione fecha',
      tipo: 'date'  // 'date' | 'month' | 'year'
    }
  ],
  
  // FILTROS SELECT
  filtrosSelect: [
    {
      id: 'seguimiento',
      titulo: 'Seguimiento',
      placeholder: 'Seleccione un tipo de seguimiento',
      opciones: [
        { valor: 'requiere', etiqueta: 'Requiere seguimiento' },
        { valor: 'completado', etiqueta: 'Completado' },
        { valor: 'sin', etiqueta: 'Sin seguimiento' }
      ]
    }
  ],
  
  // TEXTOS DE BOTONES
  mostrarBotonLimpiar: true,
  textoBotonAplicar: 'Filtrar',
  textoBotonLimpiar: 'Limpiar Todo',
  textoBotonCerrar: 'Cancelar'
};

// Métodos
abrirModalFiltros(): void {
  this.mostrarModalFiltros = true;
}

cerrarModalFiltros(): void {
  this.mostrarModalFiltros = false;
}

onAplicarFiltros(filtros: ResultadoFiltros): void {
  console.log('Filtros aplicados:', filtros);
  
  // Estructura del objeto filtros:
  // {
  //   checkboxes: { puntaje: ['90-100', '80-89'] },
  //   fechas: { fecha: '2025-01-15' },
  //   selects: { seguimiento: 'requiere' }
  // }
  
  this.filtrosActuales = filtros;
  this.aplicarFiltrosADatos(filtros);
}

onLimpiarFiltros(): void {
  this.filtrosActuales = undefined;
  this.cargarTodosLosDatos();
}

private aplicarFiltrosADatos(filtros: ResultadoFiltros): void {
  // Lógica para filtrar tus datos
  const puntajesSeleccionados = filtros.checkboxes['puntaje'] || [];
  const fechaSeleccionada = filtros.fechas['fecha'];
  const seguimientoSeleccionado = filtros.selects['seguimiento'];
  
  // Aplicar filtros...
}
```

---

## Interfaces

### ConfiguracionModalFiltros
```typescript
interface ConfiguracionModalFiltros {
  titulo?: string;
  gruposCheckbox?: GrupoFiltroCheckbox[];
  filtrosFecha?: FiltroFecha[];
  filtrosSelect?: FiltroSelect[];
  mostrarBotonLimpiar?: boolean;
  textoBotonAplicar?: string;
  textoBotonLimpiar?: string;
  textoBotonCerrar?: string;
}
```

### GrupoFiltroCheckbox
```typescript
interface GrupoFiltroCheckbox {
  id: string;           // Identificador único del grupo
  titulo: string;       // Título visible
  opciones: OpcionCheckbox[];
}

interface OpcionCheckbox {
  valor: any;           // Valor que se enviará
  etiqueta: string;     // Texto principal
  descripcion?: string; // Texto secundario (opcional)
  seleccionado?: boolean;
}
```

### FiltroFecha
```typescript
interface FiltroFecha {
  id: string;
  titulo: string;
  placeholder: string;
  tipo: 'date' | 'month' | 'year';
}
```

### FiltroSelect
```typescript
interface FiltroSelect {
  id: string;
  titulo: string;
  placeholder: string;
  opciones: { valor: any; etiqueta: string }[];
}
```

### ResultadoFiltros
```typescript
interface ResultadoFiltros {
  checkboxes: Record<string, any[]>;  // { grupoId: [valores seleccionados] }
  fechas: Record<string, string>;      // { filtroId: 'fecha' }
  selects: Record<string, any>;        // { filtroId: valor }
}
```

---

## Propiedades del Componente

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `visible` | boolean | Controla visibilidad del modal |
| `configuracion` | ConfiguracionModalFiltros | Configuración completa |
| `filtrosAplicados` | ResultadoFiltros | Filtros previamente aplicados (para restaurar estado) |

---

## Eventos

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `cerrar` | void | Se dispara al cerrar el modal |
| `aplicarFiltros` | ResultadoFiltros | Se dispara al hacer clic en "Filtrar" |
| `limpiarFiltros` | void | Se dispara al hacer clic en "Limpiar" |

---

## Ejemplo: Solo Checkboxes

```typescript
configFiltros: ConfiguracionModalFiltros = {
  titulo: 'Filtrar por Estado',
  gruposCheckbox: [
    {
      id: 'estado',
      titulo: 'Estado',
      opciones: [
        { valor: 'activo', etiqueta: 'Activo' },
        { valor: 'inactivo', etiqueta: 'Inactivo' },
        { valor: 'pendiente', etiqueta: 'Pendiente' }
      ]
    }
  ],
  textoBotonAplicar: 'Aplicar'
};
```

---

## Ejemplo: Múltiples Grupos

```typescript
configFiltros: ConfiguracionModalFiltros = {
  titulo: 'Filtros Avanzados',
  gruposCheckbox: [
    {
      id: 'categoria',
      titulo: 'Categoría',
      opciones: [
        { valor: 'A', etiqueta: 'Categoría A' },
        { valor: 'B', etiqueta: 'Categoría B' }
      ]
    },
    {
      id: 'prioridad',
      titulo: 'Prioridad',
      opciones: [
        { valor: 'alta', etiqueta: 'Alta', descripcion: 'Urgente' },
        { valor: 'media', etiqueta: 'Media' },
        { valor: 'baja', etiqueta: 'Baja' }
      ]
    }
  ],
  filtrosFecha: [
    { id: 'desde', titulo: 'Desde', placeholder: 'Fecha inicio', tipo: 'date' },
    { id: 'hasta', titulo: 'Hasta', placeholder: 'Fecha fin', tipo: 'date' }
  ]
};
```

---

## Notas Importantes

1. **El modal se cierra automáticamente** al aplicar filtros
2. **Los filtros se mantienen** mientras el componente padre los guarde en `filtrosActuales`
3. **El indicador "Activos"** aparece cuando hay filtros aplicados
4. **Clic fuera del modal** lo cierra (en el overlay)