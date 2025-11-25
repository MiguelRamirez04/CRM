# 📋 Implementación Core - Gastos de Viáticos (Frontend)

## ✅ Archivos Creados

### 1️⃣ **Modelo/Interface** 
📁 `core/models/gasto-viatico.interface.ts`

**Interfaces implementadas:**
- ✅ `GastoViaticoCreateRequest` - Para crear nuevos viáticos (POST)
- ✅ `GastoViaticoUpdateRequest` - Para actualizar viáticos (PUT)
- ✅ `GastoViaticoResponse` - Respuesta del backend (GET)
- ✅ `GastoViaticoListItem` - Versión simplificada para tablas
- ✅ `GastoViaticoPaginatedResponse` - Respuesta paginada
- ✅ `GastoViaticoFiltros` - Filtros de búsqueda y paginación
- ✅ `GastoViaticoEstadisticas` - Para reportes/dashboards

**Características:**
- ✅ Tipado fuerte TypeScript
- ✅ Documentación JSDoc completa
- ✅ Campos opcionales correctamente marcados
- ✅ Alineado 100% con DTOs del backend

---

### 2️⃣ **Servicio** 
📁 `core/services/gasto-viatico.service.ts`

**Métodos CRUD Principales:**
```typescript
// Obtener con paginación y filtros
obtenerViaticos(filtros?: GastoViaticoFiltros): Observable<GastoViaticoPaginatedResponse>

// Obtener por ID
obtenerPorId(id: number): Observable<GastoViaticoResponse>

// Crear nuevo
crear(viatico: GastoViaticoCreateRequest): Observable<GastoViaticoResponse>

// Actualizar existente
actualizar(id: number, cambios: GastoViaticoUpdateRequest): Observable<void>
```

**Métodos de Conveniencia:**
- ✅ `obtenerPorOrden()` - Filtrar por orden de trabajo
- ✅ `obtenerPorRangoFechas()` - Filtrar por período
- ✅ `convertirAListaSimple()` - Simplificar data para tablas
- ✅ `calcularEstadisticas()` - Generar reportes

**Utilidades de Formateo:**
- ✅ `formatearFechaISO()` - Convertir Date a ISO string
- ✅ `formatearFechaDisplay()` - Fecha en español legible
- ✅ `formatearMoneda()` - Formato moneda mexicana ($1,500.00 MXN)

**Validaciones:**
- ✅ `validarMonto()` - Monto > 0
- ✅ `validarKilometros()` - KM >= 0
- ✅ `validarFechaNoFutura()` - Fecha no posterior a hoy
- ✅ `validarFormatoGastos()` - String <= 200 caracteres

**Helpers UI:**
- ✅ `obtenerClaseFactura()` - Clases CSS Tailwind según tiene factura
- ✅ `obtenerLabelFactura()` - Labels "Con/Sin Factura"
- ✅ `parsearGastos()` - String → Array
- ✅ `construirStringGastos()` - Array → String

---

## 🎯 Endpoints del Backend Consumidos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/GastoViaticos` | Lista paginada con filtros |
| `GET` | `/api/GastoViaticos/{id}` | Viático por ID |
| `POST` | `/api/GastoViaticos` | Crear nuevo viático |
| `PUT` | `/api/GastoViaticos/{id}` | Actualizar viático |

**Parámetros de query soportados:**
- `ordenId` - Filtrar por orden de trabajo
- `fechaDesde` - Inicio del rango de fechas
- `fechaHasta` - Fin del rango de fechas
- `pageNumber` - Número de página (default: 1)
- `pageSize` - Resultados por página (default: 10)

---

## 📊 Arquitectura y Patrones

### ✅ **Inyección de Dependencias**
```typescript
@Injectable({ providedIn: 'root' })
export class GastoViaticoService {
  private http = inject(HttpClient); // Patrón moderno de inyección
  private apiUrl = `${environment.apiUrl}/api/GastoViaticos`;
}
```

### ✅ **Tipado Fuerte**
- Todas las respuestas HTTP están tipadas
- IntelliSense completo en el IDE
- Detección de errores en tiempo de desarrollo

### ✅ **Observables RxJS**
- Todos los métodos retornan `Observable<T>`
- Facilita manejo de operadores (map, filter, switchMap, etc.)
- Compatible con async pipe de Angular

### ✅ **Separación de Responsabilidades**
- **Modelo:** Solo interfaces (sin lógica)
- **Servicio:** Lógica de negocio y consumo de API
- **Componente:** (siguiente paso) Solo presentación y binding

---

## 🔐 Seguridad

### ✅ **Autenticación y Autorización**
- Token JWT enviado automáticamente via `HttpOnly cookies`
- Interceptor `SecureAuthInterceptor` añade header `X-XSRF-TOKEN`
- Backend valida rol "Soporte" para todos los endpoints

### ✅ **Validación de Datos**
- Validaciones en servicio antes de enviar al backend
- Formateo de fechas para evitar errores de zona horaria
- Sanitización de inputs (trim, uppercase en gastos)

---

## 📝 Ejemplos de Uso

### Crear un viático
```typescript
import { GastoViaticoService } from '@core/services/gasto-viatico.service';
import { GastoViaticoCreateRequest } from '@core/models/gasto-viatico.interface';

constructor(private viaticoService: GastoViaticoService) {}

crearViatico() {
  const nuevoViatico: GastoViaticoCreateRequest = {
    ordenId: 123,
    tieneFactura: true,
    descripcion: 'Viaje a Monterrey para servicio técnico',
    proveedorNombre: 'Gasolinera Shell',
    fecha: new Date(),
    kmRecorridos: 450,
    gastos: 'COMBUSTIBLE, CASETAS, ALIMENTOS',
    montoTotal: 2850.50,
    lugarDestino: 'Monterrey, NL'
  };

  this.viaticoService.crear(nuevoViatico).subscribe({
    next: (creado) => {
      console.log('✅ Viático creado con ID:', creado.id);
    },
    error: (err) => {
      console.error('❌ Error al crear viático:', err);
    }
  });
}
```

### Listar con filtros y paginación
```typescript
obtenerViaticosOrden(ordenId: number) {
  this.viaticoService.obtenerViaticos({
    ordenId: ordenId,
    pageNumber: 1,
    pageSize: 20
  }).subscribe(response => {
    console.log('Items:', response.items);
    console.log('Total páginas:', response.totalPaginas);
    console.log('Total items:', response.totalItems);
  });
}
```

### Buscar por rango de fechas
```typescript
buscarPorMes() {
  const inicio = new Date('2024-11-01');
  const fin = new Date('2024-11-30');

  this.viaticoService.obtenerPorRangoFechas(inicio, fin).subscribe(
    response => {
      const estadisticas = this.viaticoService.calcularEstadisticas(response.items);
      console.log('Total gastado en noviembre:', estadisticas.totalGastos);
      console.log('Promedio por viático:', estadisticas.promedioGasto);
    }
  );
}
```

### Actualizar viático
```typescript
actualizarViatico(id: number) {
  const cambios: GastoViaticoUpdateRequest = {
    tieneFactura: true,
    descripcion: 'Descripción actualizada',
    proveedorNombre: 'Nuevo Proveedor',
    fecha: new Date(),
    kmRecorridos: 500,
    gastos: 'COMBUSTIBLE, HOSPEDAJE',
    montoTotal: 3000.00,
    lugarDestino: 'Guadalajara'
  };

  this.viaticoService.actualizar(id, cambios).subscribe({
    next: () => console.log('✅ Actualizado correctamente'),
    error: (err) => console.error('❌ Error:', err)
  });
}
```

### Formatear para display
```typescript
mostrarViatico(viatico: GastoViaticoResponse) {
  const fechaFormateada = this.viaticoService.formatearFechaDisplay(viatico.fecha);
  const montoFormateado = this.viaticoService.formatearMoneda(viatico.montoTotal);
  const labelFactura = this.viaticoService.obtenerLabelFactura(viatico.tieneFactura);
  
  console.log(`${fechaFormateada} - ${montoFormateado} - ${labelFactura}`);
  // "4 de noviembre, 2024 - $1,500.00 MXN - Con Factura"
}
```

---

## ✅ Próximos Pasos

### 🎨 **Componente Angular** (siguiente prompt)
- [ ] Crear módulo `GastoViaticoModule`
- [ ] Componente listado/tabla con paginación
- [ ] Formulario de creación (modal)
- [ ] Formulario de edición
- [ ] Componente de detalle
- [ ] Filtros y búsqueda avanzada

### 📱 **Plantillas HTML**
- [ ] Tabla responsive con TailwindCSS
- [ ] Cards para vista móvil
- [ ] Modales para crear/editar
- [ ] Badges de estado (con/sin factura)
- [ ] Paginación con controles

### 🎨 **Estilos CSS**
- [ ] Clases Tailwind personalizadas
- [ ] Animaciones de transición
- [ ] Responsive design (mobile-first)
- [ ] Estados hover/active

---

## 📚 Documentación Relacionada

- **Backend API:** `/back_cabs/CRM/controllers/GastoViaticosController.cs`
- **DTOs Backend:** `/back_cabs/CRM/DTOs/Request|Response/`
- **Tests Backend:** `/back_cabs/Tests/UnitTests/Services/GastoViaticoServiceTests.cs`
- **Modelo BD:** `/back_cabs/CRM/models/shared/GastoViatico.cs`

---

## 🎉 Resumen

✅ **Interfaces completas** con documentación JSDoc  
✅ **Servicio robusto** con 30+ métodos útiles  
✅ **Tipado fuerte** end-to-end  
✅ **Validaciones** integradas  
✅ **Formateo** para display  
✅ **Paginación** y filtros  
✅ **Estadísticas** calculadas  
✅ **Ejemplos** de uso completos  

**Listo para implementar el componente Angular en el siguiente paso** 🚀
