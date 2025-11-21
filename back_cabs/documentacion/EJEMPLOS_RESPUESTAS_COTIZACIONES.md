# 📬 EJEMPLOS DE RESPUESTAS - API COTIZACIONES

## 🟢 RESPUESTAS EXITOSAS

### **1. GET /api/Cotizaciones - Obtener Todas**

**Status:** `200 OK`

```json
[
  {
    "id": 1,
    "serieDocumento": "COT",
    "folio": 1001,
    "fecha": "2025-11-12T14:30:00Z",
    "fechaVencimiento": "2025-12-12T00:00:00Z",
    "fechaEntregaRecepcion": "2025-11-15T00:00:00Z",
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
    "pendiente": 11600.00,
    "totalUnidades": 5
  },
  {
    "id": 2,
    "serieDocumento": "COT",
    "folio": 1002,
    "fecha": "2025-11-10T10:15:00Z",
    "fechaVencimiento": "2025-12-10T00:00:00Z",
    "fechaEntregaRecepcion": null,
    "razonSocial": "Empresa XYZ Ltda.",
    "rfc": "XYZ987654ABC",
    "referencia": "REF-2025-002",
    "observaciones": null,
    "naturaleza": 1,
    "usaCliente": 1,
    "afectado": 1,
    "impreso": 1,
    "cancelado": 0,
    "neto": 5000.00,
    "impuesto1": 800.00,
    "descuentoMovimiento": 200.00,
    "total": 5600.00,
    "pendiente": 0.00,
    "totalUnidades": 3
  }
]
```

---

### **2. GET /api/Cotizaciones/1 - Obtener por ID**

**Status:** `200 OK`

```json
{
  "id": 1,
  "serieDocumento": "COT",
  "folio": 1001,
  "fecha": "2025-11-12T14:30:00Z",
  "fechaVencimiento": "2025-12-12T00:00:00Z",
  "fechaEntregaRecepcion": "2025-11-15T00:00:00Z",
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
  "pendiente": 11600.00,
  "totalUnidades": 5
}
```

---

### **3. GET /api/Cotizaciones/estado/cancelado/1 - Canceladas**

**Status:** `200 OK`

```json
[
  {
    "id": 15,
    "serieDocumento": "COT",
    "folio": 2050,
    "fecha": "2025-10-20T09:00:00Z",
    "fechaVencimiento": "2025-11-20T00:00:00Z",
    "fechaEntregaRecepcion": null,
    "razonSocial": "Cliente Cancelado S.A.",
    "rfc": "CAN123456789",
    "referencia": "REF-CANCEL-001",
    "observaciones": "Cotización cancelada por el cliente",
    "naturaleza": 1,
    "usaCliente": 1,
    "afectado": 0,
    "impreso": 1,
    "cancelado": 1,
    "neto": 25000.00,
    "impuesto1": 4000.00,
    "descuentoMovimiento": 0.00,
    "total": 29000.00,
    "pendiente": 29000.00,
    "totalUnidades": 10
  }
]
```

---

### **4. GET /api/Cotizaciones/estado/cancelado/0 - No Canceladas**

**Status:** `200 OK`

```json
[
  {
    "id": 1,
    "cancelado": 0,
    // ...resto de campos
  },
  {
    "id": 2,
    "cancelado": 0,
    // ...resto de campos
  }
]
```

---

### **5. GET /api/Cotizaciones/cliente/123 - Por Cliente ID**

**Status:** `200 OK`

```json
[
  {
    "id": 5,
    "serieDocumento": "COT",
    "folio": 3001,
    "fecha": "2025-11-05T11:00:00Z",
    "razonSocial": "Cliente 123 Corp.",
    "rfc": "CLI123456789",
    "clienteProveedorId": 123,
    "total": 15000.00,
    // ...resto de campos
  },
  {
    "id": 12,
    "serieDocumento": "COT",
    "folio": 3050,
    "fecha": "2025-11-08T15:30:00Z",
    "razonSocial": "Cliente 123 Corp.",
    "rfc": "CLI123456789",
    "clienteProveedorId": 123,
    "total": 8500.00,
    // ...resto de campos
  }
]
```

---

### **6. POST /api/Cotizaciones - Crear Exitosa**

**Status:** `201 Created`

**Headers:**
```
Location: /api/Cotizaciones/25
```

**Body:**
```json
{
  "id": 25,
  "serieDocumento": "COT",
  "folio": 5001,
  "fecha": "2025-11-12T19:45:00Z",
  "fechaVencimiento": "2025-12-12T00:00:00Z",
  "fechaEntregaRecepcion": "2025-11-15T00:00:00Z",
  "razonSocial": "Nueva Empresa S.A.",
  "rfc": "NUE123456789",
  "referencia": "REF-NEW-001",
  "observaciones": "Cotización creada exitosamente",
  "naturaleza": 1,
  "usaCliente": 1,
  "afectado": 0,
  "impreso": 0,
  "cancelado": 0,
  "neto": 12000.00,
  "impuesto1": 1920.00,
  "descuentoMovimiento": 0.00,
  "total": 13920.00,
  "pendiente": 13920.00,
  "totalUnidades": 6
}
```

---

### **7. PUT /api/Cotizaciones/1 - Actualizar Exitosa**

**Status:** `200 OK`

```json
{
  "id": 1,
  "serieDocumento": "COT",
  "folio": 1001,
  "fecha": "2025-11-12T14:30:00Z",
  "fechaVencimiento": "2025-12-15T00:00:00Z",
  "fechaEntregaRecepcion": "2025-11-18T00:00:00Z",
  "razonSocial": "Empresa ABC S.A. de C.V. ACTUALIZADA",
  "rfc": "ABC123456XYZ",
  "referencia": "REF-2025-001-UPD",
  "observaciones": "Cotización actualizada con nuevos datos",
  "naturaleza": 1,
  "usaCliente": 1,
  "afectado": 0,
  "impreso": 1,
  "cancelado": 0,
  "neto": 15000.00,
  "impuesto1": 2400.00,
  "descuentoMovimiento": 500.00,
  "total": 16900.00,
  "pendiente": 16900.00,
  "totalUnidades": 8
}
```

---

### **8. DELETE /api/Cotizaciones/99 - Eliminar Exitosa**

**Status:** `204 No Content`

*(Sin body)*

---

## 🔴 RESPUESTAS DE ERROR

### **ERROR 1: FK No Encontrada - DocumentoDeId**

**Status:** `400 Bad Request`

**Request:**
```json
POST /api/Cotizaciones
{
  "documentoDeId": 0,
  "conceptoDocumentoId": 5,
  "clienteProveedorId": 10,
  "agenteId": 1,
  // ...resto de campos
}
```

**Response:**
```json
{
  "error": "FOREIGN_KEY_NOT_FOUND",
  "message": "El DocumentoDeId con ID 0 no existe en la base de datos.",
  "campo": "DocumentoDeId",
  "valorId": 0
}
```

---

### **ERROR 2: FK No Encontrada - ClienteProveedorId**

**Status:** `400 Bad Request`

**Request:**
```json
POST /api/Cotizaciones
{
  "documentoDeId": 1,
  "conceptoDocumentoId": 5,
  "clienteProveedorId": 99999,
  "agenteId": 1,
  // ...resto de campos
}
```

**Response:**
```json
{
  "error": "FOREIGN_KEY_NOT_FOUND",
  "message": "El ClienteProveedorId con ID 99999 no existe en la base de datos.",
  "campo": "ClienteProveedorId",
  "valorId": 99999
}
```

---

### **ERROR 3: FK No Encontrada - AgenteId**

**Status:** `400 Bad Request`

**Request:**
```json
POST /api/Cotizaciones
{
  "documentoDeId": 1,
  "conceptoDocumentoId": 5,
  "clienteProveedorId": 10,
  "agenteId": 99999,
  // ...resto de campos
}
```

**Response:**
```json
{
  "error": "FOREIGN_KEY_NOT_FOUND",
  "message": "El AgenteId con ID 99999 no existe en la base de datos.",
  "campo": "AgenteId",
  "valorId": 99999
}
```

---

### **ERROR 4: Registro Duplicado**

**Status:** `409 Conflict`

**Request:**
```json
POST /api/Cotizaciones
{
  "folio": 1001,  // Ya existe
  // ...resto de campos
}
```

**Response:**
```json
{
  "error": "DUPLICATE_RECORD",
  "message": "Ya existe un registro con Folio = '1001'.",
  "campo": "Folio",
  "valor": "1001"
}
```

---

### **ERROR 5: Validación de Campos Requeridos**

**Status:** `400 Bad Request`

**Request:**
```json
POST /api/Cotizaciones
{
  "serieDocumento": "COT",
  "folio": 1002
  // Faltan campos requeridos
}
```

**Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "DocumentoDeId": [
      "The DocumentoDeId field is required."
    ],
    "ConceptoDocumentoId": [
      "The ConceptoDocumentoId field is required."
    ],
    "ClienteProveedorId": [
      "The ClienteProveedorId field is required."
    ],
    "AgenteId": [
      "The AgenteId field is required."
    ]
  }
}
```

---

### **ERROR 6: Cotización No Encontrada**

**Status:** `404 Not Found`

**Request:**
```
GET /api/Cotizaciones/99999
```

**Response:**
```json
{
  "message": "Cotización con ID 99999 no encontrada."
}
```

---

### **ERROR 7: Valor de Estado Inválido**

**Status:** `400 Bad Request`

**Request:**
```
GET /api/Cotizaciones/estado/cancelado/5
```

**Response:**
```json
{
  "message": "El valor debe ser 0 o 1."
}
```

---

### **ERROR 8: Acceso Denegado**

**Status:** `403 Forbidden`

**Request:**
```json
POST /api/Cotizaciones
{
  "agenteId": 10,  // Usuario sin permisos
  // ...resto de campos
}
```

**Response:**
```json
{
  "error": "ACCESS_DENIED",
  "message": "No se puede acceder al recurso solicitado."
}
```

---

### **ERROR 9: Error Interno del Servidor**

**Status:** `500 Internal Server Error`

**Response:**
```json
{
  "message": "Error interno del servidor."
}
```

---

### **ERROR 10: No Autorizado (Sin Token)**

**Status:** `401 Unauthorized`

**Request:**
```
GET /api/Cotizaciones
(Sin header Authorization)
```

**Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

---

## 🟡 RESPUESTAS CON RESULTADOS VACÍOS

### **GET con Filtros Sin Resultados**

**Status:** `200 OK`

**Request:**
```
GET /api/Cotizaciones/estado/cancelado/1
(Cuando no hay cotizaciones canceladas)
```

**Response:**
```json
[]
```

**Request:**
```
GET /api/Cotizaciones/cliente/99999
(Cuando el cliente no tiene cotizaciones)
```

**Response:**
```json
[]
```

---

## 📊 CÓDIGOS DE ESTADO HTTP UTILIZADOS

| Código | Descripción | Cuándo se usa |
|--------|-------------|---------------|
| 200 | OK | GET exitoso, PUT exitoso |
| 201 | Created | POST exitoso (cotización creada) |
| 204 | No Content | DELETE exitoso |
| 400 | Bad Request | Validación fallida, FK no encontrada, valor inválido |
| 401 | Unauthorized | Sin token de autenticación |
| 403 | Forbidden | Sin permisos para acceder al recurso |
| 404 | Not Found | Recurso no encontrado (GET/PUT/DELETE por ID) |
| 409 | Conflict | Registro duplicado |
| 500 | Internal Server Error | Error no controlado del servidor |

---

## 🔑 CAMPOS CLAVE DE RESPUESTA

### **Campos Siempre Presentes:**
- `id`: Identificador único
- `fecha`: Fecha de creación (siempre la establece el servidor)
- `cancelado`, `afectado`, `impreso`, `usaCliente`: Banderas de estado (0 o 1)

### **Campos Opcionales (pueden ser null):**
- `fechaVencimiento`
- `fechaEntregaRecepcion`
- `serieDocumento`
- `referencia`
- `observaciones`
- `documentoOrigenId`

### **Campos Calculados:**
- `pendiente`: Total pendiente de pago (calculado por el sistema)

---

## 💡 TIPS PARA CONSUMIR LA API

1. **Siempre incluir token JWT:** Todos los endpoints requieren autenticación
2. **Validar IDs antes de enviar:** Verificar que ClienteProveedorId y AgenteId existan
3. **Manejar arrays vacíos:** Los filtros sin resultados retornan `[]`, no error
4. **Capturar errores por tipo:** Usar el campo `error` para identificar el tipo de problema
5. **Logs del servidor:** Revisar logs para errores 500

---

**Última Actualización:** 12 de Noviembre, 2025
