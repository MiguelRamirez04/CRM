# Integración de APIs AdmDocumentos - Cotizaciones Legacy

## 📋 Resumen de Implementación

Se ha completado la integración del módulo de **Cotizaciones Legacy** con todas las APIs del controlador `AdmDocumentosController`.

---

## ✅ APIs Implementadas

### 1. **GET /api/AdmDocumentos/search**
- **Servicio**: `CotizacionLegacyService.buscar(filtros)`
- **Descripción**: Búsqueda paginada con múltiples filtros
- **Filtros disponibles**:
  - Rango de fechas (documento y vencimiento)
  - Folio específico
  - Razón social del cliente
  - ID de agente
  - ID de concepto
  - Opción para incluir movimientos (productos)
  - Paginación configurable (page, pageSize)

### 2. **GET /api/AdmDocumentos/{id}**
- **Servicio**: `CotizacionLegacyService.obtenerPorId(id)`
- **Descripción**: Obtiene una cotización específica por ID
- **Retorna**: Datos completos del documento incluyendo:
  - Información del cliente
  - Montos (subtotal, IVA, total, pendiente)
  - Movimientos (productos) si están disponibles

### 3. **GET /api/AdmDocumentos/resumen**
- **Servicio**: `CotizacionLegacyService.obtenerResumen(fechaInicio, fechaFin)`
- **Descripción**: Obtiene resumen de cotizaciones por rango de fechas
- **Parámetros requeridos**:
  - `fechaInicio`: Fecha inicial (formato ISO: YYYY-MM-DD)
  - `fechaFin`: Fecha final (formato ISO: YYYY-MM-DD)
- **Retorna**: Total de documentos en el rango especificado
- **Uso**: Widget de resumen en el dashboard (último mes)

### 4. **POST /api/AdmDocumentos/cotizacion**
- **Servicio**: `CotizacionLegacyService.crear(request)`
- **Descripción**: Crea una nueva cotización
- **Request incluye**:
  - ID del cliente (obligatorio)
  - Lista de productos con cantidades, precios y descuentos
  - Descuentos a nivel documento (opcional)
  - Configuración de IVA
  - Fechas (vencimiento, pronto pago, entrega)
  - Observaciones y referencia

### 5. **PUT /api/AdmDocumentos/cotizacion/cancelar**
- **Servicio**: `CotizacionLegacyService.cancelar(request)`
- **Descripción**: Cancela una cotización existente
- **Parámetros**:
  - `idDocumento`: ID del documento a cancelar
  - `motivo`: Motivo de la cancelación (obligatorio)
- **Efecto**: Marca el documento como cancelado (CCANCELADO = 1)

---

## 🗂️ Archivos Modificados/Creados

### Interfaces TypeScript
- **`cotizacion-legacy.interface.ts`**
  - ✅ Agregada interfaz `CotizacionLegacyResumen` para el endpoint de resumen
  - ✅ Todas las interfaces existentes validadas

### Servicios
- **`cotizacion-legacy.service.ts`**
  - ✅ Agregado método `obtenerResumen(fechaInicio, fechaFin)`
  - ✅ Métodos existentes: `crear()`, `cancelar()`, `buscar()`, `obtenerPorId()`
  - ✅ Método auxiliar: `calcularTotalesPreview()` (cálculos cliente-side)
  - ✅ Manejo de errores HTTP con logs descriptivos

### Componentes
- **`documentos.component.ts`**
  - ✅ Integración completa con todas las APIs
  - ✅ Signal `resumenTotal` para widget de resumen
  - ✅ Método `cargarResumen()` que consume la API de resumen
  - ✅ CRUD completo: crear, ver detalle, cancelar cotizaciones
  - ✅ Búsqueda con múltiples filtros
  - ✅ Paginación funcional
  
- **`documentos.component.html`**
  - ✅ Widget de resumen con totales del último mes
  - ✅ Diseño con gradiente azul y icono de documento
  - ✅ Formulario de filtros avanzados
  - ✅ Tabla de cotizaciones con acciones
  - ✅ Modal de creación con autocompletado
  - ✅ Modal de detalle con opción de cancelar

### Menús
- **`operaciones-menu.component.ts`**
  - ✅ Tab "Documentos" ya existente y funcional
  - ✅ Navegación: `/legacy/operaciones/documentos`

---

## 🎨 Características del Componente

### Widget de Resumen
```html
<!-- Muestra el total de cotizaciones del último mes -->
<div class="bg-gradient-to-r from-blue-500 to-blue-600 rounded-lg shadow-lg p-6 mb-6 text-white">
  <p class="text-blue-100 text-sm font-medium mb-1">Total Cotizaciones (último mes)</p>
  <p class="text-4xl font-bold">{{ resumenTotal() }}</p>
</div>
```

### Filtros de Búsqueda
- Rango de fechas (inicio/fin)
- Folio de documento
- Razón social del cliente
- Checkbox para incluir movimientos (productos)

### Tabla de Cotizaciones
- Folio, Fecha, Cliente
- Subtotal, IVA, Total
- Estado (cancelado/activo)
- Botón "Ver Detalle"

### Modal de Creación
- Autocompletado de clientes (búsqueda por razón social)
- Autocompletado de productos (búsqueda por nombre/código)
- Selección de almacén
- Tabla dinámica de productos seleccionados
- Cálculo en tiempo real de totales
- Descuentos a nivel producto y documento
- Toggle de IVA con porcentaje configurable

### Modal de Detalle
- Información completa del documento
- Datos del cliente
- Lista de productos cotizados
- Montos detallados
- Botón "Cancelar Cotización" con motivo

---

## 🔐 Autenticación

El servicio utiliza **HttpOnly cookies** para autenticación:
- No se usan Bearer tokens en headers
- El interceptor `SecureAuthInterceptor` añade automáticamente `withCredentials: true`
- Los tokens CSRF se incluyen automáticamente en POST/PUT/DELETE

---

## 🚀 Cómo Acceder

### Navegación desde el menú
1. Ingresar a la aplicación: `http://localhost:4200`
2. En el sidebar, seleccionar **"Legacy"** → **"Operaciones"**
3. Hacer clic en la tab **"Documentos"**
4. URL directa: `http://localhost:4200/legacy/operaciones/documentos`

### Flujo de uso
1. **Ver resumen**: El widget muestra automáticamente el total del último mes
2. **Buscar cotizaciones**: Usar filtros y hacer clic en "Buscar"
3. **Ver detalle**: Hacer clic en "Ver Detalle" en cualquier fila
4. **Crear nueva**: Hacer clic en "+ Nueva Cotización"
   - Buscar cliente (escribir al menos 2 caracteres)
   - Agregar productos uno por uno
   - Configurar descuentos e IVA
   - Hacer clic en "Crear Cotización"
5. **Cancelar**: Desde el modal de detalle, hacer clic en "Cancelar Cotización" e ingresar motivo

---

## 📦 Dependencias del Backend

El componente consume las APIs del controlador:
- **Controlador**: `AdmDocumentosController.cs`
- **Servicio**: `AdmDocumentosService.cs`
- **Repositorio**: `AdmDocumentosRepository.cs`
- **Base de datos**: SQL Server (Adminpaq Legacy - adCABS2016)

---

## 🧪 Testing

### Backend
- **Tests unitarios**: `AdmDocumentosControllerTests.cs`
- **Cobertura**: 21 tests (100% pass rate)
- **Archivo de documentación**: `README_TESTS_COTIZACIONES_LEGACY.md`

### Frontend
- **Servidor de desarrollo**: `ng serve` en puerto 4200
- **Build de producción**: ✅ Compilación exitosa
- **Tamaño del componente**: ~137 KB (lazy loaded)

---

## 📝 Notas Técnicas

### Signals (Angular 17+)
El componente usa signals para estado reactivo:
```typescript
resumenTotal = signal<number>(0);
cotizaciones = signal<CotizacionLegacyResponse[]>([]);
loading = signal<boolean>(false);
```

### Observables y RxJS
Todas las peticiones HTTP retornan `Observable<T>` con:
- Operadores `map()` para logging
- Operador `catchError()` para manejo de errores
- Logs descriptivos con emojis en consola

### Tailwind CSS v4
- Clases inline directamente en HTML
- No se usan directivas `@apply` en TypeScript
- Gradientes y animaciones CSS modernas

---

## ✨ Estado Final

- ✅ **Todas las APIs integradas**
- ✅ **Componente funcional y completo**
- ✅ **Menú configurado correctamente**
- ✅ **Widget de resumen implementado**
- ✅ **Compilación exitosa**
- ✅ **Servidor corriendo en puerto 4200**

---

## 📚 Referencias

- **Documentación del servicio**: Ver comentarios JSDoc en `cotizacion-legacy.service.ts`
- **Interfaces**: Ver `cotizacion-legacy.interface.ts` para estructura de datos
- **Ejemplos de uso**: Comentarios en el servicio incluyen ejemplos completos
- **Tests backend**: `back_cabs/Tests/AdmDocumentosControllerTests.cs`

---

**Fecha**: 19 de Noviembre, 2025  
**Desarrollado por**: GitHub Copilot  
**Stack**: Angular 17+ Standalone, .NET 8.0, SQL Server, Tailwind CSS v4
