# Componente UI-Icono

Componente reutilizable de los iconos de heroicons, los iconos que estan displibles son los que se usaron en figma. 

# Instalación

```typescript

import { UiIconoComponent } from './ruta/atomic/icono/icono.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiIconoComponent],
  // ...
})
```

# Explicación de las variantes 

- name: es escribe el nombre del icono que se quiere usar, si no se encutra saldra un mensaje "icono no disponible".

- size: se escribe el tamaño del icono se debera de usar las clases de tailwind de tamaños.

- color: se escribe el color en texto, se debe de escribir la clase de tailwind.

```html
<app-ui-icono 
  name="check"
  size="h-6 w-6"
  color="text-gray-700" 
>
</app-ui-icono>
```