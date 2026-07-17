# 🧪 Ejemplos de Prueba para Sistema de Archivos

## 📁 Estructura de Carpetas Creada

```
C:\Users\adria\source\repos\fullstack_cabs\back_cabs\CRM\uploads\
├── evaluacion/     ✅ (para fotos de evaluación → conversión a WebP)
└── reparacion/     ✅ (para fotos de reparación → conversión a WebP)
```

---

## 🔧 Configuración Actual

**appsettings.json:**
```json
{
  "FileStorage": {
    "UploadPath": "CRM/uploads",
    "MaxFileSizeMB": 10,
    "WebPQuality": 80,
    "MaxImageWidth": 1920,
    "MaxImageHeight": 1080
  }
}
```

---

## 📤 Ejemplo 1: Subir Foto de Evaluación (se convertirá a WebP)

### **Request (Postman/Thunder Client/REST Client):**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body (form-data):
┌─────────────────┬──────────────────────────────────────────┐
│ Key             │ Value                                    │
├─────────────────┼──────────────────────────────────────────┤
│ Archivo         │ [FILE] Seleccionar imagen (JPG/PNG)     │
│ EntidadTipo     │ Evaluacion                               │
│ EntidadId       │ 1                                        │
│ Descripcion     │ Foto de daño frontal antes de reparar   │
│ Categoria       │ FotoEvaluacion                           │
└─────────────────┴──────────────────────────────────────────┘
```

### **Respuesta Esperada (201 Created):**

```json
{
  "id": 1,
  "nombreArchivo": "abc123-def456-789.webp",
  "nombreOriginal": "foto_dano_frontal.jpg",
  "mimeType": "image/webp",
  "tamanoBytes": 245000,
  "tamanoFormateado": "239.26 KB",
  "creadoEn": "2025-10-17T18:14:30Z",
  "creadoPorNombre": "Usuario Admin",
  "descripcion": "Foto de daño frontal antes de reparar",
  "urlDescarga": "/api/Files/1/download",
  "esImagen": true,
  "convertidoAWebP": true,
  "categoria": "FotoEvaluacion"
}
```

### **Verificación:**
- ✅ Archivo guardado en: `CRM/uploads/evaluacion/abc123-def456-789.webp`
- ✅ Imagen convertida de JPG/PNG a WebP
- ✅ Tamaño reducido (optimizado)
- ✅ Registro creado en tabla `files_documentos`

---

## 📤 Ejemplo 2: Subir Foto de Reparación (se convertirá a WebP)

### **Request:**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body (form-data):
┌─────────────────┬──────────────────────────────────────────┐
│ Key             │ Value                                    │
├─────────────────┼──────────────────────────────────────────┤
│ Archivo         │ [FILE] Seleccionar imagen               │
│ EntidadTipo     │ Reparacion                               │
│ EntidadId       │ 5                                        │
│ Descripcion     │ Reparación de parachoques - durante     │
│ Categoria       │ FotoDurante                              │
└─────────────────┴──────────────────────────────────────────┘
```

### **Respuesta Esperada (201 Created):**

```json
{
  "id": 2,
  "nombreArchivo": "xyz789-abc123-456.webp",
  "nombreOriginal": "reparacion_parachoques.png",
  "mimeType": "image/webp",
  "tamanoBytes": 180000,
  "tamanoFormateado": "175.78 KB",
  "creadoEn": "2025-10-17T18:16:45Z",
  "creadoPorNombre": "Técnico Juan",
  "descripcion": "Reparación de parachoques - durante",
  "urlDescarga": "/api/Files/2/download",
  "esImagen": true,
  "convertidoAWebP": true,
  "categoria": "FotoDurante"
}
```

### **Verificación:**
- ✅ Archivo guardado en: `CRM/uploads/reparacion/xyz789-abc123-456.webp`
- ✅ Conversión a WebP completada
- ✅ Registro en BD con checksum SHA256

---

## 📤 Ejemplo 3: Subir PDF de Gasto Viático (sin conversión)

### **Request:**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body (form-data):
┌─────────────────┬──────────────────────────────────────────┐
│ Key             │ Value                                    │
├─────────────────┼──────────────────────────────────────────┤
│ Archivo         │ [FILE] Seleccionar PDF                   │
│ EntidadTipo     │ GastoViatico                             │
│ EntidadId       │ 10                                       │
│ Descripcion     │ Factura de gasolina - viaje a cliente   │
│ Categoria       │ Factura                                  │
└─────────────────┴──────────────────────────────────────────┘
```

### **Respuesta Esperada (201 Created):**

```json
{
  "id": 3,
  "nombreArchivo": "def456-ghi789-012.pdf",
  "nombreOriginal": "factura_gasolina_2025.pdf",
  "mimeType": "application/pdf",
  "tamanoBytes": 850000,
  "tamanoFormateado": "830.08 KB",
  "creadoEn": "2025-10-17T18:20:12Z",
  "creadoPorNombre": "Usuario Admin",
  "descripcion": "Factura de gasolina - viaje a cliente",
  "urlDescarga": "/api/Files/3/download",
  "esImagen": false,
  "convertidoAWebP": false,
  "categoria": "Factura"
}
```

### **Verificación:**
- ✅ Archivo guardado en: `CRM/uploads/gastoviatico/def456-ghi789-012.pdf`
- ✅ PDF guardado sin conversión (original)
- ✅ Registro en BD

---

## 📥 Ejemplo 4: Descargar Archivo

### **Request:**

```http
GET http://localhost:5176/api/Files/1/download
```

**No requiere Authorization** (AllowAnonymous)

### **Respuesta:**
- Stream del archivo
- Headers:
  - `Content-Type: image/webp`
  - `Content-Disposition: attachment; filename="foto_dano_frontal.jpg"`

---

## 📋 Ejemplo 5: Listar Fotos de una Evaluación

### **Request:**

```http
GET http://localhost:5176/api/Files/entity?EntidadTipo=Evaluacion&EntidadId=1&SoloImagenes=true
Authorization: Bearer {{token}}
```

### **Respuesta (200 OK):**

```json
[
  {
    "id": 1,
    "nombreArchivo": "abc123-def456-789.webp",
    "nombreOriginal": "foto_dano_frontal.jpg",
    "mimeType": "image/webp",
    "tamanoBytes": 245000,
    "tamanoFormateado": "239.26 KB",
    "creadoEn": "2025-10-17T18:14:30Z",
    "urlDescarga": "/api/Files/1/download",
    "esImagen": true,
    "convertidoAWebP": true,
    "categoria": "FotoEvaluacion"
  },
  {
    "id": 4,
    "nombreArchivo": "mno345-pqr678-901.webp",
    "nombreOriginal": "foto_lateral_izquierdo.jpg",
    "mimeType": "image/webp",
    "tamanoBytes": 198000,
    "tamanoFormateado": "193.36 KB",
    "creadoEn": "2025-10-17T18:22:10Z",
    "urlDescarga": "/api/Files/4/download",
    "esImagen": true,
    "convertidoAWebP": true,
    "categoria": "FotoEvaluacion"
  }
]
```

---

## ❌ Ejemplo 6: Subir Archivo Duplicado

### **Request:**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body: (mismo archivo que ya fue subido anteriormente)
```

### **Respuesta (409 Conflict):**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Este archivo ya fue subido anteriormente (ID: 1).",
  "traceId": "00-abc123-def456-00"
}
```

---

## ❌ Ejemplo 7: Subir Archivo Muy Grande

### **Request:**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body: (archivo de 15MB, excede el límite de 10MB)
```

### **Respuesta (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "El archivo no puede exceder 10MB. Tamaño actual: 15MB",
  "traceId": "00-xyz789-uvw012-00"
}
```

---

## ❌ Ejemplo 8: Subir Tipo de Archivo No Permitido

### **Request:**

```http
POST http://localhost:5176/api/Files/upload
Content-Type: multipart/form-data
Authorization: Bearer {{token}}

Body (form-data):
┌─────────────────┬──────────────────────────────────────────┐
│ Archivo         │ [FILE] script.exe                        │
│ EntidadTipo     │ Evaluacion                               │
│ EntidadId       │ 1                                        │
└─────────────────┴──────────────────────────────────────────┘
```

### **Respuesta (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Tipo de archivo no permitido: application/x-msdownload. Permitidos: imágenes (JPEG, PNG, WebP, GIF), PDF, Excel, Word.",
  "traceId": "00-ghi345-jkl678-00"
}
```

---

## ✅ Ejemplo 9: Validar Integridad de Archivo

### **Request:**

```http
GET http://localhost:5176/api/Files/1/validate
Authorization: Bearer {{token}}
```

### **Respuesta (200 OK) - Archivo Íntegro:**

```json
{
  "id": 1,
  "esValido": true,
  "mensaje": "El archivo es íntegro y no ha sido modificado"
}
```

### **Respuesta (400 Bad Request) - Archivo Corrupto:**

```json
{
  "id": 1,
  "esValido": false,
  "mensaje": "El archivo ha sido modificado o está corrupto. El checksum no coincide."
}
```

---

## ℹ️ Ejemplo 10: Obtener Información de Archivo

### **Request:**

```http
GET http://localhost:5176/api/Files/1
Authorization: Bearer {{token}}
```

### **Respuesta (200 OK):**

```json
{
  "id": 1,
  "nombreArchivo": "abc123-def456-789.webp",
  "nombreOriginal": "foto_dano_frontal.jpg",
  "mimeType": "image/webp",
  "tamanoBytes": 245000,
  "tamanoFormateado": "239.26 KB",
  "creadoEn": "2025-10-17T18:14:30Z",
  "creadoPorNombre": "Usuario Admin",
  "descripcion": "Foto de daño frontal antes de reparar",
  "urlDescarga": "/api/Files/1/download",
  "esImagen": true,
  "convertidoAWebP": true,
  "categoria": "FotoEvaluacion"
}
```

---

## 🗑️ Ejemplo 11: Eliminar Archivo (Soft Delete)

### **Request:**

```http
DELETE http://localhost:5176/api/Files/1
Authorization: Bearer {{token}}
```

### **Respuesta (204 No Content):**

(Sin cuerpo de respuesta)

### **Verificación:**
- ✅ Campo `activo` en BD cambiado a `false`
- ✅ Archivo físico NO eliminado (preservado)
- ✅ Ya no aparece en listados por defecto

---

## 🚀 Cómo Probar en Postman

### **Paso 1: Obtener Token JWT**

```http
POST http://localhost:5176/api/Auth/login
Content-Type: application/json

{
  "email": "admin@cabs.com",
  "password": "tu_password"
}
```

**Copiar el token de la respuesta.**

---

### **Paso 2: Configurar Variable de Entorno**

En Postman:
1. Click en "Environments"
2. Crear nuevo environment "CABs Local"
3. Agregar variables:
   - `baseUrl` = `http://localhost:5176`
   - `token` = (pegar el token JWT)

---

### **Paso 3: Crear Request de Subida de Imagen**

1. **Method:** POST
2. **URL:** `{{baseUrl}}/api/Files/upload`
3. **Authorization:**
   - Type: Bearer Token
   - Token: `{{token}}`
4. **Body:**
   - Type: form-data
   - Agregar campos:
     - `Archivo` (type: File) → Seleccionar imagen
     - `EntidadTipo` (type: Text) → `Evaluacion`
     - `EntidadId` (type: Text) → `1`
     - `Descripcion` (type: Text) → `Foto de prueba`
     - `Categoria` (type: Text) → `FotoEvaluacion`

5. **Click "Send"**

---

## 📊 Verificar en Base de Datos

```sql
-- Ver todos los archivos
SELECT 
    id,
    nombre_archivo,
    nombre_original,
    entidad_tipo,
    entidad_id,
    mimetype,
    tamano_bytes,
    convertido_a_webp = CASE 
        WHEN mimetype = 'image/webp' THEN 'SI' 
        ELSE 'NO' 
    END,
    activo,
    creado_en
FROM files_documentos
ORDER BY creado_en DESC;

-- Ver archivos de evaluación
SELECT * FROM files_documentos 
WHERE entidad_tipo = 'Evaluacion' AND activo = 1;

-- Ver archivos de reparación
SELECT * FROM files_documentos 
WHERE entidad_tipo = 'Reparacion' AND activo = 1;
```

---

## 🎯 Checklist de Pruebas

- [ ] Subir imagen JPG para Evaluación → Verificar conversión a WebP
- [ ] Subir imagen PNG para Reparación → Verificar conversión a WebP
- [ ] Subir PDF para GastoViatico → Verificar guardado sin conversión
- [ ] Intentar subir archivo duplicado → Verificar error 409
- [ ] Intentar subir archivo > 10MB → Verificar error 400
- [ ] Intentar subir archivo .exe → Verificar error 400
- [ ] Descargar archivo por ID → Verificar descarga correcta
- [ ] Listar archivos de una entidad → Verificar filtros
- [ ] Validar integridad → Verificar checksum
- [ ] Eliminar archivo → Verificar soft delete
- [ ] Verificar que archivo físico NO se elimina tras soft delete

---

## 🎉 ¡Listo para Probar!

Todas las carpetas están creadas:
- ✅ `CRM/uploads/evaluacion/`
- ✅ `CRM/uploads/reparacion/`

El sistema creará automáticamente carpetas adicionales cuando subas archivos de otros tipos:
- `CRM/uploads/gastoviatico/`
- `CRM/uploads/ordentrabajo/`
- Etc.

**¡Ahora puedes empezar a probar con Postman, Thunder Client o REST Client!** 🚀
