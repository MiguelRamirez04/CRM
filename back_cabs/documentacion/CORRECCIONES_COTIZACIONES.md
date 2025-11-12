# 🔧 CORRECCIONES COMPLETAS AL COMPONENTE COTIZACIONES

## 📋 **Resumen de Problemas Corregidos**

### **Problema 1: Validación de Llaves Foráneas (FK)**
**Antes:** No se validaba si los IDs de `DocumentoDeId`, `ConceptoDocumentoId`, `ClienteProveedorId`, `AgenteId` existían.
**Ahora:** Se validan todas las FK antes de crear/actualizar, lanzando excepciones específicas.

### **Problema 2: Búsqueda por Estado Retornaba Vacío**
**Antes:** 
- Endpoint: `GET /api/Cotizaciones/estado/{estado}` 
- Recibía string ("CANCELADO", "AFECTADO") pero no distinguía correctamente
- Retornaba arreglo vacío

**Ahora:**
- Endpoint: `GET /api/Cotizaciones/estado/{campo}/{valor}`
- Recibe campo específico y valor numérico (0 o 1)
- Ejemplo: `GET /api/Cotizaciones/estado/cancelado/1` (cotizaciones canceladas)
- Ejemplo: `GET /api/Cotizaciones/estado/afectado/0` (cotizaciones no afectadas)

**Campos disponibles:**
- `cancelado` (0 = No cancelado, 1 = Cancelado)
- `afectado` (0 = No afectado, 1 = Afectado)
- `impreso` (0 = No impreso, 1 = Impreso)
- `usacliente` (0 = No usa cliente, 1 = Usa cliente)

### **Problema 3: Búsqueda por Cliente Retornaba Vacío**
**Antes:** 
- Endpoint: `GET /api/Cotizaciones/cliente/{cliente}`
- Buscaba por string en RazonSocial (campo que puede ser nulo)
- Retornaba arreglo vacío

**Ahora:**
- Endpoint: `GET /api/Cotizaciones/cliente/{clienteId}`
- Busca directamente por `ClienteProveedorId` (int)
- Ejemplo: `GET /api/Cotizaciones/cliente/123`

### **Problema 4: Manejo de Excepciones Genérico**
**Antes:** Todas las excepciones retornaban error 500 genérico.

**Ahora:** Excepciones específicas con códigos HTTP apropiados:

#### **Excepción 1: FK No Encontrada (400 Bad Request)**
```json
{
  "error": "FOREIGN_KEY_NOT_FOUND",
  "message": "El DocumentoDeId con ID 0 no existe en la base de datos.",
  "campo": "DocumentoDeId",
  "valorId": 0
}
```

#### **Excepción 2: Registro Duplicado (409 Conflict)**
```json
{
  "error": "DUPLICATE_RECORD",
  "message": "Ya existe un registro con Folio = '12345'.",
  "campo": "Folio",
  "valor": "12345"
}
```

#### **Excepción 3: Acceso Denegado (403 Forbidden)**
```json
{
  "error": "ACCESS_DENIED",
  "message": "No se puede acceder al recurso solicitado."
}
```

---

## 🔄 **Cambios en Endpoints**

### **Endpoints Modificados:**

| Método | Antes | Ahora | Descripción |
|--------|-------|-------|-------------|
| GET | `/api/Cotizaciones/estado/{estado}` | `/api/Cotizaciones/estado/{campo}/{valor}` | Filtrado por campo y valor específico |
| GET | `/api/Cotizaciones/cliente/{cliente}` | `/api/Cotizaciones/cliente/{clienteId}` | Búsqueda por ID numérico |

### **Endpoints Sin Cambios:**

- `GET /api/Cotizaciones` - Obtiene todas las cotizaciones
- `GET /api/Cotizaciones/{id}` - Obtiene cotización por ID
- `POST /api/Cotizaciones` - Crea nueva cotización (con validaciones mejoradas)
- `PUT /api/Cotizaciones/{id}` - Actualiza cotización
- `DELETE /api/Cotizaciones/{id}` - Elimina cotización

---

## 📝 **Ejemplos de Uso Corregidos**

### **1. Crear Cotización con Validación**

**Request:**
```http
POST /api/Cotizaciones
Content-Type: application/json

{
  "documentoDeId": 1,
  "conceptoDocumentoId": 5,
  "clienteProveedorId": 123,
  "agenteId": 10,
  "documentoOrigenId": null,
  "serieDocumento": "COT",
  "folio": 1001,
  "fecha": "2025-11-12T19:23:40.554Z",
  "fechaVencimiento": "2025-12-12T19:23:40.554Z",
  "fechaEntregaRecepcion": "2025-11-15T19:23:40.554Z",
  "razonSocial": "Empresa ABC S.A. de C.V.",
  "rfc": "ABC123456XYZ",
  "referencia": "REF-2025-001",
  "observaciones": "Cotización para proyecto nuevo",
  "naturaleza": 1,
  "usaCliente": 1,
  "afectado": 0,
  "impreso": 0,
  "cancelado": 0,
  "neto": 10000.00,
  "impuesto1": 1600.00,
  "descuentoMovimiento": 0.00,
  "total": 11600.00,
  "totalUnidades": 5
}
```

**Respuestas Posibles:**

✅ **201 Created** - Si todo es válido
```json
{
  "id": 50,
  "serieDocumento": "COT",
  "folio": 1001,
  "fecha": "2025-11-12T19:23:40.554Z",
  ...
}
```

❌ **400 Bad Request** - Si ClienteProveedorId no existe
```json
{
  "error": "FOREIGN_KEY_NOT_FOUND",
  "message": "El ClienteProveedorId con ID 123 no existe en la base de datos.",
  "campo": "ClienteProveedorId",
  "valorId": 123
}
```

### **2. Obtener Cotizaciones Canceladas**

**Request:**
```http
GET /api/Cotizaciones/estado/cancelado/1
```

**Response:**
```json
[
  {
    "id": 10,
    "folio": 500,
    "cancelado": 1,
    ...
  },
  {
    "id": 25,
    "folio": 750,
    "cancelado": 1,
    ...
  }
]
```

### **3. Obtener Cotizaciones No Afectadas**

**Request:**
```http
GET /api/Cotizaciones/estado/afectado/0
```

### **4. Obtener Cotizaciones por Cliente**

**Request:**
```http
GET /api/Cotizaciones/cliente/123
```

**Response:**
```json
[
  {
    "id": 45,
    "clienteProveedorId": 123,
    "razonSocial": "Empresa ABC S.A. de C.V.",
    ...
  }
]
```

---

## 📂 **Archivos Creados/Modificados**

### **Nuevos Archivos:**
- `crm/Core/Exceptions/CotizacionExceptions.cs` - Excepciones personalizadas

### **Archivos Modificados:**
- `crm/controllers/Recepcion/CotizacionesController.cs`
- `crm/services/Recepcion/CotizacionService.cs`
- `crm/repositories/recepcion/CotizacionRepository.cs`
- `crm/Interfaces/Recepcion/ICotizacionRepository.cs`

---

## ✅ **Validaciones Implementadas**

### **En Creación (POST):**
1. ✅ Validación de ModelState (DataAnnotations)
2. ✅ Validación de DocumentoDeId existe
3. ✅ Validación de ConceptoDocumentoId existe
4. ✅ Validación de ClienteProveedorId existe (contra tabla catalog_clientes)
5. ✅ Validación de AgenteId existe (contra tabla usuarios_auth)

### **En Consultas (GET):**
1. ✅ Validación de parámetros (campo y valor para estado)
2. ✅ Validación de ID numérico para cliente
3. ✅ Retorno de arreglo vacío si no hay resultados (no error)

---

## 🚀 **Próximos Pasos Sugeridos**

1. **Validación de Duplicados:** Agregar validación para evitar Folios duplicados
2. **Endpoint de Orden:** Descomentar y corregir el endpoint `GET /api/Cotizaciones/orden/{ordenId}`
3. **Paginación:** Agregar paginación a los endpoints que retornan listas
4. **Búsqueda Combinada:** Endpoint para filtrar por múltiples criterios
5. **Validación de Contpaqi:** Integrar con API de Contpaqi para validar DocumentoDeId y ConceptoDocumentoId

---

## 📞 **Soporte**

Para cualquier duda o problema con las nuevas implementaciones, revisar:
- Logs del servidor (buscar por "Cotización")
- Códigos de error específicos en las respuestas
- Validar que los IDs de FK existan antes de enviar requests

---

**Fecha de Correcciones:** 12 de Noviembre, 2025
**Estado:** ✅ Completado y Probado
