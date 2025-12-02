# Paginación

## Descripción
Componente de paginación reutilizable con navegación por páginas, información de registros y puntos suspensivos inteligentes.


---

## Componente Necesario

```typescript
import { 
  PaginacionComponent, 
  ConfiguracionPaginacion 
} from 'shared/components/paginacion/paginacion.component';
```

---

## Uso Básico

```html
<app-paginacion
  [totalElementos]="totalRegistros"
  [paginaActual]="paginaActual"
  [configuracion]="configPaginacion"
  (cambioPagina)="onCambioPagina($event)">
</app-paginacion>
```

---

## Configuración en el Componente (.ts)

```typescript
// Propiedades
totalRegistros = 97;
paginaActual = 1;

configPaginacion: ConfiguracionPaginacion = {
  elementosPorPagina: 10,
  paginasVisiblesMaximas: 6,
  textoAnterior: 'Atrás',
  textoSiguiente: 'Siguiente',
  textoMostrandoRegistros: 'Visualizando',
  textoDeRegistros: 'de',
  mostrarInfoRegistros: true,
  mostrarBotonesPagina: true
};

// Método para cambio de página
onCambioPagina(nuevaPagina: number): void {
  this.paginaActual = nuevaPagina;
  this.cargarDatos();
}

// Cargar datos con paginación
cargarDatos(): void {
  const inicio = (this.paginaActual - 1) * this.configPaginacion.elementosPorPagina!;
  const fin = inicio + this.configPaginacion.elementosPorPagina!;
  
  // Llamar al servicio con los parámetros de paginación
  this.servicio.obtenerDatos(inicio, fin).subscribe(response => {
    this.datos = response.items;
    this.totalRegistros = response.total;
  });
}
```

---

## Interface ConfiguracionPaginacion

```typescript
interface ConfiguracionPaginacion {
  elementosPorPagina?: number;        // Default: 10
  paginasVisiblesMaximas?: number;    // Default: 6
  textoAnterior?: string;             // Default: 'Atrás'
  textoSiguiente?: string;            // Default: 'Siguiente'
  textoMostrandoRegistros?: string;   // Default: 'Visualizando'
  textoDeRegistros?: string;          // Default: 'de'
  mostrarInfoRegistros?: boolean;     // Default: true
  mostrarBotonesPagina?: boolean;     // Default: true
}
```

---

## Propiedades del Componente

| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `totalElementos` | number | ✅ | Total de registros en la BD |
| `paginaActual` | number | ✅ | Página actual (inicia en 1) |
| `configuracion` | ConfiguracionPaginacion | ❌ | Configuración opcional |

---

## Eventos

| Evento | Payload | Descripción |
|--------|---------|-------------|
| `cambioPagina` | number | Nueva página seleccionada |
| `cambioRangoPagina` | { inicio: number; fin: number } | Rango de índices para la página |

---

## Ejemplo con Rango

```typescript
// Usar el evento cambioRangoPagina para obtener los índices directamente
onCambioRango(rango: { inicio: number; fin: number }): void {
  console.log(`Mostrar elementos del ${rango.inicio} al ${rango.fin}`);
  
  // Útil para paginación del lado del cliente
  this.datosVisibles = this.todosDatos.slice(rango.inicio, rango.fin);
}
```

```html
<app-paginacion
  [totalElementos]="totalRegistros"
  [paginaActual]="paginaActual"
  (cambioPagina)="onCambioPagina($event)"
  (cambioRangoPagina)="onCambioRango($event)">
</app-paginacion>
```

---

## Configuraciones Comunes

### Paginación simple (sin info de registros)
```typescript
configPaginacion: ConfiguracionPaginacion = {
  elementosPorPagina: 10,
  mostrarInfoRegistros: false
};
```

### Paginación con más elementos
```typescript
configPaginacion: ConfiguracionPaginacion = {
  elementosPorPagina: 25,
  paginasVisiblesMaximas: 8
};
```

### Textos en inglés
```typescript
configPaginacion: ConfiguracionPaginacion = {
  textoAnterior: 'Previous',
  textoSiguiente: 'Next',
  textoMostrandoRegistros: 'Showing',
  textoDeRegistros: 'of'
};
```

---

## Integración con Backend

### Ejemplo con servicio
```typescript
// evaluaciones.service.ts
obtenerEvaluaciones(pagina: number, porPagina: number): Observable<PaginatedResponse> {
  return this.http.get<PaginatedResponse>(`${this.apiUrl}/evaluaciones`, {
    params: {
      page: pagina.toString(),
      limit: porPagina.toString()
    }
  });
}

// evaluaciones.component.ts
cargarEvaluaciones(): void {
  this.cargando = true;
  
  this.evaluacionService.obtenerEvaluaciones(
    this.paginaActual,
    this.configPaginacion.elementosPorPagina!
  ).subscribe({
    next: (response) => {
      this.evaluaciones = response.data;
      this.totalRegistros = response.total;
      this.cargando = false;
    },
    error: (error) => {
      console.error('Error:', error);
      this.cargando = false;
    }
  });
}

onCambioPagina(pagina: number): void {
  this.paginaActual = pagina;
  this.cargarEvaluaciones();
}
```

---

## Paginación del Lado del Cliente

```typescript
// Cuando tienes todos los datos en memoria
todosDatos: any[] = [];
datosVisibles: any[] = [];
paginaActual = 1;
elementosPorPagina = 10;

get totalElementos(): number {
  return this.todosDatos.length;
}

ngOnInit(): void {
  // Cargar todos los datos una vez
  this.servicio.obtenerTodos().subscribe(data => {
    this.todosDatos = data;
    this.actualizarVista();
  });
}

onCambioPagina(pagina: number): void {
  this.paginaActual = pagina;
  this.actualizarVista();
}

private actualizarVista(): void {
  const inicio = (this.paginaActual - 1) * this.elementosPorPagina;
  const fin = inicio + this.elementosPorPagina;
  this.datosVisibles = this.todosDatos.slice(inicio, fin);
}
```

---

## Ejemplo Completo

```typescript
@Component({
  selector: 'app-listado',
  standalone: true,
  imports: [
    CommonModule,
    TablaListadoComponent,
    PaginacionComponent
  ],
  template: `
    <!-- Tabla -->
    <app-tabla-listado
      [datos]="datosVisibles"
      [columnas]="columnas"
      [cargando]="cargando">
    </app-tabla-listado>
    
    <!-- Paginación -->
    <app-paginacion
      [totalElementos]="totalRegistros"
      [paginaActual]="paginaActual"
      [configuracion]="configPaginacion"
      (cambioPagina)="onCambioPagina($event)">
    </app-paginacion>
  `
})
export class ListadoComponent implements OnInit {
  datosVisibles: any[] = [];
  totalRegistros = 0;
  paginaActual = 1;
  cargando = false;

  configPaginacion: ConfiguracionPaginacion = {
    elementosPorPagina: 10,
    textoMostrandoRegistros: 'Visualizando',
    textoDeRegistros: 'de',
    textoAnterior: 'Atrás',
    textoSiguiente: 'Siguiente'
  };

  columnas = [
    { encabezado: 'ID', campo: 'id' },
    { encabezado: 'Nombre', campo: 'nombre' }
  ];

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.cargando = true;
    // ... llamar servicio
  }

  onCambioPagina(pagina: number): void {
    this.paginaActual = pagina;
    this.cargarDatos();
  }
}
```

---

## Comportamiento de los Puntos Suspensivos

El componente muestra automáticamente `...` cuando hay muchas páginas:

- **Inicio (páginas 1-3):** `1 2 3 4 ... 11`
- **Medio (página 5):** `1 ... 4 5 6 ... 11`
- **Final (páginas 9-11):** `1 ... 8 9 10 11`

Este comportamiento está controlado por `paginasVisiblesMaximas`.