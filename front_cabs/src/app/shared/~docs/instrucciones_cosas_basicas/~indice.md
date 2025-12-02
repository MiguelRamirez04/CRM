# Documentación de Componentes Shared

## Índice de Guías

Esta documentación explica cómo usar los componentes reutilizables de la carpeta `shared` para construir las pantallas estándar del sistema.

---

## 📄 Guías Disponibles

| # | Documento | Descripción |
|---|-----------|-------------|
| 1 | [HEADER-BUSCADOR.md](./HEADER-BUSCADOR.md) | Header de página + barra de búsqueda y filtros |
| 2 | [MODAL-DETALLES.md](./MODAL-DETALLES.md) | Modal para ver detalles de un registro |
| 3 | [MODAL-FILTROS.md](./MODAL-FILTROS.md) | Modal lateral para filtrar datos |
| 4 | [MODAL-REGISTRO.md](./MODAL-REGISTRO.md) | Modal para crear/editar registros |
| 5 | [PAGINACION.md](./PAGINACION.md) | Componente de paginación |
| 6 | [STATUS-DOT.md](./STATUS-DOT.md) | Badges de estado con colores |
| 7 | [TABLA-LISTADO.md](./TABLA-LISTADO.md) | Tabla con columnas personalizables y acciones |





---

## 🏗️ Estructura de una Pantalla Típica

```
┌─────────────────────────────────────────────────────────┐
│  HEADER                                                 │
│  ┌─────────────────────────────────────────────────────┐│
│  │ Título + Descripción                                ││
│  │ [Buscar...] [Filtro] [+ Nuevo]                      ││
│  └─────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────┤
│  TABLA DE LISTADO                                       │
│  ┌──────┬──────────┬──────────┬─────────┬─────────────┐ │
│  │ ID   │          │          │         │ Acciones    │ │
│  ├──────┼──────────┼──────────┼─────────┼─────────────┤ │
│  │ 1    │ Item 1   │          │         │ [Ver][Edit] │ │
│  │ 2    │ Item 2   │          │         │ [Ver][Edit] │ │
│  └──────┴──────────┴──────────┴─────────┴─────────────┘ │
├─────────────────────────────────────────────────────────┤
│  PAGINACIÓN                                             │
│  Visualizando 1-10 de 50    [←] [1][2][3]...[10] [→]    │
└─────────────────────────────────────────────────────────┘

          ↓ Al hacer clic en botones ↓

┌─ MODAL FILTROS ─┐   ┌─ MODAL DETALLES  ─┐   ┌─ MODAL REGISTRO ─┐
│ Panel lateral   │   │ Panel lateral     │   │ Modal centrado   │
│ con checkboxes, │   │ con información   │   │ con formulario   │
│ fechas y        │   │ detallada del     │   │ para crear o     │
│ selects         │   │ registro          │   │ editar           │
└─────────────────┘   └───────────────────┘   └──────────────────┘
```

---

## 📦 Imports Comunes

### Para páginas de listado
```typescript
// Header y buscador
import { UiHeaderComponent } from 'shared/molecules/header/header.component';
import { BuscadorFiltroComponent } from 'shared/components/buscador-filtro/buscador-filtro.component';

// Tabla
import { TablaListadoComponent } from 'shared/components/tabla-listado/tabla-listado.component';
import { StatusDotComponent } from 'shared/atoms/status-dot/status-dot.component';

// Paginación
import { PaginacionComponent } from 'shared/components/paginacion/paginacion.component';

// Modal de filtros
import { ModalFiltrosComponent } from 'shared/components/modal-filtros/modal-filtros.component';
```

### Para formularios
```typescript
import {
  FormInputComponent,
  FormSelectComponent,
  FormTextareaComponent,
  FormToggleComponent,
  LockedFieldComponent,
  FormRowComponent,
  FormSectionComponent,
  FormInfoAlertComponent,
  ScoreDisplayComponent,
  LoadingOverlayComponent
} from 'shared/form-system.index';
```

---

## 🎨 Componentes Disponibles

### Atoms (shared/atoms/)
| Componente | Descripción |
|------------|-------------|
| `avatar` | Avatar de usuario |
| `bage` | Badge genérico |
| `boton` | Botones con variantes |
| `filter-checkbox` | Checkbox para filtros |
| `filter-input` | Input para filtros |
| `filter-select` | Select para filtros |
| `form-input` | Input de formulario |
| `form-select` | Select de formulario |
| `form-textarea` | Textarea de formulario |
| `form-toggle` | Switch on/off |
| `icono` | Iconos SVG centralizados |
| `inputs` | Inputs UI genéricos |
| `linea` | Línea separadora |
| `loading-spinner` | Spinner de carga |
| `locked-field` | Campo bloqueado (modo editar) |
| `status-dot` | Badges de estado con colores |
| `tabla-base` | Celdas base de tabla |
| `tipografia` | Textos con estilos |

### Molecules (shared/molecules/)
| Componente | Descripción |
|------------|-------------|
| `alert` | Alertas genéricas |
| `barrGrafica` | Barra gráfica/progreso |
| `card` | Tarjeta contenedora |
| `detail-field` | Campo de detalle (solo lectura) |
| `detail-section` | Sección de detalles |
| `etiqueta` | Etiquetas/tags |
| `filter-checkbox-group` | Grupo de checkboxes para filtros |
| `filter-field` | Campo de filtro |
| `form-info-alert` | Alertas informativas en formularios |
| `form-row` | Fila de formulario (grid) |
| `form-section` | Sección de formulario con título |
| `header` | Header de página |
| `loading-overlay` | Overlay de carga con spinner |
| `score-display` | Display de puntuación |
| `tabla-base` | Tabla base reutilizable |

### Organisms (shared/organisms/)
| Componente | Descripción |
|------------|-------------|
| `filter-panel` | Panel completo de filtros |
| `form-panel` | Panel completo de formulario |
| `side-panel` | Panel lateral genérico |

### Components (shared/components/)
| Componente | Descripción |
|------------|-------------|
| `buscador-filtro` | Barra de búsqueda con botones |
| `checkbox` | Checkbox standalone |
| `modal-filtros` | Modal de filtros |
| `notificaciones` | Sistema de notificaciones |
| `paginacion` | Paginación |
| `radio-button` | Radio button |
| `tabla-listado` | Tabla de datos con acciones |
| `unauthorized` | Página de no autorizado |
| `under-construction` | Página en construcción |

### Directives (shared/directives/)
| Directiva | Descripción |
|-----------|-------------|
| `click-outside` | Detecta clicks fuera de un elemento |

---

## 🔄 Flujo Típico de Desarrollo

1. **Crear el componente de la página**
2. **Importar componentes necesarios**
3. **Configurar el header con buscador**
4. **Configurar la tabla con columnas**
5. **Agregar paginación**
6. **Crear modal de filtros**
7. **Crear modal de detalles**
8. **Crear modal de registro/edición**
9. **Conectar con servicios**

---

## 📁 Archivos de Referencia

Para ver ejemplos reales de implementación, revisar:
- `evaluaciones/` - Implementación completa del módulo de evaluaciones
- `shared/` - Código fuente de los componentes

---

## ✅ Checklist de Pantalla

- [ ] Header con título y descripción
- [ ] Buscador con debounce
- [ ] Botón de filtros
- [ ] Botón de crear nuevo
- [ ] Tabla con columnas configuradas
- [ ] Plantillas para columnas especiales (status, score)
- [ ] Acciones por fila (ver, editar)
- [ ] Paginación conectada
- [ ] Modal de filtros configurado
- [ ] Modal de detalles
- [ ] Modal de registro/edición
- [ ] Estados de carga (loading)
- [ ] Manejo de errores
- [ ] Mensajes cuando no hay datos