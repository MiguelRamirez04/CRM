# 📸 API de Gestión de Fotos de Reparaciones

## 🎯 Descripción
Sistema completo de subida, descarga y gestión de fotos para reparaciones con conversión automática a formato WebP para optimización de tamaño y rendimiento.

---

## ✨ Características

- ✅ **Subida de fotos** con conversión automática a WebP
- ✅ **Compresión inteligente** (calidad configurable, default 80%)
- ✅ **Redimensionamiento automático** (máx 1920x1080px)
- ✅ **Validación de archivos** (tipo, tamaño, contenido)
- ✅ **Metadatos completos** (dimensiones, tamaño original, etc.)
- ✅ **Descarga optimizada** con streaming
- ✅ **Eliminación segura** (archivo físico + BD)
- ✅ **Auditoría completa** (usuario, fecha, cambios)
- ✅ **Transacciones atómicas** (rollback en errores)

---

## 🏗️ Arquitectura

### **Componentes**

```
┌─────────────────────────────────────────────┐
│         ReparacionFotosController           │
│  (POST, GET, DELETE /api/reparaciones/      │
│             {id}/fotos)                      │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│        ReparacionFotoService                │
│  - UploadFotoAsync()                        │
│  - GetFotosByReparacionIdAsync()            │
│  - DownloadFotoAsync()                      │
│  - DeleteFotoAsync()                        │
└─────────────────┬───────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────┐
│      ImageProcessingService                 │
│  - ConvertToWebPAsync()                     │
│  - IsValidImage()                           │
│  - IsValidImageContentAsync()               │
└─────────────────────────────────────────────┘
```

### **Modelos**

- **`FilesDocumento`**: Tabla genérica para documentos (polimórfica)
- **`ReparacionFoto`**: Relación entre reparación y foto con metadatos específicos

---

## 🚀 Endpoints

### **1. Subir Foto**
```http
POST /api/reparaciones/{reparacionId}/fotos
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Data:
- Archivo: [imagen.jpg]
- Etapa: "Recibido" (opcional)
- Descripcion: "Foto del equipo al momento de ingreso" (opcional)
```

**Respuesta (201 Created):**
```json
{
  "id": 123,
  "reparacionId": 456,
  "documentoId": 789,
  "etapa": "Recibido",
  "descripcion": "Foto del equipo al momento de ingreso",
  "creadoEn": "2025-10-15T13:45:00Z",
  "nombreArchivo": "abc123_imagen.webp",
  "mimeType": "image/webp",
  "tamanoBytes": 245678,
  "creadoPorUsuario": "Juan Pérez",
  "urlDescarga": "/api/reparaciones/456/fotos/123/download",
  "metadatos": "{\"ArchivoOriginal\":\"imagen.jpg\",\"Ancho\":1920,\"Alto\":1080}"
}
```

---

### **2. Listar Fotos de una Reparación**
```http
GET /api/reparaciones/{reparacionId}/fotos
Authorization: Bearer {token}
```

**Respuesta (200 OK):**
```json
[
  {
    "id": 123,
    "reparacionId": 456,
    "documentoId": 789,
    "etapa": "Recibido",
    "descripcion": "Foto inicial",
    "creadoEn": "2025-10-15T13:45:00Z",
    "nombreArchivo": "abc123_imagen.webp",
    "mimeType": "image/webp",
    "tamanoBytes": 245678,
    "creadoPorUsuario": "Juan Pérez",
    "urlDescarga": "/api/reparaciones/456/fotos/123/download"
  }
]
```

---

### **3. Descargar Foto**
```http
GET /api/reparaciones/{reparacionId}/fotos/{id}/download
Authorization: Bearer {token}
```

**Respuesta (200 OK):**
- Content-Type: `image/webp`
- Content-Disposition: `attachment; filename="abc123_imagen.webp"`
- Body: [Binary data]

---

### **4. Eliminar Foto**
```http
DELETE /api/reparaciones/{reparacionId}/fotos/{id}
Authorization: Bearer {token}
```

**Respuesta (204 No Content)**

---

## ⚙️ Configuración

### **appsettings.json**
```json
{
  "FileStorage": {
    "UploadPath": "uploads/reparaciones",
    "MaxFileSizeMB": 10,
    "WebPQuality": 80,
    "MaxImageWidth": 1920,
    "MaxImageHeight": 1080
  }
}
```

### **Parámetros**
- **`UploadPath`**: Ruta relativa donde se guardan archivos (desde directorio del proyecto)
- **`MaxFileSizeMB`**: Tamaño máximo de archivo permitido (default: 10MB)
- **`WebPQuality`**: Calidad de compresión WebP, 1-100 (default: 80)
- **`MaxImageWidth`**: Ancho máximo de imagen (default: 1920px)
- **`MaxImageHeight`**: Alto máximo de imagen (default: 1080px)

---

## 🔒 Seguridad

- **Autenticación JWT**: Requerida para todos los endpoints
- **Roles permitidos**: `Soporte`, `Admin`
- **Validación de archivos**:
  - Tipos permitidos: JPEG, PNG, WebP, GIF
  - Tamaño máximo: 10MB (configurable)
  - Validación de contenido (no solo extensión)
- **Sanitización de nombres**: Caracteres inválidos eliminados
- **Transacciones**: Rollback automático en errores

---

## 📁 Estructura de Archivos

```
uploads/
└── reparaciones/
    ├── abc123_foto1.webp
    ├── def456_foto2.webp
    └── ghi789_foto3.webp
```

---

## 🔧 Migración a Windows Server

### **Pasos para Producción**

1. **Crear carpeta en el servidor:**
   ```powershell
   New-Item -Path "D:\AppFiles\Reparaciones" -ItemType Directory
   ```

2. **Actualizar appsettings.Production.json:**
   ```json
   {
     "FileStorage": {
       "UploadPath": "D:\\AppFiles\\Reparaciones"
     }
   }
   ```

3. **Configurar permisos en IIS:**
   - El usuario del Application Pool debe tener permisos de lectura/escritura en `D:\AppFiles\Reparaciones`

4. **Backup:**
   - Programar backups regulares de la carpeta de archivos y la base de datos

---

## 🧪 Pruebas con Postman

### **Subir Foto**
1. Método: `POST`
2. URL: `http://localhost:5000/api/reparaciones/1/fotos`
3. Headers:
   - `Authorization: Bearer {tu_token}`
4. Body: `form-data`
   - `Archivo`: Seleccionar archivo de imagen
   - `Etapa`: "Recibido"
   - `Descripcion`: "Prueba de subida"

### **Descargar Foto**
1. Método: `GET`
2. URL: `http://localhost:5000/api/reparaciones/1/fotos/123/download`
3. Headers:
   - `Authorization: Bearer {tu_token}`
4. Send and Download: Guardar archivo

---

## 📊 Logs

El sistema genera logs detallados en:
- **Consola**: Logs en tiempo real
- **Archivo**: `logs/app-{fecha}.log`

Ejemplos:
```
[13:45:00 INF] Iniciando subida de foto para reparación 456 por usuario 1
[13:45:01 INF] Imagen convertida exitosamente a WebP: uploads/reparaciones/abc123_foto.webp, Tamaño: 240.12KB
[13:45:02 INF] Foto subida exitosamente para reparación 456, Foto ID: 123, Tamaño: 245KB
```

---

## 🛠️ Tecnologías

- **ASP.NET Core 8**: Framework web
- **Entity Framework Core**: ORM para base de datos
- **SixLabors.ImageSharp**: Procesamiento de imágenes
- **SQL Server**: Base de datos
- **Serilog**: Logging estructurado
- **JWT**: Autenticación

---

## 📝 Notas Importantes

- Las imágenes se convierten automáticamente a WebP (60-80% más ligeras)
- El archivo original NO se guarda, solo la versión WebP optimizada
- Los metadatos del archivo original se guardan en JSON
- Eliminación física y lógica simultánea (archivo + BD)
- Las transacciones aseguran consistencia entre archivo físico y BD

---

## 🆘 Troubleshooting

### **Error: "El archivo no puede exceder 10MB"**
- Aumentar `MaxFileSizeMB` en `appsettings.json`
- Verificar límite en `Program.cs` → `FormOptions.MultipartBodyLengthLimit`

### **Error: "Directorio no encontrado"**
- Verificar que la carpeta existe: `uploads/reparaciones`
- Verificar permisos de escritura del usuario del proceso

### **Error: "Foto no encontrada"**
- Verificar que el archivo físico existe en la ruta almacenada
- Revisar logs para errores de guardado previos

---

## 👥 Contribuidores

- **Desarrollador Principal**: [Tu Nombre]
- **Fecha de Creación**: 15 de Octubre, 2025

---

## 📄 Licencia

Proyecto privado - Todos los derechos reservados
