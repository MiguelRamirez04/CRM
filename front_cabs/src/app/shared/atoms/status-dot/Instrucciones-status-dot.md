# Componente Atomo Status-dot

Componente reutilizable para mostrar **etiquetas de estado** con íconos representativos, colores automáticos y soporte para SVG personalizados.

# Instalación

```typescript
import { StatusDotComponent } from './ruta/shared/components/atoms/status-dot/status-dot.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [StatusDotComponent],
})
export class MiComponente {}
```

# Propiedades (Inputs)
| Propiedad       | Tipo              | Valores posibles                                                                                                                                                 | Por defecto      | Descripción                                                                                              |
|------------------|-------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------|----------------------------------------------------------------------------------------------------------|
| **texto**        | `string`          | Cualquier texto                                                                                                                                                  | `''`             | Texto visible junto al icono que describe el estado.                                                     |
| **tipo**         | `StatusType`      | `'sin-seguimiento'`, `'requiere-seguimiento'`, `'nuevo'`, `'revision'`, `'rechazado'`, `'enviada'`, `'aprobado'`, `'venta-realizada'`, `'personalizado'`          | `'personalizado'` | Define el tipo de estado y cambia color + icono automáticamente.                                         |
| **icono**        | `TemplateRef<any>`| —                                                                                                                                                                | `undefined`      | Permite usar una plantilla Angular personalizada para el icono.                                          |
| **colorTexto**   | `string`          | Cualquier color CSS válido (`#HEX`, `rgb()`, nombre, etc.)                                                                                                       | `undefined`      | Color del texto (solo para `tipo='personalizado'`).                                                      |
| **colorFondo**   | `string`          | Cualquier color CSS válido                                                                                                                                       | `undefined`      | Fondo de la etiqueta (solo para `tipo='personalizado'`).                                                 |
| **svgPersonalizado** | `string`      | SVG en formato string (`<svg>...</svg>`)                                                                                                                         | `undefined`      | Ícono SVG personalizado que se renderiza cuando `tipo='personalizado'`.                                  |



## Ejemplos de Uso

###  Estatus por su tipo
```html
<app-status-dot
  texto="Sin seguimiento"
  tipo="sin-seguimiento">
</app-status-dot>

```



###  Estatus Personalizado
```typescript
// En tu componente .ts
iconoCheckAzul = \`
<svg class="w-5 h-5" fill="none" stroke="blue" stroke-width="2" viewBox="0 0 24 24">
  <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
</svg>\`;
;
```

```html
<app-status-dot
  texto="Personalizado"
  tipo="personalizado"
  [svgPersonalizado]="iconoCheckAzul"
  colorFondo="#E0F2FE"
  colorTexto="#1E40AF">
</app-status-dot>

```



## Notas de Seguridad

El componente usa `DomSanitizer` para permitir SVGs personalizados de forma segura. Esto permite usar a `iconoPersonalizado`.

---

