# Componente UI-Boton

Componente reutilizable de botón  con soporte para múltiples variantes, iconos personalizados y estados de carga.

# Instalación

```typescript

import { UiBotonComponent } from './ruta/ui/boton/boton.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiBotonComponent],
  // ...
})
```

---

# Propiedades (Inputs)
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
| Propiedad           | Tipo      | Valores                                        |  POR Defecto | Descripción                                          |
|---------------------|-----------|------------------------------------------------|--------------|------------------------------------------------------|
| `variante`          | `string`  | `'primario'` \| `'secundario'` \| `'terciario'`| `'primario'` | sefine el estilo visual del botón                    |
| `texto`             | `string`  | Cualquier texto                                | `'Botón'`    | texto que se muestra en el botón                     |
| `tipo`              | `string`  | `'button'` \| `'submit'` \| `'reset'`          | `'button'`   | tipo HTML del botón                                  |
| `estaCargando`      | `boolean` | `true` \| `false`                              | `false`      | muestra icono cargando y deshabilita el botón        |
| `estaDeshabilitado` | `boolean` | `true` \| `false`                              | `false`      | deshabilita el botón manualmente                     |
| `anchoCompleto`     | `boolean` | `true` \| `false`                              | `true`       | si es `true`, el botón ocupa el 100% del ancho       |
| `mostrarIcono`      | `boolean` | `true` \| `false`                              | `false`      | muestra un icono (predeterminado o personalizado)    |
| `iconoPersonalizado`| `string`  | SVG como string                                | `undefined`  | SVG personalizado para mostrar                       |
| `textoAlCargar`     | `string`  | Cualquier texto                                | `undefined`  | texto alternativo mientras carga                     |
| `clasesAdicionales` | `string`  | Clases Tailwind                                | `undefined`  | clases CSS adicionales                               |
----------------------------------------------------------------------------------------------------------------------------------------------------------

# Eventos (Outputs)
-----------------------------------------------------------------------------------
| Evento       | Tipo                  | Descripción                              |
|--------------|-----------------------|------------------------------------------|
| `alClickear` | `EventEmitter<Event>` | Se emite cuando se hace clic en el botón |
-----------------------------------------------------------------------------------

## -> Variantes Disponibles

### 1 --Primario-- (Azul degradado)
- **Uso:** Acciones principales (guardar, enviar, confirmar)
- **Colores:** Degradado azul (`bg-gradient-to-r from-blue-600 to-blue-800`)
- **Estilo:** Fondo azul con texto blanco

```html
<app-ui-boton
  variante="primario"
  texto="Guardar">
</app-ui-boton>
```

### 2 --Secundario-- (Cyan vibrante)
- **Uso:** Acciones secundarias importantes
- **Colores:** Cyan `#35D9FD` con texto azul oscuro `#34428F`
- **Estilo:** Fondo cyan con contraste alto

```html
<app-ui-boton
  variante="secundario"
  texto="Buscar">
</app-ui-boton>
```

### 3️⃣ **Terciario** (Blanco con borde)
- **Uso:** acciones terciarias
- **Colores:** Fondo blanco con borde gris
- **Estilo:** Minimalista con borde visible

```html
<app-ui-boton
  variante="terciario"
  texto="Cancelar">
</app-ui-boton>
```

---

## Ejemplos de Uso

###  Botón Simple
```html
<app-ui-boton
  texto="Aceptar"
  (alClickear)="aceptar()">
</app-ui-boton>
```

### Botón con Estado de Carga
```html
<app-ui-boton
  variante="primario"
  texto="Guardar"
  [estaCargando]="guardando"
  [textoAlCargar]="'Guardando...'"
  (alClickear)="guardar()">
</app-ui-boton>
```

```typescript
// En tu componente
guardando = false;

guardar() {
  this.guardando = true;
  setTimeout(() => {
    this.guardando = false;
  }, 2000);
}
```

###  Botón Deshabilitado
```html
<app-ui-boton
  texto="Enviar"
  [estaDeshabilitado]="formulario.invalid"
  (alClickear)="enviar()">
</app-ui-boton>
```

###  Botón Compacto (sin ancho completo)
```html
<app-ui-boton
  texto="OK"
  [anchoCompleto]="false"
  (alClickear)="confirmar()">
</app-ui-boton>
```

###  Botón con Icono Predeterminado
```html
<app-ui-boton
  texto="Acción Rápida"
  [mostrarIcono]="true"
  (alClickear)="accion()">
</app-ui-boton>
```

###  Botón con Icono Personalizado
```typescript
// En tu componente .ts
iconoBuscar = `<svg class="w-full h-full" fill="none" stroke="currentColor" viewBox="0 0 24 24">
  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
</svg>`;
```

```html
<app-ui-boton
  variante="secundario"
  texto="Buscar"
  [mostrarIcono]="true"
  [iconoPersonalizado]="iconoBuscar"
  (alClickear)="buscar()">
</app-ui-boton>
```

###  Botón de Eliminar (con clases adicionales)
```html
<app-ui-boton
  variante="terciario"
  texto="Eliminar"
  [mostrarIcono]="true"
  [iconoPersonalizado]="iconoEliminar"
  clasesAdicionales="!bg-red-500 !text-white !border-red-600 hover:!bg-red-600"
  (alClickear)="eliminar()">
</app-ui-boton>
```

###  Botón de Submit en Formulario
```html
<form [formGroup]="miFormulario" (ngSubmit)="enviar()">
  <!-- Campos del formulario -->
  
  <app-ui-boton
    variante="primario"
    tipo="submit"
    texto="Enviar"
    [estaCargando]="enviando"
    [estaDeshabilitado]="miFormulario.invalid"
    [anchoCompleto]="true">
  </app-ui-boton>
</form>
```

###  Botones Múltiples (en línea)
```html
<div class="flex gap-3">
  <app-ui-boton
    variante="primario"
    texto="Confirmar"
    [anchoCompleto]="false"
    (alClickear)="confirmar()">
  </app-ui-boton>
  
  <app-ui-boton
    variante="terciario"
    texto="Cancelar"
    [anchoCompleto]="false"
    (alClickear)="cancelar()">
  </app-ui-boton>
</div>
```

---


## Notas de Seguridad

El componente usa `DomSanitizer` para permitir SVGs personalizados de forma segura. Esto permite usar a `iconoPersonalizado`.

---

