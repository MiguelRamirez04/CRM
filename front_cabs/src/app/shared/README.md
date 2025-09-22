# Shared - Componentes Reutilizables

Módulo que contiene todos los componentes, directivas, pipes y utilidades que pueden ser reutilizados en diferentes módulos de la aplicación.

## 📋 Responsabilidades

- Componentes de UI reutilizables
- Directivas personalizadas
- Pipes para transformación de datos
- Utilidades compartidas
- Lógica común entre módulos

## 📁 Estructura

### components/
Componentes que se usan en múltiples módulos:
- **data-table/** - Tabla de datos reutilizable
- **modal/** - Modal genérico
- **loading-spinner/** - Indicador de carga
- **confirm-dialog/** - Diálogo de confirmación
- **form-controls/** - Controles de formulario personalizados
- **breadcrumb/** - Navegación breadcrumb
- **notification/** - Componente de notificaciones

### directives/
Directivas personalizadas:
- **click-outside.directive.ts** - Detectar clics fuera del elemento
- **highlight.directive.ts** - Resaltar texto
- **permission.directive.ts** - Mostrar/ocultar según permisos
- **auto-focus.directive.ts** - Auto-focus en elementos

### pipes/
Pipes para transformar datos:
- **format-date.pipe.ts** - Formatear fechas
- **safe-html.pipe.ts** - HTML seguro
- **filter.pipe.ts** - Filtrar arrays
- **sort.pipe.ts** - Ordenar arrays
- **truncate.pipe.ts** - Truncar texto

### utils/
Utilidades JavaScript/TypeScript:
- **validators.ts** - Validadores personalizados
- **date-utils.ts** - Utilidades de fechas
- **string-utils.ts** - Utilidades de strings
- **file-utils.ts** - Utilidades de archivos

## 🔗 Conexiones

- **Usado por**: Todos los módulos (administracion, recepcion, soporte)
- **Importa de**: Core (services, models, enums)
- **No debe importar**: Funcionalidades específicas de módulos

## 💡 Ejemplo de Uso

```typescript
// En cualquier módulo
import { DataTableComponent } from '../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';

// En templates
<app-data-table [data]="empleados" [columns]="columns"></app-data-table>
<app-confirm-dialog (confirm)="deleteItem()"></app-confirm-dialog>
```

## ⚙️ Buenas Prácticas

- Todos los componentes deben ser genéricos
- No contener lógica de negocio específica
- Bien documentados con ejemplos
- Probados unitariamente
- Configurables mediante @Input() y @Output()