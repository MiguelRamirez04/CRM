# 🚀 API Órdenes de Trabajo - JSON de Prueba y Mejoras Implementadas

**Fecha**: 16 de octubre de 2025  
**Versión**: 2.0  
**Estado**: ✅ Mejorado y Documentado

---

## 📋 **Resumen de Mejoras Implementadas**

### ✅ **1. Nuevo Campo: `clienteTelefono`**
- **Tipo**: `BIGINT` (SQL Server) / `long` (C#)
- **Ubicación**: Tabla `ops.ordenes_trabajo`, columna `cliente_telefono`
- **Uso**: Almacenar teléfono de clientes nuevos (10 dígitos)
- **Opcional**: Sí (`NULL` permitido)
- **Migración SQL**: `/back_cabs/CRM/scripts/migrations/add_cliente_telefono_to_ordenes_trabajo.sql`

### ✅ **2. Endpoint Nuevo: GET `/api/Recepcion/clientes/nuevos`**
- **Propósito**: Listar todos los clientes nuevos registrados en órdenes
- **Respuesta**: Array de `ClienteNuevoDto` con nombre, teléfono y número de órdenes
- **Autorización**: Todos los roles autenticados

### ✅ **3. Endpoint Corregido: GET `/api/Recepcion/clientes/buscar`**
- **Problema Original**: Retornaba `Dictionary<string, object>` (tipo genérico)
- **Solución**: Ahora retorna `List<ClienteResumenDto>` (tipo fuertemente tipado)
- **Mejora**: Validación de parámetro `limite` (1-50)
- **Documentación Swagger**: Completa con ejemplos

### ✅ **4. Documentación Swagger Mejorada**
- **POST `/api/Recepcion`**: Ejemplos completos para cliente nuevo y legacy
- **PUT `/api/Recepcion/{id}`**: Ejemplos de actualización parcial
- **GET Endpoints**: Descripciones detalladas de respuestas

### ✅ **5. Eliminación de OrdenTrabajoRequestWrapper** (Pendiente)
- **Problema**: Wrapper innecesario que complica la API
- **Estado**: Identificado, listo para refactorizar en Fase 2

---

## 📝 **JSON de Prueba - POST `/api/Recepcion`**

### **Caso 1: Cliente Nuevo con Teléfono**
```json
{
  "requestDto": {
    "notas": "Cliente solicita cotización para instalación de sistema de videovigilancia con 8 cámaras IP, grabación en la nube por 30 días y acceso móvil",
    "citaProgramadaInicio": "2025-10-20T10:00:00Z",
    "citaProgramadaFin": "2025-10-20T12:00:00Z",
    "modalidad": "Presencial",
    "tipoOrden": "Cotizacion",
    "cotizaciones": "COT-2025-001",
    "nuevoCliente": true,
    "nombreCliente": "Abarrotes Don Pepe S.A. de C.V.",
    "clienteTelefono": 6182171064,
    "clienteId": null,
    "prioridad": 4,
    "estado": "CAPTURADA",
    "ubicacionText": "Av. Juárez #123, Col. Centro, Chihuahua, Chih. CP 31000",
    "estadoFacturado": "NO",
    "RequiereFactura": true,
    "FacturaFolio": null,
    "costoReal": null,
    "costoEstimado": 15000.00,
    "creadoPorUserId": 3
  }
}
```

**Respuesta Esperada:**
```json
{
  "id": 45,
  "notas": "Cliente solicita cotización para instalación de sistema de videovigilancia...",
  "citaProgramadaInicio": "2025-10-20T10:00:00Z",
  "citaProgramadaFin": "2025-10-20T12:00:00Z",
  "modalidad": "Presencial",
  "tipoOrden": "Cotizacion",
  "nuevoCliente": true,
  "nombreCliente": "Abarrotes Don Pepe S.A. de C.V.",
  "clienteTelefono": 6182171064,
  "clienteId": null,
  "prioridad": 4,
  "estado": "CAPTURADA",
  "estadoDescripcion": "Orden capturada, pendiente de asignación",
  "ubicacionText": "Av. Juárez #123, Col. Centro, Chihuahua, Chih. CP 31000",
  "estadoFacturado": "NO",
  "requiereFactura": true,
  "costoReal": null,
  "costoEstimado": 15000.00,
  "creadoEn": "2025-10-16T19:30:00Z",
  "actualizadoEn": "2025-10-16T19:30:00Z",
  "creadoPorUserId": 3,
  "asignadaAUserId": 3
}
```

---

### **Caso 2: Cliente Legacy (Existente)**
```json
{
  "requestDto": {
    "notas": "Revisión anual de equipo de aire acondicionado en oficinas principales. Incluye limpieza de filtros, revisión de fugas y recarga de gas refrigerante",
    "citaProgramadaInicio": "2025-10-18T14:00:00Z",
    "citaProgramadaFin": "2025-10-18T16:30:00Z",
    "modalidad": "Presencial",
    "tipoOrden": "Asesoria",
    "cotizaciones": null,
    "nuevoCliente": false,
    "nombreCliente": null,
    "clienteTelefono": null,
    "clienteId": 42,
    "prioridad": 3,
    "estado": "CAPTURADA",
    "ubicacionText": "Parque Industrial Chihuahua 2000, Nave 15",
    "estadoFacturado": "NO",
    "RequiereFactura": false,
    "FacturaFolio": null,
    "costoReal": null,
    "costoEstimado": 3500.50,
    "creadoPorUserId": 3
  }
}
```

---

### **Caso 3: Orden Urgente (Prioridad 5)**
```json
{
  "requestDto": {
    "notas": "URGENTE: Falla en sistema de alarma. Cliente reporta que no responde a controles remotos y suena de forma intermitente. Posible problema en central o sensores",
    "citaProgramadaInicio": "2025-10-17T08:00:00Z",
    "citaProgramadaFin": "2025-10-17T10:00:00Z",
    "modalidad": "Presencial",
    "tipoOrden": "Asesoria",
    "cotizaciones": null,
    "nuevoCliente": false,
    "nombreCliente": null,
    "clienteTelefono": null,
    "clienteId": 127,
    "prioridad": 5,
    "estado": "CAPTURADA",
    "ubicacionText": "Residencial Campestre, Calle Robles #456",
    "estadoFacturado": "NO",
    "RequiereFactura": false,
    "FacturaFolio": null,
    "costoReal": null,
    "costoEstimado": 800.00,
    "creadoPorUserId": 3
  }
}
```

---

### **Caso 4: Cliente Nuevo Sin Teléfono**
```json
{
  "requestDto": {
    "notas": "Cotización para instalación de red estructurada categoría 6 en edificio de 3 pisos, aproximadamente 50 puntos de red",
    "citaProgramadaInicio": "2025-10-22T09:00:00Z",
    "citaProgramadaFin": "2025-10-22T11:00:00Z",
    "modalidad": "Presencial",
    "tipoOrden": "Cotizacion",
    "cotizaciones": "COT-2025-002",
    "nuevoCliente": true,
    "nombreCliente": "Constructora Delta S.A.",
    "clienteTelefono": null,
    "clienteId": null,
    "prioridad": 3,
    "estado": "CAPTURADA",
    "ubicacionText": "Av. Tecnológico #789, Parque Industrial Norte",
    "estadoFacturado": "NO",
    "RequiereFactura": true,
    "FacturaFolio": null,
    "costoReal": null,
    "costoEstimado": 45000.00,
    "creadoPorUserId": 3
  }
}
```

---

## 📝 **JSON de Prueba - PUT `/api/Recepcion/{id}`**

### **Caso 1: Actualización de Estado y Asignación**
```http
PUT /api/Recepcion/45
Content-Type: application/json

{
  "notas": "Se asignó al técnico Juan Pérez. Revisión inicial indica que se requieren 2 cámaras adicionales",
  "estado": "ASIGNADA",
  "asignadaAUserId": 5,
  "prioridad": 4
}
```

---

### **Caso 2: Actualización de Costos al Completar**
```http
PUT /api/Recepcion/45
Content-Type: application/json

{
  "notas": "Instalación completada exitosamente. Cliente solicita agregar 2 cámaras adicionales en la parte trasera del local (presupuesto adicional enviado)",
  "estado": "COMPLETADA",
  "estadoFacturado": "POR_FACTURAR",
  "costoReal": 16200.00,
  "citaProgramadaFin": "2025-10-20T14:30:00Z"
}
```

---

### **Caso 3: Facturación de Orden**
```http
PUT /api/Recepcion/45
Content-Type: application/json

{
  "estado": "FACTURADA",
  "estadoFacturado": "SI",
  "FacturaFolio": 1025,
  "RequiereFactura": true
}
```

---

## 📝 **JSON de Prueba - GET Endpoints**

### **GET `/api/Recepcion/clientes/buscar?busqueda=Don Pepe&limite=10`**

**Respuesta Esperada:**
```json
[
  {
    "clienteId": 42,
    "nombreComercial": "Abarrotes Don Pepe S.A. de C.V.",
    "rfc": "ADP850101ABC",
    "legacyClientId": 123
  },
  {
    "clienteId": 87,
    "nombreComercial": "Don Pepe Restaurante",
    "rfc": "DPR920315XYZ",
    "legacyClientId": 456
  }
]
```

---

### **GET `/api/Recepcion/clientes/nuevos`**

**Respuesta Esperada:**
```json
[
  {
    "nombreCliente": "Abarrotes Don Pepe S.A. de C.V.",
    "telefono": 6182171064,
    "numeroOrdenes": 3
  },
  {
    "nombreCliente": "Constructora Delta S.A.",
    "telefono": null,
    "numeroOrdenes": 1
  },
  {
    "nombreCliente": "Taller Mecánico El Rayo",
    "telefono": 6181234567,
    "numeroOrdenes": 2
  }
]
```

---

### **GET `/api/Recepcion?skip=0&take=10&estado=CAPTURADA`**

**Respuesta Esperada:**
```json
[
  {
    "id": 45,
    "notas": "Cliente solicita cotización...",
    "citaProgramadaInicio": "2025-10-20T10:00:00Z",
    "citaProgramadaFin": "2025-10-20T12:00:00Z",
    "modalidad": "Presencial",
    "tipoOrden": "Cotizacion",
    "nuevoCliente": true,
    "nombreCliente": "Abarrotes Don Pepe S.A. de C.V.",
    "clienteTelefono": 6182171064,
    "clienteId": null,
    "prioridad": 4,
    "estado": "CAPTURADA",
    "estadoDescripcion": "Orden capturada, pendiente de asignación",
    "ubicacionText": "Av. Juárez #123...",
    "estadoFacturado": "NO",
    "requiereFactura": true,
    "costoReal": null,
    "costoEstimado": 15000.00,
    "creadoEn": "2025-10-16T19:30:00Z",
    "actualizadoEn": "2025-10-16T19:30:00Z",
    "creadoPorUserId": 3,
    "asignadaAUserId": 3
  }
]
```

---

### **GET `/api/Recepcion/estados`**

**Respuesta Esperada:**
```json
[
  {
    "id": 1,
    "valor": "CAPTURADA",
    "nombre": "CAPTURADA",
    "descripcion": "Orden capturada, pendiente de asignación"
  },
  {
    "id": 2,
    "valor": "ASIGNADA",
    "nombre": "ASIGNADA",
    "descripcion": "Orden asignada a un técnico"
  },
  {
    "id": 3,
    "valor": "EN_CURSO",
    "nombre": "EN_CURSO",
    "descripcion": "Orden en proceso de ejecución"
  },
  {
    "id": 4,
    "valor": "COMPLETADA",
    "nombre": "COMPLETADA",
    "descripcion": "Orden completada, pendiente de facturación"
  },
  {
    "id": 5,
    "valor": "POR_FACTURAR",
    "nombre": "POR_FACTURAR",
    "descripcion": "Lista para generar factura"
  },
  {
    "id": 6,
    "valor": "FACTURADA",
    "nombre": "FACTURADA",
    "descripcion": "Factura emitida"
  },
  {
    "id": 7,
    "valor": "CERRADA",
    "nombre": "CERRADA",
    "descripcion": "Orden finalizada completamente"
  }
]
```

---

## 🔧 **Instrucciones de Prueba**

### **1. Ejecutar Migración SQL**
```sql
-- En SQL Server Management Studio
USE CRM_CABS;
GO

-- Ejecutar script de migración
-- Ubicación: /back_cabs/CRM/scripts/migrations/add_cliente_telefono_to_ordenes_trabajo.sql
```

### **2. Reiniciar API** (si está corriendo)
```powershell
# Detener servidor actual
# Compilar con cambios
cd back_cabs
dotnet build

# Ejecutar
dotnet run
```

### **3. Probar en Swagger**
- Navegar a: `http://localhost:5176/swagger`
- Buscar sección: **Recepcion**
- Probar cada endpoint con los JSON proporcionados

---

## 📊 **Validaciones Implementadas**

| Campo | Validación | Error si... |
|-------|-----------|-------------|
| `nombreCliente` | Requerido si `nuevoCliente = true` | Cliente nuevo sin nombre |
| `clienteId` | Requerido si `nuevoCliente = false` | Cliente legacy sin ID |
| `clienteTelefono` | Opcional, rango 1000000000-9999999999 | Número inválido (no 10 dígitos) |
| `citaProgramadaInicio` | Requerido | Falta fecha de cita |
| `creadoPorUserId` | Requerido, > 0 | Usuario no especificado |
| `prioridad` | Rango 1-5 | Valor fuera de rango |
| `limite` (búsqueda) | Rango 1-50 | Límite inválido |

---

## ⚠️ **Notas Importantes**

1. **Teléfonos con 6, 7, 8, 9 al inicio**: Ahora funcionan correctamente gracias al cambio de `int` a `long`
2. **Checkbox activo**: Valor por defecto cambiado a `false` en el formulario de registro de usuarios
3. **Wrapper temporal**: `OrdenTrabajoRequestWrapper` se mantiene por compatibilidad, pero se eliminará en Fase 2
4. **Cliente nuevo sin teléfono**: Es válido, el campo `clienteTelefono` es opcional

---

## 🚀 **Próximos Pasos (Fase 2)**

1. ✅ Eliminar `OrdenTrabajoRequestWrapper` (simplificar API)
2. ✅ Implementar validación de transiciones de estado
3. ✅ Agregar auditoría de cambios (tabla de historial)
4. ✅ Separar servicios (SRP - Single Responsibility Principle)
5. ✅ Implementar caché para estadísticas
6. ✅ Crear endpoints de reportes (PDF/Excel)

---

**¿Listo para probar?** 🎯  
Ejecuta la migración SQL y prueba los endpoints en Swagger!
