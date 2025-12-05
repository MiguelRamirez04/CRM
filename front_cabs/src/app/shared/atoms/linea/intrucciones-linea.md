# Componente UI-linea

Componente reutilizable de las lineas separadoreas, para reutlizarlos en varios lugares. 

# Instalación

```typescript

import { UiDividerComponent } from './ruta/atomic/linea/linea.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiDividerComponent],
  // ...
})
```


# Uso
## Divisor horizontal (por defecto)
```html
  </app-ui-divider>
```

## Divisor vertical

```html
  <app-ui-divider [horizontal]="false"></app-ui-divider>
```
