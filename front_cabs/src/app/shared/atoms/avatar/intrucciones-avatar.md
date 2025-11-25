# Componente UI-Avtar

Componente reutilizable del avatar, para reutlizarlos en varios lugares. 

# Instalación

```typescript

import { UiAvatarComponent } from './ruta/atomic/avatar/avatar.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiAvatarComponent],
  // ...
})
```

# Propiedades (Avatar)
----------------------------------------------------------------------------------------------------
| Propiedad           | Tipo      | Valores   | Descripción                                        |
|---------------------|-----------|-----------|----------------------------------------------------|
| `user`              | `User \`  | `null `   | Se extraen las iniciales para mostrar en el avatar.|
----------------------------------------------------------------------------------------------------