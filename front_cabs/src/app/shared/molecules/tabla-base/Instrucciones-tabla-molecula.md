# Componente Tabla-Molecula

Componente reutilizable de tabla genérica con columnas configurables, plantillas personalizadas y acciones por fila.

## Instalación

```typescript
import { TablaListadoComponent } from './ruta/molecular/tabla-listado/tabla-listado.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [TablaListadoComponent],
  // ...
})
```

## Propiedades (Inputs)

| Propiedad | Tipo | Defecto | Descripción |
|-----------|------|---------|-------------|
| datos | T[] | [] | Array de objetos a mostrar |
| columnas | ConfiguracionColumna<T>[] | [] | Configuración de columnas |
| acciones | AccionTabla<T>[] | [] | Botones de acción por fila |
| cargando | boolean | false | Muestra indicador de carga |
| mensajeSinDatos | string | 'No se encontraron registros' | Mensaje cuando no hay datos |
| mensajeCargando | string | 'Cargando datos...' | Mensaje durante carga |
| mostrarIndice | boolean | false | Muestra columna con número de fila |
| clasePersonalizada | string | '' | Clases CSS adicionales |

## Eventos (Outputs)

| Evento | Tipo | Descripción |
|--------|------|-------------|
| filaClick | EventEmitter<T> | Se emite al hacer clic en una fila |
| filaDobleClick | EventEmitter<T> | Se emite al hacer doble clic en una fila |

## Interfaces

```typescript
interface ConfiguracionColumna<T = any> {
  encabezado: string;
  campo?: keyof T;
  plantilla?: TemplateRef<any>;
  ancho?: string;
  alineacion?: 'left' | 'center' | 'right';
}

interface AccionTabla<T = any> {
  etiqueta: string;
  icono?: TemplateRef<any>;
  clase?: string;
  accion: (item: T) => void;
  mostrar?: (item: T) => boolean;
}
```

## Ejemplos de Uso

### Tabla Básica

```typescript
interface Usuario {
  id: number;
  nombre: string;
  email: string;
}

usuarios: Usuario[] = [
  { id: 1, nombre: 'Juan Pérez', email: 'juan@example.com' },
  { id: 2, nombre: 'María García', email: 'maria@example.com' }
];

columnas: ConfiguracionColumna<Usuario>[] = [
  { encabezado: 'ID', campo: 'id', ancho: '80px', alineacion: 'center' },
  { encabezado: 'Nombre', campo: 'nombre', ancho: '40%', alineacion: 'left' },
  { encabezado: 'Email', campo: 'email', ancho: '40%', alineacion: 'left' }
];
```

```html
<app-tabla-listado
  [datos]="usuarios"
  [columnas]="columnas">
</app-tabla-listado>
```

### Tabla con Estado de Carga

```typescript
cargando = false;

cargarDatos() {
  this.cargando = true;
  setTimeout(() => {
    this.usuarios = [...];
    this.cargando = false;
  }, 2000);
}
```

```html
<app-tabla-listado
  [datos]="usuarios"
  [columnas]="columnas"
  [cargando]="cargando">
</app-tabla-listado>
```

### Tabla con Acciones

```typescript
acciones: AccionTabla<Usuario>[] = [
  {
    etiqueta: 'Ver',
    clase: 'boton-ver',
    accion: (usuario) => this.verDetalle(usuario)
  },
  {
    etiqueta: 'Editar',
    clase: 'boton-editar',
    accion: (usuario) => this.editar(usuario)
  },
  {
    etiqueta: 'Eliminar',
    clase: 'boton-eliminar',
    accion: (usuario) => this.eliminar(usuario),
    mostrar: (usuario) => usuario.id !== 1 // Ocultar para ID 1
  }
];
```

```html
<app-tabla-listado
  [datos]="usuarios"
  [columnas]="columnas"
  [acciones]="acciones">
</app-tabla-listado>
```

### Tabla con Plantillas Personalizadas

```html
<ng-template #estadoTemplate let-dato="dato">
  <span class="etiqueta" 
        [ngClass]="dato.activo ? 'etiqueta-exito' : 'etiqueta-alerta'">
    {{ dato.activo ? 'Activo' : 'Inactivo' }}
  </span>
</ng-template>

<app-tabla-listado
  [datos]="usuarios"
  [columnas]="columnas">
</app-tabla-listado>
```

```typescript
@ViewChild('estadoTemplate') estadoTemplate!: TemplateRef<any>;

ngAfterViewInit() {
  this.columnas = [
    { encabezado: 'Nombre', campo: 'nombre', ancho: '40%' },
    { encabezado: 'Estado', plantilla: this.estadoTemplate, ancho: '30%' }
  ];
}
```

### Tabla con Índice y Eventos

```typescript
alClickearFila(usuario: Usuario) {
  console.log('Fila clickeada:', usuario);
}
```

```html
<app-tabla-listado
  [datos]="usuarios"
  [columnas]="columnas"
  [mostrarIndice]="true"
  (filaClick)="alClickearFila($event)">
</app-tabla-listado>
```

### Tabla con Iconos en Acciones

```html
<ng-template #iconoEditarTemplate>
  <svg class="icono-tabla" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
          d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
  </svg>
</ng-template>
```

```typescript
@ViewChild('iconoEditarTemplate') iconoEditarTemplate!: TemplateRef<any>;

ngAfterViewInit() {
  this.acciones = [
    {
      etiqueta: 'Editar',
      icono: this.iconoEditarTemplate,
      clase: 'boton-editar',
      accion: (usuario) => this.editar(usuario)
    }
  ];
}
```

## Clases CSS Disponibles

- **boton-ver** - Botón azul para visualización
- **boton-editar** - Botón azul para edición



## Notas

- El componente es genérico (`<T>`), funciona con cualquier tipo de dato
- Usa `TemplateRef` para personalizar celdas específicas
- Las acciones pueden ocultarse condicionalmente con `mostrar`
