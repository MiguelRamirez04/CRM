# Header con Buscador-Filtro

## Descripción
Combina el componente `app-ui-header` con `app-buscador-filtro` para crear el encabezado estándar de las páginas de listado.


---

## Componentes Necesarios

```typescript
import { UiHeaderComponent } from 'shared/molecules/header/header.component';
import { BuscadorFiltroComponent, ConfiguracionBuscador } from 'shared/components/buscador-filtro/buscador-filtro.component';
```

---

## Estructura HTML

```html
<app-ui-header
  [titulo]="'Evaluaciones'"
  [descripcion]="'Gestiona y consulta todas las evaluaciones realizadas'"
  [visualizarButton]="false">
  
  <!-- El buscador-filtro va dentro del header como contenido proyectado -->
  <app-buscador-filtro
    [configuracion]="configBuscador"
    [valorBusqueda]="terminoBusqueda"
    (cambioBusqueda)="onBuscar($event)"
    (clicFiltro)="abrirModalFiltros()"
    (clicNuevo)="crearNuevo()">
  </app-buscador-filtro>
  
</app-ui-header>
```

---

## Configuración en el Componente (.ts)

```typescript
// Importar la interfaz
import { ConfiguracionBuscador } from 'shared/components/buscador-filtro/buscador-filtro.component';

// Propiedades
terminoBusqueda: string = '';

configBuscador: ConfiguracionBuscador = {
  placeholderBusqueda: 'Buscar por ID o evaluador',
  mostrarBotonFiltro: true,
  mostrarBotonNuevo: true,
  textoBotonNuevo: 'Nueva evaluación',
  tiempoEsperaBusqueda: 300  // milisegundos de debounce
};

// Métodos
onBuscar(termino: string): void {
  this.terminoBusqueda = termino;
  this.filtrarDatos();
}

abrirModalFiltros(): void {
  this.mostrarModalFiltros = true;
}

crearNuevo(): void {
  // Navegar o abrir modal de creación
  this.router.navigate(['/evaluaciones/nueva']);
}
```

---

## Propiedades del Header

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `titulo` | string | '' | Título principal de la página |
| `descripcion` | string | '' | Subtítulo/descripción |
| `buttonLabel` | string | '' | Texto del botón (si se usa) |
| `visualizarDescripcion` | boolean | true | Mostrar/ocultar descripción |
| `visualizarButton` | boolean | true | Mostrar/ocultar botón del header |

---

## Propiedades del Buscador-Filtro

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `placeholderBusqueda` | string | 'Buscar...' | Placeholder del input |
| `mostrarBotonFiltro` | boolean | true | Mostrar botón "Filtro" |
| `mostrarBotonNuevo` | boolean | true | Mostrar botón "Nuevo" |
| `textoBotonNuevo` | string | 'Nuevo' | Texto del botón nuevo |
| `tiempoEsperaBusqueda` | number | 300 | Debounce en ms |

---

## Eventos del Buscador-Filtro

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `cambioBusqueda` | string | Se dispara al escribir (con debounce) |
| `clicFiltro` | void | Se dispara al hacer clic en "Filtro" |
| `clicNuevo` | void | Se dispara al hacer clic en "Nuevo" |

---

## Ejemplo Completo

```typescript
@Component({
  selector: 'app-evaluaciones',
  standalone: true,
  imports: [
    CommonModule,
    UiHeaderComponent,
    BuscadorFiltroComponent
  ],
  template: `
    <app-ui-header
      [titulo]="'Evaluaciones'"
      [descripcion]="'Gestiona y consulta todas las evaluaciones realizadas'"
      [visualizarButton]="false">
      
      <app-buscador-filtro
        [configuracion]="configBuscador"
        [valorBusqueda]="terminoBusqueda"
        (cambioBusqueda)="onBuscar($event)"
        (clicFiltro)="abrirModalFiltros()"
        (clicNuevo)="crearNuevo()">
      </app-buscador-filtro>
      
    </app-ui-header>
    
    <!-- Resto del contenido de la página -->
  `
})
export class EvaluacionesComponent {
  terminoBusqueda = '';
  
  configBuscador: ConfiguracionBuscador = {
    placeholderBusqueda: 'Buscar por ID o evaluador',
    mostrarBotonFiltro: true,
    mostrarBotonNuevo: true,
    textoBotonNuevo: 'Nueva evaluación'
  };

  onBuscar(termino: string): void {
    console.log('Buscando:', termino);
  }

  abrirModalFiltros(): void {
    console.log('Abrir filtros');
  }

  crearNuevo(): void {
    console.log('Crear nuevo');
  }
}
```

---

## Variaciones

### Sin botón de filtro
```typescript
configBuscador: ConfiguracionBuscador = {
  placeholderBusqueda: 'Buscar...',
  mostrarBotonFiltro: false,
  mostrarBotonNuevo: true,
  textoBotonNuevo: 'Agregar'
};
```

### Solo búsqueda
```typescript
configBuscador: ConfiguracionBuscador = {
  placeholderBusqueda: 'Buscar usuarios...',
  mostrarBotonFiltro: false,
  mostrarBotonNuevo: false
};
```